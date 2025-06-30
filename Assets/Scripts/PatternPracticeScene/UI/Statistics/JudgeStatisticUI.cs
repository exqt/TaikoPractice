using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JudgeStatisticUI : MonoBehaviour
{
    public TMP_Text goodText, okText, badText, avgText, stdText, accText, finesseText;

    JudgeStatistic judgeStatistic;

    public void SetData(JudgeStatistic judgeStatistic)
    {
        this.judgeStatistic = judgeStatistic;
    }

    void Update()
    {
        if (judgeStatistic == null) return;

        //     Good:   1000
        //       OK:   1000
        //      Bad:   1000
        // Accuracy:  99.0%
        // Avg Diff: 12.4ms
        // Std Diff: 45.1ms
        //  Finesse: 100.0%

        goodText.text = $"Good: {judgeStatistic.NGood,7}";
        okText.text = $"OK: {judgeStatistic.NOk,7}";
        badText.text = $"Bad: {judgeStatistic.NBad,7}";
        accText.text = $"Accuracy: {judgeStatistic.Accuracy * 100,6:F1}%";

        var sign = judgeStatistic.AverageDiff < 0 ? "-" : "+";
        var avgStr = $"{sign}{Math.Abs(judgeStatistic.AverageDiff * 1000):F1}";
        avgText.text = $"Avg Diff:{avgStr,6}ms";
        stdText.text = $"Std Diff: {judgeStatistic.StdDevDiff * 1000,5:F1}ms";
        finesseText.text = $"Finesse: {judgeStatistic.HandFinesse * 100,6:F1}%";
    }
}
