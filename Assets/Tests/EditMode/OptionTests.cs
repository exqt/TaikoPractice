using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using YamlDotNet.RepresentationModel;

public class OptionTests
{
    private class TestOptionGroup : OptionGroup
    {
        [OptionDesc("Test Int", "An integer option")]
        public int testInt = 42;

        [OptionDesc("Test Float", "A float option")]
        public float testFloat = 3.14f;
    }

    [Test]
    public void EasyTest()
    {
    }

    [Test]
    public void TestSerialization()
    {

    }

    [Test]
    public void TestDeserialization()
    {
        string yamlString = @"
name: Sample Pattern
description: This is a sample pattern.
options:
  bpm: 200
  speed: 1.5
  onExceedMaxStroke: RestartMap
patterns:
- ddkdd.ddkdd.ddd.
- dkd.dkd.dkd.dkd.
- d.k.d.k.dkkkddk.
";

        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        var deserializedPattern = deserializer.Deserialize<FumenPatternMap>(yamlString);

        Assert.IsNotNull(deserializedPattern);
        Assert.AreEqual("Sample Pattern", deserializedPattern.Name);
        Assert.AreEqual(200, deserializedPattern.options.bpm);
        Assert.AreEqual(1.5f, deserializedPattern.options.speed);
        Assert.AreEqual(3, deserializedPattern.Patterns.Count);
    }
}
