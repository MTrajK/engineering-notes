using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Application.Registrations;

public sealed record RegistrationDetails(Guid Id, Guid WorkshopId, Guid ParticipantId, DateTimeOffset CreatedAt);

public interface IRegistrationStore
{
    Task<bool> PairExistsAsync(Guid workshopId, Guid participantId, CancellationToken cancellationToken);
    Task<Outcome<Unit>> InsertAsync(Registration registration, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<RegistrationDetails>> ListAsync(Guid workshopId, CancellationToken cancellationToken);
}
