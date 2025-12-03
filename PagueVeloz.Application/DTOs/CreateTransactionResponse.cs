using System.Text.Json.Serialization;

namespace PagueVeloz.Application.DTOs;

public record CreateTransactionResponse(
    [property: JsonPropertyName("transaction_id")] string TransactionId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("balance")] long Balance,
    [property: JsonPropertyName("reserved_balance")] long ReservedBalance,
    [property: JsonPropertyName("available_balance")] long AvailableBalance,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("error_message")] string? ErrorMessage
);