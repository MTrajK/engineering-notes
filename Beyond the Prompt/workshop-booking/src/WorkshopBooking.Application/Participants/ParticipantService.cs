using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Application.Participants;

public sealed class ParticipantService(IParticipantStore store, IMutationCoordinator coordinator)
{
    public async Task<Outcome<ParticipantDetails>> CreateAsync(string? name, string? email, CancellationToken cancellationToken)
    {
        Dictionary<string, IReadOnlyList<string>> errors = new(StringComparer.Ordinal);
        string displayName = string.Empty;
        string displayEmail = string.Empty;
        try
        {
            displayName = ParticipantPolicy.Text(name ?? string.Empty, 100);
        }
        catch (ArgumentException) { errors["name"] = ["Name must contain 1 to 100 characters."]; }
        try
        {
            displayEmail = ParticipantPolicy.Email(email ?? string.Empty);
        }
        catch (ArgumentException) { errors["email"] = ["A valid email address is required."]; }
        if (errors.Count > 0)
        {
            return Outcome.Failed<ParticipantDetails>(ApplicationFailure.Validation(errors));
        }

        using IDisposable lease = await coordinator.AcquireAsync(cancellationToken);
        if (await store.EmailExistsAsync(ParticipantPolicy.Key(displayEmail), cancellationToken))
        {
            return Outcome.Failed<ParticipantDetails>(ApplicationFailure.Conflict("participant_email_conflict"));
        }

        var participant = Participant.Create(displayName, displayEmail);
        Outcome<Unit> inserted = await store.InsertAsync(participant, cancellationToken);
        return inserted.IsSuccess ? Outcome.Success(new ParticipantDetails(participant.Id, participant.Name, participant.Email)) : Outcome.Failed<ParticipantDetails>(inserted.Failure);
    }

    public async Task<Outcome<ParticipantDetails>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return Outcome.Failed<ParticipantDetails>(ApplicationFailure.Validation(new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { ["id"] = ["A nonempty ID is required."] }));
        }

        ParticipantDetails? participant = await store.GetAsync(id, cancellationToken);
        return participant is null ? Outcome.Failed<ParticipantDetails>(ApplicationFailure.NotFound("participant_not_found")) : Outcome.Success(participant);
    }

    public async Task<Outcome<IReadOnlyList<ParticipantDetails>>> ListAsync(CancellationToken cancellationToken) => Outcome.Success(await store.ListAsync(cancellationToken));
}
