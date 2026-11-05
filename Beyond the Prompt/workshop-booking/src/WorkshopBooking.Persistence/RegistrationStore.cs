using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Registrations;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Persistence;

public sealed class RegistrationStore(WorkshopBookingContext context, BookingDatabase database) : IRegistrationStore
{
    public Task<bool> PairExistsAsync(Guid workshopId, Guid participantId, CancellationToken cancellationToken) => database.ExecuteAsync(
        () => context.Registrations.AnyAsync(x => x.WorkshopId == workshopId && x.ParticipantId == participantId, cancellationToken), cancellationToken);

    public Task<Outcome<Unit>> InsertAsync(Registration registration, CancellationToken cancellationToken) => database.ExecuteAsync(async () =>
    {
        context.Registrations.Add(registration);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Outcome.Success(Unit.Value);
        }
        catch (DbUpdateException exception) when (ConstraintFailures.Recognize(exception) is not null)
        {
            context.Entry(registration).State = EntityState.Detached;
            return Outcome.Failed<Unit>(ConstraintFailures.Recognize(exception)!);
        }
    }, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) => database.ExecuteAsync(async () =>
        await context.Registrations.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken) == 1, cancellationToken);

    public async Task<IReadOnlyList<RegistrationDetails>> ListAsync(Guid workshopId, CancellationToken cancellationToken) => await database.ExecuteAsync(() => context.Registrations.AsNoTracking()
        .Where(x => x.WorkshopId == workshopId).OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
        .Select(x => new RegistrationDetails(x.Id, x.WorkshopId, x.ParticipantId, x.CreatedAt)).ToListAsync(cancellationToken), cancellationToken);
}
