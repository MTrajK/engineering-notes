using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Application.Workshops;
using WorkshopBooking.Domain;
using Xunit;

namespace WorkshopBooking.UnitTests;

public sealed class WorkshopServiceTests
{
    [Fact]
    public async Task ValidationCollectsErrorsAndAvoidsPersistence()
    {
        Store store = new();
        Clock clock = new();
        WorkshopService service = new(store, new Coordinator(), clock);
        Outcome<WorkshopDetails> result = await service.CreateAsync(new CreateWorkshop(" ", new string('a', 101), clock.GetUtcNow(), clock.GetUtcNow(), 0), TestContext.Current.CancellationToken);
        Assert.False(result.IsSuccess);
        Assert.Equal((string[])["capacity", "endsAt", "startsAt", "title", "topic"], result.Failure.Errors!.Keys.Order(StringComparer.Ordinal));
        Assert.Null(store.Inserted);
        Assert.Equal(0, store.Reads);
    }

    [Fact]
    public async Task CreationUsesOneClockInstantAfterLeaseAndPersistsTrimmedEntity()
    {
        Store store = new();
        Clock clock = new();
        Coordinator coordinator = new();
        clock.OnRead = () => Assert.True(coordinator.Held);
        WorkshopService service = new(store, coordinator, clock);
        DateTimeOffset now = clock.Instant;
        Outcome<WorkshopDetails> result = await service.CreateAsync(new CreateWorkshop(" Title ", " élève ", now.AddTicks(1), now.AddTicks(2), 1), TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, clock.Reads);
        Assert.False(coordinator.Held);
        Assert.NotNull(store.Inserted);
        Assert.Equal("Title", store.Inserted.Title);
        Assert.Equal("ÉLÈVE", store.Inserted.NormalizedTopic);
        Assert.Equal(1, result.Value.AvailableSeats);
    }

    [Fact]
    public async Task ReadsValidateAndNormalizeWithoutClockOrMutationLease()
    {
        Store store = new();
        Clock clock = new();
        Coordinator coordinator = new();
        WorkshopService service = new(store, coordinator, clock);
        Assert.False((await service.GetAsync(Guid.Empty, TestContext.Current.CancellationToken)).IsSuccess);
        Assert.False((await service.ListAsync(" ", TestContext.Current.CancellationToken)).IsSuccess);
        Assert.Equal(0, store.Reads);
        Assert.Equal("workshop_not_found", (await service.GetAsync(Guid.NewGuid(), TestContext.Current.CancellationToken)).Failure.Code);
        await service.ListAsync(" éLèVe ", TestContext.Current.CancellationToken);
        Assert.Equal("ÉLÈVE", store.Key);
        await service.ListAsync(null, TestContext.Current.CancellationToken);
        Assert.Null(store.Key);
        Assert.Equal(0, clock.Reads);
        Assert.False(coordinator.Held);
        store.Details = new WorkshopDetails(Guid.NewGuid(), "A", "B", clock.Instant.AddDays(-2), clock.Instant.AddDays(-1), 3, 1);
        Assert.Equal(store.Details, (await service.GetAsync(store.Details.Id, TestContext.Current.CancellationToken)).Value);
    }

    [Fact]
    public async Task PersistenceFailureReleasesLeaseAndPropagates()
    {
        Store store = new()
        {
            Fail = true
        };
        Clock clock = new();
        Coordinator coordinator = new();
        WorkshopService service = new(store, coordinator, clock);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateWorkshop("A", "B", clock.Instant.AddHours(1), clock.Instant.AddHours(2), 1), TestContext.Current.CancellationToken));
        Assert.False(coordinator.Held);
    }

    private sealed class Store : IWorkshopStore
    {
        public Workshop? Inserted
        {
            get; private set;
        }
        public WorkshopDetails? Details
        {
            get; set;
        }
        public string? Key
        {
            get; private set;
        }
        public int Reads
        {
            get; private set;
        }
        public bool Fail
        {
            get; set;
        }
        public Task InsertAsync(Workshop workshop, CancellationToken cancellationToken)
        {
            if (Fail)
            {
                throw new InvalidOperationException();
            }

            Inserted = workshop;
            return Task.CompletedTask;
        }
        public Task<WorkshopDetails?> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            Reads++;
            return Task.FromResult(Details);
        }
        public Task<IReadOnlyList<WorkshopDetails>> ListAsync(string? normalizedTopic, CancellationToken cancellationToken)
        {
            Reads++;
            Key = normalizedTopic;
            return Task.FromResult<IReadOnlyList<WorkshopDetails>>([]);
        }
    }
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Instant { get; } = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public int Reads
        {
            get; private set;
        }
        public Action? OnRead
        {
            get; set;
        }
        public override DateTimeOffset GetUtcNow()
        {
            OnRead?.Invoke();
            Reads++;
            return Instant;
        }
    }
    private sealed class Coordinator : IMutationCoordinator, IDisposable
    {
        public bool Held
        {
            get; private set;
        }
        public Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Held = true;
            return Task.FromResult<IDisposable>(this);
        }
        public void Dispose() => Held = false;
    }
}
