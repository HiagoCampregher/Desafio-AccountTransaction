using PagueVeloz.Application.DTOs;

namespace PagueVeloz.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> ProcessTransactionAsync(TransactionRequest request);
}
