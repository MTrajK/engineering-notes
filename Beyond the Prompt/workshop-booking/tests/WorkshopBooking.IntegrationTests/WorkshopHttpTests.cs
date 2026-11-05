using System.Net;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class WorkshopHttpTests
{
    [Fact]
    public async Task Wks017EmptyListIsAvailable()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/api/workshops", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }
}
