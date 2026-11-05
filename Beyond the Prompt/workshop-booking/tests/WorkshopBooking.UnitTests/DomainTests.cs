using WorkshopBooking.Domain;
using Xunit;

namespace WorkshopBooking.UnitTests;

public sealed class DomainTests
{
    private static readonly DateTimeOffset _now = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreationPreservesDisplayAndNormalizesKeys()
    {
        var participant = Participant.Create("  Éva  Smith  ", " Alex+demo@Example.com ");
        Assert.NotEqual(Guid.Empty, participant.Id);
        Assert.Equal("Éva  Smith", participant.Name);
        Assert.Equal("Alex+demo@Example.com", participant.Email);
        Assert.Equal("ALEX+DEMO@EXAMPLE.COM", participant.NormalizedEmail);
        var workshop = Workshop.Create(" Title ", " élève ", _now.AddTicks(1), _now.AddHours(1), 2, _now);
        Assert.Equal("ÉLÈVE", workshop.NormalizedTopic);
        Assert.Equal("élève", workshop.Topic);
        Assert.Equal(1, workshop.AvailableSeats(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => workshop.AvailableSeats(3));
    }

    [Theory]
    [InlineData("a b@example")]
    [InlineData("a@@example")]
    [InlineData("@example")]
    [InlineData("a@")]
    [InlineData("a<@example")]
    [InlineData("a;@example")]
    [InlineData("")]
    public void InvalidEmailIsRejected(string email) => Assert.Throws<ArgumentException>(() => ParticipantPolicy.Email(email));

    [Fact]
    public void TextAndEmailBoundariesAreEnforced()
    {
        Assert.Equal("alex@example", ParticipantPolicy.Email("alex@example"));
        Assert.Equal(254, ParticipantPolicy.Email(new string('a', 252) + "@b").Length);
        Assert.Throws<ArgumentException>(() => ParticipantPolicy.Email(new string('a', 253) + "@b"));
        Assert.Throws<ArgumentException>(() => Participant.Create(new string('a', 101), "a@b"));
        Assert.Throws<ArgumentException>(() => Participant.Create(" ", "a@b"));
        Assert.Equal(100, Participant.Create(new string('a', 100), "a@b").Name.Length);
    }

    [Fact]
    public void CreationTimeAndHistoricalMaterializationHaveDistinctRules()
    {
        Assert.Throws<ArgumentException>(() => Workshop.Create("a", "b", _now, _now.AddHours(1), 1, _now));
        Assert.Throws<ArgumentException>(() => Workshop.Create("a", "b", _now.AddTicks(-1), _now.AddHours(1), 1, _now));
        Assert.Throws<ArgumentException>(() => Workshop.Create("a", "b", _now.AddTicks(1), _now.AddTicks(1), 1, _now));
        Assert.Throws<ArgumentOutOfRangeException>(() => Workshop.Create("a", "b", _now.AddTicks(1), _now.AddHours(1), 0, _now));
        var historical = Workshop.Restore(Guid.NewGuid(), "a", "b", _now.AddDays(-2), _now.AddDays(-1), 1);
        Assert.Equal(_now.AddDays(-2), historical.StartsAt);
        Assert.Throws<ArgumentException>(() => Workshop.Restore(Guid.Empty, "a", "b", _now, _now.AddHours(1), 1));
        Assert.Throws<ArgumentException>(() => Registration.Create(Guid.Empty, Guid.NewGuid(), _now));
        var registration = Registration.Create(historical.Id, Guid.NewGuid(), _now.ToOffset(TimeSpan.FromHours(2)));
        Assert.Equal(_now.UtcTicks, registration.CreatedAt.Ticks);
        Assert.Equal(TimeSpan.Zero, registration.CreatedAt.Offset);
    }
}
