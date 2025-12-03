using PagueVeloz.Domain.Enums;
using System.Text.Json;

namespace PagueVeloz.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public OperationTypeTransaction Operation { get; private set; }
    public long Amount { get; private set; }
    public string Currency { get; private set; }
    public string ReferenceId { get; private set; }
    public TransactionStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; private set; }

    //public long Balance { get; set; }
    //public long ReservedBalance { get; set; }

    public Transaction(Guid accountId, OperationTypeTransaction operation, long amount, string currency, string referenceId, Dictionary<string, string>? metadata = null)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Operation = operation;
        Amount = amount;
        Currency = currency;
        ReferenceId = referenceId;
        Timestamp = DateTime.UtcNow;
        Status = TransactionStatus.Pending; // Confirmar
        Metadata = metadata;
    }

    public void SetStatusSucess()
    {
        Status = TransactionStatus.Success;
    }

    public void SetStatusFailed(string error)
    {
        Status = TransactionStatus.Failed;
        ErrorMessage = error;
    }
}
