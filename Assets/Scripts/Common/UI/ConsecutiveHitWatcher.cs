using System;

public class ConsecutiveHitWatcher
{
    int currentConsecutiveHits;

    readonly int consecutiveHitsRequired;
    readonly Action onConsecutiveHitsReached;
    readonly TaikoKeyType keyType;

    public ConsecutiveHitWatcher(
        Action onConsecutiveHitsReached,
        TaikoKeyType keyType = TaikoKeyType.RIGHT_KA,
        int consecutiveHitsRequired = 8
    )
    {
        this.consecutiveHitsRequired = consecutiveHitsRequired;
        currentConsecutiveHits = 0;
        this.onConsecutiveHitsReached = onConsecutiveHitsReached;
        this.keyType = keyType;
    }

    public void HandleHit(TaikoKeyType key)
    {
        if (key == keyType) currentConsecutiveHits++;
        else currentConsecutiveHits = 0;

        if (currentConsecutiveHits >= consecutiveHitsRequired)
        {
            onConsecutiveHitsReached?.Invoke();
            currentConsecutiveHits = 0;
        }
    }
}
