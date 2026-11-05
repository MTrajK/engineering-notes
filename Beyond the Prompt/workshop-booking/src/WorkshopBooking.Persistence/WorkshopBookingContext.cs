using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Persistence;

public sealed class WorkshopBookingContext(DbContextOptions<WorkshopBookingContext> options) : DbContext(options)
{
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Registration> Registrations => Set<Registration>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkshopBookingContext).Assembly);
}

internal sealed class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
{
    public void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Topic).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NormalizedTopic).IsRequired().UseCollation("BINARY");
        builder.Property(x => x.StartsAt).HasConversion(new UtcTicksConverter());
        builder.Property(x => x.EndsAt).HasConversion(new UtcTicksConverter());
    }
}

internal sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(254);
        builder.Property(x => x.NormalizedEmail).IsRequired().UseCollation("BINARY");
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
    }
}

internal sealed class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CreatedAt).HasConversion(new UtcTicksConverter());
        builder.HasOne<Workshop>().WithMany().HasForeignKey(x => x.WorkshopId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Participant>().WithMany().HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.WorkshopId, x.ParticipantId }).IsUnique();
    }
}

internal sealed class UtcTicksConverter() : ValueConverter<DateTimeOffset, long>(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
