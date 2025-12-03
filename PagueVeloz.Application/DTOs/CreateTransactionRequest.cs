using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PagueVeloz.Application.DTOs;

public record CreateTransactionRequest(
    [property: RegularExpression("^(credit|debit|reserve|capture|reversal|transfer)$", ErrorMessage = "Operação desconhecida.")]
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("account_id")] string AccountId,
    [property: JsonPropertyName("amount")] long Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("reference_id")] string ReferenceId,
    [property: JsonPropertyName("metadata")] Dictionary<string, string>? Metadata
);