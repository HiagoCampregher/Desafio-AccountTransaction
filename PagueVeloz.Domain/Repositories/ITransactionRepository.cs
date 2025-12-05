using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<bool> ExistsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);
}