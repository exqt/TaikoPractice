using TMPro;
using UnityEngine;

public class FrameIndicatorUI : MonoBehaviour
{
    TMP_Text mainFPS;
    TMP_Text backFPS;

    void Awake()
    {
        mainFPS = transform.Find("Main").GetComponent<TMP_Text>();
        backFPS = transform.Find("Back").GetComponent<TMP_Text>();
    }

    public void SetData(int main, int back)
    {
        mainFPS.text = $"Main : {main,4}";
        backFPS.text = $"Back : {back,4}";
    }
}
