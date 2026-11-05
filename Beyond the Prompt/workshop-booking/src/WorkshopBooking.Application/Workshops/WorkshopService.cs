using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Application.Workshops;

public sealed class WorkshopService(IWorkshopStore store, IMutationCoordinator coordinator, TimeProvider clock)
{
    public async Task<Outcome<WorkshopDetails>> CreateAsync(CreateWorkshop input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        using IDisposable lease = await coordinator.AcquireAsync(cancellationToken);
        DateTimeOffset now = clock.GetUtcNow();
        Dictionary<string, IReadOnlyList<string>> errors = new(StringComparer.Ordinal);
        ValidateText(input.Title, 200, "title", errors);
        ValidateText(input.Topic, 100, "topic", errors);
        if (input.Capacity is null or <= 0)
        {
            errors["capacity"] = ["Capacity must be positive."];
        }

        if (input.StartsAt is null || input.StartsAt <= now)
        {
            errors["startsAt"] = ["Start must be in the future."];
        }

        if (input.EndsAt is null || (input.StartsAt is not null && input.EndsAt <= input.StartsAt))
        {
            errors["endsAt"] = ["End must follow start."];
        }

        if (errors.Count > 0)
        {
            return Outcome.Failed<WorkshopDetails>(ApplicationFailure.Validation(errors));
        }

        var workshop = Workshop.Create(input.Title!, input.Topic!, input.StartsAt!.Value, input.EndsAt!.Value, input.Capacity!.Value, now);
        await store.InsertAsync(workshop, cancellationToken);
        return Outcome.Success(new WorkshopDetails(workshop.Id, workshop.Title, workshop.Topic, workshop.StartsAt, workshop.EndsAt, workshop.Capacity, workshop.AvailableSeats(0)));
    }

    public async Task<Outcome<WorkshopDetails>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return Outcome.Failed<WorkshopDetails>(Invalid("id"));
        }

        WorkshopDetails? workshop = await store.GetAsync(id, cancellationToken);
        return workshop is null ? Outcome.Failed<WorkshopDetails>(ApplicationFailure.NotFound("workshop_not_found")) : Outcome.Success(workshop);
    }

    public async Task<Outcome<IReadOnlyList<WorkshopDetails>>> ListAsync(string? topic, CancellationToken cancellationToken)
    {
        if (topic is not null && string.IsNullOrWhiteSpace(topic))
        {
            return Outcome.Failed<IReadOnlyList<WorkshopDetails>>(Invalid("topic"));
        }

        return Outcome.Success(await store.ListAsync(topic is null ? null : ParticipantPolicy.Key(topic), cancellationToken));
    }

    private static ApplicationFailure Invalid(string field) => ApplicationFailure.Validation(new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [field] = ["A nonempty value is required."] });

    private static void ValidateText(string? text, int maximum, string field, Dictionary<string, IReadOnlyList<string>> errors)
    {
        try
        {
            ParticipantPolicy.Text(text ?? string.Empty, maximum);
        }
        catch (ArgumentException) { errors[field] = ["Text length is invalid."]; }
    }
}
