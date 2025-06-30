using UnityEngine;
using UnityEngine.Events;

public class UIMainListItemButton : MonoBehaviour, ITaikoUIMove
{
    UIMainListItemContent uIMainListItemContent;

    public UnityEvent buttonPressed = new();

    void Awake()
    {
        uIMainListItemContent = GetComponentInParent<UIMainListItemContent>();
    }

    public void OnTaikoUIMoveLeft()
    {
        uIMainListItemContent.SubUIMoveLeft();
    }
    public void OnTaikoUIMoveRight()
    {
        uIMainListItemContent.SubUIMoveRight();
    }

    public void OnTaikoUISelect()
    {
        buttonPressed?.Invoke();
    }
}
