using TMPro;
// Uses StrokeMode enum for mode selection

namespace MainListUI
{
    public class ItemContentDrumometer : UIMainListItemContent
    {
        public TMP_Text best5, best10, best30, best60;
        public TMP_Text best5Mul, best10Mul, best30Mul, best60Mul;

        public void Start()
        {
            LoadData();
        }

        public void LoadData()
        {
            var drumometerBest = DrumometerBest.Load();
            best5.text = $"Best: {drumometerBest.best5}";
            best10.text = $"Best: {drumometerBest.best10}";
            best30.text = $"Best: {drumometerBest.best30}";
            best60.text = $"Best: {drumometerBest.best60}";
            best5Mul.text = $"Best: {drumometerBest.multiBest5}";
            best10Mul.text = $"Best: {drumometerBest.multiBest10}";
            best30Mul.text = $"Best: {drumometerBest.multiBest30}";
            best60Mul.text = $"Best: {drumometerBest.multiBest60}";
        }

        public void OnPressDrumometerPlay(int seconds)
        {
            DrumometerSceneContext.Instance.duration = seconds;
            DrumometerSceneContext.Instance.strokeMode = StrokeMode.Single;
            SceneUtil.LoadScene("DrumometerScene");
        }

        public void OnPressDrumometerPlayMultiple(int seconds)
        {
            DrumometerSceneContext.Instance.duration = seconds;
            DrumometerSceneContext.Instance.strokeMode = StrokeMode.Multiple;
            SceneUtil.LoadScene("DrumometerScene");
        }
    }
}
