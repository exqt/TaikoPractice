using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class DrumometerBest
{
    public int best5;
    public int best10;
    public int best30;
    public int best60;

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
}
