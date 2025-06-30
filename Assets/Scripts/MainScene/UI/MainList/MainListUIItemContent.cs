using System;
using UnityEngine;
using UnityEngine.Events;

public class UIMainListItemContent : MonoBehaviour
{
    [NonSerialized]
    public UIOptionGroup uiOptionGroup;

    UIMainListItemFoldable uIMainListItem;

    public GameObject[] subTaikoUIList;
    int currentSubTaikoUIIndex = 0;

    public void Awake()
    {
        uiOptionGroup = GetComponentInChildren<UIOptionGroup>(true);
        uIMainListItem = GetComponentInParent<UIMainListItemFoldable>();
    }

    public virtual void GenerateContent() { }

    void SubUIMove(int delta)
    {
        currentSubTaikoUIIndex += delta;
        currentSubTaikoUIIndex = Mathf.Clamp(currentSubTaikoUIIndex, 0, subTaikoUIList.Length - 1);

        var ui = subTaikoUIList[currentSubTaikoUIIndex].GetComponent<ITaikoUIMove>();
        TaikoUIMoveManager.Instance.SetItem(ui);
    }

    public void SubUIMoveLeft() => SubUIMove(-1);
    public void SubUIMoveRight() => SubUIMove(1);

    public void FocusContent()
    {
        TaikoUIMoveManager.Instance.SetItem(subTaikoUIList[0].GetComponent<ITaikoUIMove>());
    }

    public void UnfocusContent()
    {
        TaikoUIMoveManager.Instance.SetItem(uIMainListItem.GetComponent<ITaikoUIMove>());
        uIMainListItem.Fold();
    }

    public void OnPressBack()
    {
        UnfocusContent();
    }
}
