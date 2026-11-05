using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Domain;
using WorkshopBooking.Persistence;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class ParticipantAcceptanceTests
{
    [Fact]
    public async Task Par025ExistingWorkshopAndRegistrationRemainUnchanged()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        var workshop = Workshop.Create("Existing", "Topic", factory.Clock.GetUtcNow().AddHours(1), factory.Clock.GetUtcNow().AddHours(2), 3, factory.Clock.GetUtcNow());
        var participant = Participant.Create("Original", "original@example");
        var registration = Registration.Create(workshop.Id, participant.Id, factory.Clock.GetUtcNow());
        await factory.WithDatabaseAsync(async context =>
        {
            context.AddRange(workshop, participant, registration);
            return await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        using HttpResponseMessage success = await client.PostAsJsonAsync("/api/participants", new
        {
            name = "New",
            email = "new@example"
        }, TestContext.Current.CancellationToken);
        using HttpResponseMessage rejected = await client.PostAsJsonAsync("/api/participants", new
        {
            name = "Overwrite",
            email = "ORIGINAL@EXAMPLE"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, success.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, rejected.StatusCode);
        await factory.WithDatabaseAsync(async context =>
        {
            Workshop stored = await context.Workshops.SingleAsync(TestContext.Current.CancellationToken);
            Registration storedRegistration = await context.Registrations.SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(workshop.Id, stored.Id);
            Assert.Equal(workshop.Capacity, stored.Capacity);
            Assert.Equal(workshop.StartsAt, stored.StartsAt);
            Assert.Equal(registration.Id, storedRegistration.Id);
            Assert.Equal(registration.CreatedAt, storedRegistration.CreatedAt);
            Assert.Equal("Original", (await context.Participants.SingleAsync(x => x.Id == participant.Id, TestContext.Current.CancellationToken)).Name);
            return true;
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Par001002004006010012018024025CreateAndReadback(int length)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        string name = length == 1 ? "É" : new string('a', 100);
        string email = length == 1 ? "Alex@example" : new string('a', 252) + "@b";
        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/participants", new
        {
            name = " " + name + " ",
            email = " " + email + " ",
            id = Guid.Empty,
            normalizedEmail = "spoof"
        }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(3, body.EnumerateObject().Count());
        Assert.NotEqual(Guid.Empty, body.GetProperty("id").GetGuid());
        Assert.Equal(name, body.GetProperty("name").GetString());
        Assert.Equal(email, body.GetProperty("email").GetString());
        Assert.NotNull(response.Headers.Location);
        JsonElement read = await client.GetFromJsonAsync<JsonElement>(response.Headers.Location, TestContext.Current.CancellationToken);
        Assert.Equal(body.ToString(), read.ToString());
        await factory.WithDatabaseAsync(async context =>
        {
            Participant stored = await context.Participants.SingleAsync(TestContext.Current.CancellationToken);
            Assert.Equal(ParticipantPolicy.Key(email), stored.NormalizedEmail);
            Assert.Equal(0, await context.Workshops.CountAsync(TestContext.Current.CancellationToken));
            Assert.Equal(0, await context.Registrations.CountAsync(TestContext.Current.CancellationToken));
            return true;
        });
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("name", "")]
    [InlineData("name", "  ")]
    [InlineData("name", "missing")]
    [InlineData("name", "long")]
    [InlineData("email", null)]
    [InlineData("email", "")]
    [InlineData("email", "  ")]
    [InlineData("email", "missing")]
    [InlineData("email", "long")]
    [InlineData("email", "abc")]
    [InlineData("email", "a@@b")]
    [InlineData("email", "@b")]
    [InlineData("email", "a@")]
    [InlineData("email", "a b@c")]
    [InlineData("email", "a\u0001@b")]
    [InlineData("email", "a<@b")]
    [InlineData("email", "a>@b")]
    [InlineData("email", "a,@b")]
    [InlineData("email", "a;@b")]
    public async Task Par003005007008009011InvalidFieldsDoNotWrite(string field, string? value)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        Dictionary<string, object?> input = new(StringComparer.Ordinal)
        {
            ["name"] = "A",
            ["email"] = "a@b"
        };
        if (value == "missing")
        {
            input.Remove(field);
        }
        else
        {
            input[field] = value == "long" ? new string('a', field == "name" ? 101 : 253) + (field == "email" ? "@b" : "") : value;
        }

        using HttpResponseMessage response = await client.PostAsJsonAsync("/api/participants", input, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal("validation_failed", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty(field, out _));
        Assert.Equal(0, await factory.WithDatabaseAsync(context => context.Participants.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Par013014015016017022026DuplicatesPrecedenceAndOrdinalOrdering()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            await using InfrastructureTestApplicationFactory factory = new();
            using HttpClient client = factory.CreateClient();
            string[] emails = ["İ@example", "Z@example", "i@example", "a.b@example", "ab@example", "ab+tag@example", "\U00010000@example", "\uE000@example"];
            foreach (string email in emails)
            {
                using HttpResponseMessage created = await client.PostAsJsonAsync("/api/participants", new
                {
                    name = " Éva  Smith! ",
                    email
                }, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
            }
            foreach (string email in new[] { "i@example", " I@EXAMPLE " })
            {
                using HttpResponseMessage duplicate = await client.PostAsJsonAsync("/api/participants", new
                {
                    name = "Other",
                    email
                }, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
                JsonElement problem = await duplicate.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
                Assert.Equal("participant_email_conflict", problem.GetProperty("code").GetString());
                Assert.DoesNotContain("i@example", problem.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            using HttpResponseMessage invalid = await client.PostAsJsonAsync("/api/participants", new
            {
                name = " ",
                email = "i@example"
            }, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
            JsonElement[] list = await client.GetFromJsonAsync<JsonElement[]>("/api/participants", TestContext.Current.CancellationToken) ?? [];
            Assert.Equal(emails.OrderBy(ParticipantPolicy.Key, StringComparer.Ordinal), list.Select(x => x.GetProperty("email").GetString()));
            Assert.All(list, item => Assert.Equal("Éva  Smith!", item.GetProperty("name").GetString()));
            Assert.Equal(emails.Length, await factory.WithDatabaseAsync(context => context.Participants.CountAsync(TestContext.Current.CancellationToken)));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData("bad", 400)]
    [InlineData("00000000-0000-0000-0000-000000000000", 400)]
    [InlineData("00000000-0000-0000-0000-000000000001", 404)]
    public async Task Par019020IdentityErrors(string id, int status)
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/api/participants/" + id, TestContext.Current.CancellationToken);
        Assert.Equal(status, (int)response.StatusCode);
        JsonElement problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal(status == 404 ? "participant_not_found" : "validation_failed", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Par023CompetingRequestsReachProductionCoordinator()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        using ObservedCoordinator coordinator = new();
        await using WebApplicationFactory<Program> host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMutationCoordinator>();
            services.AddSingleton<IMutationCoordinator>(coordinator);
        }));
        using HttpClient client = host.CreateClient();
        Task<HttpResponseMessage> first;
        Task<HttpResponseMessage> second;
        using (await coordinator.HoldAsync())
        {
            first = client.PostAsJsonAsync("/api/participants", new
            {
                name = "A",
                email = "Alex@example"
            }, TestContext.Current.CancellationToken);
            second = client.PostAsJsonAsync("/api/participants", new
            {
                name = "B",
                email = " ALEX@EXAMPLE "
            }, TestContext.Current.CancellationToken);
            await coordinator.BothArrived.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            Assert.False(first.IsCompleted);
            Assert.False(second.IsCompleted);
        }
        using HttpResponseMessage one = await first.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        using HttpResponseMessage two = await second.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        Assert.Equal((int[])[201, 409], new[] { (int)one.StatusCode, (int)two.StatusCode }.Order());
        HttpResponseMessage conflict = one.StatusCode == HttpStatusCode.Conflict ? one : two;
        Assert.Equal("participant_email_conflict", (await conflict.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken)).GetProperty("code").GetString());
        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
        WorkshopBookingContext context = scope.ServiceProvider.GetRequiredService<WorkshopBookingContext>();
        Assert.Equal(1, await scope.ServiceProvider.GetRequiredService<BookingDatabase>().ExecuteAsync(() => context.Participants.CountAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken));
    }

    private sealed class ObservedCoordinator : IMutationCoordinator, IDisposable
    {
        private readonly MutationCoordinator _inner = new();
        private int _arrivals;
        public TaskCompletionSource BothArrived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
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
