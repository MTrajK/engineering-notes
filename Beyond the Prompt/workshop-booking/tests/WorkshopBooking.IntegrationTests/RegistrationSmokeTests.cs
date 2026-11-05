using System.Net;
using Xunit;
namespace WorkshopBooking.IntegrationTests;

public sealed class RegistrationSmokeTests
{
    [Fact]
    public async Task Reg020MissingFilterIsValidationFailure()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/api/registrations", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
