namespace WorkshopBooking.Application.Persistence;

public interface IMutationCoordinator
{
    Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default);
}
