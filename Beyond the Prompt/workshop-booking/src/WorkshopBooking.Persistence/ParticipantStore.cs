using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Participants;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Persistence;

public sealed class ParticipantStore(WorkshopBookingContext context, BookingDatabase database) : IParticipantStore
{
    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) => database.ExecuteAsync(
        () => context.Participants.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken), cancellationToken);

    public Task<Outcome<Unit>> InsertAsync(Participant participant, CancellationToken cancellationToken) => database.ExecuteAsync(async () =>
    {
        context.Participants.Add(participant);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return Outcome.Success(Unit.Value);
        }
        catch (DbUpdateException exception) when (ConstraintFailures.Recognize(exception) is not null)
        {
            context.Entry(participant).State = EntityState.Detached;
            return Outcome.Failed<Unit>(ConstraintFailures.Recognize(exception)!);
        }
    }, cancellationToken);

    public Task<ParticipantDetails?> GetAsync(Guid id, CancellationToken cancellationToken) => database.ExecuteAsync(() => context.Participants.AsNoTracking()
        .Where(x => x.Id == id).Select(x => new ParticipantDetails(x.Id, x.Name, x.Email)).SingleOrDefaultAsync(cancellationToken), cancellationToken);

    public async Task<IReadOnlyList<ParticipantDetails>> ListAsync(CancellationToken cancellationToken) => await database.ExecuteAsync(async () =>
    {
        var participants = await context.Participants.AsNoTracking().Select(x => new { x.Id, x.Name, x.Email, x.NormalizedEmail }).ToListAsync(cancellationToken);
        return (IReadOnlyList<ParticipantDetails>)participants.OrderBy(x => x.NormalizedEmail, StringComparer.Ordinal)
            .ThenBy(x => x.Id.ToString("D"), StringComparer.Ordinal).Select(x => new ParticipantDetails(x.Id, x.Name, x.Email)).ToArray();
    }, cancellationToken);
}
