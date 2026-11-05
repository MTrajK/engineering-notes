using WorkshopBooking.Application.Outcomes;
using Xunit;

namespace WorkshopBooking.UnitTests;

public sealed class OutcomeTests
{
    [Fact]
    public void SuccessContainsValueAndNoFailure()
    {
        var outcome = Outcome.Success("created");

        Assert.True(outcome.IsSuccess);
        Assert.Equal("created", outcome.Value);
        Assert.Throws<InvalidOperationException>(() => _ = outcome.Failure);
    }

    [Fact]
    public void FailureContainsOneFailureAndNoValue()
    {
        var failure = ApplicationFailure.NotFound("workshop_not_found");

        var outcome = Outcome.Failed<string>(failure);

        Assert.False(outcome.IsSuccess);
        Assert.Same(failure, outcome.Failure);
        Assert.Throws<InvalidOperationException>(() => _ = outcome.Value);
    }

    [Fact]
    public void ValueTypeOutcomesSupportSuccessAndFailure()
    {
        var success = Outcome.Success(Unit.Value);
        var failure = Outcome.Failed<Unit>(ApplicationFailure.Conflict("registration_duplicate"));

        Assert.True(success.IsSuccess);
        Assert.Equal(Unit.Value, success.Value);
        Assert.False(failure.IsSuccess);
        Assert.Equal("registration_duplicate", failure.Failure.Code);
    }

    [Fact]
    public void ValidationFailureHasStableCodeAndCopiedFieldErrors()
    {
        List<string> messages = ["A workshop ID is required."];
        Dictionary<string, IReadOnlyList<string>> suppliedErrors = new(StringComparer.Ordinal)
        {
            ["workshopId"] = messages
        };

        var failure = ApplicationFailure.Validation(suppliedErrors);
        messages.Add("Later mutation.");

        Assert.Equal(ApplicationFailureKind.Validation, failure.Kind);
        Assert.Equal("validation_failed", failure.Code);
        Assert.Equal(["A workshop ID is required."], failure.Errors!["workshopId"]);
    }

    [Fact]
    public void ValidationFailureRejectsEmptyOrIncompleteErrors()
    {
        Dictionary<string, IReadOnlyList<string>> noErrors = new(StringComparer.Ordinal);
        Dictionary<string, IReadOnlyList<string>> emptyMessages = new(StringComparer.Ordinal)
        {
            ["participantId"] = []
        };

        Assert.Throws<ArgumentException>(() => ApplicationFailure.Validation(noErrors));
        Assert.Throws<ArgumentException>(() => ApplicationFailure.Validation(emptyMessages));
    }

    [Fact]
    public void FailureFactoriesRejectMissingCodesAndValues()
    {
        Assert.Throws<ArgumentException>(() => ApplicationFailure.Conflict(" "));
        Assert.Throws<ArgumentNullException>(() => Outcome.Success<string>(null!));
        Assert.Throws<ArgumentNullException>(() => Outcome.Failed<string>(null!));
    }
}
