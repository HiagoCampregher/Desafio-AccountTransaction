using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Interfaces;

namespace PagueVeloz.Infrastructure.Persistence;

// Implementamos a interface IUnitOfWork definida no Domain
public class AppDbContext : DbContext, IUnitOfWork
{
    // Mapeamento das tabelas
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Isso vai ler seus arquivos AccountConfiguration.cs e TransactionConfiguration.cs
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // Implementação da transação atômica do UnitOfWork
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Tenta salvar todas as alterações (Account e Transaction) numa única transação
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // --- PONTO CRÍTICO ---
            // Quando o Lock Otimista falha (alguém alterou o saldo enquanto líamos),
            // o EF Core lança DbUpdateConcurrencyException.
            // Relançamos uma Exception com mensagem específica para o Polly capturar e fazer o Retry.
            throw new Exception("Conflito de concorrência detectado.", ex);
        }
    }
}