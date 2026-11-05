using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkshopBooking.Application.Persistence;
using WorkshopBooking.Domain;
using WorkshopBooking.Persistence;
using Xunit;

namespace WorkshopBooking.IntegrationTests;

public sealed class PersistenceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HostOwnsConnectionAndScopesOwnContexts(bool asynchronousDisposal)
    {
        InfrastructureTestApplicationFactory factory = new();
        BookingDatabase database = factory.Services.GetRequiredService<BookingDatabase>();
        Assert.Equal(System.Data.ConnectionState.Open, database.Connection.State);
        WorkshopBookingContext disposed;
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            disposed = scope.ServiceProvider.GetRequiredService<WorkshopBookingContext>();
            Assert.Same(disposed, scope.ServiceProvider.GetRequiredService<WorkshopBookingContext>());
            using IServiceScope other = factory.Services.CreateScope();
            Assert.NotSame(disposed, other.ServiceProvider.GetRequiredService<WorkshopBookingContext>());
        }

        Assert.Throws<ObjectDisposedException>(() => disposed.Participants.ToList());
        Assert.Equal(System.Data.ConnectionState.Open, database.Connection.State);
        if (asynchronousDisposal)
        {
            await factory.DisposeAsync();
        }
        else
        {
            factory.Dispose();
        }
        Assert.Equal(System.Data.ConnectionState.Closed, database.Connection.State);
    }

    [Fact]
    public async Task StartupCreatesEmptyIsolatedSchemasAndReplacesClock()
    {
        await using InfrastructureTestApplicationFactory first = new();
        await using InfrastructureTestApplicationFactory second = new();
        var participant = Participant.Create("A", "a@example");
        await first.WithDatabaseAsync(async context =>
        {
            context.Participants.Add(participant);
            return await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        Assert.Equal(1, await first.WithDatabaseAsync(context => context.Participants.CountAsync(TestContext.Current.CancellationToken)));
        Assert.Equal(0, await second.WithDatabaseAsync(context => context.Participants.CountAsync(TestContext.Current.CancellationToken)));
        Assert.Same(first.Clock, first.Services.GetRequiredService<TimeProvider>());
        Assert.NotSame(first.Services.GetRequiredService<IMutationCoordinator>(), second.Services.GetRequiredService<IMutationCoordinator>());
    }

    [Fact]
    public async Task ProviderPreservesTicksHistoricalStateAndUnicodeKeys()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        DateTimeOffset instant = factory.Clock.GetUtcNow().AddDays(-3).AddTicks(1234567).ToOffset(TimeSpan.FromHours(3));
        var firstId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var historical = Workshop.Restore(firstId, "Past", " élève ", instant, instant.AddHours(1), 3);
        var next = Workshop.Restore(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Next", "ÉLÈVE", instant.AddTicks(1), instant.AddHours(1), 3);
        var participant = Participant.Create("A", "a@example");
        var registration = Registration.Create(historical.Id, participant.Id, instant.AddTicks(2));
        await factory.WithDatabaseAsync(async context =>
        {
            context.AddRange(next, historical, participant, registration);
            return await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        await factory.WithDatabaseAsync(async context =>
        {
            List<Workshop> workshops = await context.Workshops.Where(x => x.NormalizedTopic == ParticipantPolicy.Key("Élève"))
                .OrderBy(x => x.StartsAt).ThenBy(x => x.Id).ToListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(2, workshops.Count);
            Assert.Equal(firstId, workshops[0].Id);
            Assert.Equal(instant.UtcTicks, workshops[0].StartsAt.Ticks);
            Assert.Equal(TimeSpan.Zero, workshops[0].StartsAt.Offset);
            Assert.Equal(instant.UtcTicks + 1, workshops[1].StartsAt.Ticks);
            Assert.Equal(instant.UtcTicks + 2, (await context.Registrations.SingleAsync(TestContext.Current.CancellationToken)).CreatedAt.Ticks);
            return true;
        });
    }

    [Fact]
    public async Task ConstraintsRejectDuplicatesAndMissingReferences()
    {
        await using InfrastructureTestApplicationFactory factory = new();
        var participant = Participant.Create("A", "a@example");
        var workshop = Workshop.Create("A", "B", factory.Clock.GetUtcNow().AddHours(1), factory.Clock.GetUtcNow().AddHours(2), 1, factory.Clock.GetUtcNow());
        await factory.WithDatabaseAsync(async context =>
        {
            context.AddRange(participant, workshop, Registration.Create(workshop.Id, participant.Id, factory.Clock.GetUtcNow()));
            return await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
        await AssertFailureAsync(factory, Participant.Create("Other", " A@EXAMPLE "), "participant_email_conflict");
        await AssertFailureAsync(factory, Registration.Create(workshop.Id, participant.Id, factory.Clock.GetUtcNow()), "registration_duplicate");
        await AssertFailureAsync(factory, Registration.Create(Guid.NewGuid(), participant.Id, factory.Clock.GetUtcNow()), null);
        await AssertFailureAsync(factory, Registration.Create(workshop.Id, Guid.NewGuid(), factory.Clock.GetUtcNow()), null);
        Assert.Equal(1, await factory.WithDatabaseAsync(context => context.Registrations.CountAsync(TestContext.Current.CancellationToken)));
    }

    private static async Task AssertFailureAsync(InfrastructureTestApplicationFactory factory, object entity, string? code)
    {
        await factory.WithDatabaseAsync(async context =>
        {
            context.Add(entity);
            DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
            Assert.Equal(code, ConstraintFailures.Recognize(exception)?.Code);
            return true;
        });
    }

    [Fact]
    public async Task MutationLeaseSerializesAndReleasesAfterFailureAndCancellation()
    {
        using MutationCoordinator coordinator = new();
        IDisposable lease = await coordinator.AcquireAsync(TestContext.Current.CancellationToken);
        using CancellationTokenSource cancellation = new();
        Task<IDisposable> cancelled = coordinator.AcquireAsync(cancellation.Token);
        Task<IDisposable> waiting = coordinator.AcquireAsync(TestContext.Current.CancellationToken);
        Assert.False(waiting.IsCompleted);
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelled);
        lease.Dispose();
        lease.Dispose();
        using (await waiting.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken))
        {
        }
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            using IDisposable acquired = await coordinator.AcquireAsync(TestContext.Current.CancellationToken);
            throw new InvalidOperationException("Injected failure");
        });
        using IDisposable final = await coordinator.AcquireAsync(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task IoGateProtectsReadsAndReleasesAfterFailure()
    {
        using BookingDatabase database = new();
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<int> first = database.ExecuteAsync<int>(async () =>
        {
            entered.SetResult();
            await release.Task;
            throw new InvalidOperationException();
        }, TestContext.Current.CancellationToken);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task<int> second = database.ExecuteAsync(() => Task.FromResult(2), TestContext.Current.CancellationToken);
        Assert.False(second.IsCompleted);
        release.SetResult();
        await Assert.ThrowsAsync<InvalidOperationException>(() => first);
        Assert.Equal(2, await second.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
    }
}
