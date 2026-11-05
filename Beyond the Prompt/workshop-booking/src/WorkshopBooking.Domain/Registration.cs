namespace WorkshopBooking.Domain;

public sealed class Registration
{
    private Registration()
    {
    }
    public Guid Id
    {
        get; private set;
    }
    public Guid WorkshopId
    {
        get; private set;
    }
    public Guid ParticipantId
    {
        get; private set;
    }
    public DateTimeOffset CreatedAt
    {
        get; private set;
    }

    public static Registration Create(Guid workshopId, Guid participantId, DateTimeOffset now) => Restore(Guid.NewGuid(), workshopId, participantId, now);

    internal static Registration Restore(Guid id, Guid workshopId, Guid participantId, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty || workshopId == Guid.Empty || participantId == Guid.Empty)
        {
            throw new ArgumentException("Nonempty identities are required.", nameof(id));
        }

        return new Registration { Id = id, WorkshopId = workshopId, ParticipantId = participantId, CreatedAt = createdAt.ToUniversalTime() };
    }
}
