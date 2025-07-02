using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class PatternPracticeSceneContext : SceneContext<PatternPracticeSceneContext>
{
    public FumenPatternMap fumenPatternMap;
    public PatternPracticeOptionGroup patternPracticeOptionGroup;

    public int seed = 0;
    public double songBeginTime;
}

public partial class PatternPracticeScene : MonoBehaviour
{
    public enum GameStateType
    {
        Playing,
        Paused,
        Finished,
        WaitingToStart
    }

    public GameStateType StateType { get; private set; } = GameStateType.Playing;

    PatternPracticeState currentState;

    SongPlay songPlay;

    public JudgeStatistic Statistic => songPlay?.judgeStatistic;

    public JudgeStatisticUI judgeStatisticUI;

    public HorizontalNoteLaneUI noteLane;

    public VerticalNoteLaneUI verticalNoteLaneGroup;

    #region Input
    public InputActionAsset inputActionAsset;
    #endregion

    #region FMOD
    FMODMetronome metronome;
    #endregion

    #region UI
    public FrameIndicatorUI frameIndicatorUI;
    public GameObject uiPausePanel;
    public GameObject resultPanelUI;
    public DecrementalAccuracyIndicator decrementalAccuracyIndicator;
    public JudgeStatisticRecentUI judgeStatisticRecent;
    public IconIndicatorUI iconIndicatorUI;
    #endregion

    InputThread inputThread;

    public AudioClip don, ka;
    float judgeOffsetMS = 0.0f;

    public double CurrentTime
    {
        get
        {
            // return StateType switch
            // {
            //     GameStateType.WaitingToStart => 0.0,
            //     GameStateType.Playing => TimeUtil.Time - startTime,
            //     GameStateType.Paused => TimeUtil.Time - startTime,
            //     GameStateType.Finished => 1.0,
            //     _ => 0.0
            // };
            return TimeUtil.Time - baseTime;
        }
    }

    AutoPlayer autoPlayer;

    readonly ConcurrentQueue<SongPlay.NoteJudge> judgeQueue = new();

    float targetAccuracy = 0.0f;
    OnFailAction onFail = OnFailAction.None;

    ReadOnlyDictionary<GameStateType, PatternPracticeState> stateMap;

    #region Unity Events
    void Awake()
    {
        stateMap = new ReadOnlyDictionary<GameStateType, PatternPracticeState>(new Dictionary<GameStateType, PatternPracticeState>
        {
            { GameStateType.WaitingToStart, new WaitingToStartState(this, inputActionAsset) },
            { GameStateType.Playing, new PlayingState(this, inputActionAsset) },
            { GameStateType.Paused, new PausedState(this, inputActionAsset) },
            { GameStateType.Finished, new FinishedState(this, inputActionAsset) }
        });

        ChangeState(GameStateType.WaitingToStart);

        inputThread = new InputThread();
        inputThread.OnKeyPressed += OnKeyInput;

        var context = PatternPracticeSceneContext.Instance;
        var map = context?.fumenPatternMap;
        var patterns = map?.Patterns;
        var option = context?.patternPracticeOptionGroup;

        PatternLanguage patternLanguage =
            patterns != null ? new PatternLanguage(patterns) : new PatternLanguage(new List<string> {
                "<1>dk",
                "<1>dk",
        });

        double bpm = option?.bpm ?? 120;
        var speed = option?.speed ?? 1f;
        var minimumNotes = option?.minimumNotes ?? 10;
        targetAccuracy = option?.targetAccuracy ?? 0.0f;
        onFail = option?.onFail ?? OnFailAction.None;
        var detarame = option?.detarame ?? false;

        int seed = context.seed;
        var (_notes, intervalStarts) = patternLanguage.GetNotes(
            bpm, minimumNotes, 4, detarame,
            patternShuffle: option?.patternShuffle ?? PatternShuffle.None,
            seed: seed
        );

        //
        var notes = new List<Fumen.Note>();
        var beginTime = context.songBeginTime;

        foreach (var note in _notes)
        {
            if (note.time < beginTime - 0.01) continue;
            notes.Add(note);
        }

        var fumen = new Fumen(map?.Name, notes, intervalStarts);
        songPlay = new SongPlay(fumen, bpm, option);

        judgeStatisticUI.SetData(songPlay.judgeStatistic);

        noteLane.Setup(fumen, bpm, speed);
        noteLane.songPlay = songPlay;
        verticalNoteLaneGroup.Setup(songPlay, fumen, bpm, speed);

        var systemOption = SystemOptionGroup.Load();
        var metronomeOffset = systemOption.metronomeOffset;
        metronome = new FMODMetronome((float)bpm, metronomeOffset);
        judgeOffsetMS = systemOption.judgeOffset / 1000f;

        autoPlayer = new AutoPlayer(inputThread, songPlay, this);

        verticalNoteLaneGroup.gameObject.SetActive(false);
    }

    void Start()
    {
        if (PatternPracticeSceneContext.Instance.songBeginTime > 0)
        {
            songPlay.isCheated = true;
            iconIndicatorUI.SetNoCrownIcon(true);
        }
    }

    void Update()
    {
        currentState.Update();
        frameIndicatorUI.SetData((int)(Time.deltaTime * 1000), inputThread.FPS);
    }

    void OnDestroy()
    {
        inputThread.Stop();
        metronome.Pause();
    }
    #endregion

    public void OnKeyInput(TaikoKeyType key)
    {
        noteLane.OnKeyInput(key);
        verticalNoteLaneGroup.OnKeyInput(key);

        currentState.OnGameKeyInput(key);
    }

    // public void RegisterSceneEnterCallback(GameStateType stateType, UnityAction callback)
    // {
    //     if (stateMap.TryGetValue(stateType, out var state)) state.stateEntered += callback;
    //     else Debug.LogError($"State {stateType} not found in state map.");
    // }

    // public void UnregisterSceneEnterCallback(GameStateType stateType, UnityAction callback)
    // {
    //     if (stateMap.TryGetValue(stateType, out var state)) state.stateEntered -= callback;
    //     else Debug.LogError($"State {stateType} not found in state map.");
    // }

    // public void RegisterSceneExitCallback(GameStateType stateType, UnityAction callback)
    // {
    //     if (stateMap.TryGetValue(stateType, out var state)) state.stateExited += callback;
    //     else Debug.LogError($"State {stateType} not found in state map.");
    // }

    // public void UnregisterSceneExitCallback(GameStateType stateType, UnityAction callback)
    // {
    //     if (stateMap.TryGetValue(stateType, out var state)) state.stateExited -= callback;
    //     else Debug.LogError($"State {stateType} not found in state map.");
    // }

    public void ChangeState(GameStateType newStateType)
    {
        StateType = newStateType;
        var newState = stateMap[newStateType];

        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    double baseTime;
    public void SongStart()
    {
        var beatInterval = 60f / songPlay.bpm;
        var fourBeats = 4 * beatInterval;
        metronome.StartMetronome(CurrentTime % fourBeats);
        PatternPracticeSceneContext.Instance.songBeginTime = 0;
    }

    public void HitNote(TaikoKeyType key)
    {
        if (songPlay == null) return;

        songPlay.SetTime(CurrentTime);

        var audioManager = AudioManager.Instance;
        var noteJudge = songPlay.Hit(key, songPlay.CurrentTime - judgeOffsetMS);

        judgeQueue.Enqueue(noteJudge);

        var keyStr = key switch
        {
            TaikoKeyType.LEFT_DON or TaikoKeyType.RIGHT_DON => "dong",
            TaikoKeyType.LEFT_KA or TaikoKeyType.RIGHT_KA => "ka",
            _ => null
        };

        if (keyStr == null) return;

        audioManager.PlaySound(keyStr);
    }

    public void Resume()
    {
        ChangeState(GameStateType.Playing);
    }

    [ContextMenu("Reset UI Positions")]
    public void ResetUIPositions()
    {
        DraggablePositionManager.ResetAllPositions();
    }

    void HandleJudgeQueue()
    {
        while (judgeQueue.Count > 0)
        {
            if (judgeQueue.TryDequeue(out var judge))
            {
                if (judge.judge == JudgeType.None) return;

                // judgeStatisticBar.AddJudgment(judge);
                if (verticalNoteLaneGroup.gameObject.activeInHierarchy)
                    verticalNoteLaneGroup.HandleJudge(judge);

                if (noteLane.gameObject.activeInHierarchy)
                    noteLane.HandleJudge(judge);

                judgeStatisticRecent.AddJudge(judge);
            }
        }
    }

    public double GetSongDuration()
    {
        return songPlay.fumen.notes[^1].time;
    }

    public void ToggleAuto()
    {
        if (autoPlayer == null) return;
        autoPlayer.enabled = !autoPlayer.enabled;
        iconIndicatorUI.SetAutoIcon(autoPlayer.enabled);
        songPlay.isCheated |= autoPlayer.enabled;
        iconIndicatorUI.SetNoCrownIcon(songPlay.isCheated);
    }

    public void ToggleVerticalNoteLane()
    {
        verticalNoteLaneGroup.gameObject.SetActive(!verticalNoteLaneGroup.gameObject.activeSelf);
    }

    public void SetFail()
    {
        if (onFail == OnFailAction.None) return;
        iconIndicatorUI.SetNoCrownIcon(true);

        if (onFail == OnFailAction.Result)
        {
            ChangeState(GameStateType.Finished);
        }
        else if (onFail == OnFailAction.RestartMap)
        {
            PatternPracticeSceneContext.Instance.songBeginTime = 0;
            ChangeState(GameStateType.Finished);
            SceneUtil.ReloadCurrentScene();
        }
        else if (onFail == OnFailAction.RestartPattern ||
            onFail == OnFailAction.RestartBar)
        {
            SetRestartPoint();
            SceneUtil.ReloadCurrentScene();
        }
        else
        {
            Debug.LogError($"Unknown on fail action: {onFail}");
        }
    }

    public void DelayedFn(Action action, float delay)
    {
        static IEnumerator DelayedCoroutine(Action act, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            act();
        }

        StartCoroutine(DelayedCoroutine(action, delay));
    }

    public void SetRestartPoint()
    {
        var fumen = songPlay.fumen;
        var index = songPlay.LastJudge.index;
        if (index < 0) index = 0;
        var note = fumen.notes[index];
        var failedTime = note.time;

        if (onFail == OnFailAction.RestartBar)
        {
            // Restart at the beginning of the bar
            double beatInterval = 60f / songPlay.bpm;
            double barInterval = 4 * beatInterval; // 4 beats per bar
            double barStartTime = Math.Floor(failedTime / barInterval) * barInterval;
            PatternPracticeSceneContext.Instance.songBeginTime = barStartTime;
            return;
        }
        else if (onFail == OnFailAction.RestartPattern)
        {
            double restartIntervalBeginTime = fumen.intervalBeginTime[0];
            foreach (var beginTime in fumen.intervalBeginTime)
            {
                if (failedTime < beginTime) break;
                restartIntervalBeginTime = beginTime;
            }
            PatternPracticeSceneContext.Instance.songBeginTime = restartIntervalBeginTime;
        }
    }

    // public void RewindJudge()
    // {
    //     var fumen = songPlay.fumen;
    //     var cur = CurrentTime;
    //     double restartIntervalStart = fumen.intervalStarts[0];
    //     foreach (var intervalStart in fumen.intervalStarts)
    //     {
    //         if (cur < intervalStart) break;
    //         restartIntervalStart = intervalStart;
    //     }
    //     songPlay.ResetJudge(restartIntervalStart);

    //     Debug.Log(songPlay.judgeStatistic.DecrementalAccuracy);

    //     int openingBeats = 4;
    //     double beatInterval = 60f / songPlay.bpm;
    //     double openingTime = openingBeats * beatInterval;

    //     startTime = TimeUtil.Time - restartIntervalStart + openingTime;
    //     Debug.Log(CurrentTime);

    //     ChangeState(GameStateType.Paused);
    // }

    public void OnPressShowRecord()
    {
        RecordSceneContext.Instance.bpm = songPlay.bpm;
        RecordSceneContext.Instance.songPlay = songPlay;
        SceneUtil.LoadScene("RecordScene");
    }
}
