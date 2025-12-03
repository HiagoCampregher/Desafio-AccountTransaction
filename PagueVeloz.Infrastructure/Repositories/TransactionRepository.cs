using Microsoft.EntityFrameworkCore;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Interfaces;
using PagueVeloz.Infrastructure.Persistence;

namespace PagueVeloz.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<bool> ExistsByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        // Validação de Idempotência: Checa se já existe transação com este ID externo [cite: 67]
        return await _context.Transactions
            .AnyAsync(t => t.ReferenceId == referenceId, cancellationToken);
    }
}