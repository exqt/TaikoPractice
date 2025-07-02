using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class UIMainListItemContentPatternPractice : UIMainListItemContent
{
    public UISongPreviewNotes fumenPreview;

#region UI
    public TMP_Text desc;
#endregion

    FumenPatternMap fumenPatternMap;

    public void SetData(FumenPatternMap map)
    {
        fumenPatternMap = map;
    }

    public override void GenerateContent()
    {
        fumenPreview.CreateNotes(new PatternLanguage(fumenPatternMap.Patterns));

        desc.text = fumenPatternMap.Description;

        if (fumenPatternMap?.options?.bpm != null)
            uiOptionGroup.options[0].SetValue(fumenPatternMap.options.bpm);
        if (fumenPatternMap?.options?.speed != null)
            uiOptionGroup.options[1].SetValue(fumenPatternMap.options.speed);
        if (fumenPatternMap?.options?.minimumNotes != null)
            uiOptionGroup.options[2].SetValue(fumenPatternMap.options.minimumNotes);
        if (fumenPatternMap?.options?.targetAccuracy != null)
            uiOptionGroup.options[3].SetValue(fumenPatternMap.options.targetAccuracy);
        if (fumenPatternMap?.options?.badFail != null)
            uiOptionGroup.options[4].SetValue(fumenPatternMap.options.badFail);
        if (fumenPatternMap?.options?.onFail != null)
            uiOptionGroup.options[5].SetValue(fumenPatternMap.options.onFail);
        if (fumenPatternMap?.options?.detarame != null)
            uiOptionGroup.options[6].SetValue(fumenPatternMap.options.detarame);
        if (fumenPatternMap?.options?.patternShuffle != null)
            uiOptionGroup.options[7].SetValue(fumenPatternMap.options.patternShuffle);
    }

    public void OnPressPlay()
    {
        PatternPracticeSceneContext.Instance.fumenPatternMap = fumenPatternMap;
        PatternPracticeSceneContext.Instance.patternPracticeOptionGroup =
            (PatternPracticeOptionGroup)uiOptionGroup.GetOptionGroup();
        PatternPracticeSceneContext.Instance.songBeginTime = 0;
        PatternPracticeSceneContext.Instance.seed = Time.frameCount;

        SceneUtil.LoadScene("PatternPracticeScene");
    }
}
