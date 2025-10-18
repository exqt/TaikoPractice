using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
// StrokeMode enum used for differentiating single vs multiple stroke records

public class DrumometerBest
{
    public int best5;
    public int best10;
    public int best30;
    public int best60;
    public int multiBest5;   // Multiple stroke mode
    public int multiBest10;
    public int multiBest30;
    public int multiBest60;

    static readonly string path = "Score/drumometer.yaml";

    public static DrumometerBest Load()
    {
        var fileManager = FileManager.Instance;
        if (!fileManager.ExistsFile(path))
        {
            return new DrumometerBest { best5 = 0, best10 = 0, best30 = 0, best60 = 0 };
        }

        var str = fileManager.ReadFile(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<DrumometerBest>(str);
    }

    public static void Save(DrumometerBest data)
    {
        var fileManager = FileManager.Instance;
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(data);
        fileManager.WriteFile(path, yaml);
    }

    public static int GetBestCount(int duration)
    {
        var drumometerBest = Load();
        return duration switch
        {
            5 => drumometerBest.best5,
            10 => drumometerBest.best10,
            30 => drumometerBest.best30,
            60 => drumometerBest.best60,
            _ => 0
        };
    }

    public static int GetBestCount(int duration, StrokeMode mode)
    {
        var d = Load();
        if (mode == StrokeMode.Multiple)
        {
            return duration switch
            {
                5 => d.multiBest5,
                10 => d.multiBest10,
                30 => d.multiBest30,
                60 => d.multiBest60,
                _ => 0
            };
        }
        return GetBestCount(duration);
    }

    public static void SetBestCount(int duration, int count)
    {
        var drumometerBest = Load();
        switch (duration)
        {
            case 5:
                drumometerBest.best5 = Math.Max(drumometerBest.best5, count);
                break;
            case 10:
                drumometerBest.best10 = Math.Max(drumometerBest.best10, count);
                break;
            case 30:
                drumometerBest.best30 = Math.Max(drumometerBest.best30, count);
                break;
            case 60:
                drumometerBest.best60 = Math.Max(drumometerBest.best60, count);
                break;
        }
        Save(drumometerBest);
    }

    public static void SetBestCount(int duration, int count, StrokeMode mode)
    {
        var d = Load();
        if (mode == StrokeMode.Multiple)
        {
            switch (duration)
            {
                case 5:
                    d.multiBest5 = Math.Max(d.multiBest5, count);
                    break;
                case 10:
                    d.multiBest10 = Math.Max(d.multiBest10, count);
                    break;
                case 30:
                    d.multiBest30 = Math.Max(d.multiBest30, count);
                    break;
                case 60:
                    d.multiBest60 = Math.Max(d.multiBest60, count);
                    break;
            }
        }
        else SetBestCount(duration, count);
        Save(d);
    }
}
