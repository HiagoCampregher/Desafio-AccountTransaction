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

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        if (await _accountRepository.ExistsAsync(request.ClientId, ct))
        {
            return Conflict(new { message = "Conta já existe" });
        }

        var account = new Account(
            request.ClientId,
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
