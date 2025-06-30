using System;
using UnityEngine;
using UnityEngine.UI;

public class JudgeStatisticBarUI : MonoBehaviour
{
    public static float MS_INTERVAL = 5f; // 각 구간의 ms 간격
    public static int BAR_COUNT = 40;
    public static float BAR_WIDTH = 12f; // 각 막대의 너비

    private Image[] bars;
    int[] judgmentCounts = new int[BAR_COUNT]; // 각 구간별 판정 수
    private int maxCount = 0; // 최대 판정 수 (높이 정규화용)

    public Transform barContainer;

    public Color goodColor;
    public Color okColor;
    public Color badColor;

    void Awake()
    {
        bars = barContainer.GetComponentsInChildren<Image>();
        InitializeCounts();
    }

    void InitializeCounts()
    {
        for (int i = 0; i < BAR_COUNT; i++) judgmentCounts[i] = 0;
        maxCount = 0;
    }

    public void Reset()
    {
        InitializeCounts();
        UpdateBars();
    }

    public void SetData(JudgeStatistic judgeStatistic)
    {
        Reset();
        foreach (var judge in judgeStatistic.judges)
        {
            AddJudgment(judge);
        }
        UpdateBars();
    }

    void AddJudgment(SongPlay.NoteJudge judge)
    {
        if (judge.judge == JudgeType.None) return;

        int sign = judge.diff < 0 ? -1 : 1;
        int val = Mathf.FloorToInt((float)Math.Abs(judge.diff) * 1000 / MS_INTERVAL);

        int index;
        if (sign >= 0) index = BAR_COUNT / 2 + val; // 양수 판정
        else index = BAR_COUNT / 2 - val; // 음수 판정

        if (!(0 <= index && index < BAR_COUNT)) return;

        if (index == 0) return; // 거의 놓친 경우라 무시
        judgmentCounts[index]++;

        // 최대 판정 수 업데이트
        if (judgmentCounts[index] > maxCount) maxCount = judgmentCounts[index];
    }

    [ContextMenu("Update Color")]
    void UpdateColor()
    {
        bars = barContainer.GetComponentsInChildren<Image>();
        for (int i = 0; i < BAR_COUNT; i++)
        {
            int index = i - BAR_COUNT / 2;
            if (-5 <= index && index < 5) bars[i].color = goodColor;
            else if (-15 <= index && index < 15) bars[i].color = okColor;
            else bars[i].color = badColor;
        }
    }

    [ContextMenu("Update Bars")]
    void UpdateBars()
    {
        var barMaxHeight = GetComponent<RectTransform>().rect.height - 10f;
        bars = barContainer.GetComponentsInChildren<Image>();
        for (int i = 0; i < BAR_COUNT; i++)
        {
            int count = judgmentCounts[i];

            // 높이 설정 (최대 판정 수에 따라 정규화)
            float height = maxCount > 0 ? (float)count / maxCount * barMaxHeight : 0;
            bars[i].rectTransform.sizeDelta =
                new Vector2(bars[i].rectTransform.sizeDelta.x, height);
        }
    }
}
