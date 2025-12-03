using System.Text.Json.Serialization;

namespace PagueVeloz.Application.DTOs;

public enum TransactionStatusDto
{
    Success,
    Failed,
    Pending
}

public class TransactionResponse
{
    [JsonPropertyName("transaction_id")]
    public required string TransactionId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("status")]
    public TransactionStatusDto Status { get; init; }

    [JsonPropertyName("balance")]
    public long Balance { get; init; }

    [JsonPropertyName("reserved_balance")]
    public long ReservedBalance { get; init; }

    [JsonPropertyName("available_balance")]
    public long AvailableBalance { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }
}