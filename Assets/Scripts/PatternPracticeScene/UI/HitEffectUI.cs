using System.Diagnostics.Eventing.Reader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HitEffectUI : MonoBehaviour
{
    Image hitCircle;
    Animator effectAnimator;

    float circleAlpha = 0f;
    public float alphaRate = 5f;

    public Color GoodColor = new(0.898f, 0.615f, 0.043f, 1.0f);
    public Color OkColor = new(0.757f, 0.757f, 0.757f, 1.0f);

    void Awake()
    {
        hitCircle = transform.Find("Circle").GetComponent<Image>();
        effectAnimator = transform.Find("Explosion").GetComponent<Animator>();

        hitCircle.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    public void Play(SongPlay.NoteJudge noteJudge)
    {
        if (noteJudge.judge == JudgeType.None) return;
        if (noteJudge.judge == JudgeType.Bad) return;

        effectAnimator.StopPlayback();
        circleAlpha = 1.0f;

        switch (noteJudge.judge)
        {
            case JudgeType.Good:
                effectAnimator.Play("HitExplosionGood", -1, 0);
                effectAnimator.speed = 0.5f;
                hitCircle.color = GoodColor;
                break;

            case JudgeType.Ok:
                effectAnimator.Play("HitExplosionOk", -1, 0);
                effectAnimator.speed = 0.5f;
                hitCircle.color = OkColor;
                break;
        }
    }

    void Update()
    {
        circleAlpha = Mathf.Max(0.0f, circleAlpha - Time.deltaTime * alphaRate);
        if (circleAlpha > 0.0f)
        {
            var c = hitCircle.color;
            hitCircle.color = new Color(c.r, c.g, c.b, circleAlpha);
        }
    }
}
