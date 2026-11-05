using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Participants;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Application.Workshops;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Application.Registrations;

public sealed class RegistrationService(IRegistrationStore registrations, IWorkshopStore workshops, IParticipantStore participants, IMutationCoordinator coordinator, TimeProvider clock)
{
    public async Task<Outcome<RegistrationDetails>> CreateAsync(Guid? workshopId, Guid? participantId, CancellationToken cancellationToken)
    {
        Dictionary<string, IReadOnlyList<string>> errors = new(StringComparer.Ordinal);
        if (workshopId is null || workshopId == Guid.Empty)
        {
            errors["workshopId"] = ["A nonempty workshop ID is required."];
        }

        if (participantId is null || participantId == Guid.Empty)
        {
            errors["participantId"] = ["A nonempty participant ID is required."];
        }

        if (errors.Count > 0)
        {
            return Outcome.Failed<RegistrationDetails>(ApplicationFailure.Validation(errors));
        }

        using IDisposable lease = await coordinator.AcquireAsync(cancellationToken);
        DateTimeOffset now = clock.GetUtcNow();
        Guid workshopKey = workshopId.GetValueOrDefault();
        Guid participantKey = participantId.GetValueOrDefault();
        WorkshopDetails? workshop = await workshops.GetAsync(workshopKey, cancellationToken);
        if (workshop is null)
        {
            return Outcome.Failed<RegistrationDetails>(ApplicationFailure.NotFound("workshop_not_found"));
        }

        if (await participants.GetAsync(participantKey, cancellationToken) is null)
        {
            return Outcome.Failed<RegistrationDetails>(ApplicationFailure.NotFound("participant_not_found"));
        }

        if (now >= workshop.StartsAt)
        {
            return Outcome.Failed<RegistrationDetails>(ApplicationFailure.Conflict("workshop_started"));
        }

        if (await registrations.PairExistsAsync(workshopKey, participantKey, cancellationToken))
        {
            return Outcome.Failed<RegistrationDetails>(ApplicationFailure.Conflict("registration_duplicate"));
        }

        if (workshop.AvailableSeats <= 0)
        {
            return Outcome.Failed<RegistrationDetails>(ApplicationFailure.Conflict("workshop_full"));
        }

        var registration = Registration.Create(workshopKey, participantKey, now);
        Outcome<Unit> inserted = await registrations.InsertAsync(registration, cancellationToken);
        return inserted.IsSuccess ? Outcome.Success(new RegistrationDetails(registration.Id, registration.WorkshopId, registration.ParticipantId, registration.CreatedAt)) : Outcome.Failed<RegistrationDetails>(inserted.Failure);
    }

    public async Task<Outcome<IReadOnlyList<RegistrationDetails>>> ListAsync(Guid? workshopId, CancellationToken cancellationToken)
    {
        if (workshopId is null || workshopId == Guid.Empty)
        {
            return Outcome.Failed<IReadOnlyList<RegistrationDetails>>(Invalid("workshopId"));
        }

        if (await workshops.GetAsync(workshopId.Value, cancellationToken) is null)
        {
            return Outcome.Failed<IReadOnlyList<RegistrationDetails>>(ApplicationFailure.NotFound("workshop_not_found"));
        }

        return Outcome.Success(await registrations.ListAsync(workshopId.Value, cancellationToken));
    }

    public async Task<Outcome<Unit>> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return Outcome.Failed<Unit>(Invalid("id"));
        }

        using IDisposable lease = await coordinator.AcquireAsync(cancellationToken);
        return await registrations.DeleteAsync(id, cancellationToken) ? Outcome.Success(Unit.Value) : Outcome.Failed<Unit>(ApplicationFailure.NotFound("registration_not_found"));
    }

    private static ApplicationFailure Invalid(string field) => ApplicationFailure.Validation(new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [field] = ["A nonempty ID is required."] });
}
