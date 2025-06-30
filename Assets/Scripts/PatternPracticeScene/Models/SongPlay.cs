using System;
using System.Collections.Generic;
using System.Diagnostics;

public class SongPlay
{
    public Fumen fumen;
    public double bpm;
    public PatternPracticeOptionGroup optionGroup;

    readonly List<Fumen.Note>[] notesByLane =
        new List<Fumen.Note>[] { new(), new() };
    readonly NoteJudge[] judges;
    public double CurrentTime { get; private set; } = 0;
    readonly int[] _ptrs = new int[] { 0, 0 };

    public JudgeStatistic judgeStatistic = new();

    public bool isCheated = false;

    public NoteJudge LastJudge { get; private set; } = new NoteJudge
    {
        index = -1,
        judge = JudgeType.None,
        diff = 0f
    };

    public SongPlay(Fumen fumen, double bpm, PatternPracticeOptionGroup optionGroup)
    {
        this.bpm = bpm;
        this.optionGroup = optionGroup;
        for (int i = 0; i < fumen.notes.Count; i++)
        {
            Fumen.Note note = fumen.notes[i];
            note.index = i;
            fumen.notes[i] = note;

            notesByLane[(int)note.type].Add(fumen.notes[i]);
        }

        this.fumen = fumen;
        judgeStatistic.totalNotes = fumen.notes.Count;
        judges = new NoteJudge[fumen.notes.Count];
        ResetJudge();
    }

    public void ResetJudge(double t = 0)
    {
        for (int i = 0; i < judges.Length; i++)
        {
            if (fumen.notes[i].time < t) continue;
            judges[i] = new NoteJudge
            {
                judge = JudgeType.None,
                diff = 0f
            };
        }
    }

    public CrownType CalculateCrown()
    {
        CrownType crown;
        if (judgeStatistic.NGood == judgeStatistic.totalNotes) crown = CrownType.Rainbow;
        else if (judgeStatistic.NBad == 0) crown = CrownType.Gold;
        else crown = CrownType.Silver;

        var acc = judgeStatistic.Accuracy;
        var targetAcc = optionGroup?.targetAccuracy ?? 0.0f;

        if (acc < targetAcc) crown = CrownType.None;
        if (isCheated) crown = CrownType.None;

        return crown;
    }

    public bool IsEnded()
    {
        MovePtr();
        return _ptrs[0] >= notesByLane[0].Count && _ptrs[1] >= notesByLane[1].Count;
    }

    public List<NoteJudge> GetJudges()
    {
        List<NoteJudge> result = new List<NoteJudge>();
        foreach (var judge in judges)
        {
            if (judge.judge != JudgeType.None)
            {
                result.Add(judge);
            }
        }
        return result;
    }

    readonly NoteJudge EMPTY_JUDGE = new()
    {
        judge = JudgeType.None,
        diff = 0f
    };

    public void SetTime(double time)
    {
        CurrentTime = time;
        MovePtr();
    }

    void RecordJudge(NoteJudge judge)
    {
        if (judge.judge == JudgeType.None) return;

        if (judge.index < 0 || judge.index >= judges.Length) return;
        if (judges[judge.index].judge != JudgeType.None) return;


        judges[judge.index] = judge;
        LastJudge = judge;

        judgeStatistic.AddJudge(judge);
    }

    public Fumen.Note? GetNoteToHit(NoteType type, double time)
    {
        MovePtr();

        List<Fumen.Note> notes = notesByLane[(int)type];

        int ptr = _ptrs[(int)type];
        if (ptr >= notes.Count) return null;

        Fumen.Note note = notes[ptr];

        if (ptr >= notes.Count) return null;
        if (time + Consts.BAD_TIME < note.time) return null;

        return note;
    }

    public NoteJudge Hit(TaikoKeyType key, double time)
    {
        NoteType type = TypeConverter.ToNoteType(key);
        HandType hand = TypeConverter.ToHandType(key);

        var _note = GetNoteToHit(type, time);
        if (_note == null) return EMPTY_JUDGE;

        var note = _note.Value;
        double diff = note.time - time;
        double adiff = Math.Abs(diff);

        if (adiff > Consts.BAD_TIME) return EMPTY_JUDGE;

        JudgeType judge;
        if (adiff <= Consts.GOOD_TIME) judge = JudgeType.Good;
        else if (adiff <= Consts.OK_TIME) judge = JudgeType.Ok;
        else judge = JudgeType.Bad;

        RecordJudge(new NoteJudge
        {
            index = note.index,
            judge = judge,
            hand = hand,
            diff = diff
        });

        return judges[note.index];
    }

    public int GetLeftNoteCount()
    {
        MovePtr();
        int ret = notesByLane[0].Count - _ptrs[0];
        ret += notesByLane[1].Count - _ptrs[1];
        return ret;
    }

    public void GoToEnd()
    {
        SetTime(fumen.notes[^1].time + Consts.BAD_TIME);
        MovePtr();
    }

    object _LockMovePtr = new();
    void MovePtr()
    {
        lock (_LockMovePtr) {
        for (int i = 0; i < 2; i++)
        {
            int ptr = _ptrs[i];
            if (ptr >= notesByLane[i].Count) continue;

            while (ptr < notesByLane[i].Count &&
                (judges[notesByLane[i][ptr].index].judge != JudgeType.None ||
                notesByLane[i][ptr].time <= CurrentTime - Consts.BAD_TIME))
            {
                RecordJudge(new NoteJudge
                {
                    index = notesByLane[i][ptr].index,
                    judge = JudgeType.Bad,
                    diff = -Consts.BAD_TIME,
                });

                ptr++;
            }
            _ptrs[i] = ptr;
        }}
    }

    public struct NoteJudge
    {
        public int index;
        public JudgeType judge;
        public HandType hand;
        public double diff;
    }
}
