using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanelUI : MonoBehaviour
{
    public TMP_Text title;

    public Image resultCrown;
    public Sprite[] crownSprite;

    public JudgeStatisticBarUI judgeStatisticBar;
    public JudgeStatisticScatterGraphUI judgeStatisticGraph;
    public JudgeStatisticUI judgeStatisticUI;

    public void Awake()
    {

    }

    public void SetData(SongPlay songPlay)
    {
        title.text = songPlay.fumen.title;

        var judgeStatistic = songPlay.judgeStatistic;


        judgeStatisticBar.SetData(judgeStatistic);
        judgeStatisticGraph.SetPoints(judgeStatistic.judges);
        judgeStatisticUI.SetData(judgeStatistic);

        int crownIndex = (int)songPlay.CalculateCrown();
        if (crownIndex < 0 || crownIndex >= crownSprite.Length) resultCrown.color = Color.clear;
        else resultCrown.sprite = crownSprite[crownIndex];
    }
}
