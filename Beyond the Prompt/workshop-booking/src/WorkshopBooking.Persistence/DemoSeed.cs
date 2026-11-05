using WorkshopBooking.Domain;

namespace WorkshopBooking.Persistence;

public sealed record DatabaseInitializationOptions(bool SeedDemoData = true);

public static class DemoSeed
{
    public static readonly Guid AvailableWorkshopId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid FullWorkshopId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid CompletedWorkshopId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid AlexId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid BlairId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid CaseyId = Guid.Parse("20000000-0000-0000-0000-000000000003");

    internal static void AddTo(WorkshopBookingContext context, DateTimeOffset startup)
    {
        context.AddRange(
            Workshop.Restore(AvailableWorkshopId, "Practical API Design", "API Design", startup.AddDays(3), startup.AddDays(3).AddHours(2), 4),
            Workshop.Restore(FullWorkshopId, "Testing Together", "Testing", startup.AddDays(5), startup.AddDays(5).AddHours(2), 2),
            Workshop.Restore(CompletedWorkshopId, "Introduction to SQLite", "Databases", startup.AddDays(-5), startup.AddDays(-5).AddHours(2), 3),
            Participant.Restore(AlexId, "Alex River", "Alex.River@example.test"),
            Participant.Restore(BlairId, "Blair Meadow", "Blair.Meadow@example.test"),
            Participant.Restore(CaseyId, "Casey Brook", "Casey.Brook@example.test"),
            Registration.Restore(Guid.Parse("30000000-0000-0000-0000-000000000001"), AvailableWorkshopId, AlexId, startup.AddDays(-1)),
            Registration.Restore(Guid.Parse("30000000-0000-0000-0000-000000000002"), FullWorkshopId, AlexId, startup.AddDays(-2)),
            Registration.Restore(Guid.Parse("30000000-0000-0000-0000-000000000003"), FullWorkshopId, BlairId, startup.AddDays(-1)),
            Registration.Restore(Guid.Parse("30000000-0000-0000-0000-000000000004"), CompletedWorkshopId, BlairId, startup.AddDays(-6)));
    }
}
