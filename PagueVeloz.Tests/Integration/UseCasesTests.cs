using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using PagueVeloz.Application.DTOs;
using PagueVeloz.Infrastructure.Persistence;
using System.Net.Http.Json;

namespace PagueVeloz.Tests.Integration;

public class UseCasesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UseCasesTests(WebApplicationFactory<Program> factory)
    {
        // Configura o cliente HTTP com banco em memória isolado
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("UseCasesDb"));
            });
        }).CreateClient();
    }

    // Helper para criar conta rapidamente
    private async Task CreateAccountAsync(string accountId, long limit = 0)
    {
        var command = new
        {
            client_id = "CLI-TEST",
            account_id = accountId,
            initial_balance = 0,
            credit_limit = limit
        };
        var response = await _client.PostAsJsonAsync("/api/accounts", command);
        response.EnsureSuccessStatusCode();
    }

    // Helper para enviar transação
    private async Task<CreateTransactionResponse?> PostTransactionAsync(string accountId, string op, long amount, string refId)
    {
        var command = new
        {
            operation = op,
            account_id = accountId,
            amount = amount,
            currency = "BRL",
            reference_id = refId
        };
        var response = await _client.PostAsJsonAsync("/api/transactions", command);

        // Retorna o objeto mesmo se falhar (pois queremos testar status 'failed')
        return await response.Content.ReadFromJsonAsync<CreateTransactionResponse>();
    }

    // --- CASOS DE TESTE ABAIXO ---
}