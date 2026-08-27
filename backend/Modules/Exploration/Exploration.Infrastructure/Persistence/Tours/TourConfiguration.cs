using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.Property(tour => tour.Name).HasMaxLength(200);
        builder.Property(tour => tour.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.Property(tour => tour.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(tour => tour.TransportTimes, transportTimes =>
        {
            transportTimes.ToJson();
            transportTimes.Property(time => time.Transport).HasConversion<string>();
        });
    }
}
