using System;

public enum TaikoKeyType
{
    LEFT_KA = 'D',
    LEFT_DON = 'F',
    RIGHT_DON = 'J',
    RIGHT_KA = 'K'
}

public enum NoteType { Don, Ka }

public enum HandType { None, Left, Right }

public enum JudgeType { None, Good, Ok, Bad }

public enum CrownType { Rainbow, Gold, Silver, None }

public enum StrokeMode { Single, Multiple }

public static class TypeConverter
{
    public static NoteType ToNoteType(this TaikoKeyType key)
    {
        return key switch
        {
            TaikoKeyType.LEFT_DON or TaikoKeyType.RIGHT_DON => NoteType.Don,
            TaikoKeyType.LEFT_KA or TaikoKeyType.RIGHT_KA => NoteType.Ka,
            _ => throw new ArgumentException("Invalid key for note type conversion")
        };
    }

    public static HandType ToHandType(this TaikoKeyType key)
    {
        return key switch
        {
            TaikoKeyType.LEFT_DON or TaikoKeyType.LEFT_KA => HandType.Left,
            TaikoKeyType.RIGHT_DON or TaikoKeyType.RIGHT_KA => HandType.Right,
            _ => throw new ArgumentException("Invalid key for hand type conversion")
        };
    }

    public static TaikoKeyType ToTaikoKeyType(this NoteType noteType, HandType hand)
    {
        return (noteType, hand) switch
        {
            (NoteType.Don, HandType.Left) => TaikoKeyType.LEFT_DON,
            (NoteType.Don, HandType.Right) => TaikoKeyType.RIGHT_DON,
            (NoteType.Ka, HandType.Left) => TaikoKeyType.LEFT_KA,
            (NoteType.Ka, HandType.Right) => TaikoKeyType.RIGHT_KA,
            _ => throw new ArgumentException("Invalid combination of note type and hand type")
        };
    }
}
