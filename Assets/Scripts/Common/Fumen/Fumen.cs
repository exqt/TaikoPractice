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

    public Fumen(string title, List<Note> notes, List<double> intervalStarts)
    {
        this.title = title;
        this.notes = notes;
        this.intervalBeginTime = intervalStarts;
        if (intervalStarts.Count == 0)
        {
            intervalStarts.Add(0);
        }
    }
}
