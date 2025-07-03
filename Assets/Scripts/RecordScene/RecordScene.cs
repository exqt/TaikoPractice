using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class RecordSceneContext : SceneContext<RecordSceneContext>
{
    public double bpm = 120;
    public SongPlay songPlay;
}

public partial class RecordScene : MonoBehaviour
{
    public enum GameState
    {
        None,
        WaitingToStart,
        Playing,
        Paused,
        Moving,
        Finished
    }

    GameState gameState = GameState.None;

    #region FMOD

    FMODMetronome metronome;
    #endregion

    public PlayRecordLaneContainer laneContainer;

    #region UI
    public GameObject uiPausePanel;
    public FrameIndicatorUI frameIndicatorUI;
    #endregion
    readonly float offset = 0.000f;

    InputThread inputThread;

    #region  Input
    public InputActionAsset inputActionAsset;
    #endregion


    double startTime = 0;

    void Awake()
    {
        inputThread = new InputThread();
        inputThread.OnKeyPressed += OnKeyInput;

        stateMap = new(
            new System.Collections.Generic.Dictionary<GameState, RecordSceneState>
            {
                { GameState.WaitingToStart, new WaitingToStartState(this, inputActionAsset) },
                { GameState.Playing, new PlayingState(this, inputActionAsset) },
                { GameState.Paused, new PausedState(this, inputActionAsset) },
                { GameState.Moving, new MovingState(this, inputActionAsset) },
                { GameState.Finished, new FinishedState(this, inputActionAsset) }
            }
        );

    }

    public void MakeNotesFromSongPlay(SongPlay songPlay)
    {
        var notes = songPlay.fumen.notes;
        laneContainer.bpm = songPlay.bpm;
        foreach (var judge in songPlay.GetJudges())
        {
            if (judge.judge == JudgeType.None) continue;
            var note = notes[judge.index];
            note.time += judge.diff;
            laneContainer.AddNote(note);
        }
    }

    void OnDestroy()
    {
        RecordSceneContext.Instance.songPlay = null;
        // updateThread.Stop();
        // inputThread.Stop();
    }

    ConcurrentQueue<Fumen.Note> noteQueue = new();

    int noteIndex = 0;
    public void OnKeyInput(TaikoKeyType key)
    {
        if (gameState == GameState.WaitingToStart)
        {
            metronome.StartMetronome();
            ChangeState(GameState.Playing);
        }
        else if (gameState != GameState.Playing) return;

        var audioManager = AudioManager.Instance;

        var t = TimeUtil.Time - startTime;

        if (key == TaikoKeyType.LEFT_DON || key == TaikoKeyType.RIGHT_DON)
        {
            audioManager.PlaySound("dong");
            noteQueue.Enqueue(new Fumen.Note()
            {
                index = noteIndex++,
                time = t,
                type = NoteType.Don
            });
        }

        if (key == TaikoKeyType.LEFT_KA || key == TaikoKeyType.RIGHT_KA)
        {
            audioManager.PlaySound("ka");
            noteQueue.Enqueue(new Fumen.Note()
            {
                index = noteIndex++,
                time = t,
                type = NoteType.Ka
            });
        }
    }

    void Start()
    {
        var context = RecordSceneContext.Instance;

        var bpm = context?.bpm ?? 120;

        laneContainer.bpm = bpm;

        var systemOption = SystemOptionGroup.Load();
        var metronomeOffset = systemOption.metronomeOffset;
        Debug.Log($"Metronome offset: {metronomeOffset}");
        metronome = new FMODMetronome((float)bpm, metronomeOffset);

        if (RecordSceneContext.Instance.songPlay == null)
        {
            ChangeState(GameState.WaitingToStart);
        }
        else
        {
            MakeNotesFromSongPlay(RecordSceneContext.Instance.songPlay);
            ChangeState(GameState.Moving);
        }
    }

    RecordSceneState currentState;
    System.Collections.ObjectModel.ReadOnlyDictionary<GameState, RecordSceneState> stateMap;

    public void ChangeState(GameState newState)
    {
        gameState = newState;
        var newStateObj = stateMap.ContainsKey(newState) ? stateMap[newState] : null;
        currentState?.Exit();
        currentState = newStateObj;
        currentState?.Enter();
    }

    void Update()
    {
        currentState?.Update();
        while (noteQueue.Count > 0)
        {
            if (noteQueue.TryDequeue(out var note)) laneContainer.AddNote(note);
        }
        frameIndicatorUI.SetData((int)(Time.deltaTime * 1000), inputThread.FPS);
    }

    public void OnSelectResume()
    {
        ChangeState(GameState.Playing);
    }
}
