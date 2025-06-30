using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMainListItemFoldable : UIMainListItem, ITaikoUIMove
{
    public static float closedHeight = 150;
    public static float openHeight = 800;

    public GameObject contentPlaceholder;
    public GameObject contentPrefab;
    public GameObject content;

    RectTransform rectTransform;

    public bool IsFolded { get; private set; } = true;

    public override float GetHeight()
    {
        return IsFolded ? closedHeight : openHeight;
    }

    public UIMainListItemContent GetContent()
    {
        // find if there a object in contentPlaceholder
        if (content != null)
        {
            return content.GetComponent<UIMainListItemContent>();
        }

        if (content == null)
        {
            var c = contentPlaceholder.GetComponentInChildren<UIMainListItemContent>();

            if (c != null)
            {
                content = c.gameObject;
                return c;
            }
        }

        if (content == null && contentPrefab != null)
        {
            content = Instantiate(contentPrefab, contentPlaceholder.transform);
            content.SetActive(true);
            return content.GetComponent<UIMainListItemContent>();
        }

        return null;
    }

    public override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        Fold(immediate: true);
    }

    public virtual void Fold(bool immediate = false)
    {
        IsFolded = true;
        if (immediate)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, closedHeight);
        }
        else
        {
            rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, closedHeight), UIMainList.tweenTime)
                .OnComplete(() =>
                {
                });
            uiMainList.MoveItemCursor(this);
        }
    }

    public virtual void Unfold(bool immediate = false)
    {
        if (immediate)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, openHeight);
        }
        else
        {
            rectTransform.DOSizeDelta(new Vector2(rectTransform.sizeDelta.x, openHeight), UIMainList.tweenTime)
                .OnComplete(() => {  });
        }

        GetContent();

        // content.GetComponent<UIMainListItemContent>().FocusContent();
        if (content != null && content.TryGetComponent<UIMainListItemContent>(out var contentComponent) == true)
        {
            contentComponent.FocusContent();
        }
        else
        {
            Debug.LogWarning("Content component not found in UIMainListItemFoldable.");
        }

        IsFolded = false;
    }

    public override void OnTaikoUISelect()
    {
        if (IsFolded)
        {
            Unfold();
            uiMainList.OpenItem(this);
        }
        else
        {
            Fold();
        }
    }

#region Editor
    [ContextMenu("FoldOnEditor")]
    public void FoldOnEditor()
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, closedHeight);
    }
    [ContextMenu("UnfoldOnEditor")]
    public void UnfoldOnEditor()
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, openHeight);
    }
    #endregion
}
