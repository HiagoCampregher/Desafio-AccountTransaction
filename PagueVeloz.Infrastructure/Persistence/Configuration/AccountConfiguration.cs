using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Define o nome da tabela no Banco de Dados
        builder.ToTable("Accounts");

        // Define a Chave Primária
        builder.HasKey(a => a.Id);

        // Define que o CustomerId é obrigatório e tem tamanho máximo (varchar 50)
        // Isso atende ao requisito de persistência relacional adequada [cite: 125]
        builder.Property(a => a.CustomerId)
            .IsRequired()
            .HasMaxLength(50);

        // Define que o AccountNumber é obrigatório, tamanho máx 20 e ÚNICO
        builder.Property(a => a.AccountNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.AccountNumber)
            .IsUnique();

        // Configuração do Enum convertendo para string no banco (Ex: "Active", "Blocked")
        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // --- CONCORRÊNCIA OTIMISTA ---
        // Cria uma coluna byte[] que muda automaticamente a cada update.
        // Isso atende aos requisitos de controle de concorrência [cite: 60, 62]
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}