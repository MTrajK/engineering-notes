using System.Net;
using Xunit;
namespace WorkshopBooking.IntegrationTests;

public sealed class ParticipantHttpTests
{
    [Fact]
    public async Task Par021EmptyList()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/api/participants", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
