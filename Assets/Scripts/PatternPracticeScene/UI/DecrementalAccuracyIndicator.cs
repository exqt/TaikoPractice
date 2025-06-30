using UnityEngine;
using UnityEngine.UI;

public class DecrementalAccuracyIndicator: MonoBehaviour
{
    public Image totalTime;
    public Image currentTime;

    float totalWidth;

    public PatternPracticeScene scene;

    float targetAccuracy;

    void Start()
    {
        totalWidth = totalTime.GetComponent<RectTransform>().rect.width;
        targetAccuracy = PatternPracticeSceneContext.Instance
            .patternPracticeOptionGroup?.targetAccuracy ?? 0.0f;
        if (targetAccuracy == 0f) gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        var t = targetAccuracy == 1 ? 1 : (scene.Statistic.DecrementalAccuracy - targetAccuracy) / (1.0f - targetAccuracy);
        var rect = currentTime.GetComponent<RectTransform>().rect;
        currentTime.GetComponent<RectTransform>().sizeDelta = new Vector2(totalWidth * t, rect.height);
    }

    void Update()
    {
        if (scene.StateType == PatternPracticeScene.GameStateType.Playing)
        {
            UpdateUI();
        }
    }
}
