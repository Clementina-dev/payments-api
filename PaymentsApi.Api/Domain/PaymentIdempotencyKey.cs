namespace PaymentsApi.Api.Domain;

public sealed class PaymentIdempotencyKey
{
    public long Id { get; set; }

    public string Key { get; set; } = default!;

    public Guid PaymentId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}