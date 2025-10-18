using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public enum OnFailAction
{
    None,
    RestartMap,
    RestartPattern,
    RestartBar,
    RestartTwoBar,
    Result,
}

public enum AudioBackend
{
    WASAPI,
    ASIO,
}

public enum PatternShuffle
{
    None,
    Bag,
    FullRandom
}

public enum NoteModifier { None, Detarame, Abekobe }

public static class OptionChoices
{
    static List<T> Range<T>(T start, T end, T step)
    {
        List<T> l = new();
        for (dynamic i = start; i <= end; i += step) l.Add(i);
        return l;
    }

    // Helper method to convert any typed array to object[] (for use in pre-C# 11)
    public static object[] ToObjectArray<T>(T[] array)
    {
        if (array == null) return null;
        object[] result = new object[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            result[i] = array[i];
        }
        return result;
    }

    public static readonly int[] BPM = Range(60, 140, 10).Concat(Range(150, 360, 5)).ToArray();
    public static readonly int[] MaxStrokes = new int[] { -1, 1, 2, 3 };
    public static readonly int[] MinimumNotes = new int[] { 100, 200, 300, 400, 500, 765, 1000, 1500, 2000 };
    public static readonly OnFailAction[] FailBehaviors = (OnFailAction[])Enum.GetValues(typeof(OnFailAction));
    public static readonly float[] Speed = new float[] { 0.5f, 0.6666f, 0.75f, 1f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1.8f, 1.9f, 2f, 2.5f, 3f, 4f };
    public static readonly float[] TargetAccuracies = new float[] { 0f, 0.8f, 0.85f, 0.9f, 0.95f, 0.96f, 0.97f, 0.98f, 0.99f, 0.995f, 1f };
    public static readonly bool[] Booleans = new bool[] { false, true };
    public static readonly PatternShuffle[] PatternShuffles = (PatternShuffle[])Enum.GetValues(typeof(PatternShuffle));

    public static readonly AudioBackend[] AudioBackends = (AudioBackend[])Enum.GetValues(typeof(AudioBackend));
    public static readonly int[] AudioBufferSizes = new int[] { 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
    public static readonly float[] AudioVolumes = new float[] { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
    public static readonly int[] TimeOffsets = Range(-100, 100, 1).ToArray();

    public static readonly int[] FastSlowCutOff = Range(1, 25, 1).ToArray();
    public static readonly NoteModifier[] Modifiers = (NoteModifier[])Enum.GetValues(typeof(NoteModifier));
}

public class OptionDescAttribute : Attribute
{
    public string label;
    public string description;
    public object[] choices;
    public string choicesName;

    public OptionDescAttribute(string label, string description)
    {
        this.label = label;
        this.description = description;
    }

    public OptionDescAttribute(string label, string description, string choicesName)
    {
        this.label = label;
        this.description = description;
        this.choicesName = choicesName;
    }
}

public class OptionGroup
{

}

public class OptionGroupGlobal<T> : OptionGroup where T : OptionGroup, new()
{
    public static T _instance;
    public static T Instance
    {
        get
        {
            _instance ??= Load();
            return _instance;
        }
    }

    static string path = "Options/";

    public void Save()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yaml = serializer.Serialize(this);
        string filePath = Path.Combine(path, GetType().Name + ".yaml");

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, yaml);
    }

    public static T Load()
    {
        string filePath = Path.Combine(path, typeof(T).Name + ".yaml");
        if (!File.Exists(filePath))
        {
            return new T();
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yaml = File.ReadAllText(filePath);
        return deserializer.Deserialize<T>(yaml);
    }
}

public class PatternPracticeOptionGroup : OptionGroupGlobal<PatternPracticeOptionGroup>
{
    [OptionDesc("BPM", "Beats Per Minute", "BPM")]
    public int? bpm;

    [OptionDesc("Speed", "Speed of the pattern", "Speed")]
    public float? speed;

    [OptionDesc("Minimum Notes", "Minimum number of notes required", "MinimumNotes")]
    public int? minimumNotes;

    [OptionDesc("Target Accuracy", "Target accuracy for the pattern", "TargetAccuracies")]
    public float? targetAccuracy;

    [OptionDesc("Fail on Bad", "Restart the pattern if the player fails a note", "Booleans")]
    public bool? badFail;

    [OptionDesc("On Fail", "Behavior when failing to meet target accuracy or aborted", "FailBehaviors")]
    public OnFailAction? onFail;

    [OptionDesc("Modifier", "Enable or disable modifier mode", "Modifiers")]
    public NoteModifier? modifier;

    [OptionDesc("Pattern Shuffle", "Shuffle patterns in practice mode", "PatternShuffles")]
    public PatternShuffle patternShuffle = PatternShuffle.None;

    public PatternPracticeOptionGroup GetDefault()
    {
        return new PatternPracticeOptionGroup
        {
            bpm = 120,
            speed = 1.0f,
            minimumNotes = 300,
            targetAccuracy = 0.00f,
            onFail = OnFailAction.None,
            modifier = NoteModifier.None,
            patternShuffle = PatternShuffle.None
        };
    }
}

public class GraphicsSettingOptionGroup : OptionGroup
{
    public float slowFastCutOff = 0.025f;

    public float judgeLineOffset = 0.0f;
}


public class SystemOptionGroup : OptionGroupGlobal<SystemOptionGroup>
{
    [OptionDesc("Audio Backend", "Audio backend to use", "AudioBackends")]
    public AudioBackend audioBackend = AudioBackend.WASAPI;

    // [OptionDesc("Audio Buffer Size", "Size of the audio buffer", "AudioBufferSizes")]
    // public int audioBufferSize = 64;

    [OptionDesc("Metronome Volume", "Volume level for the metronome", "AudioVolumes")]
    public float metronomeVolume = 1.0f;

    [OptionDesc("Hit Sound Volume", "Hit sound volume level", "AudioVolumes")]
    public float hitSoundVolume = 1.0f;

    [OptionDesc("Judge Offset", "Offset for the judge timing", "TimeOffsets")]
    public int judgeOffset = 0;

    [OptionDesc("Metronome Offset", "Offset for the metronome timing", "TimeOffsets")]
    public int metronomeOffset = 0;

    [OptionDesc("Draw Offset", "Offset for the draw timing", "TimeOffsets")]
    public int drawOffset = 0;
}

public class RecordingOptionGroup : OptionGroup
{
    [OptionDesc("BPM", "Beats Per Minute", "BPM")]
    public int? bpm;
}
