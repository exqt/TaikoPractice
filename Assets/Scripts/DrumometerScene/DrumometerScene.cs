using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrumometerSceneContext : SceneContext<DrumometerSceneContext>
{
    public int duration = 5; // 측정 시간(초)
}

public partial class DrumometerScene : MonoBehaviour
{
    // === Inspector references ===
    public TMP_Text timeText;  // UI에 연결
    public TMP_Text currentBPMText;
    public TMP_Text targetCountText;
    public GameObject resultPanel;

    // === State and game logic ===
    public enum GameStateType { BeforeStart, Running, Finished, WaitingToAction }
    DrumometerSceneState currentState;
    Dictionary<GameStateType, DrumometerSceneState> stateMap;

    // === Input and audio ===
    InputThread inputThread;
    public InputActionAsset inputActionAsset;

    public DrumUI drumUI;
    public FrameIndicatorUI frameIndicatorUI;

    // === Gameplay data ===
    int hitCount = 0;
    double startTime = 0;
    HandType lastHand = HandType.None;
    double finishedMenuShowTime = 2f;

    // === Properties ===
    public double Duration { get; set; }
    public double BestCount { get; set; }
    public double StartTime => startTime;
    public double FinishedMenuShowTime => finishedMenuShowTime;

    public struct DrumometerHit
    {
        public TaikoKeyType Key;
        public double Time;
        public HandType Hand;

        public DrumometerHit(TaikoKeyType key, double time, HandType hand)
        {
            Key = key;
            Time = time;
            Hand = hand;
        }
    }

    readonly ConcurrentQueue<DrumometerHit> hitQueue = new();
    public List<DrumometerHit> hitHistory = new(); // Add to store all hits for graph

    void Awake()
    {
        stateMap = new()
        {
            { GameStateType.BeforeStart, new BeforeStartState(this) },
            { GameStateType.Running, new RunningState(this, inputActionAsset) },
            { GameStateType.Finished, new FinishedState(this) },
        };
        Duration = DrumometerSceneContext.Instance.duration;
        StartCoroutine(AwakeCoroutine());

        BestCount = DrumometerBest.GetBestCount(DrumometerSceneContext.Instance.duration);
    }

    void Start()
    {
        ChangeState(GameStateType.BeforeStart);
    }

    IEnumerator AwakeCoroutine()
    {
        yield return new WaitForSeconds(1);
        inputThread = new InputThread();
        inputThread.OnKeyPressed += OnKeyInput;
    }

    void OnDestroy()
    {
        inputThread?.Stop();
    }

    public void ChangeState(GameStateType newStateType)
    {
        var newState = stateMap[newStateType];
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    void OnKeyInput(TaikoKeyType key)
    {
        currentState.HandleKeyInput(key);
    }

    void Update()
    {
        currentState.Update();
        frameIndicatorUI.SetData(
            (int)(Time.deltaTime * 1000), inputThread?.FPS ?? 0);
        ProcessQueue();
    }

    public void InitUI()
    {
        var remain = Duration;
        timeText.text = $"Time: {remain:0.00}";
        targetCountText.text = $"Best: {"0000"} / {BestCount.ToString().PadLeft(4, '0')}";

        var _currentUserSpeedAsBPM = string.Format("{0:0.00}", 0).PadLeft(7, ' ');
        currentBPMText.text = $"BPM   : {_currentUserSpeedAsBPM}";

        drumUI.SetCombo(hitCount);
    }

    public void UpdateUI()
    {
        var elapsed = TimeUtil.Time - startTime;
        var userSpeed = hitCount > 0 && elapsed > 0 ? hitCount / elapsed * 15.0 : 0;
        var remain = Math.Max(0, Duration - elapsed);
        var targetCount = (int)(elapsed / Duration * BestCount);

        timeText.text = $"Time: {remain:0.00}";
        targetCountText.text = $"Best: {targetCount.ToString().PadLeft(4, '0')} / {BestCount.ToString().PadLeft(4, '0')}";

        var _currentUserSpeedAsBPM = string.Format("{0:0.00}", userSpeed).PadLeft(7, ' ');
        currentBPMText.text = $"BPM   : {_currentUserSpeedAsBPM}";

        drumUI.SetCombo(hitCount);
    }

    public void HandleHit(TaikoKeyType key)
    {
        var elapsed = TimeUtil.Time - startTime;
        if (elapsed < 0 || elapsed > Duration) return; // Ignore hits outside the duration

        var audioManager = AudioManager.Instance;
        var hand = TypeConverter.ToHandType(key);

        if (lastHand == hand) return;

        lastHand = hand;
        hitCount++;

        var noteType = TypeConverter.ToNoteType(key);
        if (noteType == NoteType.Don) audioManager.PlaySound("dong");
        else if (noteType == NoteType.Ka) audioManager.PlaySound("ka");

        var hit = new DrumometerHit(key, TimeUtil.Time, hand);
        hitQueue.Enqueue(hit);
        hitHistory.Add(hit);
    }

    public float[] CalculateBPMGraph(float windowSec = 0.5f)
    {
        var hitTimes = new List<double>();
        foreach (var hit in hitHistory) hitTimes.Add(hit.Time - startTime);
        if (hitTimes.Count == 0) return new float[1] { 0 };
        var totalTime = Duration;
        int n = Mathf.CeilToInt((float)totalTime / windowSec);
        var bpmArr = new float[n];
        int hitIdx = 0;
        int cumulativeHits = 0;
        for (int i = 0; i < n; ++i)
        {
            double t = (i + 1) * windowSec;
            while (hitIdx < hitTimes.Count && hitTimes[hitIdx] < t)
            {
                cumulativeHits++;
                hitIdx++;
            }
            bpmArr[i] = (float)(cumulativeHits * 60.0 / t) / 4;
        }
        return bpmArr;
    }

    void ProcessQueue()
    {
        while (hitQueue.TryDequeue(out var hit))
        {
            var elapsed = hit.Time - startTime;
            if (elapsed < 0) continue; // Ignore hits before start time

            // Update UI
            drumUI.Hit(hit.Key);
        }
    }
}
