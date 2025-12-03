namespace PagueVeloz.Domain.Interfaces;

public interface IUnitOfWork
{
    // Salva todas as alterações (Account e Transaction) numa única transação atômica [cite: 63]
    Task CommitAsync(CancellationToken cancellationToken = default);
}