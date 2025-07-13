using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalNoteLaneUI : MonoBehaviour
{
    public SongPlay songPlay;

    public GameObject notePrefab;
    public GameObject linePrefab;
    public GameObject noteContainer;
    public GameObject lineContainer;
    readonly Dictionary<int, GameObject> noteDict = new();


    double bpm;
    float speed = 1f;

    public HitEffectUI hitEffect;
    public HitFastSlowUI hitFastSlow;

    public HorizonalNoteLaneFrameUI frame;
    public DrumUI drum;

    [Header("Sprite")]
    public NoteSkin noteSkin;

    float drawOffset = 0f;

    void Start()
    {
        // ClearNoteObjects();
        var systemOptionGroup = SystemOptionGroup.Load();
        drawOffset = systemOptionGroup.drawOffset / 1000f;
    }

    void Update()
    {
        UpdateNotes();
    }

    bool isStarted = false;

    public void Setup(Fumen fumen, double bpm, float speed = 1f)
    {
        this.bpm = bpm;
        this.speed = speed;

        CreateNotes(fumen);
        CreateLines(fumen);
    }

    public void Play()
    {
        if (isStarted) return;
        isStarted = true;
    }

    public void OnKeyInput(TaikoKeyType key)
    {
        frame.Hit(key);
        drum.Hit(key);
    }

    public void HandleJudge(SongPlay.NoteJudge judge)
    {
        if (judge.judge == JudgeType.None) return;

        hitEffect.Play(judge);
        hitFastSlow.Play(judge);
        drum.SetCombo(songPlay.judgeStatistic.Combo);

        var o = noteDict[judge.index];
        o.SetActive(false);
    }

    //
    void CreateNotes(Fumen fumen)
    {
        bool showLeftRight = true;
        int hand = 0;

        Sprite GetNoteSkin(NoteType type, int index)
        {
            if (type == NoteType.Don)
            {
                // if (showLeftRight) return index % 2 == hand ? noteSkin.donLeft : noteSkin.donRight;
                // else return noteSkin.don;
                return noteSkin.don;
            }
            else if (type == NoteType.Ka)
            {
                // if (showLeftRight) return index % 2 == hand ? noteSkin.kaLeft : noteSkin.kaRight;
                // else return noteSkin.ka;
                return noteSkin.ka;
            }

            return null;
        }

        for (int i = fumen.notes.Count - 1; i >= 0; i--)
        {
            var note = fumen.notes[i];
            var o = Instantiate(notePrefab);
            o.name = note.index.ToString();
            o.transform.SetParent(noteContainer.transform);

            var x = Consts.ONE_BEAT_SCALE * (note.time * (bpm / 60f)) * speed;
            o.transform.localPosition = new Vector3((float)x, 0, 0);
            o.transform.localScale = new Vector3(1, 1, 1);

            var img = o.GetComponent<Image>();
            img.sprite = GetNoteSkin(note.type, i);

            noteDict[note.index] = o;
        }
    }

    void CreateLines(Fumen fumen)
    {
        var lastNote = fumen.notes[^1];
        var nBeats = lastNote.time * bpm / 60f;

        for (int i = 0; i < nBeats; i++)
        {
            if (i % 4 != 0) continue;

            var o = Instantiate(linePrefab);
            o.transform.SetParent(lineContainer.transform);

            var x = Consts.ONE_BEAT_SCALE * i * speed;
            o.transform.localPosition = new Vector3((float)x, 0, 0);
            o.transform.localScale = new Vector3(1, 1, 1);

            var img = o.GetComponent<Image>();
            if (i % 4 == 0) img.color = new Color(1, 1, 1, 1f);
            else img.color = new Color(1, 1, 1, 0.1f);
        }
    }

    void ClearNoteObjects()
    {
        noteDict.Clear();
        while (noteContainer.transform.childCount > 0)
        {
            Destroy(noteContainer.transform.GetChild(0).gameObject);
        }
    }

    void UpdateNotes()
    {
        if (songPlay == null) return;
        if (!isStarted) return;

        double x = -(Consts.ONE_BEAT_SCALE * (songPlay.CurrentTime + drawOffset) * (bpm / 60f)) * speed;
        noteContainer.transform.localPosition = new Vector3((float)x, 0, 0);
        lineContainer.transform.localPosition = new Vector3((float)x, 0, 0);

        // Inactive Notes
        for (int i = 0; i < noteDict.Count; i++)
        {
            var o = noteDict[i];

            // if (o.transform.position.x < -100) o.SetActive(false);
            // if (o.transform.position.x > 1000) break;
        }
    }

}
