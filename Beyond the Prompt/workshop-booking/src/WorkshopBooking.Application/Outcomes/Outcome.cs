namespace WorkshopBooking.Application.Outcomes;

public sealed class Outcome<T>
{
    private readonly T? _value;
    private readonly ApplicationFailure? _failure;

    internal Outcome(bool isSuccess, T? value, ApplicationFailure? failure)
    {
        if (isSuccess == (failure is not null) || (isSuccess && value is null))
        {
            throw new ArgumentException("An outcome must contain either a value or a failure.");
        }

        IsSuccess = isSuccess;
        _value = value;
        _failure = failure;
    }

    public bool IsSuccess
    {
        get;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("A failed outcome does not contain a value.");

    public ApplicationFailure Failure => _failure
        ?? throw new InvalidOperationException("A successful outcome does not contain a failure.");

}

public static class Outcome
{
    public static Outcome<T> Success<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Outcome<T>(true, value, null);
    }

    public static Outcome<T> Failed<T>(ApplicationFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new Outcome<T>(false, default, failure);
    }
}

public readonly record struct Unit
{
    public static Unit Value => default;
}
