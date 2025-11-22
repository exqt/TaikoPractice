using System.Collections.Generic;

// 譜面 | ふめん
public class Fumen
{
    public string title;

    public struct Note
    {
        public int index;
        public NoteType type;
        public double time;
    }

    public List<Note> notes;
    public List<double> intervalBeginTime = new();
    public int beatNote = 4;
    public int beatPerBar = 4;

    public Fumen(string title, List<Note> notes, List<double> intervalStarts, int beatNote = 4, int beatPerBar = 4)
    {
        this.title = title;
        this.notes = notes;
        this.intervalBeginTime = intervalStarts;
        this.beatNote = beatNote;
        this.beatPerBar = beatPerBar;
        if (intervalStarts.Count == 0)
        {
            intervalStarts.Add(0);
        }
    }
}
