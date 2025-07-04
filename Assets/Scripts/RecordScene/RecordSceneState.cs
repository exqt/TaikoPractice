using UnityEngine;
using UnityEngine.InputSystem;

public partial class RecordScene
{
    public class WaitingToStartState : RecordSceneState
    {
        public WaitingToStartState(RecordScene scene, InputActionAsset inputActionAsset)
            : base(scene, inputActionAsset)
        {
        }
        public override void Update()
        {
        }

        public override void Exit()
        {
            scene.startTime = TimeUtil.Time;
        }
    }

    public class PlayingState : RecordSceneState
    {
        readonly InputAction escapeAction;
        readonly ConsecutiveHitWatcher consecutiveHitWatcher1, consecutiveHitWatcher2;
        readonly InputAction leftKaAction, rightKaAction, leftDonAction, rightDonAction;
        public PlayingState(RecordScene scene, InputActionAsset inputActionAsset)
            : base(scene, inputActionAsset)
        {
            escapeAction = inputActionAsset.FindAction("Escape");
            consecutiveHitWatcher1 = new ConsecutiveHitWatcher(
                () => scene.ChangeState(GameState.Moving),
                TaikoKeyType.LEFT_KA,
                8
            );
            consecutiveHitWatcher2 = new ConsecutiveHitWatcher(
                () => scene.ChangeState(GameState.Paused),
                TaikoKeyType.RIGHT_KA,
                8
            );

            leftKaAction = inputActionAsset.FindAction("LeftKa");
            rightKaAction = inputActionAsset.FindAction("RightKa");
            leftDonAction = inputActionAsset.FindAction("LeftDon");
            rightDonAction = inputActionAsset.FindAction("RightDon");
        }

        public override void Enter()
        {
        }

        public override void Update()
        {
            if (escapeAction.WasPressedThisFrame())
            {
                scene.uiPausePanel.GetComponent<UIPopup>().Show();
                scene.ChangeState(GameState.Paused);
            }


            if (leftKaAction.triggered)
            {
                consecutiveHitWatcher1.HandleHit(TaikoKeyType.LEFT_KA);
                consecutiveHitWatcher2.HandleHit(TaikoKeyType.LEFT_KA);
            }
            if (rightKaAction.triggered)
            {
                consecutiveHitWatcher1.HandleHit(TaikoKeyType.RIGHT_KA);
                consecutiveHitWatcher2.HandleHit(TaikoKeyType.RIGHT_KA);
            }
            if (leftDonAction.triggered)
            {
                consecutiveHitWatcher1.HandleHit(TaikoKeyType.LEFT_DON);
                consecutiveHitWatcher2.HandleHit(TaikoKeyType.LEFT_DON);
            }
            if (rightDonAction.triggered)
            {
                consecutiveHitWatcher1.HandleHit(TaikoKeyType.RIGHT_DON);
                consecutiveHitWatcher2.HandleHit(TaikoKeyType.RIGHT_DON);
            }

            scene.laneContainer.SetTime(TimeUtil.Time - scene.startTime);
        }
    }

    public class PausedState : RecordSceneState
    {
        public double pauseEnterTime;
        readonly InputAction escapeAction;
        public PausedState(RecordScene scene, InputActionAsset inputActionAsset)
            : base(scene, inputActionAsset)
        {
            escapeAction = inputActionAsset.FindAction("Escape");
        }

        public override void Enter()
        {
            scene.uiPausePanel.GetComponent<UIPopup>().Show();
            scene.metronome.Pause();
            pauseEnterTime = TimeUtil.Time;
        }

        public override void Update()
        {
            if (escapeAction.WasPressedThisFrame())
            {
                scene.ChangeState(GameState.Playing);
            }
        }

        public override void Exit()
        {
            scene.uiPausePanel.GetComponent<UIPopup>().Hide();
            scene.metronome.Resume();

            scene.startTime += TimeUtil.Time - pauseEnterTime;
        }
    }

    public class MovingState : RecordSceneState
    {
        readonly InputAction movePrevAction, moveNextAction, selectAction;
        readonly InputAction escapeAction;

        public MovingState(RecordScene scene, InputActionAsset inputActionAsset)
            : base(scene, inputActionAsset)
        {
            var map = inputActionAsset.FindActionMap("Drum");
            movePrevAction = map.FindAction("LeftKa");
            moveNextAction = map.FindAction("RightKa");
            selectAction = map.FindAction("Don");
            escapeAction = map.FindAction("Escape");
        }

        public override void Enter()
        {
            scene.metronome.Pause();
        }

        public override void Update()
        {
            if (movePrevAction.WasPressedThisFrame())
            {
                scene.laneContainer.MoveUpLane();
            }
            else if (moveNextAction.WasPressedThisFrame())
            {
                scene.laneContainer.MoveDownLane();
            }
            else if (selectAction.WasPressedThisFrame())
            {
                SceneUtil.BackToMainScene();
            }
        }
    }

    public class FinishedState : RecordSceneState
    {
        public FinishedState(RecordScene scene, InputActionAsset inputActionAsset)
            : base(scene, inputActionAsset) { }
        public override void Enter()
        {
            scene.metronome.Pause();
            // Show result UI or handle finish logic here
        }
    }

    public class RecordSceneState
    {
        protected RecordScene scene;
        protected InputActionAsset inputActionAsset;
        public RecordSceneState(RecordScene scene, InputActionAsset inputActionAsset = null)
        {
            this.scene = scene;
            this.inputActionAsset = inputActionAsset;
        }
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}
