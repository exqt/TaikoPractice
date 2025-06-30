using TMPro;

namespace MainListUI
{
    public class ItemContentDrumometer : UIMainListItemContent
    {
        public TMP_Text best5, best10, best30, best60;

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
        }

        public void OnPressDrumometerPlay(int seconds)
        {
            DrumometerSceneContext.Instance.duration = seconds;
            SceneUtil.LoadScene("DrumometerScene");
        }
    }
}
