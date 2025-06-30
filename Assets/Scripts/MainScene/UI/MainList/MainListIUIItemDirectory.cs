using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class UIMainListDirectory : UIMainListItem
{
    public GameObject patternPracticeItemPrefab;

    readonly List<GameObject> items = new();

    public bool isExpanded { get; private set; } = false;

    public Image background;

    public static Color closedColor = new(1.0f, 0.8f, 0.5f, 1f);
    public static Color openedColor = new(0.9f, 0.7f, 0.4f, 1f);

    public override void OnTaikoUISelect()
    {
        if (isExpanded)
        {
            Collapse();
            isExpanded = false;
        }
        else
        {
            Expand("PatternMaps/" + gameObject.name);
            isExpanded = true;
        }
    }

    public void Expand(string path = null)
    {
        path ??= "PatternMaps/" + gameObject.name;
        var files = FileManager.Instance.GetFiles(path);

        files.Sort();
        files.Reverse();

        foreach (var file in files)
        {
            if (!file.EndsWith(".yaml")) continue;

            FumenPatternMap fumenPatternMap;
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var itemPath = Path.Combine(path, file);
            var str = FileManager.Instance.ReadFile(itemPath);

            try
            {
                fumenPatternMap = deserializer.Deserialize<FumenPatternMap>(str);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize {itemPath}: {e.Message}");
                continue;
            }

            var item = Instantiate(patternPracticeItemPrefab, transform.parent).GetComponent<UIMainListItemFoldable>();
            var fileWithoutExtension = file[..^5];
            var dirName = gameObject.name;

            item.title.text = fileWithoutExtension;
            item.gameObject.name = $"{dirName}/{fileWithoutExtension}";

            item.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);

            if (string.IsNullOrEmpty(fumenPatternMap.Name))
            {
                fumenPatternMap.Name = fileWithoutExtension;
            }

            var patternPracticeItem = item.GetComponent<UIMainListItemPatternPractice>();
            patternPracticeItem.SetData(fumenPatternMap);

            items.Add(item.gameObject);
        }

        var mainList = GetComponentInParent<UIMainList>();
        mainList.UpdateItemList();
        mainList.UpdateItemPositions();

        isExpanded = true;
        background.color = openedColor;
    }

    public void Collapse()
    {
        foreach (var item in items)
        {
            item.SetActive(false);
            Destroy(item);
        }

        items.Clear();

        var mainList = GetComponentInParent<UIMainList>();
        mainList.UpdateItemList();
        mainList.UpdateItemPositions();
        mainList.MoveItemCursor(mainList.SelectedIndex, immediate: true);

        TaikoUIMoveManager.Instance.SetItem(this);

        isExpanded = false;
        background.color = closedColor;
    }
}
