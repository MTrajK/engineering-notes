using WorkshopBooking.Domain;

namespace WorkshopBooking.Application.Workshops;

public sealed record WorkshopDetails(Guid Id, string Title, string Topic, DateTimeOffset StartsAt, DateTimeOffset EndsAt, int Capacity, int AvailableSeats);
public sealed record CreateWorkshop(string? Title, string? Topic, DateTimeOffset? StartsAt, DateTimeOffset? EndsAt, int? Capacity);

public interface IWorkshopStore
{
    Task InsertAsync(Workshop workshop, CancellationToken cancellationToken);
    Task<WorkshopDetails?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<WorkshopDetails>> ListAsync(string? normalizedTopic, CancellationToken cancellationToken);
}
