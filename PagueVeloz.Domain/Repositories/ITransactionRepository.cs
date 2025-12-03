using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    // CRÍTICO: Para garantir a idempotência exigida no desafio [cite: 67]
    Task<bool> ExistsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);
}