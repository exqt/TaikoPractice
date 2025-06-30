using System;
using System.Diagnostics;

public class TimeUtil
{
    public static double Time
    {
        get
        {
            instance ??= new TimeUtil();
            return instance.GetTime();
        }
    }

    public static double Measure(Action action)
    {
        instance ??= new TimeUtil();

        var stopwatch = Stopwatch.StartNew();
        action.Invoke();
        stopwatch.Stop();
        return stopwatch.Elapsed.TotalSeconds;
    }

    private static TimeUtil instance = new();

    private readonly Stopwatch stopwatch;

    public TimeUtil()
    {
        stopwatch = Stopwatch.StartNew();
    }

    private double GetTime()
    {
        return stopwatch.Elapsed.TotalSeconds;
    }
}
