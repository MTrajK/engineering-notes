namespace WorkshopBooking.Domain;

public sealed class Workshop
{
    private Workshop()
    {
    }
    public Guid Id
    {
        get; private set;
    }
    public string Title { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public string NormalizedTopic { get; private set; } = string.Empty;
    public DateTimeOffset StartsAt
    {
        get; private set;
    }
    public DateTimeOffset EndsAt
    {
        get; private set;
    }
    public int Capacity
    {
        get; private set;
    }

    public static Workshop Create(string title, string topic, DateTimeOffset startsAt, DateTimeOffset endsAt, int capacity, DateTimeOffset now)
    {
        if (startsAt <= now)
        {
            throw new ArgumentException("Start must be in the future.", nameof(startsAt));
        }

        return Restore(Guid.NewGuid(), title, topic, startsAt, endsAt, capacity);
    }

    internal static Workshop Restore(Guid id, string title, string topic, DateTimeOffset startsAt, DateTimeOffset endsAt, int capacity)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity is required.", nameof(id));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        if (endsAt <= startsAt)
        {
            throw new ArgumentException("End must follow start.", nameof(endsAt));
        }

        string displayTopic = ParticipantPolicy.Text(topic, 100);
        return new Workshop
        {
            Id = id,
            Title = ParticipantPolicy.Text(title, 200),
            Topic = displayTopic,
            NormalizedTopic = ParticipantPolicy.Key(displayTopic),
            StartsAt = startsAt.ToUniversalTime(),
            EndsAt = endsAt.ToUniversalTime(),
            Capacity = capacity,
        };
    }

    public int AvailableSeats(int registrationCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(registrationCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(registrationCount, Capacity);

        return Capacity - registrationCount;
    }
}
