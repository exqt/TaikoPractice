using UnityEngine;

public class IconIndicatorUI : MonoBehaviour
{
    public GameObject metronomeMutedIcon;
    public GameObject hitSoundMutedIcon;
    public GameObject autoIcon;
    public GameObject noCrownIcon;

    AudioManager audioManager;

    public void Awake()
    {
        autoIcon.SetActive(false);
        noCrownIcon.SetActive(false);
    }

    void Start()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogError("FMODAudioManager not found");
            return;
        }

        UpdateUI();
        audioManager.volumeChanged.AddListener(UpdateUI);
    }

    public void UpdateUI()
    {
        metronomeMutedIcon.SetActive(!audioManager.IsMetronomeOn);
        hitSoundMutedIcon.SetActive(!audioManager.IsHitSoundOn);
    }

    public void SetAutoIcon(bool isAuto)
    {
        autoIcon.SetActive(isAuto);
    }

    public void SetNoCrownIcon(bool isNoCrown)
    {
        noCrownIcon.SetActive(isNoCrown);
    }
}
