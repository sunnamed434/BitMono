namespace BitMono.Utilities.Time;

public static class StopwatchUtilities
{
    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;
    
    public static TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
    {
        var tickFrequency = (double)TicksPerSecond / Stopwatch.Frequency;
        return new TimeSpan((long)((endingTimestamp - startingTimestamp) * tickFrequency));
    }
}