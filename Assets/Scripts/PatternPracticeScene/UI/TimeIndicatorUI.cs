using UnityEngine;
using UnityEngine.UI;

public class TimeIndicator : MonoBehaviour
{
    public Image totalTime;
    public Image currentTime;

    float totalWidth;
    double duration;

    public PatternPracticeScene scene;

    void Start()
    {
        duration = scene.GetSongDuration();
        totalWidth = totalTime.GetComponent<RectTransform>().rect.width;
    }

    void Update()
    {
        if (scene.StateType == PatternPracticeScene.GameStateType.Playing)
        {
            double current = scene.CurrentTime;
            var t = (float)current / (float)duration;
            t = Mathf.Clamp01(t);
            var rect = currentTime.GetComponent<RectTransform>().rect;
            currentTime.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth * t, rect.height);
        }
    }
}
