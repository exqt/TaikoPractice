using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public partial class PatternPracticeScene
{
    public class WaitingToStartState : PatternPracticeState
    {
        public WaitingToStartState(PatternPracticeScene scene, InputActionAsset inputActionAsset) : base(scene, inputActionAsset)
        {
        }

        public override void Enter()
        {

            scene.DelayedFn(() => { scene.ChangeState(GameStateType.Playing); }, 1.0f);
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
            var context = PatternPracticeSceneContext.Instance;
            var beginTime = context.songBeginTime;
            int openingBeats = 4;
            double beatInterval = 60f / scene.songPlay.bpm;
            double openingTime = openingBeats * beatInterval;

            beginTime -= openingTime;
            beginTime = Math.Max(beginTime, 0.0);
            scene.baseTime = TimeUtil.Time - beginTime;
            scene.songPlay.SetTime(scene.CurrentTime);
            scene.SongStart();
        }
    }

    public class PlayingState : PatternPracticeState
    {
        readonly InputAction escapeAction, toggleAutoAction;
        readonly InputAction leftKaAction, rightKaAction, leftDonAction, rightDonAction;
        readonly InputAction toggleVerticalLaneAction, toggleHorizontalLaneAction;
        readonly InputAction abortAction;
        readonly ConsecutiveHitWatcher consecutiveHitWatcher;

        public PlayingState(PatternPracticeScene scene, InputActionAsset inputActionAsset) : base(scene, inputActionAsset)
        {
            var globalMap = inputActionAsset.FindActionMap("Global");
            escapeAction = globalMap.FindAction("Escape");

            var patternPracticeMap = inputActionAsset.FindActionMap("PatternPracticeScene");
            toggleAutoAction = patternPracticeMap.FindAction("ToggleAuto");
            toggleVerticalLaneAction = patternPracticeMap.FindAction("ToggleVerticalLane");
            toggleHorizontalLaneAction = patternPracticeMap.FindAction("ToggleHorizontalLane"); // ToggleHorizontalLane
            abortAction = patternPracticeMap.FindAction("Abort");

            var drumMap = inputActionAsset.FindActionMap("Drum");
            leftKaAction = drumMap.FindAction("LeftKa");
            rightKaAction = drumMap.FindAction("RightKa");
            leftDonAction = drumMap.FindAction("LeftDon");
            rightDonAction = drumMap.FindAction("RightDon");
            consecutiveHitWatcher = new(
                () => scene.ChangeState(GameStateType.Paused),
                TaikoKeyType.RIGHT_KA
            );
        }

        public override void Enter()
        {
            scene.noteLane.Play();
            scene.verticalNoteLaneGroup.Play();
        }

        public override void Exit()
        {
        }

        public override void OnGameKeyInput(TaikoKeyType key)
        {
            scene.HitNote(key);
        }

        public override void Update()
        {
            if (escapeAction.triggered) scene.ChangeState(GameStateType.Paused);
            if (toggleAutoAction.triggered) scene.ToggleAuto();

            if (leftKaAction.triggered) consecutiveHitWatcher.HandleHit(TaikoKeyType.LEFT_KA);
            if (rightKaAction.triggered) consecutiveHitWatcher.HandleHit(TaikoKeyType.RIGHT_KA);
            if (leftDonAction.triggered) consecutiveHitWatcher.HandleHit(TaikoKeyType.LEFT_DON);
            if (rightDonAction.triggered) consecutiveHitWatcher.HandleHit(TaikoKeyType.RIGHT_DON);

            if (toggleVerticalLaneAction.triggered)
            {
                scene.verticalNoteLaneGroup.gameObject.SetActive(!scene.verticalNoteLaneGroup.gameObject.activeSelf);
            }

            if (toggleHorizontalLaneAction.triggered)
            {
                scene.noteLane.gameObject.SetActive(!scene.noteLane.gameObject.activeSelf);
            }

            if (abortAction.triggered)
            {
                scene.SetFail();
                return;
            }

            if (scene.songPlay.judgeStatistic.NBad > 0 &&
                PatternPracticeSceneContext.Instance.patternPracticeOptionGroup.badFail == true)
            {
                scene.SetFail();
                return;
            }

            var acc = scene.songPlay.judgeStatistic.DecrementalAccuracy;
            if (acc < scene.targetAccuracy) scene.SetFail();

            scene.HandleJudgeQueue();
            scene.songPlay.SetTime(scene.CurrentTime);

            var fumen = scene.songPlay.fumen;
            if (fumen.notes[^1].time + 0.5f < scene.songPlay.CurrentTime)
            {
                scene.ChangeState(GameStateType.Finished);
                return;
            }
        }
    }

    public class PausedState : PatternPracticeState
    {
        readonly InputAction escapeAction;

        public double pauseEnterTime;

        public PausedState(
            PatternPracticeScene scene, InputActionAsset inputActionAsset
        ) : base(scene, inputActionAsset)
        {
            var globalMap = inputActionAsset.FindActionMap("Global");
            escapeAction = globalMap.FindAction("Escape");
        }

        public override void Enter()
        {
            scene.uiPausePanel.GetComponent<UIPopup>().Show();
            scene.metronome.Pause();
            pauseEnterTime = TimeUtil.Time;
        }

        public override void Update()
        {
            if (escapeAction.WasPressedThisFrame()) scene.Resume();
        }

        public override void Exit()
        {
            scene.baseTime += TimeUtil.Time - pauseEnterTime;
            scene.uiPausePanel.GetComponent<UIPopup>().Hide();
            scene.metronome.Resume();
        }
    }

    public class FinishedState : PatternPracticeState
    {
        public FinishedState(
            PatternPracticeScene scene, InputActionAsset inputActionAsset)
            : base(scene, inputActionAsset) {}

        public override void Enter()
        {
            scene.songPlay.GoToEnd();
            scene.metronome.Pause();
            scene.resultPanelUI.GetComponent<ResultPanelUI>().SetData(scene.songPlay);
            scene.resultPanelUI.GetComponent<UIPopup>().Show();
            scene.decrementalAccuracyIndicator.UpdateUI();
            TaikoUIMoveManager.Instance.BlockInput(1);

            if (!scene.songPlay.isCheated)
            {
                ScoreManager.Instance.RecordScore
                (
                    PatternPracticeSceneContext.Instance.fumenPatternMap,
                    scene.songPlay.judgeStatistic
                );
            }

            var judges = scene.songPlay.GetJudges();
        }
    }


    public class PatternPracticeState
    {
        protected PatternPracticeScene scene;

        protected InputActionAsset inputActionAsset;

        // public UnityAction stateEntered, stateExited;

        public PatternPracticeState(PatternPracticeScene scene, InputActionAsset inputActionAsset = null)
        {
            this.inputActionAsset = inputActionAsset;
            this.scene = scene;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void OnGameKeyInput(TaikoKeyType key) { }
        public virtual void Exit() { }
    }
}
