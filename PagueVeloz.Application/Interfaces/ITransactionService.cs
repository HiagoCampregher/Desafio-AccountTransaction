using PagueVeloz.Application.DTOs;

public interface ITransactionService
{
    Task<CreateTransactionResponse> ProcessTransactionAsync(CreateTransactionRequest request, CancellationToken ct);
}
