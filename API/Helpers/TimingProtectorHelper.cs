using System.Diagnostics;

namespace DotNetAngularTemplate.Helpers;

public class TimingProtectorHelper
{
    public static async Task<T> RunWithMinimumDelayAsync<T>(Func<Task<T>> action, int minimumMilliseconds,
        ILogger? logger = null)
    {
        var start = Stopwatch.GetTimestamp();

        var result = await action();

        var elapsed = (int)((Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency);
        var remaining = minimumMilliseconds - elapsed;

        logger?.LogDebug("Action took {ElapsedMs}ms, delaying for {RemainingMs}ms to normalize timing.",
            elapsed, Math.Max(0, remaining));

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }

        return result;
    }

    public static async Task RunWithMinimumDelayAsync(Func<Task> action, int minimumMilliseconds,
        ILogger? logger = null)
    {
        var start = Stopwatch.GetTimestamp();

        await action();

        var elapsed = (int)((Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency);
        var remaining = minimumMilliseconds - elapsed;

        logger?.LogDebug("Action took {ElapsedMs}ms, delaying for {RemainingMs}ms to normalize timing.",
            elapsed, Math.Max(0, remaining));

        if (remaining > 0)
        {
            await Task.Delay(remaining);
        }
    }
}