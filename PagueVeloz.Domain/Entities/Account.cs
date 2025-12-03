using PagueVeloz.Domain.Enums;
using PagueVeloz.Domain.Exceptions;
using InvalidOperationException = PagueVeloz.Domain.Exceptions.InvalidOperationException;

namespace PagueVeloz.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string CustomerId { get; private set; } // "Cada cliente pode possuir N contas" [cite: 40]
    // ver se precisa

    public long Balance { get; private set; }
    public long ReservedBalance { get; private set; }
    public long CreditLimit { get; private set; }

    public AccountStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Account()
    {
    }

    public Account(string customerId, long creditLimit)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        //AccountNumber = accountNumber;
        CreditLimit = creditLimit;
        Balance = 0;
        ReservedBalance = 0;
        Status = AccountStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    public void Credit(long amount)
    {
        EnsureActive();
        if (amount <= 0)
            throw new InvalidOperationException("Valor de crédito deve ser positivo.");

        Balance += amount;
    }

    public void Debit(long amount)
    {
        EnsureActive();
        if (amount <= 0) throw new InvalidOperationException("Valor de débito deve ser positivo.");

        long availableFunds = Balance + CreditLimit;

        if (availableFunds < amount)
        {
            throw new InsufficientFundsException();
        }

        Balance -= amount;
    }

    public void Reserve(long amount)
    {
        EnsureActive();
        if (amount <= 0) throw new InvalidOperationException("Valor de reserva deve ser positivo.");

        long availableFunds = Balance + CreditLimit;

        if (availableFunds < amount)
        {
            throw new InsufficientFundsException();
        }

        Balance -= amount; // Remove do disponível
        ReservedBalance += amount; // Adiciona ao reservado
    }

    public void Capture(long amount)
    {
        EnsureActive();
        if (amount <= 0) throw new InvalidOperationException("Valor de captura deve ser positivo.");

        if (ReservedBalance < amount)
        {
            throw new InvalidOperationException("Saldo reservado insuficiente para captura.");
        }

        ReservedBalance -= amount;
    }

    public void CancelReservation(long amount)
    {
        EnsureActive();
        if (amount <= 0) throw new InvalidOperationException("Valor deve ser positivo.");

        if (ReservedBalance < amount)
        {
            throw new InvalidOperationException("Não há saldo reservado suficiente para estornar.");
        }

        ReservedBalance -= amount;
        Balance += amount; // Devolve ao disponível
    }

    private void EnsureActive()
    {
        if (Status != AccountStatus.Active)
            throw new AccountBlockedException();
    }

    public long AvailableBalance => Balance + CreditLimit;
}
