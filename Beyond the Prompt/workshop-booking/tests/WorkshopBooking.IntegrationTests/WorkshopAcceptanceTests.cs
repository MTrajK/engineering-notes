using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Domain;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class WorkshopAcceptanceTests
{
    private static Dictionary<string, object?> Input(InfrastructureTestApplicationFactory factory) => new(StringComparer.Ordinal)
    {
        ["title"] = " Title  Case ",
        ["topic"] = " élève ",
        ["capacity"] = 3,
        ["startsAt"] = factory.Clock.GetUtcNow().AddTicks(1),
        ["endsAt"] = factory.Clock.GetUtcNow().AddTicks(2),
    };

    [Theory]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public async Task Wks001002004007009011013025026CreationAndReadback(int capacity)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        Dictionary<string, object?> input = Input(factory);
        input["title"] = " " + new string('a', 200) + " ";
        input["topic"] = " " + new string('é', 100) + " ";
        input["capacity"] = capacity;
        input["startsAt"] = factory.Clock.GetUtcNow().AddTicks(1).ToOffset(TimeSpan.FromHours(3));
        input["id"] = Guid.Empty;
        input["availableSeats"] = -99;
        using HttpResponseMessage created = await client.PostAsJsonAsync("/api/workshops", input, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        JsonElement body = await created.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(7, body.EnumerateObject().Count());
        Assert.NotEqual(Guid.Empty, body.GetProperty("id").GetGuid());
        Assert.Equal(capacity, body.GetProperty("availableSeats").GetInt32());
        Assert.Equal(new string('a', 200), body.GetProperty("title").GetString());
        Assert.Equal(new string('é', 100), body.GetProperty("topic").GetString());
        Assert.EndsWith("Z", body.GetProperty("startsAt").GetString());
        Assert.Equal(factory.Clock.GetUtcNow().AddTicks(1), body.GetProperty("startsAt").GetDateTimeOffset());
        Assert.NotNull(created.Headers.Location);
        using HttpResponseMessage read = await client.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal(body.ToString(), (await read.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).ToString());
        using HttpResponseMessage duplicate = await client.PostAsJsonAsync("/api/workshops", input, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, duplicate.StatusCode);
        Assert.NotEqual(body.GetProperty("id").GetGuid(), (await duplicate.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("id").GetGuid());
        Assert.Equal(factory.Clock.GetUtcNow().AddTicks(2), body.GetProperty("endsAt").GetDateTimeOffset());
        Assert.Equal(2, await factory.WithDatabaseAsync(context => context.Workshops.CountAsync(TestContext.Current.CancellationToken)));
        Assert.Equal(0, await factory.WithDatabaseAsync(context => context.Registrations.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Theory]
    [InlineData("title", "missing")]
    [InlineData("title", "null")]
    [InlineData("title", "empty")]
    [InlineData("title", "space")]
    [InlineData("title", "long")]
    [InlineData("topic", "missing")]
    [InlineData("topic", "null")]
    [InlineData("topic", "empty")]
    [InlineData("topic", "space")]
    [InlineData("topic", "long")]
    [InlineData("capacity", "missing")]
    [InlineData("capacity", "null")]
    [InlineData("capacity", "zero")]
    [InlineData("capacity", "negative")]
    [InlineData("capacity", "fraction")]
    [InlineData("capacity", "overflow")]
    [InlineData("startsAt", "missing")]
    [InlineData("startsAt", "null")]
    [InlineData("startsAt", "equal")]
    [InlineData("startsAt", "before")]
    [InlineData("startsAt", "bad")]
    [InlineData("startsAt", "offsetless")]
    [InlineData("endsAt", "missing")]
    [InlineData("endsAt", "null")]
    [InlineData("endsAt", "equal")]
    [InlineData("endsAt", "before")]
    [InlineData("endsAt", "bad")]
    [InlineData("endsAt", "offsetless")]
    public async Task Wks003005006008010012InvalidCreationDoesNotWrite(string field, string kind)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        Dictionary<string, object?> input = Input(factory);
        if (kind == "missing")
        {
            input.Remove(field);
        }
        else
        {
            input[field] = kind switch
            {
                "null" => null,
                "empty" => "",
                "space" => "  ",
                "long" => new string('a', field == "title" ? 201 : 101),
                "zero" => 0,
                "negative" => -1,
                "fraction" => 1.5,
                "overflow" => 2147483648L,
                "equal" => field == "startsAt" ? factory.Clock.GetUtcNow() : input["startsAt"],
                "before" => factory.Clock.GetUtcNow().AddTicks(-1),
                "offsetless" => "2031-01-01T12:00:00",
                _ => "broken",
            };
        }

        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/workshops", input, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        string expectedKey = kind is "bad" or "offsetless" or "fraction" or "overflow" ? "$" : field;
        Assert.True(problem.GetProperty("errors").TryGetProperty(expectedKey, out _));
        Assert.Equal(0, await factory.WithDatabaseAsync(context => context.Workshops.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Theory]
    [InlineData("not-a-guid", 400)]
    [InlineData("00000000-0000-0000-0000-000000000000", 400)]
    [InlineData("00000000-0000-0000-0000-000000000001", 404)]
    public async Task Wks015016InvalidAndMissingIdentities(string id, int status)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/api/workshops/" + id, TestContext.Current.CancellationToken);
        Assert.Equal(status, (int)response.StatusCode);
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(status == 404 ? "workshop_not_found" : "validation_failed", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Wks018019020021022023027HistoricalAvailabilityFilteringAndOrdering()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        DateTimeOffset now = factory.Clock.GetUtcNow();
        Guid[] ids = [Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("00000000-0000-0000-0000-000000000003")];
        await factory.WithDatabaseAsync(async context =>
        {
            for (int i = 2; i >= 0; i--)
            {
                var workshop = Workshop.Restore(ids[i], "Title", i == 2 ? "Other" : "Élève", i == 2 ? now.AddHours(1) : now.AddHours(-1), i == 0 ? now.AddMinutes(-1) : now.AddHours(2), 2);
                context.Add(workshop);
                for (int j = 0; j < i; j++)
                {
                    var participant = Participant.Create("A", $"a{i}{j}@example");
                    context.Add(participant);
                    context.Add(Registration.Create(workshop.Id, participant.Id, now));
                }
            }
            return await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        JsonElement[] all = await client.GetFromJsonAsync<JsonElement[]>("/api/workshops", TestContext.Current.CancellationToken) ?? [];
        Assert.Equal(ids, all.Select(x => x.GetProperty("id").GetGuid()));
        Assert.Equal((int[])[2, 1, 0], all.Select(x => x.GetProperty("availableSeats").GetInt32()));
        foreach (Guid id in ids)
        {
            JsonElement detail = await client.GetFromJsonAsync<JsonElement>("/api/workshops/" + id, TestContext.Current.CancellationToken);
            Assert.Equal(all.Single(x => x.GetProperty("id").GetGuid() == id).ToString(), detail.ToString());
        }
        JsonElement[] filtered = await client.GetFromJsonAsync<JsonElement[]>("/api/workshops?topic=" + Uri.EscapeDataString(" élÈVE "), TestContext.Current.CancellationToken) ?? [];
        Assert.Equal(ids.Take(2), filtered.Select(x => x.GetProperty("id").GetGuid()));
        foreach (string topic in new[] { "Él", "absent" })
        {
            Assert.Empty(await client.GetFromJsonAsync<JsonElement[]>("/api/workshops?topic=" + Uri.EscapeDataString(topic), TestContext.Current.CancellationToken) ?? []);
        }

        foreach (string topic in new[] { "", "  " })
        {
            using HttpResponseMessage response = await client.GetAsync("/api/workshops?topic=" + Uri.EscapeDataString(topic), TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True((await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("errors").TryGetProperty("topic", out _));
        }
    }
}
