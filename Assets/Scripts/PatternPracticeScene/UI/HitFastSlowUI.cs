using System;
using System.Diagnostics.Eventing.Reader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HitFastSlowUI : MonoBehaviour
{
    public GameObject slow;
    public GameObject fast;

    float recentTime = 0f;

    static float showTime = 0.5f;

    void Awake()
    {
        fast.SetActive(false);
        slow.SetActive(false);
    }

    void ShowFastSlow(double diff)
    {
        float cutOff = 0.025f;
        double adiff = Math.Abs(diff);

        if (adiff > cutOff)
        {
            slow.SetActive(diff < 0);
            fast.SetActive(diff > 0);

            var text = (diff < 0 ? slow : fast).GetComponent<TMP_Text>();
            var sign = diff < 0 ? "-" : "+";
            text.text = $"{sign}{adiff * 1000:F0}ms";

            recentTime = Time.time;
        }
    }

    public void Play(SongPlay.NoteJudge noteJudge)
    {
        if (noteJudge.judge == JudgeType.None) return;
        if (noteJudge.judge == JudgeType.Bad) return;

        ShowFastSlow(noteJudge.diff);
    }

    void Update()
    {
        if (Time.time - recentTime > showTime)
        {
            slow.SetActive(false);
            fast.SetActive(false);
        }
    }
}
