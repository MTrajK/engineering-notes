using System.ComponentModel.DataAnnotations;

namespace WorkshopBooking.Domain;

public static class ParticipantPolicy
{
    public static string Text(string value, int maximum)
    {
        ArgumentNullException.ThrowIfNull(value);
        string trimmed = value.Trim();
        if (trimmed.Length == 0 || trimmed.Length > maximum)
        {
            throw new ArgumentException("Text length is invalid.", nameof(value));
        }

        return trimmed;
    }

    public static string Email(string value)
    {
        string email = Text(value, 254);
        if (email.Count(c => c == '@') != 1 ||
            email.Any(c => char.IsWhiteSpace(c) || char.IsControl(c) || "<>,;".Contains(c)) ||
            !new EmailAddressAttribute().IsValid(email))
        {
            throw new ArgumentException("Email is invalid.", nameof(value));
        }

        return email;
    }

    public static string Key(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Trim().ToUpperInvariant();
    }
}
