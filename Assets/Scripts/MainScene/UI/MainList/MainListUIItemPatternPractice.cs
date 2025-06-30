using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMainListItemPatternPractice : UIMainListItemFoldable
{
    FumenPatternMap fumenPatternMap;
    new void Awake()
    {
        base.Awake();
    }

    public void SetData(FumenPatternMap fumenPatternMap)
    {
        this.fumenPatternMap = fumenPatternMap;
        var score = ScoreManager.Instance.GetScore(fumenPatternMap);
        if (score == null)
        {
            sub.text = "";
        }
        else
        {
            var acc = score.Accuracy * 100.0f;
            var fin = score.HandFinesse * 100.0f;
            sub.text = $"Accuracy: {acc:F2}%  / Finesse: {fin:F2}%";
        }
    }

    public override void Fold(bool immediate = false)
    {
        base.Fold(immediate);
        Destroy(content);
    }

    public override void Unfold(bool immediate = false)
    {
        base.Unfold(immediate);
        var comp = content.GetComponent<UIMainListItemContentPatternPractice>();
        comp.SetData(fumenPatternMap);
        comp.GenerateContent();
    }
}
