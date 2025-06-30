using System.Data.OleDb;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class ScoreManager
{
    private static ScoreManager _instance;

    public static ScoreManager Instance
    {
        get
        {
            _instance ??= new ScoreManager();
            return _instance;
        }
    }

    static readonly string scorePath = "Score/";

    public void RecordScore(FumenPatternMap fumenMap, JudgeStatistic judgeStatistic)
    {
        if (fumenMap == null || judgeStatistic == null)
        {
            Debug.Log("FumenMap or JudgeStatistic is null");
            return;
        }

        Score score = new()
        {
            Accuracy = judgeStatistic.DecrementalAccuracy,
            HandFinesse = judgeStatistic.HandFinesse
        };

        var oldScore = GetScore(fumenMap);
        oldScore ??= new Score
        {
            Accuracy = 0.0f,
            HandFinesse = 0.0f
        };

        score = score.Max(oldScore);
        var hash = fumenMap.GetLongHashCode();

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        string yaml = serializer.Serialize(score);

        var path = Path.Combine(scorePath, hash.ToString());
        FileManager.Instance.WriteFile(path, yaml);
    }

    public Score GetScore(FumenPatternMap fumenMap)
    {
        if (fumenMap == null) return null;
        var hash = fumenMap.GetLongHashCode();

        var path = Path.Combine(scorePath, hash.ToString());
        if (!FileManager.Instance.ExistsFile(path)) return null;

        string content = FileManager.Instance.ReadFile(path);

        if (!string.IsNullOrEmpty(content))
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            try
            {
                return deserializer.Deserialize<Score>(content);
            }
            catch
            {
                Debug.LogError($"Failed to deserialize score for hash: {hash}");
            }
        }

        return null;
    }
}
