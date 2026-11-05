using System.Globalization;
using WorkshopBooking.Domain;
using Xunit;

namespace WorkshopBooking.UnitTests;

public sealed class PolicyAuditTests
{
    [Fact]
    public void NormalizationIsInvariantAndHistoricalIdentityCannotBeEmpty()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal("I@EXAMPLE", Participant.Create("İpek", " i@example ").NormalizedEmail);
            Assert.Equal("ÉLÈVE", ParticipantPolicy.Key(" élève "));
            Assert.Throws<ArgumentException>(() => Participant.Restore(Guid.Empty, "A", "a@b"));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
