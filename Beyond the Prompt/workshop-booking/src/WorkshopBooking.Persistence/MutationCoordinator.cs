using WorkshopBooking.Application.Persistence;

namespace WorkshopBooking.Persistence;

public sealed class MutationCoordinator : IMutationCoordinator, IDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        return new Lease(_gate);
    }

    public void Dispose() => _gate.Dispose();

    private sealed class Lease(SemaphoreSlim semaphore) : IDisposable
    {
        private SemaphoreSlim? _gate = semaphore;
        public void Dispose() => Interlocked.Exchange(ref _gate, null)?.Release();
    }
}
