namespace PaymentsApi.Api.Contracts;

public sealed record CreatePaymentRequest(
    decimal Amount,
    string Currency,
    string? Narration
);