using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainSceneContext : SceneContext<MainSceneContext>
{
    public UIMainList.State state;
}

public class MainScene : MonoBehaviour
{
    public UIMainList mainList;
    public UIOptionGroup recordOptionGroup;
    public InputActionAsset inputActionAsset;
    public GameObject keepHoldingEscPanel;

    InputAction escapeAction, reloadAction;
    float escapeHoldStartTime = -1f;
    readonly float escapeHoldDuration = 1f;

    public UIOptionGroup playOptionGroupUI;
    public UIOptionGroup systemOptionGroupUI;

#if UNITY_EDITOR || UNITY_STANDALONE
    FileSystemWatcher patternMapWatcher;
#endif

    #region Unity Events
    void Awake()
    {
        SceneUtil.LoadingScene.AddListener(OnLoadScene);
        escapeAction = inputActionAsset.FindAction("Global/Escape");

#if UNITY_EDITOR || UNITY_STANDALONE
        var dir = Path.Combine(Application.dataPath, "../", Paths.patternMapsDirectory);
        dir = Path.GetFullPath(dir);
        if (Directory.Exists(dir))
        {
            patternMapWatcher = new FileSystemWatcher(dir);
            patternMapWatcher.IncludeSubdirectories = true;
            patternMapWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            patternMapWatcher.Changed += OnPatternMapChanged;
            patternMapWatcher.Created += OnPatternMapChanged;
            patternMapWatcher.Deleted += OnPatternMapChanged;
            patternMapWatcher.Renamed += OnPatternMapChanged;
            patternMapWatcher.EnableRaisingEvents = true;
        }
#endif
    }

    void Start()
    {
        var state = MainSceneContext.Instance.state;
        if (state != null) mainList.RecoverState(state);
    }

    void OnDestroy()
    {
        SceneUtil.LoadingScene.RemoveListener(OnLoadScene);

#if UNITY_EDITOR || UNITY_STANDALONE
        if (patternMapWatcher != null)
        {
            patternMapWatcher.EnableRaisingEvents = false;
            patternMapWatcher.Dispose();
            patternMapWatcher = null;
        }
#endif
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (patternMapChangedPending)
        {
            patternMapChangedPending = false;
            ReloadPatternMapDir();
        }
#endif

        if (escapeAction.IsPressed())
        {
            if (escapeHoldStartTime < 0) escapeHoldStartTime = Time.time;

            if (Time.time - escapeHoldStartTime >= escapeHoldDuration)
            {
                Debug.Log("Escape held for 2 seconds, quitting application");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
        else if (escapeHoldStartTime >= 0) escapeHoldStartTime = -1f;

        keepHoldingEscPanel.SetActive(escapeHoldStartTime >= 0);
        if (keepHoldingEscPanel.activeSelf)
        {
            var canvasGroup = keepHoldingEscPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                var alpha = (Time.time - escapeHoldStartTime) * 2f;
                canvasGroup.alpha = Mathf.Clamp01(alpha / escapeHoldDuration);
            }
        }
    }
    #endregion

    void OnLoadScene(string sceneName)
    {
        var state = mainList.GetState();
        MainSceneContext.Instance.state = state;
    }

    public void OnPressRecordPlay()
    {
        RecordSceneContext.Instance.optionGroup =
            recordOptionGroup.GetOptionGroup() as RecordingOptionGroup;
        RecordSceneContext.Instance.songPlay = null;
        SceneUtil.LoadScene("RecordScene");
    }

    public void ReloadPatternMapDir()
    {
        Debug.Log("Reloading scene...");
        MainSceneContext.Instance.state = mainList.GetState();
        SceneUtil.ReloadCurrentScene();
    }

    public void OnSaveSetting()
    {
        PatternPracticeOptionGroup playOptionGroup =
            playOptionGroupUI.GetOptionGroup() as PatternPracticeOptionGroup;

        SystemOptionGroup systemOptionGroup =
            systemOptionGroupUI.GetOptionGroup() as SystemOptionGroup;

        AudioManager.Instance.ApplySetting(systemOptionGroup);

        playOptionGroup.Save();
        systemOptionGroup.Save();
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    bool patternMapChangedPending = false;
    void OnPatternMapChanged(object sender, FileSystemEventArgs e)
    {
        patternMapChangedPending = true;
    }
#endif
}
