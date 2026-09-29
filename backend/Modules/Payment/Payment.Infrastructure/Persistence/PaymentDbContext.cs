using Microsoft.EntityFrameworkCore;
using Payment.Application;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
    }
}
