using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Interfaces;

namespace PagueVeloz.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountsController(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    // POST api/accounts
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        // Regra simples de criação para facilitar os testes
        if (await _accountRepository.ExistsAsync(request.AccountId, ct))
        {
            return Conflict(new { message = "Conta já existe" });
        }

        var account = new Account(
            request.ClientId,
            request.AccountId,
            request.CreditLimit
        );

        if (request.InitialBalance > 0)
        {
            account.Credit(request.InitialBalance);
        }

        await _accountRepository.AddAsync(account, ct);
        await _unitOfWork.CommitAsync(ct);

        return CreatedAtAction(nameof(CreateAccount), new { id = account.Id }, account);
    }
}

// DTO simples para este controller (pode colocar em arquivo separado)
public record CreateAccountRequest(
    [property: System.Text.Json.Serialization.JsonPropertyName("client_id")] string ClientId,
    [property: System.Text.Json.Serialization.JsonPropertyName("account_id")] string AccountId, // O desafio chama de account_id no JSON, mas mapeamos para AccountNumber
    [property: System.Text.Json.Serialization.JsonPropertyName("initial_balance")] long InitialBalance,
    [property: System.Text.Json.Serialization.JsonPropertyName("credit_limit")] long CreditLimit
);