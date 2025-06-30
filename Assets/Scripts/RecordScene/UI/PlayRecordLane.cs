using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayRecordLane : MonoBehaviour
{
    PlayRecordLaneContainer laneContainer;
    int laneIndex;

    public TMP_Text laneNumberText;

    public GameObject notePrefab;

    public Sprite[] noteSprites;
    public GameObject playLine;

    public GameObject noteContainer;

    void Awake()
    {
        laneContainer = GetComponentInParent<PlayRecordLaneContainer>();
        if (laneContainer == null)
        {
            Debug.LogError("PlayRecordLane must be a child of PlayRecordLaneContainer.");
        }
    }

    public void SetLaneIndex(int index)
    {
        laneIndex = index;
        laneNumberText.text = $"# {index + 1}";
    }

    // beat position [0, 4)
    static Color transparentColor = new(1, 1, 1, 0.25f);
    public void AddNote(Fumen.Note note, float beatPosition)
    {
        var obj = Instantiate(notePrefab, noteContainer.transform);
        var type = note.type == NoteType.Don ? 0 : 1;
        var image = obj.GetComponent<Image>();
        image.sprite = noteSprites[type];

        // Position the note based on its beat position
        var rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector3(
            (float)(Consts.ONE_BEAT_SCALE * beatPosition), 0, 0);

        if (beatPosition < 0 || beatPosition >= 4)
        {
            image.color = transparentColor;
        }
    }

    public void SetPlayLine(double beatPosition)
    {
        playLine.transform.localPosition = new Vector3(
            (float)(Consts.ONE_BEAT_SCALE * beatPosition), 0, 0);
    }

    void Update()
    {
        var isOn = laneContainer.playingLaneIndex == laneIndex;
        playLine.SetActive(isOn);
    }
}
