using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentsApi.Api.Contracts;
using PaymentsApi.Api.Data;
using PaymentsApi.Api.Domain;

namespace PaymentsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController : ControllerBase
{
    private const string IdempotencyHeader = "Idempotency-Key";
    private readonly PaymentsDbContext _db;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentsDbContext db, ILogger<PaymentsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(
        [FromBody] CreatePaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return BadRequest(new { error = $"Missing required header: {IdempotencyHeader}" });
        }

        var idemKey = idempotencyKey.Trim();

        if (request.Amount <= 0) return BadRequest(new { error = "Amount must be greater than 0." });
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            return BadRequest(new { error = "Currency must be a 3-letter code (e.g., NGN)." });

        // If we have seen the idempotency key before, return the original payment
        var existingKey = await _db.PaymentIdempotencyKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == idemKey, ct);

        if (existingKey is not null)
        {
            var existingPayment = await _db.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == existingKey.PaymentId, ct);

            if (existingPayment is null)
            {
                // Defensive: key exists but payment missing (should not happen)
                _logger.LogWarning("Idempotency key {Key} found but payment {PaymentId} missing", idemKey, existingKey.PaymentId);
                return Conflict(new { error = "Idempotency conflict. Please retry with a new key." });
            }

            return Ok(ToResponse(existingPayment));
        }

        var payment = new Payment
        {
            Reference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".ToUpperInvariant()[..32],
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            Narration = request.Narration,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };

        // Transaction: insert payment + idempotency key
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);

            _db.PaymentIdempotencyKeys.Add(new PaymentIdempotencyKey
            {
                Key = idemKey,
                PaymentId = payment.Id,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, ToResponse(payment));
        }
        catch (DbUpdateException ex)
        {
            // Most likely: unique constraint conflict on Key or Reference.
            await tx.RollbackAsync(ct);
            _logger.LogWarning(ex, "DbUpdateException creating payment for idempotency key {Key}", idemKey);

            // Attempt to re-read by idempotency key (covers race conditions)
            var racedKey = await _db.PaymentIdempotencyKeys.AsNoTracking().FirstOrDefaultAsync(x => x.Key == idemKey, ct);
            if (racedKey is not null)
            {
                var racedPayment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == racedKey.PaymentId, ct);
                if (racedPayment is not null) return Ok(ToResponse(racedPayment));
            }

            return Conflict(new { error = "Could not create payment due to a conflict. Retry with the same Idempotency-Key." });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (payment is null) return NotFound();
        return Ok(ToResponse(payment));
    }

    [HttpGet]
    public async Task<ActionResult<PaymentResponse>> GetByReference([FromQuery] string reference, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reference)) return BadRequest(new { error = "reference is required" });

        var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Reference == reference, ct);
        if (payment is null) return NotFound();
        return Ok(ToResponse(payment));
    }

    private static PaymentResponse ToResponse(Payment p) =>
        new(p.Id, p.Reference, p.Amount, p.Currency, p.Status, p.Narration, p.CreatedAtUtc);
}