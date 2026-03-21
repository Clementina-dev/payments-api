namespace PaymentsApi.Api.Domain;

public sealed class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // reference (unique)
    public string Reference { get; set; } = default!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "NGN";

    // Example statuses: Pending, Successful, Failed, Reversed
    public string Status { get; set; } = "Pending";

    public string? Narration { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}