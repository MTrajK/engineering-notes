namespace WorkshopBooking.Domain;

public sealed class Participant
{
    private Participant()
    {
    }
    public Guid Id
    {
        get; private set;
    }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;

    public static Participant Create(string name, string email) => Restore(Guid.NewGuid(), name, email);

    internal static Participant Restore(Guid id, string name, string email)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Identity is required.", nameof(id));
        }

        string displayEmail = ParticipantPolicy.Email(email);
        return new Participant { Id = id, Name = ParticipantPolicy.Text(name, 100), Email = displayEmail, NormalizedEmail = ParticipantPolicy.Key(displayEmail) };
    }
}
