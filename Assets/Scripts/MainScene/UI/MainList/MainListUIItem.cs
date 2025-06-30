using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMainListItem : MonoBehaviour, ITaikoUIMove
{
    public TMP_Text title;
    public TMP_Text sub;

    protected UIMainList uiMainList;

    public virtual void Awake()
    {
        uiMainList = GetComponentInParent<UIMainList>();
    }

    public virtual float GetHeight()
    {
        return 150;
    }

    public void OnTaikoUIMoveLeft()
    {
        uiMainList.OnTaikoUIMoveLeft();
    }

    public void OnTaikoUIMoveRight()
    {
        uiMainList.OnTaikoUIMoveRight();
    }

    public virtual void OnTaikoUISelect()
    {
        uiMainList.OpenItem(this);
    }
}
