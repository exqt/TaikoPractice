using UnityEngine.Events;
using SM = UnityEngine.SceneManagement.SceneManager;

public static class SceneUtil
{
    public static UnityEvent<string> LoadingScene = new();
    public static UnityEvent<string> SceneLoaded = new();

    public static void LoadScene(string sceneName)
    {
        SM.LoadScene(sceneName);
        LoadingScene.Invoke(sceneName);
    }

    public static void BackToMainScene()
    {
        LoadScene("MainScene");
    }

    public static void ReloadCurrentScene()
    {
        var currentScene = SM.GetActiveScene();
        if (currentScene.isLoaded)
        {
            LoadScene(currentScene.name);
            LoadingScene.Invoke(currentScene.name);
        }
    }
}
