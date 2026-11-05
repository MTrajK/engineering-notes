using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Domain;
using WorkshopBooking.Persistence;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class RegistrationAcceptanceTests
{
    private static async Task<(Workshop Workshop, Participant First, Participant Second)> FixtureAsync(InfrastructureTestApplicationFactory factory, int capacity = 1)
    {
        var workshop = Workshop.Create("A", "B", factory.Clock.GetUtcNow().AddTicks(1), factory.Clock.GetUtcNow().AddHours(1), capacity, factory.Clock.GetUtcNow());
        var first = Participant.Create("First", "first@example");
        var second = Participant.Create("Second", "second@example");
        await factory.WithDatabaseAsync(async context => { context.AddRange(workshop, first, second); return await context.SaveChangesAsync(TestContext.Current.CancellationToken); });
        return (workshop, first, second);
    }
    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, Guid workshopId, Guid participantId) => client.PostAsJsonAsync("/api/registrations", new { workshopId, participantId, id = Guid.Empty, createdAt = "1900-01-01T00:00:00Z" }, TestContext.Current.CancellationToken);
    private static async Task<JsonElement> BodyAsync(HttpResponseMessage response) => JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    private static async Task AssertCodeAsync(HttpResponseMessage response, int status, string code)
    {
        Assert.Equal(status, (int)response.StatusCode);
        JsonElement problem = await BodyAsync(response);
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.Equal(status, problem.GetProperty("status").GetInt32());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Reg001008013023025027032Wks024RoundtripAndReregistration()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        (Workshop? workshop, Participant? participant, Participant _) = await FixtureAsync(factory);
        using HttpResponseMessage response = await CreateAsync(client, workshop.Id, participant.Id);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Null(response.Headers.Location);
        JsonElement body = await BodyAsync(response);
        Assert.Equal(4, body.EnumerateObject().Count());
        Guid id = body.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);
        Assert.Equal(workshop.Id, body.GetProperty("workshopId").GetGuid());
        Assert.Equal(participant.Id, body.GetProperty("participantId").GetGuid());
        Assert.Equal(factory.Clock.GetUtcNow(), body.GetProperty("createdAt").GetDateTimeOffset());
        Assert.EndsWith("Z", body.GetProperty("createdAt").GetString());
        await AssertSeatsAsync(client, workshop.Id, 0);
        using HttpResponseMessage deleted = await client.DeleteAsync("/api/registrations/" + id, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal("", await deleted.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        await AssertSeatsAsync(client, workshop.Id, 1);
        using HttpResponseMessage repeated = await client.DeleteAsync("/api/registrations/" + id, TestContext.Current.CancellationToken);
        await AssertCodeAsync(repeated, 404, "registration_not_found");
        factory.Clock.SetUtcNow(factory.Clock.GetUtcNow().AddTicks(-1));
        using HttpResponseMessage recreated = await CreateAsync(client, workshop.Id, participant.Id);
        Assert.Equal(HttpStatusCode.Created, recreated.StatusCode);
        Assert.NotEqual(id, (await BodyAsync(recreated)).GetProperty("id").GetGuid());
        Assert.Equal(factory.Clock.GetUtcNow(), (await BodyAsync(recreated)).GetProperty("createdAt").GetDateTimeOffset());
        await factory.WithDatabaseAsync(async context =>
        {
            Workshop stored = await context.Workshops.SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(workshop.Capacity, stored.Capacity);
            Assert.Equal(workshop.StartsAt, stored.StartsAt);
            Assert.Equal(workshop.EndsAt, stored.EndsAt);
            Assert.Equal(participant.Email, (await context.Participants.SingleAsync(x => x.Id == participant.Id, TestContext.Current.CancellationToken)).Email);
            Assert.Equal(1, await context.Registrations.CountAsync(TestContext.Current.CancellationToken));
            return true;
        });
    }
    private static async Task AssertSeatsAsync(HttpClient client, Guid id, int seats)
    {
        JsonElement detail = await client.GetFromJsonAsync<JsonElement>("/api/workshops/" + id, TestContext.Current.CancellationToken);
        Assert.Equal(seats, detail.GetProperty("availableSeats").GetInt32());
        JsonElement[] list = await client.GetFromJsonAsync<JsonElement[]>("/api/workshops", TestContext.Current.CancellationToken) ?? [];
        Assert.Equal(seats, list.Single(x => x.GetProperty("id").GetGuid() == id).GetProperty("availableSeats").GetInt32());
    }

    [Theory]
    [InlineData("workshopId", "missing")]
    [InlineData("workshopId", "null")]
    [InlineData("workshopId", "bad")]
    [InlineData("workshopId", "empty")]
    [InlineData("workshopId", "number")]
    [InlineData("participantId", "missing")]
    [InlineData("participantId", "null")]
    [InlineData("participantId", "bad")]
    [InlineData("participantId", "empty")]
    [InlineData("participantId", "number")]
    [InlineData("both", "empty")]
    public async Task Reg002003InvalidCreation(string field, string value)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        Dictionary<string, object?> input = new(StringComparer.Ordinal)
        {
            ["workshopId"] = Guid.NewGuid(),
            ["participantId"] = Guid.NewGuid()
        };
        if (field == "both")
        {
            input["workshopId"] = Guid.Empty;
            input["participantId"] = Guid.Empty;
        }
        else if (value == "missing")
        {
            input.Remove(field);
        }
        else
        {
            input[field] = value switch
            {
                "null" => null,
                "empty" => Guid.Empty,
                "number" => 123,
                _ => "invalid"
            };
        }

        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/registrations", input, TestContext.Current.CancellationToken);
        await AssertCodeAsync(response, 400, "validation_failed");
        JsonElement errors = (await BodyAsync(response)).GetProperty("errors");
        if (field == "both")
        {
            Assert.True(errors.TryGetProperty("workshopId", out _));
            Assert.True(errors.TryGetProperty("participantId", out _));
        }
        else
        {
            Assert.True(errors.TryGetProperty(field, out _) || errors.TryGetProperty("$", out _));
        }

        Assert.Equal(0, await factory.WithDatabaseAsync(context => context.Registrations.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Theory]
    [InlineData("workshop", "workshop_not_found", 404)]
    [InlineData("both", "workshop_not_found", 404)]
    [InlineData("participant", "participant_not_found", 404)]
    [InlineData("missingStarted", "participant_not_found", 404)]
    [InlineData("missingFull", "participant_not_found", 404)]
    [InlineData("at", "workshop_started", 409)]
    [InlineData("after", "workshop_started", 409)]
    [InlineData("ended", "workshop_started", 409)]
    [InlineData("duplicate", "registration_duplicate", 409)]
    [InlineData("startedDuplicate", "workshop_started", 409)]
    [InlineData("full", "workshop_full", 409)]
    public async Task Reg004To015PrecedenceAndRejection(string mode, string code, int status)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        (Workshop? workshop, Participant? participant, Participant? second) = await FixtureAsync(factory);
        bool registered = mode is "duplicate" or "startedDuplicate" or "full" or "missingFull";
        if (registered)
        {
            using (HttpResponseMessage created = await CreateAsync(client, workshop.Id, participant.Id))
            {
                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            }
        }

        if (mode is "at" or "missingStarted" or "startedDuplicate")
        {
            factory.Clock.SetUtcNow(workshop.StartsAt);
        }

        if (mode == "after")
        {
            factory.Clock.SetUtcNow(workshop.StartsAt.AddTicks(1));
        }

        if (mode == "ended")
        {
            factory.Clock.SetUtcNow(workshop.EndsAt.AddTicks(1));
        }

        Guid workshopId = mode is "workshop" or "both" ? Guid.NewGuid() : workshop.Id;
        Guid participantId = mode is "both" or "participant" or "missingStarted" or "missingFull" ? Guid.NewGuid() : mode == "full" ? second.Id : participant.Id;
        using HttpResponseMessage response = await CreateAsync(client, workshopId, participantId);
        await AssertCodeAsync(response, status, code);
        Assert.Equal(registered ? 1 : 0, await factory.WithDatabaseAsync(context => context.Registrations.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Reg016017018019022024ListsAndCancellationAtAnyTime(int time)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        (Workshop? workshop, Participant? participant, Participant? second) = await FixtureAsync(factory, 4);
        var other = Workshop.Create("Other", "B", workshop.StartsAt, workshop.EndsAt, 3, factory.Clock.GetUtcNow());
        await factory.WithDatabaseAsync(async context => { context.Add(other); return await context.SaveChangesAsync(TestContext.Current.CancellationToken); });
        Assert.Empty(await client.GetFromJsonAsync<JsonElement[]>("/api/registrations?workshopId=" + workshop.Id, TestContext.Current.CancellationToken) ?? []);
        using HttpResponseMessage overlap = await CreateAsync(client, other.Id, participant.Id);
        Assert.Equal(HttpStatusCode.Created, overlap.StatusCode);
        Guid[] ids = [Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("00000000-0000-0000-0000-000000000002"), Guid.Parse("00000000-0000-0000-0000-000000000003")];
        var third = Participant.Create("Third", "third@example");
        await factory.WithDatabaseAsync(async context =>
        {
            context.Add(third);
            context.AddRange(Registration.Restore(ids[2], workshop.Id, third.Id, factory.Clock.GetUtcNow().AddTicks(1)), Registration.Restore(ids[1], workshop.Id, second.Id, factory.Clock.GetUtcNow()), Registration.Restore(ids[0], workshop.Id, participant.Id, factory.Clock.GetUtcNow()));
            return await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        factory.Clock.SetUtcNow(time == 0 ? factory.Clock.GetUtcNow() : time == 1 ? workshop.StartsAt : workshop.EndsAt.AddTicks(1));
        JsonElement[] list = await client.GetFromJsonAsync<JsonElement[]>("/api/registrations?workshopId=" + workshop.Id, TestContext.Current.CancellationToken) ?? [];
        Assert.Equal(ids, list.Select(x => x.GetProperty("id").GetGuid()));
        Assert.All(list, x => { Assert.Equal(4, x.EnumerateObject().Count()); Assert.Equal(workshop.Id, x.GetProperty("workshopId").GetGuid()); });
        using HttpResponseMessage deleted = await client.DeleteAsync("/api/registrations/" + ids[0], TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        await AssertSeatsAsync(client, workshop.Id, 2);
    }

    [Theory]
    [InlineData("", 400)]
    [InlineData("?workshopId=", 400)]
    [InlineData("?workshopId=bad", 400)]
    [InlineData("?workshopId=00000000-0000-0000-0000-000000000000", 400)]
    [InlineData("?workshopId=00000000-0000-0000-0000-000000000001", 404)]
    public async Task Reg020021ListErrors(string query, int status)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/api/registrations" + query, TestContext.Current.CancellationToken);
        await AssertCodeAsync(response, status, status == 400 ? "validation_failed" : "workshop_not_found");
        if (status == 400)
        {
            Assert.True((await BodyAsync(response)).GetProperty("errors").TryGetProperty("workshopId", out _));
        }
    }

    [Theory]
    [InlineData("bad", 400)]
    [InlineData("00000000-0000-0000-0000-000000000000", 400)]
    [InlineData("00000000-0000-0000-0000-000000000001", 404)]
    public async Task Reg025026CancelErrors(string id, int status)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.DeleteAsync("/api/registrations/" + id, TestContext.Current.CancellationToken);
        await AssertCodeAsync(response, status, status == 400 ? "validation_failed" : "registration_not_found");
    }

    [Theory]
    [InlineData("seat")]
    [InlineData("duplicate")]
    [InlineData("cancel")]
    [InlineData("mixed")]
    [InlineData("clock")]
    public async Task Reg028To033ContendedMutations(string mode)
    {
        using ObservedCoordinator coordinator = new();
        await using InfrastructureTestApplicationFactory factory = new()
        {
            Coordinator = coordinator
        };
        (Workshop? workshop, Participant? participant, Participant? secondParticipant) = await FixtureAsync(factory);
        Guid existingId = Guid.Empty;
        if (mode is "cancel" or "mixed")
        {
            using HttpClient setup = factory.CreateClient();
            using HttpResponseMessage created = await CreateAsync(setup, workshop.Id, participant.Id);
            existingId = (await BodyAsync(created)).GetProperty("id").GetGuid();
        }
        coordinator.Reset();
        using HttpClient client = factory.CreateClient();
        Task<HttpResponseMessage> first;
        Task<HttpResponseMessage> second;
        using (await coordinator.HoldAsync())
        {
            first = mode is "cancel" or "mixed" ? client.DeleteAsync("/api/registrations/" + existingId, TestContext.Current.CancellationToken) : CreateAsync(client, workshop.Id, participant.Id);
            second = mode == "cancel" ? client.DeleteAsync("/api/registrations/" + existingId, TestContext.Current.CancellationToken) : CreateAsync(client, workshop.Id, mode == "duplicate" ? participant.Id : secondParticipant.Id);
            await coordinator.BothArrived.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            Assert.False(first.IsCompleted);
            Assert.False(second.IsCompleted);
            if (mode == "clock")
            {
                factory.Clock.SetUtcNow(workshop.StartsAt);
            }
        }
        using HttpResponseMessage one = await first.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        using HttpResponseMessage two = await second.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        int[] statuses = [(int)one.StatusCode, (int)two.StatusCode];
        if (mode == "clock")
        {
            await AssertCodeAsync(one, 409, "workshop_started");
            await AssertCodeAsync(two, 409, "workshop_started");
        }
        else if (mode == "cancel")
        {
            Assert.Equal((int[])[204, 404], statuses.Order());
            await AssertCodeAsync(one.StatusCode == HttpStatusCode.NotFound ? one : two, 404, "registration_not_found");
        }
        else if (mode == "mixed")
        {
            Assert.Equal(HttpStatusCode.NoContent, one.StatusCode);
            Assert.True(two.StatusCode is HttpStatusCode.Created or HttpStatusCode.Conflict);
            if (two.StatusCode == HttpStatusCode.Conflict)
            {
                await AssertCodeAsync(two, 409, "workshop_full");
            }
        }
        else
        {
            Assert.Equal((int[])[201, 409], statuses.Order());
            await AssertCodeAsync(one.StatusCode == HttpStatusCode.Conflict ? one : two, 409, mode == "seat" ? "workshop_full" : "registration_duplicate");
        }
        int expected = mode is "cancel" or "clock" || (mode == "mixed" && two.StatusCode == HttpStatusCode.Conflict) ? 0 : 1;
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        WorkshopBookingContext context = scope.ServiceProvider.GetRequiredService<WorkshopBookingContext>();
        Assert.Equal(expected, await scope.ServiceProvider.GetRequiredService<BookingDatabase>().ExecuteAsync(() => context.Registrations.CountAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken));
        await AssertSeatsAsync(client, workshop.Id, 1 - expected);
    }
    private sealed class ObservedCoordinator : IMutationCoordinator, IDisposable
    {
        private readonly MutationCoordinator _inner = new(); private int _arrivals;
        public TaskCompletionSource BothArrived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public void Reset() => _arrivals = 0;
        public Task<IDisposable> HoldAsync() => _inner.AcquireAsync(TestContext.Current.CancellationToken);
        public Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _arrivals) == 2)
            {
                BothArrived.TrySetResult();
            }

            return _inner.AcquireAsync(cancellationToken);
        }
        public void Dispose() => _inner.Dispose();
    }
}
