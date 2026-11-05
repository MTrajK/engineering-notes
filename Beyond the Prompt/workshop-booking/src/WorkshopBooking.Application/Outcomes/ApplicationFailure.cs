using System.Collections.ObjectModel;

namespace WorkshopBooking.Application.Outcomes;

public enum ApplicationFailureKind
{
    Validation,
    NotFound,
    Conflict
}

public sealed class ApplicationFailure
{
    private ApplicationFailure(
        ApplicationFailureKind kind,
        string code,
        IReadOnlyDictionary<string, IReadOnlyList<string>>? errors)
    {
        Kind = kind;
        Code = code;
        Errors = errors;
    }

    public ApplicationFailureKind Kind
    {
        get;
    }

    public string Code
    {
        get;
    }

    public IReadOnlyDictionary<string, IReadOnlyList<string>>? Errors
    {
        get;
    }

    public static ApplicationFailure Validation(
        IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (errors.Count == 0)
        {
            throw new ArgumentException("Validation failures require at least one field error.", nameof(errors));
        }

        Dictionary<string, IReadOnlyList<string>> copiedErrors = new(StringComparer.Ordinal);

        foreach ((string field, IReadOnlyList<string> messages) in errors)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(field);
            ArgumentNullException.ThrowIfNull(messages);

            if (messages.Count == 0 || messages.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Each validation field requires a nonempty message.", nameof(errors));
            }

            copiedErrors.Add(field, Array.AsReadOnly(messages.ToArray()));
        }

        return new ApplicationFailure(
            ApplicationFailureKind.Validation,
            "validation_failed",
            new ReadOnlyDictionary<string, IReadOnlyList<string>>(copiedErrors));
    }

    public static ApplicationFailure NotFound(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new ApplicationFailure(ApplicationFailureKind.NotFound, code, null);
    }

    public static ApplicationFailure Conflict(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new ApplicationFailure(ApplicationFailureKind.Conflict, code, null);
    }
}
