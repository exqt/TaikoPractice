using UnityEngine;
using UnityEngine.Events;

public interface ITaikoUIMove
{
    void OnTaikoUIMoveEnter() {}
    void OnTaikoUIMoveLeave() {}
    void OnTaikoUIMoveLeft();
    void OnTaikoUIMoveRight();
    void OnTaikoUISelect();
}
