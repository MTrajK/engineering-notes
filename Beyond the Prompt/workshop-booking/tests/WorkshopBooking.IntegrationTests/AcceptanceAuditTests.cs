using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Persistence;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class AcceptanceAuditTests
{
    [Fact]
    public async Task RejectedMutationsPreserveEveryStoredRecord()
    {
        await using InfrastructureTestApplicationFactory factory = new()
        {
            SeedDemoData = true
        };
        using HttpClient client = factory.CreateClient();
        string before = await SnapshotAsync(factory);
        using HttpResponseMessage invalidWorkshop = await client.PostAsJsonAsync("/api/workshops", new
        {
            title = "A",
            topic = "B",
            startsAt = factory.Clock.GetUtcNow(),
            endsAt = factory.Clock.GetUtcNow().AddHours(1),
            capacity = 1
        }, TestContext.Current.CancellationToken);
        await ProblemAsync(invalidWorkshop, 400, "validation_failed", "startsAt");
        using HttpResponseMessage invalidParticipant = await client.PostAsJsonAsync("/api/participants", new
        {
            name = " ",
            email = "Alex.River@example.test"
        }, TestContext.Current.CancellationToken);
        await ProblemAsync(invalidParticipant, 400, "validation_failed", "name");
        using HttpResponseMessage duplicateParticipant = await client.PostAsJsonAsync("/api/participants", new
        {
            name = "Overwrite",
            email = "Alex.River@example.test"
        }, TestContext.Current.CancellationToken);
        await ProblemAsync(duplicateParticipant, 409, "participant_email_conflict");
        Assert.DoesNotContain(DemoSeed.AlexId.ToString(), await duplicateParticipant.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        foreach ((Guid, Guid, string) scenario in new[]
        {
            (DemoSeed.AvailableWorkshopId, DemoSeed.AlexId, "registration_duplicate"),
            (DemoSeed.FullWorkshopId, DemoSeed.CaseyId, "workshop_full"),
            (DemoSeed.CompletedWorkshopId, DemoSeed.BlairId, "workshop_started"),
        })
        {
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/registrations", new
            {
                workshopId = scenario.Item1,
                participantId = scenario.Item2
            }, TestContext.Current.CancellationToken);
            await ProblemAsync(response, 409, scenario.Item3);
            Assert.Equal(before, await SnapshotAsync(factory));
        }
        using HttpResponseMessage missingCancel = await client.DeleteAsync("/api/registrations/99999999-0000-0000-0000-000000000001", TestContext.Current.CancellationToken);
        await ProblemAsync(missingCancel, 404, "registration_not_found");
        Assert.Equal(before, await SnapshotAsync(factory));
    }

    [Fact]
    public async Task Reg016027OverlappingWorkshopsAndForwardTimeReregistration()
    {
        await using InfrastructureTestApplicationFactory factory = new()
        {
            SeedDemoData = true
        };
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage createdWorkshop = await client.PostAsJsonAsync("/api/workshops", new
        {
            title = "Overlap",
            topic = "API Design",
            startsAt = factory.Clock.GetUtcNow().AddDays(3),
            endsAt = factory.Clock.GetUtcNow().AddDays(3).AddHours(2),
            capacity = 1
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, createdWorkshop.StatusCode);
        Guid other = (await createdWorkshop.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("id").GetGuid();
        Guid firstId = Guid.Empty;
        foreach (Guid workshopId in new[] { DemoSeed.AvailableWorkshopId, other })
        {
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/registrations", new
            {
                workshopId,
                participantId = DemoSeed.CaseyId
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            if (workshopId == DemoSeed.AvailableWorkshopId)
            {
                firstId = (await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("id").GetGuid();
            }
        }
        using HttpResponseMessage cancelled = await client.DeleteAsync("/api/registrations/" + firstId, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
        factory.Clock.SetUtcNow(factory.Clock.GetUtcNow().AddHours(1));
        using HttpResponseMessage recreated = await client.PostAsJsonAsync("/api/registrations", new
        {
            workshopId = DemoSeed.AvailableWorkshopId,
            participantId = DemoSeed.CaseyId
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, recreated.StatusCode);
        JsonElement body = await recreated.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.NotEqual(firstId, body.GetProperty("id").GetGuid());
        Assert.Equal(factory.Clock.GetUtcNow(), body.GetProperty("createdAt").GetDateTimeOffset());
        Assert.Equal(2, await factory.WithDatabaseAsync(context => context.Registrations.CountAsync(x => x.ParticipantId == DemoSeed.CaseyId, TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Wks002SingleUnitTextAndInternalWhitespaceArePreserved()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        foreach (string title in new[] { "A", "Title  Case" })
        {
            using HttpResponseMessage response = await client.PostAsJsonAsync("/api/workshops", new
            {
                title = " " + title + " ",
                topic = " B ",
                capacity = 1,
                startsAt = factory.Clock.GetUtcNow().AddHours(1),
                endsAt = factory.Clock.GetUtcNow().AddHours(2)
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
            Assert.Equal(title, body.GetProperty("title").GetString());
            Assert.Equal("B", body.GetProperty("topic").GetString());
        }
    }

    private static Task<string> SnapshotAsync(InfrastructureTestApplicationFactory factory) => factory.WithDatabaseAsync(async context => JsonSerializer.Serialize(new
    {
        Workshops = await context.Workshops.AsNoTracking().OrderBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken),
        Participants = await context.Participants.AsNoTracking().OrderBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken),
        Registrations = await context.Registrations.AsNoTracking().OrderBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken),
    }));
    private static async Task ProblemAsync(HttpResponseMessage response, int status, string code, string? field = null)
    {
        Assert.Equal(status, (int)response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.Equal(status, problem.GetProperty("status").GetInt32());
        Assert.Equal("urn:workshop-booking:problem:" + code, problem.GetProperty("type").GetString());
        Assert.Equal(response.RequestMessage?.RequestUri?.AbsolutePath, problem.GetProperty("instance").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
        if (field is not null)
        {
            Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _));
        }
    }
}
