using System;
using UnityEngine;
using UnityEngine.Events;

public class TaikoUIMoveButton : MonoBehaviour, ITaikoUIMove
{
    TaikoUIMoveButtonList uiPausePanel;

    public UnityEvent buttonPressed = new();

    void Awake()
    {
        uiPausePanel = GetComponentInParent<TaikoUIMoveButtonList>();
    }

    public void OnTaikoUIMoveLeft()
    {
        uiPausePanel.OnTaikoUIMoveLeft();
    }
    public void OnTaikoUIMoveRight()
    {
        uiPausePanel.OnTaikoUIMoveRight();
    }

    public void OnTaikoUISelect()
    {
        buttonPressed?.Invoke();
    }
}
