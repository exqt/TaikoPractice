using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayRecordLaneContainer : MonoBehaviour
{
    List<PlayRecordLane> lanes = new();
    // Add your variables and methods here

    public GameObject lanePrefab;

    public float laneHeight = 280f;

    public int focusingLaneIndex = 0;

    public PlayRecordLane FocusingLane => focusingLaneIndex >= 0 && focusingLaneIndex < lanes.Count ? lanes[focusingLaneIndex] : null;

    [NonSerialized]
    public int playingLaneIndex = 0;

    public PlayRecordLane PlayingRecordLane => lanes.Count > playingLaneIndex ? lanes[playingLaneIndex] : null;

    public GameObject container;

    List<Fumen.Note> hitRecord;

    public double bpm;
    public double currentTime;

    void Awake()
    {
        // Initialize or set up the lanes if needed
        // For example, you can find all PlayRecordLane components in children
        lanes.AddRange(GetComponentsInChildren<PlayRecordLane>());

        for (int i = 0; i < lanes.Count; i++)
        {
            lanes[i].SetLaneIndex(i);
        }

        hitRecord = new();

        MoveLane(0);
    }

    const int MAX_LANE_COUNT = 10000;

    public void AddNote(Fumen.Note note)
    {
        var (laneIndex, beatPosition) = GetLaneIndexAndBeatPosition(note.time);

        while (laneIndex >= lanes.Count) AddLane();

        if (laneIndex < 0 || laneIndex >= lanes.Count)
        {
            Debug.LogWarning($"Lane index {laneIndex} is out of bounds. Note will not be added.");
            return;
        }

        lanes[laneIndex].AddNote(note, (float)beatPosition);
        hitRecord.Add(note);

        var cutOff = 0.2f; // Adjust this value as needed for cutoff
        if (beatPosition < cutOff)
        {
            var prevLane = laneIndex != 0 ? lanes[laneIndex - 1] : null;
            if (prevLane != null) prevLane.AddNote(note, (float)beatPosition + 4);
        }

        if (4 - cutOff < beatPosition)
        {
            var nextLane = laneIndex != lanes.Count - 1 ? lanes[laneIndex + 1] : null;
            if (nextLane != null) nextLane.AddNote(note, (float)beatPosition - 4);
        }
    }

    public void MoveLane(int index)
    {
        index = Mathf.Clamp(index, 0, lanes.Count - 1);

        focusingLaneIndex = index;

        DOTween.To(
            () => container.transform.localPosition.y,
            y => container.transform.localPosition = new Vector3(0, y, 0),
            index * laneHeight,
            0.5f
        ).SetEase(Ease.OutCubic);
    }

    public void MoveUpLane() => MoveLane(focusingLaneIndex - 1);

    public void MoveDownLane() => MoveLane(focusingLaneIndex + 1);

    (int, double) GetLaneIndexAndBeatPosition(double time)
    {
        var oneBeatInterval = 60.0 / bpm;
        var fourBeatsInterval = oneBeatInterval * 4;
        var laneIndex = time / fourBeatsInterval;
        var lane = (int)Math.Floor(laneIndex);
        var beatPosition = (time % fourBeatsInterval) / oneBeatInterval;

        return (lane, beatPosition);
    }

    public void SetTime(double time)
    {
        currentTime = time;

        var (laneIndex, beatPosition) = GetLaneIndexAndBeatPosition(currentTime);

        if (laneIndex < 0 || laneIndex >= lanes.Count)
        {
            Debug.LogWarning($"Lane index {laneIndex} is out of bounds. Cannot set play line.");
            return;
        }

        if (laneIndex != playingLaneIndex && PlayingRecordLane != null)
        {
            MoveLane(laneIndex);
        }

        playingLaneIndex = laneIndex;

        if (PlayingRecordLane != null)
        {
            PlayingRecordLane.SetPlayLine(beatPosition);
        }
    }

    void Update()
    {
        while (focusingLaneIndex + 5 > lanes.Count) { AddLane(); }
    }

    void AddLane()
    {
        if (lanes.Count >= MAX_LANE_COUNT)
        {
            Debug.LogWarning("Maximum lane count reached. Cannot add more lanes.");
            return;
        }

        if (lanePrefab == null)
        {
            Debug.LogError("Lane prefab is not assigned.");
            return;
        }

        GameObject newLaneObj = Instantiate(lanePrefab, container.transform);
        PlayRecordLane newLane = newLaneObj.GetComponent<PlayRecordLane>();
        lanes.Add(newLane);

        newLane.SetLaneIndex(lanes.Count - 1);
        if (newLane.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.anchoredPosition = new Vector2(0, -lanes.Count * laneHeight);
        }
        else
        {
            Debug.LogError("New lane does not have a RectTransform component.");
        }
    }

    [ContextMenu("Align Lanes On Editor")]
    void AlignLanesOnEditor()
    {
        var lanes = GetComponentsInChildren<PlayRecordLane>();
        for (int i = 0; i < lanes.Length; i++)
        {
            var lane = lanes[i];
            if (lane == null) continue;

            RectTransform rectTransform = lane.GetComponent<RectTransform>();
            if (rectTransform == null) continue;

            rectTransform.anchoredPosition = new Vector2(0, -i * laneHeight);
        }
    }
}
