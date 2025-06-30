using System;
using System.Runtime.InteropServices;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrumUI : MonoBehaviour
{
    public Image LD, LK, RD, RK;

    public float alphaRate = 4f;
    public float areaAlpha = 0.6f;

    float LDalpha, LKalpha, RDalpha, RKalpha;

    public TMP_Text comboText;
    RectTransform comboTextRect;

    public int showComboThreshold = 10;
    Tween tween;

    void Awake()
    {
        comboTextRect = comboText.GetComponent<RectTransform>();
    }


    void Start()
    {
        LDalpha = LKalpha = RDalpha = RKalpha = 0.0f;
        comboText.text = string.Empty;
    }

    public void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }


    public void Hit(TaikoKeyType key)
    {
        switch (key)
        {
            case TaikoKeyType.LEFT_DON:
                LDalpha = 1.0f;
                break;
            case TaikoKeyType.LEFT_KA:
                LKalpha = 1.0f;
                break;
            case TaikoKeyType.RIGHT_DON:
                RDalpha = 1.0f;
                break;
            case TaikoKeyType.RIGHT_KA:
                RKalpha = 1.0f;
                break;
        }
    }

    public void SetCombo(int combo)
    {
        comboText.text = combo >= showComboThreshold ? combo.ToString() : string.Empty;

        comboTextRect.localScale = new Vector3(1.0f, 1.1f, 1.0f);

        tween?.Kill();
        tween = DOTween.Sequence()
            .Append(comboTextRect.DOScaleY(1.0f, 0.2f).SetEase(Ease.InBack));
    }

    void Update()
    {
        LDalpha = Mathf.Max(0.0f, LDalpha - Time.deltaTime * alphaRate);
        LKalpha = Mathf.Max(0.0f, LKalpha - Time.deltaTime * alphaRate);
        RDalpha = Mathf.Max(0.0f, RDalpha - Time.deltaTime * alphaRate);
        RKalpha = Mathf.Max(0.0f, RKalpha - Time.deltaTime * alphaRate);

        LD.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(LDalpha, 2f));
        LK.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(LKalpha, 2f));
        RD.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(RDalpha, 2f));
        RK.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(RKalpha, 2f));
    }
}
