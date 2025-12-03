public record CreateAccountRequest(
    [property: System.Text.Json.Serialization.JsonPropertyName("client_id")] string ClientId,
    [property: System.Text.Json.Serialization.JsonPropertyName("initial_balance")] long InitialBalance,
    [property: System.Text.Json.Serialization.JsonPropertyName("credit_limit")] long CreditLimit
);