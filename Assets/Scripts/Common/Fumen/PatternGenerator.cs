using System;
using System.Collections.Generic;

public class PatternLanguage
{
    readonly List<string> patterns = new();
    int seed = 0;

    public PatternLanguage(List<string> patterns)
    {
        this.patterns = patterns;

        foreach (string pattern in patterns) seed += pattern.GetHashCode();
    }

    public class PatternParseException : Exception
    {
        public PatternParseException(string message) : base(message) { }
    }

    public class GeneartionResult
    {
        public List<Fumen.Note> notes;
        public double length;
    }

    GeneartionResult GenerateFromPatternString(string s, double bpm, Random rng)
    {
        s = s.Replace(" ", "");
        int ptr = 0;
        int beat = 4;
        List<Fumen.Note> notes = new();

        // 공통: 패턴 문자열을 파싱해서 노트 리스트와 길이 반환
        (List<Fumen.Note>, double) parsePattern(string pat, double bpm, int beat, double startTime, Random rng)
        {
            List<Fumen.Note> notes = new();
            double cur = startTime;
            int i = 0;
            int curBeat = beat;
            while (i < pat.Length)
            {
                char c = pat[i];
                if (c == '<')
                {
                    // Inline beat change: <n>
                    i = parseBeatFromIndex(pat, i, out curBeat);
                    continue;
                }
                if (c == 'd' || c == 'D' || c == 'k' || c == 'K' || c == '?')
                {
                    notes.Add(new Fumen.Note { type = (c == '?' ? (rng.Next(2) == 0 ? NoteType.Don : NoteType.Ka) : (c == 'd' || c == 'D' ? NoteType.Don : NoteType.Ka)), time = cur });
                    cur += 60 / bpm / curBeat;
                    i++;
                }
                else if (c == '.')
                {
                    cur += 60 / bpm / curBeat;
                    i++;
                }
                else
                {
                    throw new PatternParseException("Invalid char in pattern");
                }
            }
            return (notes, cur - startTime);
        }

        // 옵션 파싱: {a,b,c} 또는 (a,b,c)
        List<string> parseOptions(ref int ptr, string s, char endChar)
        {
            List<string> options = new();
            string cur = "";
            while (ptr < s.Length && s[ptr] != endChar)
            {
                if (s[ptr] == ',')
                {
                    options.Add(cur);
                    cur = "";
                    ptr++;
                }
                else
                {
                    cur += s[ptr++];
                }
            }
            options.Add(cur);
            if (ptr >= s.Length || s[ptr] != endChar) throw new PatternParseException($"Expected '{endChar}'");
            ptr++;
            return options;
        }

        // <n> 비트 지정 (인덱스 받는 버전)
        int parseBeatFromIndex(string s, int index, out int newBeat)
        {
            if (s[index] == '<')
            {
                index++;
                int num = 0;
                while (index < s.Length && char.IsDigit(s[index])) num = num * 10 + (s[index++] - '0');
                if (num < 1 || num > 16) throw new PatternParseException("Invalid Beat 1 <= n <= 16");
                if (index >= s.Length || s[index] != '>') throw new PatternParseException("Expected '>' after beat number");
                index++;
                newBeat = num;
                return index;
            }
            else
            {
                newBeat = 4;
                return index;
            }
        }

        // <n> 비트 지정 (ref 포인터 버전 - 기존 호출 호환성)
        void parseBeat(ref int ptr, string s, ref int beat)
        {
            ptr = parseBeatFromIndex(s, ptr, out int newBeat);
            beat = newBeat;
        }

        double patternLength = 0;
        double t = 0;
        while (ptr < s.Length)
        {
            char c = s[ptr];
            if (c == '<')
            {
                parseBeat(ref ptr, s, ref beat);
            }
            else if (c == '{' || c == '(')
            {
                char endChar = (c == '{') ? '}' : ')';
                ptr++;
                var options = parseOptions(ref ptr, s, endChar);
                int repeat = 1;
                if (ptr < s.Length && s[ptr] == '*')
                {
                    ptr++;
                    int num = 0;
                    while (ptr < s.Length && char.IsDigit(s[ptr])) num = num * 10 + (s[ptr++] - '0');
                    repeat = num;
                }
                List<Fumen.Note> result = new();
                double totalLen = 0;
                if (c == '{')
                {
                    for (int i = 0; i < repeat; i++)
                    {
                        int idx = rng.Next(options.Count);
                        var (optNotes, optLen) = parsePattern(options[idx], bpm, beat, t, rng);
                        result.AddRange(optNotes);
                        t += optLen;
                        totalLen += optLen;
                    }
                }
                else // (a,b,c)*n : 첫 랜덤 고정
                {
                    int idx = rng.Next(options.Count);
                    var (optNotes, optLen) = parsePattern(options[idx], bpm, beat, t, rng);
                    for (int i = 0; i < repeat; i++)
                    {
                        foreach (var n in optNotes)
                        {
                            var note = n;
                            note.time += optLen * i;
                            result.Add(note);
                        }
                        t += optLen;
                        totalLen += optLen;
                    }
                }
                notes.AddRange(result);
                patternLength += totalLen;
            }
            else if (c == 'd' || c == 'D' || c == 'k' || c == 'K' || c == '?' || c == '.')
            {
                var (n, len) = parsePattern(s.Substring(ptr, 1), bpm, beat, t, rng);
                notes.AddRange(n);
                t += len;
                patternLength += len;
                ptr++;
            }
            else
            {
                throw new PatternParseException($"Unexpected character: {c}");
            }
        }

        return new GeneartionResult
        {
            notes = notes,
            length = patternLength
        };
    }

    public (List<Fumen.Note>, List<double>) GetNotes(
        double bpm, int nMinNotes, double openingBeats = 4, bool deterame = false,
        PatternShuffle patternShuffle = PatternShuffle.None,
        int seed = 0)
    {
        double beatInterval = 60f / bpm;

        List<Fumen.Note> notes = new();
        List<double> intervalStarts = new() { };

        int nNotes = 0;
        double time = openingBeats * beatInterval;

        // seed += DateTime.Now.Millisecond;
        Random rng = new(seed);

        while (nNotes < nMinNotes)
        {
            var shuffledPatterns = new List<string>(patterns);

            if (patternShuffle != PatternShuffle.None)
            {
                shuffledPatterns = Shuffle(shuffledPatterns, rng);
            }

            if (patternShuffle == PatternShuffle.FullRandom)
            {
                while (shuffledPatterns.Count > 1) shuffledPatterns.RemoveAt(
                    shuffledPatterns.Count - 1
                );
            }

            foreach (var pattern in shuffledPatterns)
            {
                intervalStarts.Add(time);
                GeneartionResult result = GenerateFromPatternString(pattern, bpm, rng);
                for (int i = 0; i < result.notes.Count; i++)
                {
                    var note = result.notes[i];
                    note.time += time;
                    result.notes[i] = note;
                }

                notes.AddRange(result.notes);

                time += result.length;
                nNotes += result.notes.Count;

                if (nNotes >= nMinNotes) break;
            }
        }

        if (deterame)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                var note = notes[i];
                note.type = rng.Next(2) == 0 ? NoteType.Don : NoteType.Ka;
                notes[i] = note;
            }
        }

        return (notes, intervalStarts);
    }

    List<T> Shuffle<T>(List<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }
}
