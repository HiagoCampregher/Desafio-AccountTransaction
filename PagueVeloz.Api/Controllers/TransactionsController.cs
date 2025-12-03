using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Application.DTOs;
using PagueVeloz.Application.Services;

namespace PagueVeloz.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        CancellationToken ct)
    {
        try
        {
            var response = await _transactionService.ProcessTransactionAsync(request, ct);

            // O desafio pede status 200 mesmo se a transação falhar (status: failed no body),
            // mas retorna o objeto completo.
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // Erros de validação (ex: conta não existe, payload inválido)
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Erro interno não tratado
            return StatusCode(500, new { error = "Erro interno ao processar transação." });
        }
    }
}