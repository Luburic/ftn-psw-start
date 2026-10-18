using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.Tours;

internal static class TourConfiguration
{
    public static void ConfigureTours(this ModelBuilder modelBuilder)
    {
        ConfigureTour(modelBuilder.Entity<Tour>());
        ConfigureKeyPoint(modelBuilder.Entity<KeyPoint>());
        ConfigureMediaItem(modelBuilder.Entity<MediaItem>());
    }

    private static void ConfigureTour(EntityTypeBuilder<Tour> builder)
    {
        builder.Property(tour => tour.Name).HasMaxLength(200);
        builder.Property(tour => tour.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.Property(tour => tour.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(tour => tour.TransportTimes, transportTimes =>
        {
            transportTimes.ToJson();
            transportTimes.Property(time => time.Transport).HasConversion<string>();
        });

        builder.HasMany(tour => tour.KeyPoints)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(tour => tour.KeyPoints).AutoInclude();
    }

    private static void ConfigureKeyPoint(EntityTypeBuilder<KeyPoint> builder)
    {
        builder.ToTable("KeyPoints");
        builder.Property(keyPoint => keyPoint.Id).ValueGeneratedNever();

        builder.OwnsOne(keyPoint => keyPoint.Location);

        builder.HasMany(keyPoint => keyPoint.Media)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(keyPoint => keyPoint.Media).AutoInclude();
    }

    private static void ConfigureMediaItem(EntityTypeBuilder<MediaItem> builder)
    {
        builder.ToTable("MediaItems");
        builder.Property(mediaItem => mediaItem.Id).ValueGeneratedNever();
    }
}
