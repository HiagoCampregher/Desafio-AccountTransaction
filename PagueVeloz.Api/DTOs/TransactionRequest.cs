using System.Text.Json;
using System.Text.Json.Serialization;

namespace PagueVeloz.Application.DTOs;

public enum OperationTypeDto
{
    Credit,
    Debit,
    Reserve,
    Capture,
    Reversal,
    Transfer
}

public enum CurrencyDto
{
    BRL
}

public class TransactionRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required OperationTypeDto Operation { get; init; }

    public required string AccountId { get; init; }

    public required long Amount { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CurrencyDto Currency { get; init; }

    public required string ReferenceId { get; init; }

    public JsonDocument? Metadata { get; init; }
}