using System;
using System.Collections.Generic;

public class JudgeStatistic
{
    public int totalNotes;

    #region Main Properties
    public int NGood { get; private set; } = 0;
    public int NOk { get; private set; } = 0;
    public int NBad { get; private set; } = 0;

    public double AverageDiff
    {
        get => validCount == 0 ? 0 : sumDiff / validCount;
    }

    public double StdDevDiff
    {
        get
        {
            if (judges.Count == 0) return 0;

            // Σ(x - μ)^2 = Σx^2 - 2μΣx + μ^2Σ1
            var numerator =
                sumSquareDiff
            - 2 * AverageDiff * sumDiff
            + AverageDiff * AverageDiff * validCount;

            var denominator = validCount;
            if (denominator == 0) return 0;

            return Math.Sqrt(numerator / denominator);
        }
    }

    public float Accuracy
    {
        get => validCount == 0 ? 0 : (2 * NGood + NOk) / (2f * validCount);
    }

    public float DecrementalAccuracy
    {
        get => 1f - (2 * NBad + NOk) / (2f * totalNotes);
    }

    public int HitCount
    {
        get => NGood + NOk + NBad;
    }

    public float HandFinesse
    {
        get => HitCount == 0 ? 0 : (float)handFinesseCount / HitCount;
    }

    public int Combo { get; private set; } = 0;
    #endregion

    public readonly HashSet<SongPlay.NoteJudge> judges = new();

    HandType lastHand = HandType.None;

    int handFinesseCount = 0;

    /// <summary>
    /// 지나친 노트를 제외한 친 노트의 개수
    /// </summary>
    int validCount = 0;

    double sumDiff = 0;
    double sumSquareDiff = 0;

    public void AddJudge(SongPlay.NoteJudge noteJudge)
    {
        if (noteJudge.judge == JudgeType.None) return;
        judges.Add(noteJudge);

        if (noteJudge.judge == JudgeType.Good) NGood++;
        else if (noteJudge.judge == JudgeType.Ok) NOk++;
        else if (noteJudge.judge == JudgeType.Bad) NBad++;

        if (noteJudge.judge == JudgeType.Bad) Combo = 0;
        else Combo++;

        if (lastHand == HandType.None || lastHand != noteJudge.hand) handFinesseCount++;
        lastHand = noteJudge.hand;

        if (noteJudge.diff != -Consts.BAD_TIME)
        {
            sumDiff += noteJudge.diff;
            sumSquareDiff += noteJudge.diff * noteJudge.diff;
            validCount++;
        }
    }

    public void UndoJudge(SongPlay.NoteJudge noteJudge)
    {
        judges.Remove(noteJudge);

        if (noteJudge.judge == JudgeType.Good) NGood--;
        else if (noteJudge.judge == JudgeType.Ok) NOk--;
        else if (noteJudge.judge == JudgeType.Bad) NBad--;

        Combo = 0;
        lastHand = HandType.None;

        if (noteJudge.diff != -Consts.BAD_TIME)
        {
            sumDiff -= noteJudge.diff;
            sumSquareDiff -= noteJudge.diff * noteJudge.diff;
            validCount--;
        }
    }

    public float GetPossibleBestTargetAccuracy()
    {
        var nPossibleGoods = this.NGood + totalNotes;
        int NoteCount = nPossibleGoods + NOk + NBad + totalNotes;
        if (NoteCount == 0) return 1f;
        return (2 * nPossibleGoods + NOk) / (2f * NoteCount);
    }
}
