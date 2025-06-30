using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class ThreadBase
{
    static Thread thread;

    const double MEASURE_INTERVAL = 1.0f;
    readonly Queue<double> framecounts;

    public ThreadBase()
    {
        framecounts = new Queue<double>();
        thread = new Thread(Loop);
        thread.Start();
    }

    protected virtual void Start() { }
    protected virtual void Clean() { }
    protected abstract void Update();

    public int FPS { get => framecounts.Count; }

    public void Stop()
    {
        quit = true;
    }

    bool quit = false;
    public void Loop()
    {
        Start();

        while (!quit)
        {
            Update();
            UpdateFrameCount();
            Thread.Sleep(1);
        }

        Clean();
    }

    void UpdateFrameCount()
    {
        var time = TimeUtil.Time;

        framecounts.Enqueue(time);
        while (framecounts.Count > 0 &&
            (framecounts.Peek() < time - MEASURE_INTERVAL))
        {
            framecounts.Dequeue();
        }
    }

    // Log Utils
    public static class Debug
    {

        public enum LOGLEVEL
        {
            INFO,
            WARNING,
            ERROR,
            DEBUG,
        }
        static readonly List<Tuple<LOGLEVEL, string>> logQueue = new List<Tuple<LOGLEVEL, string>>();

        static public void Log(string message)
        {
            logQueue.Add(new Tuple<LOGLEVEL, string>(LOGLEVEL.INFO, message));
        }

        static public void LogWarning(string message)
        {
            logQueue.Add(new Tuple<LOGLEVEL, string>(LOGLEVEL.WARNING, message));
        }

        static public void LogError(string message)
        {
            logQueue.Add(new Tuple<LOGLEVEL, string>(LOGLEVEL.ERROR, message));
        }

        static public void LogDebug(string message)
        {
            logQueue.Add(new Tuple<LOGLEVEL, string>(LOGLEVEL.DEBUG, message));
        }

        static public void FlushLogFromMainUnityThread()
        {
            foreach (var log in logQueue)
            {
                switch (log.Item1)
                {
                    case LOGLEVEL.INFO:
                        UnityEngine.Debug.Log(log.Item2);
                        break;
                    case LOGLEVEL.WARNING:
                        UnityEngine.Debug.LogWarning(log.Item2);
                        break;
                    case LOGLEVEL.ERROR:
                        UnityEngine.Debug.LogError(log.Item2);
                        break;
                    case LOGLEVEL.DEBUG:
                        UnityEngine.Debug.Log(log.Item2);
                        break;
                }
            }
            logQueue.Clear();
        }
    }
}
