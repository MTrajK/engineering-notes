using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Application.Participants;

public sealed record ParticipantDetails(Guid Id, string Name, string Email);

public interface IParticipantStore
{
    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);
    Task<Outcome<Unit>> InsertAsync(Participant participant, CancellationToken cancellationToken);
    Task<ParticipantDetails?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ParticipantDetails>> ListAsync(CancellationToken cancellationToken);
}
