using System.Collections.Generic;
using FMOD;
using YamlDotNet.Serialization;

public class FumenPatternMap
{
    [YamlMember(Order = 1)]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Order = 2)]
    public string Description { get; set; } = string.Empty;

    [YamlMember(Order = 3)]
    public PatternPracticeOptionGroup options = new();

    [YamlMember(Order = 4)]
    public List<string> Patterns { get; set; } = new List<string>();

    public ulong GetLongHashCode()
    {
        unchecked
        {
            const int prime = 16777619;
            long hash = (long)2166136261;

            int bpm = options?.bpm ?? 0;
            float speed = options?.speed ?? 0;
            int minimumNotes = options?.minimumNotes ?? 0;

            hash = (hash ^ bpm.GetHashCode()) * prime;
            hash = (hash ^ speed.GetHashCode()) * prime;
            hash = (hash ^ minimumNotes.GetHashCode()) * prime;

            for (int i = 0; i < Patterns.Count; i++)
            {
                hash = (hash ^ Patterns[i].GetHashCode()) * prime;
            }

            return (ulong)hash;
        }
    }
}
