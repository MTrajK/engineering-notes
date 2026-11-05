using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Application.Participants;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Domain;
using Xunit;

namespace WorkshopBooking.UnitTests;

public sealed class ParticipantServiceTests
{
    [Fact]
    public async Task IndependentValidationPrecedesUniqueness()
    {
        Store store = new()
        {
            Exists = true
        };
        Coordinator coordinator = new();
        ParticipantService service = new(store, coordinator);
        Outcome<ParticipantDetails> result = await service.CreateAsync(" ", "bad", TestContext.Current.CancellationToken);
        Assert.Equal((string[])["email", "name"], result.Failure.Errors!.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(0, store.Lookups);
        Assert.Equal(0, coordinator.Acquisitions);
        Assert.Null(store.Inserted);
    }

    [Fact]
    public async Task CreationNormalizesLookupAndPreservesDisplayUnderLease()
    {
        Store store = new();
        Coordinator coordinator = new();
        store.OnLookup = () => Assert.True(coordinator.Held);
        ParticipantService service = new(store, coordinator);
        Outcome<ParticipantDetails> result = await service.CreateAsync(" Éva  Smith ", " Alex+tag@Example ", TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccess);
        Assert.Equal("Éva  Smith", result.Value.Name);
        Assert.Equal("Alex+tag@Example", result.Value.Email);
        Assert.Equal("ALEX+TAG@EXAMPLE", store.Key);
        Assert.NotNull(store.Inserted);
        Assert.Equal(store.Inserted.Id, result.Value.Id);
        Assert.False(coordinator.Held);
    }

    [Fact]
    public async Task ExistingEmailAndConstraintConflictsUseSharedFailure()
    {
        Store store = new()
        {
            Exists = true
        };
        ParticipantService service = new(store, new Coordinator());
        Assert.Equal("participant_email_conflict", (await service.CreateAsync("A", "a@b", TestContext.Current.CancellationToken)).Failure.Code);
        Assert.Null(store.Inserted);
        store.Exists = false;
        store.ConstraintConflict = true;
        Assert.Equal("participant_email_conflict", (await service.CreateAsync("A", "a@b", TestContext.Current.CancellationToken)).Failure.Code);
    }

    [Fact]
    public async Task UnexpectedFailureAndCancellationPropagateAndReleaseLease()
    {
        Store store = new()
        {
            Throw = true
        };
        Coordinator coordinator = new();
        ParticipantService service = new(store, coordinator);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync("A", "a@b", TestContext.Current.CancellationToken));
        Assert.False(coordinator.Held);
        using CancellationTokenSource source = new();
        await source.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateAsync("A", "a@b", source.Token));
        Assert.False(coordinator.Held);
    }

    [Fact]
    public async Task ReadsValidateIdentityAndReturnDataWithoutMutation()
    {
        Store store = new();
        Coordinator coordinator = new();
        ParticipantService service = new(store, coordinator);
        Assert.Equal("validation_failed", (await service.GetAsync(Guid.Empty, TestContext.Current.CancellationToken)).Failure.Code);
        Assert.Equal("participant_not_found", (await service.GetAsync(Guid.NewGuid(), TestContext.Current.CancellationToken)).Failure.Code);
        store.Details = new ParticipantDetails(Guid.NewGuid(), "A", "a@b");
        Assert.Equal(store.Details, (await service.GetAsync(store.Details.Id, TestContext.Current.CancellationToken)).Value);
        Assert.Empty((await service.ListAsync(TestContext.Current.CancellationToken)).Value);
        Assert.Equal(0, coordinator.Acquisitions);
    }

    private sealed class Coordinator : IMutationCoordinator, IDisposable
    {
        public bool Held
        {
            get; private set;
        }
        public int Acquisitions
        {
            get; private set;
        }
        public Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Acquisitions++;
            Held = true;
            return Task.FromResult<IDisposable>(this);
        }
        public void Dispose() => Held = false;
    }
    private sealed class Store : IParticipantStore
    {
        public bool Exists
        {
            get; set;
        }
        public bool ConstraintConflict
        {
            get; set;
        }
        public bool Throw
        {
            get; set;
        }
        public int Lookups
        {
            get; private set;
        }
        public string? Key
        {
            get; private set;
        }
        public Participant? Inserted
        {
            get; private set;
        }
        public ParticipantDetails? Details
        {
            get; set;
        }
        public Action? OnLookup
        {
            get; set;
        }
        public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            OnLookup?.Invoke();
            Lookups++;
            Key = normalizedEmail;
            return Task.FromResult(Exists);
        }
        public Task<Outcome<Unit>> InsertAsync(Participant participant, CancellationToken cancellationToken)
        {
            if (Throw)
            {
                throw new InvalidOperationException();
            }

            Inserted = participant;
            return Task.FromResult(ConstraintConflict ? Outcome.Failed<Unit>(ApplicationFailure.Conflict("participant_email_conflict")) : Outcome.Success(Unit.Value));
        }
        public Task<ParticipantDetails?> GetAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Details);
        public Task<IReadOnlyList<ParticipantDetails>> ListAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ParticipantDetails>>([]);
    }
}
