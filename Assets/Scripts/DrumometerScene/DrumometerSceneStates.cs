using UnityEngine;

public partial class DrumometerScene
{
    public class BeforeStartState : DrumometerSceneState
    {
        public BeforeStartState(DrumometerScene scene) : base(scene) {}

        public override void Enter()
        {
            scene.InitUI();
        }

        public override void HandleKeyInput(TaikoKeyType key)
        {
            scene.ChangeState(GameStateType.Running);
            scene.HandleHit(key);
        }
    }

    public class RunningState : DrumometerSceneState
    {
        ConsecutiveHitWatcher watcher;
        bool quitRequested = false;

        public RunningState(DrumometerScene scene) : base(scene)
        {
            watcher = new ConsecutiveHitWatcher(End);
        }

        public override void Enter()
        {
            scene.startTime = TimeUtil.Time;
        }

        public void End() => quitRequested = true;

        public override void Update()
        {
            var elapsed = TimeUtil.Time - scene.StartTime;
            scene.UpdateUI();
            if (elapsed > scene.Duration) scene.ChangeState(GameStateType.Finished);
            if (quitRequested) scene.ChangeState(GameStateType.Finished);
        }

        public override void HandleKeyInput(TaikoKeyType key)
        {
            watcher.HandleHit(key);
            scene.HandleHit(key);
        }
    }

    public class FinishedState : DrumometerSceneState
    {
        public FinishedState(DrumometerScene scene) : base(scene) {}

        public override void Enter()
        {
            scene.timeText.text = "0.00";

            // --- BPM Graph ---
            var bpmArray = scene.CalculateBPMGraph(0.2f);
            scene.resultPanel.GetComponent<DrumometerResultPanelUI>().SetBPMGraph(bpmArray);

            scene.resultPanel.GetComponent<UIPopup>().Show();
            float maxBpm = bpmArray.Length > 0 ? Mathf.Max(bpmArray) : 0f;

            scene.resultPanel.GetComponent<DrumometerResultPanelUI>().SetData(
                $"{DrumometerSceneContext.Instance.duration} seconds",
                $"Score: {scene.hitCount}",
                $"Max BPM: {(int)maxBpm}"
            );

            DrumometerBest.SetBestCount(DrumometerSceneContext.Instance.duration, scene.hitCount);

        }
    }

    public abstract class DrumometerSceneState
    {
        protected DrumometerScene scene;
        public DrumometerSceneState(DrumometerScene scene) { this.scene = scene; }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void HandleKeyInput(TaikoKeyType key) { }
        public virtual void Exit() { }
    }

}
