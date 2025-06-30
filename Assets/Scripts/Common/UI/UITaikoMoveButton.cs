using UnityEngine;
using UnityEngine.Events;

public class UITaikoMoveButton : MonoBehaviour, ITaikoUIMove
{
    public UnityEvent movedLeft = new();
    public UnityEvent movedRight = new();
    public UnityEvent buttonPressed = new();

    public void OnTaikoUIMoveLeft() => movedLeft?.Invoke();
    public void OnTaikoUIMoveRight() => movedRight?.Invoke();
    public void OnTaikoUISelect() => buttonPressed?.Invoke();
}

