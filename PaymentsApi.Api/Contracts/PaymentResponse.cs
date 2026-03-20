namespace PaymentsApi.Api.Contracts;

public sealed record PaymentResponse(
    Guid Id,
    string Reference,
    decimal Amount,
    string Currency,
    string Status,
    string? Narration,
    DateTime CreatedAtUtc
);