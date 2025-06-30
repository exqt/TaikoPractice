using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq; // Add DOTween namespace

public class UIPopup : MonoBehaviour
{
    public Image background;
    public GameObject window;

    static readonly float animationDuration = 0.2f;
    static readonly Ease easeType = Ease.OutBack;
    static readonly float slideDistance = 900f;
    static readonly float backgroundAlpha = 0.7f;

    public void Awake()
    {
    }

    public void Show()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        window.transform.localPosition = new Vector3(0, -slideDistance, 0);

        background.DOFade(0, 0);
        background.DOFade(backgroundAlpha, animationDuration * 0.7f);

        window.transform
            .DOLocalMoveY(0, animationDuration)
            .SetEase(easeType);

        var list = window.GetComponentInChildren<TaikoUIMoveButtonList>();
        if (list) list.Focus();
    }

    public void Hide()
    {
        background.DOFade(0, animationDuration * 0.7f);
        window.transform.DOLocalMoveY(0 - slideDistance, animationDuration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                transform.localPosition = new Vector3(0, 3000, 0);
            });

        TaikoUIMoveManager.Instance.SetItem(null as GameObject);
    }
}
