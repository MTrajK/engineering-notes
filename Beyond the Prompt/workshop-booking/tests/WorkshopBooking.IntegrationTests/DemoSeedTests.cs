using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Domain;
using WorkshopBooking.Persistence;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class DemoSeedTests
{
    [Fact]
    public async Task InitializationCapturesClockExactlyOnce()
    {
        await using InfrastructureTestApplicationFactory factory = new()
        {
            SeedDemoData = true
        };
        await factory.Services.InitializeBookingDatabaseAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, factory.Clock.Reads);
        await factory.Services.InitializeBookingDatabaseAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, factory.Clock.Reads);
    }

    [Fact]
    public async Task StableIdentitiesRelativeDatesAndScenarioIntegrity()
    {
        await using InfrastructureTestApplicationFactory first = new()
        {
            SeedDemoData = true
        };
        await using InfrastructureTestApplicationFactory second = new()
        {
            SeedDemoData = true
        };
        await using InfrastructureTestApplicationFactory ordinary = new();
        second.Clock.SetUtcNow(first.Clock.GetUtcNow().AddDays(20));
        foreach (InfrastructureTestApplicationFactory factory in new[] { first, second })
        {
            await factory.WithDatabaseAsync(async context =>
            {
                List<Workshop> workshops = await context.Workshops.OrderBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken);
                List<Participant> participants = await context.Participants.OrderBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken);
                List<Registration> registrations = await context.Registrations.OrderBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken);
                Assert.Equal((Guid[])[DemoSeed.AvailableWorkshopId, DemoSeed.FullWorkshopId, DemoSeed.CompletedWorkshopId], workshops.Select(x => x.Id));
                Assert.Equal((Guid[])[DemoSeed.AlexId, DemoSeed.BlairId, DemoSeed.CaseyId], participants.Select(x => x.Id));
                Assert.Equal(4, registrations.Count);
                Assert.Equal(Enumerable.Range(1, 4).Select(number => Guid.Parse($"30000000-0000-0000-0000-{number:D12}")), registrations.Select(x => x.Id));
                Assert.Equal(4, registrations.Select(x => x.Id).Distinct().Count());
                Assert.Equal(4, registrations.Select(x => (x.WorkshopId, x.ParticipantId)).Distinct().Count());
                Assert.Equal(3, participants.Select(x => x.NormalizedEmail).Distinct(StringComparer.Ordinal).Count());
                Assert.Equal(factory.Clock.GetUtcNow().AddDays(3), workshops[0].StartsAt);
                Assert.Equal(factory.Clock.GetUtcNow().AddDays(5), workshops[1].StartsAt);
                Assert.Equal(factory.Clock.GetUtcNow().AddDays(-5), workshops[2].StartsAt);
                Assert.All(workshops, workshop => Assert.Equal(workshop.StartsAt.AddHours(2), workshop.EndsAt));
                Assert.Equal((int[])[4, 2, 3], workshops.Select(x => x.Capacity));
                Assert.Equal((int[])[1, 2, 1], workshops.Select(x => registrations.Count(r => r.WorkshopId == x.Id)));
                Assert.All(registrations, registration =>
                {
                    Assert.Contains(participants, x => x.Id == registration.ParticipantId);
                    Assert.True(registration.CreatedAt < workshops.Single(x => x.Id == registration.WorkshopId).StartsAt);
                });
                Assert.Equal((DateTimeOffset[])[factory.Clock.GetUtcNow().AddDays(-1), factory.Clock.GetUtcNow().AddDays(-2), factory.Clock.GetUtcNow().AddDays(-1), factory.Clock.GetUtcNow().AddDays(-6)], registrations.Select(x => x.CreatedAt));
                Assert.DoesNotContain(registrations, x => x.ParticipantId == DemoSeed.CaseyId);
                return true;
            });
        }
        Assert.Equal(0, await ordinary.WithDatabaseAsync(context => context.Workshops.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task WalkthroughDoesNotReseedAndFreshHostRestoresBaseline()
    {
        await using InfrastructureTestApplicationFactory factory = new()
        {
            SeedDemoData = true
        };
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage created = await client.PostAsJsonAsync("/api/registrations", new
        {
            workshopId = DemoSeed.AvailableWorkshopId,
            participantId = DemoSeed.CaseyId
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Guid id = (await created.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("id").GetGuid();
        Assert.Equal(2, await SeatsAsync(client, DemoSeed.AvailableWorkshopId));
        using HttpResponseMessage duplicate = await client.PostAsJsonAsync("/api/registrations", new
        {
            workshopId = DemoSeed.AvailableWorkshopId,
            participantId = DemoSeed.CaseyId
        }, TestContext.Current.CancellationToken);
        Assert.Equal("registration_duplicate", (await duplicate.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("code").GetString());
        using HttpResponseMessage full = await client.PostAsJsonAsync("/api/registrations", new
        {
            workshopId = DemoSeed.FullWorkshopId,
            participantId = DemoSeed.CaseyId
        }, TestContext.Current.CancellationToken);
        Assert.Equal("workshop_full", (await full.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("code").GetString());
        using HttpResponseMessage cancelled = await client.DeleteAsync("/api/registrations/" + id, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
        Assert.Equal(3, await SeatsAsync(client, DemoSeed.AvailableWorkshopId));
        var seedRegistration = Guid.Parse("30000000-0000-0000-0000-000000000001");
        using HttpResponseMessage cancelSeed = await client.DeleteAsync("/api/registrations/" + seedRegistration, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, cancelSeed.StatusCode);
        factory.Clock.SetUtcNow(factory.Clock.GetUtcNow().AddDays(1));
        await factory.Services.InitializeBookingDatabaseAsync(TestContext.Current.CancellationToken);
        Assert.Equal(4, await SeatsAsync(client, DemoSeed.AvailableWorkshopId));
        Assert.Equal(3, await factory.WithDatabaseAsync(context => context.Registrations.CountAsync(TestContext.Current.CancellationToken)));
        await using InfrastructureTestApplicationFactory fresh = new()
        {
            SeedDemoData = true
        };
        using HttpClient freshClient = fresh.CreateClient();
        Assert.Equal(3, await SeatsAsync(freshClient, DemoSeed.AvailableWorkshopId));
        Assert.Equal(4, await fresh.WithDatabaseAsync(context => context.Registrations.CountAsync(TestContext.Current.CancellationToken)));
    }
    private static async Task<int> SeatsAsync(HttpClient client, Guid id) => (await client.GetFromJsonAsync<JsonElement>("/api/workshops/" + id, TestContext.Current.CancellationToken)).GetProperty("availableSeats").GetInt32();

    [Fact]
    public async Task OpenApiDocumentsAllNineOperationsWithoutPersistenceSchemas()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        JsonElement document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);
        JsonElement paths = document.GetProperty("paths");
        string[] baselinePaths = ["/api/workshops", "/api/workshops/{id}", "/api/participants", "/api/participants/{id}", "/api/registrations", "/api/registrations/{id}"];
        Assert.Equal(9, baselinePaths.Sum(path => paths.GetProperty(path).EnumerateObject().Count()));
        foreach (string path in baselinePaths)
        {
            foreach (JsonProperty operation in paths.GetProperty(path).EnumerateObject())
            {
                Assert.False(string.IsNullOrWhiteSpace(operation.Value.GetProperty("summary").GetString()));
                Assert.NotEmpty(operation.Value.GetProperty("tags").EnumerateArray());
            }
        }

        Assert.False(paths.GetProperty("/api/registrations/{id}").TryGetProperty("get", out _));
        Assert.False(paths.GetProperty("/api/registrations/{id}").GetProperty("delete").GetProperty("responses").GetProperty("204").TryGetProperty("content", out _));
        foreach (string path in new[] { "/api/workshops", "/api/participants" })
        {
            Assert.True(paths.GetProperty(path).GetProperty("post").GetProperty("responses").GetProperty("201").GetProperty("headers").TryGetProperty("Location", out _));
        }

        Assert.False(paths.GetProperty("/api/registrations").GetProperty("post").GetProperty("responses").GetProperty("201").TryGetProperty("headers", out _));
        Assert.True(paths.GetProperty("/api/registrations").GetProperty("get").GetProperty("parameters")[0].GetProperty("required").GetBoolean());
        JsonElement schemas = document.GetProperty("components").GetProperty("schemas");
        Assert.Equal(2, schemas.GetProperty("CreateRegistrationRequest").GetProperty("required").GetArrayLength());
        Assert.Equal(5, schemas.GetProperty("CreateWorkshopRequest").GetProperty("required").GetArrayLength());
        Assert.Equal(2, schemas.GetProperty("CreateParticipantRequest").GetProperty("required").GetArrayLength());
        Assert.Equal(DemoSeed.CaseyId, schemas.GetProperty("CreateRegistrationRequest").GetProperty("example").GetProperty("participantId").GetGuid());
        Assert.True(schemas.GetProperty("ApiProblemDetails").GetProperty("properties").TryGetProperty("errors", out _));
        foreach (string name in new[] { "Workshop", "Participant", "Registration", "WorkshopBookingContext" })
        {
            Assert.False(schemas.TryGetProperty(name, out _));
        }

        Assert.DoesNotContain("normalizedEmail", schemas.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("normalizedTopic", schemas.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.True(paths.GetProperty("/api/registrations").GetProperty("post").GetProperty("responses").GetProperty("409").GetProperty("content").TryGetProperty("application/problem+json", out _));
    }
}
