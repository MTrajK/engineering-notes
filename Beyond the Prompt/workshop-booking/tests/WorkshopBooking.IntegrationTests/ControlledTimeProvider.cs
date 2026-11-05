namespace WorkshopBooking.IntegrationTests;

public sealed class ControlledTimeProvider : TimeProvider
{
    private long _ticks = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero).Ticks;
    private int _reads;
    public int Reads => Volatile.Read(ref _reads);
    public override DateTimeOffset GetUtcNow()
    {
        Interlocked.Increment(ref _reads);
        return new(Interlocked.Read(ref _ticks), TimeSpan.Zero);
    }
    public void SetUtcNow(DateTimeOffset value) => Interlocked.Exchange(ref _ticks, value.UtcTicks);
}
