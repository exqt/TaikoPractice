using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using System; // Add DOTween namespace

public class TaikoUIMoveButtonList : MonoBehaviour
{
    readonly UnityEvent resumed = new();

    int currentButtonIndex = 0;
    GameObject[] taikoUIMoves;

    void Awake()
    {
        taikoUIMoves = GetComponentsInChildren<ITaikoUIMove>()
            .Select(x => (x as Component).gameObject)
            .ToArray();
    }

    public void Focus()
    {
        currentButtonIndex = 0;
        TaikoUIMoveManager.Instance.SetItem(taikoUIMoves[currentButtonIndex]);
    }

    void MoveUI(int delta)
    {
        currentButtonIndex = Math.Clamp(currentButtonIndex + delta, 0, taikoUIMoves.Length - 1);
        TaikoUIMoveManager.Instance.SetItem(taikoUIMoves[currentButtonIndex]);
    }

    public void OnTaikoUIMoveLeft()
    {
        MoveUI(-1);
    }
    public void OnTaikoUIMoveRight()
    {
        MoveUI(1);
    }

    public void OnResume()
    {
        gameObject.SetActive(false);
        resumed.Invoke();
    }

    public void OnGoToMainMenu()
    {
        SceneUtil.BackToMainScene();
    }

    public void OnQuitProgram()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnReloadScene()
    {
        SceneUtil.ReloadCurrentScene();
    }
}
