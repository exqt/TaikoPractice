using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VerticalNoteLaneUI : MonoBehaviour
{
    public SongPlay songPlay;

    public GameObject linePrefab;
    public GameObject lineContainer;

    double bpm = 120f;
    float speed = 1f;

    public GameObject notePrefab;
    public GameObject[] noteContainer;
    readonly Dictionary<int, GameObject> noteDict = new();

    public TMP_Text comboText;

    public GameObject noteOrigin;

    public Image[] bg = new Image[4];

    public float alphaRate = 4f;
    public float areaAlpha = 0.6f;

    readonly float[] laneHitEffect = new float[4];


    void Awake()
    {
        // var rectTransform = lineContainer.GetComponent<RectTransform>();
        // originalY = rectTransform.anchoredPosition.y;
    }

    void Start()
    {
    }

    void Update()
    {
        if (songPlay == null) return;

        double x = -(Consts.ONE_BEAT_SCALE * songPlay.CurrentTime) * speed;

        var rectTransform = noteOrigin.GetComponent<RectTransform>();
        rectTransform.localPosition = new Vector3(0, (float)x, 0);
        comboText.text = songPlay.judgeStatistic.Combo.ToString();

        for (int i = 0; i < bg.Length; i++)
        {
            laneHitEffect[i] = Mathf.Max(0.0f, laneHitEffect[i] - Time.deltaTime * alphaRate);
            bg[i].color = new Color(bg[i].color.r, bg[i].color.g, bg[i].color.b, Mathf.Pow(laneHitEffect[i] * areaAlpha, 2f));
        }
    }

    public void OnKeyInput(TaikoKeyType key)
    {
        switch (key)
        {
            case TaikoKeyType.LEFT_KA:
                laneHitEffect[0] = 1.0f;
                break;
            case TaikoKeyType.LEFT_DON:
                laneHitEffect[1] = 1.0f;
                break;
            case TaikoKeyType.RIGHT_DON:
                laneHitEffect[2] = 1.0f;
                break;
            case TaikoKeyType.RIGHT_KA:
                laneHitEffect[3] = 1.0f;
                break;
        }
    }

    public Color donColor;
    public Color kaColor;
    void CreateNotes(Fumen fumen)
    {
        for (int i = fumen.notes.Count - 1; i >= 0; i--)
        {
            var note = fumen.notes[i];
            var o = Instantiate(notePrefab);
            var hand = i % 2;
            var type = note.type == NoteType.Don ? 0 : 1;

            var index = 0;
            if      (hand == 0 && type == 1) index = 0;
            else if (hand == 0 && type == 0) index = 1;
            else if (hand == 1 && type == 0) index = 2;
            else if (hand == 1 && type == 1) index = 3;

            o.name = note.index.ToString();
            o.transform.SetParent(noteContainer[index].transform);

            var y = Consts.ONE_BEAT_SCALE * note.time * speed;
            o.transform.localPosition = new Vector3(0, (float)y, 0);
            o.transform.localScale = new Vector3(1, 1, 1);

            var img = o.GetComponent<Image>();
            int t = note.type == NoteType.Don ? 0 : 1;

            img.color = t == 0 ? donColor : kaColor;

            noteDict[note.index] = o;
        }
    }

    void CreateLines(Fumen fumen)
    {
        var lastNote = fumen.notes[^1];
        var nBeats = lastNote.time * bpm / 60f;
        var oneBeatTime = 60 / bpm;

        for (int i = 0; i < nBeats; i++)
        {
            // if (i % 4 != 0) continue;

            var o = Instantiate(linePrefab);
            o.transform.SetParent(lineContainer.transform);

            var y = Consts.ONE_BEAT_SCALE * i * oneBeatTime * speed;
            o.transform.localPosition = new Vector3(0, (float)y, 0);
            o.transform.localScale = new Vector3(1, 1, 1);

            var img = o.GetComponent<Image>();
            if (i % 4 == 0) img.color = new Color(1, 1, 1, 1f);
            else img.color = new Color(1, 1, 1, 0.1f);
        }
    }

    public void Setup(SongPlay songPlay, Fumen fumen, double bpm, float speed = 1f)
    {
        this.songPlay = songPlay;
        this.bpm = bpm;
        this.speed = 4f;

        CreateNotes(fumen);
        CreateLines(fumen);
    }

    public void Play()
    {
    }

    public void HandleJudge(SongPlay.NoteJudge judge)
    {
        if (judge.judge == JudgeType.None) return;

        var o = noteDict[judge.index];
        if (o == null) return;
        o.SetActive(false);
    }
}
