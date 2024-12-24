namespace WebApplication1;

public class Stats
{
    private readonly Lock _lock = new();
    private DateTime _currTime;
    public DateTime StartTime { get; init; } = DateTime.UtcNow;

    public DateTime CurrentTime
    {
        get
        {
            lock (_lock)
            {
                return _currTime;
            }
        }
        set
        {
            lock (_lock)
            {
                _currTime = value;
            }
        }

    }
}