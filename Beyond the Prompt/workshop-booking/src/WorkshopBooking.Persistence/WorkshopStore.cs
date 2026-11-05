using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Application.Workshops;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Persistence;

public sealed class WorkshopStore(WorkshopBookingContext context, BookingDatabase database) : IWorkshopStore
{
    public async Task InsertAsync(Workshop workshop, CancellationToken cancellationToken)
    {
        await database.ExecuteAsync(async () =>
        {
            context.Workshops.Add(workshop);
            return await context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    public Task<WorkshopDetails?> GetAsync(Guid id, CancellationToken cancellationToken) => database.ExecuteAsync(() =>
        Project(context.Workshops.Where(x => x.Id == id)).SingleOrDefaultAsync(cancellationToken), cancellationToken);

    public async Task<IReadOnlyList<WorkshopDetails>> ListAsync(string? normalizedTopic, CancellationToken cancellationToken) =>
        await database.ExecuteAsync(() => Project(context.Workshops.Where(x => normalizedTopic == null || x.NormalizedTopic == normalizedTopic).OrderBy(x => x.StartsAt).ThenBy(x => x.Id)).ToListAsync(cancellationToken), cancellationToken);

    private IQueryable<WorkshopDetails> Project(IQueryable<Workshop> workshops) => workshops.AsNoTracking().Select(x => new WorkshopDetails(
        x.Id, x.Title, x.Topic, x.StartsAt, x.EndsAt, x.Capacity, x.Capacity - context.Registrations.Count(r => r.WorkshopId == x.Id)));
}
