using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Participants;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Application.Registrations;
using WorkshopBooking.Application.Workshops;
using WorkshopBooking.Domain;
using Xunit;

namespace WorkshopBooking.UnitTests;

public sealed class RegistrationServiceTests
{
    [Theory]
    [InlineData("workshop", "workshop_not_found")]
    [InlineData("participant", "participant_not_found")]
    [InlineData("started", "workshop_started")]
    [InlineData("duplicate", "registration_duplicate")]
    [InlineData("full", "workshop_full")]
    public async Task PrecedenceAndNoWriteOnRejection(string mode, string code)
    {
        Store store = new()
        {
            Mode = mode
        };
        using Coordinator coordinator = new();
        Clock clock = new(coordinator);
        RegistrationService service = new(store, store, store, coordinator, clock);
        Outcome<RegistrationDetails> result = await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), TestContext.Current.CancellationToken);
        Assert.Equal(code, result.Failure.Code);
        Assert.Null(store.Inserted);
        Assert.False(coordinator.Held);
        Assert.Equal(mode == "workshop" ? (string[])["workshop"] : mode is "participant" or "started" ? (string[])["workshop", "participant"] : (string[])["workshop", "participant", "pair"], store.Calls);
    }

    [Fact]
    public async Task ValidationCollectsBothIdsWithoutLookupOrClock()
    {
        Store store = new();
        using Coordinator coordinator = new();
        Clock clock = new(coordinator);
        RegistrationService service = new(store, store, store, coordinator, clock);
        Outcome<RegistrationDetails> result = await service.CreateAsync(null, Guid.Empty, TestContext.Current.CancellationToken);
        Assert.Equal(2, result.Failure.Errors!.Count);
        Assert.Empty(store.Calls);
        Assert.Equal(0, clock.Reads);
        Assert.False((await service.ListAsync(null, TestContext.Current.CancellationToken)).IsSuccess);
        Assert.False((await service.ListAsync(Guid.Empty, TestContext.Current.CancellationToken)).IsSuccess);
        Assert.False((await service.CancelAsync(Guid.Empty, TestContext.Current.CancellationToken)).IsSuccess);
    }

    [Fact]
    public async Task SuccessUsesOneCapturedInstantUnderLeaseAndPropagatesWriteFailure()
    {
        Store store = new();
        using Coordinator coordinator = new();
        Clock clock = new(coordinator);
        RegistrationService service = new(store, store, store, coordinator, clock);
        Outcome<RegistrationDetails> result = await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, clock.Reads);
        Assert.Equal(Store.Now, result.Value.CreatedAt);
        Assert.NotNull(store.Inserted);
        Assert.Equal(store.Inserted.Id, result.Value.Id);
        Assert.False(coordinator.Held);
        store.Mode = "constraint";
        Assert.Equal("registration_duplicate", (await service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), TestContext.Current.CancellationToken)).Failure.Code);
        store.Mode = "exception";
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), TestContext.Current.CancellationToken));
        Assert.False(coordinator.Held);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateAsync(Guid.NewGuid(), Guid.NewGuid(), cancellation.Token));
    }

    [Fact]
    public async Task ListingAndCancellationHaveNoClockCutoff()
    {
        Store store = new();
        using Coordinator coordinator = new();
        Clock clock = new(coordinator);
        RegistrationService service = new(store, store, store, coordinator, clock);
        Assert.Empty((await service.ListAsync(Guid.NewGuid(), TestContext.Current.CancellationToken)).Value);
        store.Mode = "workshop";
        Assert.Equal("workshop_not_found", (await service.ListAsync(Guid.NewGuid(), TestContext.Current.CancellationToken)).Failure.Code);
        Assert.True((await service.CancelAsync(Guid.NewGuid(), TestContext.Current.CancellationToken)).IsSuccess);
        store.Mode = "missingRegistration";
        Assert.Equal("registration_not_found", (await service.CancelAsync(Guid.NewGuid(), TestContext.Current.CancellationToken)).Failure.Code);
        Assert.Equal(0, clock.Reads);
        Assert.False(coordinator.Held);
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
    private sealed class Clock(Coordinator coordinator) : TimeProvider
    {
        public int Reads
        {
            get; private set;
        }
        public override DateTimeOffset GetUtcNow()
        {
            Assert.True(coordinator.Held);
            Reads++;
            return Store.Now;
        }
    }
    private sealed class Store : IRegistrationStore, IWorkshopStore, IParticipantStore
    {
        internal static readonly DateTimeOffset Now = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public string Mode { get; set; } = "success";
        public List<string> Calls { get; } = [];
        public Registration? Inserted
        {
            get; private set;
        }
        public Task<bool> PairExistsAsync(Guid workshopId, Guid participantId, CancellationToken cancellationToken)
        {
            Calls.Add("pair");
            return Task.FromResult(Mode == "duplicate");
        }
        public Task<Outcome<Unit>> InsertAsync(Registration registration, CancellationToken cancellationToken)
        {
            if (Mode == "exception")
            {
                throw new InvalidOperationException();
            }

            Inserted = registration;
            return Task.FromResult(Mode == "constraint" ? Outcome.Failed<Unit>(ApplicationFailure.Conflict("registration_duplicate")) : Outcome.Success(Unit.Value));
        }
        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Mode != "missingRegistration");
        public Task<IReadOnlyList<RegistrationDetails>> ListAsync(Guid workshopId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<RegistrationDetails>>([]);
        Task<WorkshopDetails?> IWorkshopStore.GetAsync(Guid id, CancellationToken cancellationToken)
        {
            Calls.Add("workshop");
            return Task.FromResult<WorkshopDetails?>(Mode == "workshop" ? null : new WorkshopDetails(id, "A", "B", Mode is "started" or "participant" ? Now : Now.AddTicks(1), Now.AddHours(1), 1, Mode is "full" or "duplicate" ? 0 : 1));
        }
        Task<ParticipantDetails?> IParticipantStore.GetAsync(Guid id, CancellationToken cancellationToken)
        {
            Calls.Add("participant");
            return Task.FromResult<ParticipantDetails?>(Mode == "participant" ? null : new ParticipantDetails(id, "A", "a@b"));
        }
        Task IWorkshopStore.InsertAsync(Workshop workshop, CancellationToken cancellationToken) => throw new InvalidOperationException("Unexpected workshop write");
        Task<IReadOnlyList<WorkshopDetails>> IWorkshopStore.ListAsync(string? normalizedTopic, CancellationToken cancellationToken) => throw new InvalidOperationException("Unexpected list");
        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) => throw new InvalidOperationException("Unexpected lookup");
        public Task<Outcome<Unit>> InsertAsync(Participant participant, CancellationToken cancellationToken) => throw new InvalidOperationException("Unexpected participant write");
        Task<IReadOnlyList<ParticipantDetails>> IParticipantStore.ListAsync(CancellationToken cancellationToken) => throw new InvalidOperationException("Unexpected list");
    }
}
