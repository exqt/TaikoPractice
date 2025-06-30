using System;
using UnityEngine;
using UnityEngine.UI;

public class JudgeStatisticRecentUI : MonoBehaviour
{
    public GameObject judgeDotPrefab;
    public RectTransform dotContainer;
    public GameObject avgPtr;

    public Image goodLine, okLine, badLine;

    Color goodColor, okColor, badColor;

    public int maxDots = 16;

    double sumMs = 0;
    int nextIndex;
    double[] diffs;
    float scale = 400 / 50 * 1000;
    int count = 0;

    void Awake()
    {
        while (dotContainer.childCount < maxDots)
        {
            Instantiate(judgeDotPrefab, dotContainer).SetActive(false);
        }

        diffs = new double[maxDots];

        goodColor = goodLine.color;
        okColor = okLine.color;
        badColor = badLine.color;
    }

    public void AddJudge(SongPlay.NoteJudge judge)
    {
        var diff = judge.diff;
        sumMs -= diffs[nextIndex];
        diffs[nextIndex] = diff;
        sumMs += diff;

        count++;
        count = Math.Min(count, maxDots);

        var dot = dotContainer.GetChild(nextIndex);
        dot.gameObject.SetActive(true);
        var image = dot.GetComponent<Image>();

        if (Math.Abs(diff) <= Consts.GOOD_TIME) image.color = goodColor;
        else if (Math.Abs(diff) <= Consts.OK_TIME) image.color = okColor;
        else image.color = badColor;

        dot.localPosition = new Vector3((float)diff * scale, 36, 0);

        double avg = sumMs / count;
        avgPtr.transform.localPosition = new Vector3((float)avg * scale, -41, 0);

        nextIndex = (nextIndex + 1) % maxDots;
    }
}
