using Microsoft.Data.Sqlite;

namespace WorkshopBooking.Persistence;

public sealed class BookingDatabase : IDisposable, IAsyncDisposable
{
    internal SqliteConnection Connection { get; } = new("Data Source=:memory:;Foreign Keys=True");
    private readonly SemaphoreSlim _ioGate = new(1, 1);
    private readonly Lock _disposeLock = new();
    private bool _disposed;
    private bool _initialized;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _ioGate.WaitAsync(cancellationToken);
        try
        {
            return await operation();
        }
        finally { _ioGate.Release(); }
    }

    internal async Task InitializeAsync(WorkshopBookingContext context, TimeProvider clock, DatabaseInitializationOptions options, CancellationToken cancellationToken)
    {
        await ExecuteAsync(async () =>
        {
            if (_initialized)
            {
                return false;
            }

            if (Connection.State != System.Data.ConnectionState.Open)
            {
                await Connection.OpenAsync(cancellationToken);
            }

            await context.Database.EnsureCreatedAsync(cancellationToken);
            if (options.SeedDemoData)
            {
                DateTimeOffset startup = clock.GetUtcNow();
                await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                DemoSeed.AddTo(context, startup);
                await context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            _initialized = true;
            return true;
        }, cancellationToken);
    }

    public void Dispose()
    {
        // Test-host shutdown and the entry-point shutdown may both dispose the
        // provider. A second caller must wait for connection closure to finish.
        lock (_disposeLock)
        {
            if (_disposed)
            {
                return;
            }

            Connection.Dispose();
            _ioGate.Dispose();
            _disposed = true;
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
