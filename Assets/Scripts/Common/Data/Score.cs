using UnityEngine;

public class Score
{
    public float Accuracy;

    public float HandFinesse;

    public Score Max(Score other)
    {
        return new Score
        {
            Accuracy = Mathf.Max(Accuracy, other.Accuracy),
            HandFinesse = Mathf.Max(HandFinesse, other.HandFinesse)
        };
    }
}
