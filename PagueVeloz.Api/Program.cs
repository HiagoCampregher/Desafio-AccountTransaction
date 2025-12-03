using Microsoft.EntityFrameworkCore;
using PagueVeloz.Application.Services;
using PagueVeloz.Domain.Interfaces;
using PagueVeloz.Infrastructure.Persistence;
using PagueVeloz.Infrastructure.Repositories;
using Scalar.AspNetCore; // <--- Importante: Adicione este using

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração do Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Injeção de Dependências
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();

// --- MUDANÇA 1: Usando o OpenApi nativo do .NET 9 ---
// Removemos "AddSwaggerGen" e usamos este:
builder.Services.AddOpenApi();
// ----------------------------------------------------

var app = builder.Build();

// --- MUDANÇA 2: Configurando a interface do Scalar ---
if (app.Environment.IsDevelopment())
{
    // Removemos "UseSwagger" e "UseSwaggerUI"
    app.MapOpenApi();            // Gera o JSON
    app.MapScalarApiReference(); // Gera a interface visual bonita
}
// ----------------------------------------------------

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// --- MUDANÇA 3: Migrations no lugar certo ---
// Deve rodar DEPOIS do Build e ANTES do Run
// ... (seu código anterior: app.MapControllers, etc)

// Bloco de Inicialização do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Verifica se consegue conectar antes de tentar migrar
        if (context.Database.CanConnect())
        {
            Console.WriteLine("Conexão com o banco estabelecida com sucesso! Aplicando Migrations...");
            context.Database.Migrate();
            Console.WriteLine("Migrations aplicadas!");
        }
        else
        {
            Console.WriteLine("AVISO: Não foi possível conectar ao banco de dados.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n--- ERRO NA MIGRAÇÃO ---");
        Console.WriteLine($"Mensagem: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Detalhe Técnico (Inner Exception): {ex.InnerException.Message}");
        }
        Console.WriteLine("------------------------\n");
    }
}

app.Run();