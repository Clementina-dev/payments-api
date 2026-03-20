using Microsoft.EntityFrameworkCore;
using PaymentsApi.Api.Domain;

namespace PaymentsApi.Api.Data;

public sealed class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentIdempotencyKey> PaymentIdempotencyKeys => Set<PaymentIdempotencyKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Reference)
                .HasMaxLength(64)
                .IsRequired();

            b.HasIndex(x => x.Reference)
                .IsUnique();

            b.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            b.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            b.Property(x => x.Status)
                .HasMaxLength(24)
                .IsRequired();

            b.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<PaymentIdempotencyKey>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Key)
                .HasMaxLength(100)
                .IsRequired();

            b.HasIndex(x => x.Key)
                .IsUnique();

            b.Property(x => x.PaymentId)
                .IsRequired();

            b.Property(x => x.CreatedAtUtc)
                .IsRequired();
        });
    }
}