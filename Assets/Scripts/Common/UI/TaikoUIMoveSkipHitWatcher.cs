using System;
using UnityEngine;

public class TaikoUIMoveSkipHitWatcher
{
    double lastHitTime = -1;
    HandType lastHand = HandType.None;
    readonly int moveAmount;

    readonly Action<int> skipHit;

    static readonly double thresholdTime = 0.100;

    public TaikoUIMoveSkipHitWatcher(Action<int> skipHit, int moveAmount = 4)
    {
        this.skipHit = skipHit;
        this.moveAmount = moveAmount;
    }

    public void HandleHit(int dir)
    {
        HandType currentHand = dir == -1 ? HandType.Left : HandType.Right;
        double currentTime = TimeUtil.Time;

        if (currentTime - lastHitTime < thresholdTime)
        {
            int sign = currentHand == HandType.Left ? -1 : 1;
            int adjustedSignDelta = currentHand == lastHand ? -sign : sign;
            skipHit?.Invoke(sign * moveAmount + adjustedSignDelta);

            lastHitTime = -1;
            return;
        }

        lastHand = currentHand;
        lastHitTime = currentTime;
    }
}
