using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Domain.Interfaces;

public interface IAccountRepository
{
    // Usado para buscar a conta pela chave primária (interno)
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // --- O MÉTODO QUE FALTAVA ---
    // Usado para buscar a conta pelo código de negócio (ex: "ACC-001") que vem no JSON
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);

    // Verifica existência (para validação rápida na criação)
    Task<bool> ExistsAsync(string accountNumber, CancellationToken cancellationToken = default);

    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    void Update(Account account);
}