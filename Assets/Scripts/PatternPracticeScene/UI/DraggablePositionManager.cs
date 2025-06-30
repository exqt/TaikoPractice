using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[Serializable]
public struct DraggablePosition
{
    public float x;
    public float y;

    public DraggablePosition(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

// 드래그 가능한 UI 컴포넌트 위치를 YAML 파일로 저장하고 불러오는 관리 클래스
public static class DraggablePositionManager
{
    private static readonly string SaveFolderName = "UILayouts";
    private static readonly string FileName = "draggable_positions.yaml";
    private static Dictionary<string, DraggablePosition> positionCache;

    // 저장에 사용될 클래스
    [Serializable]
    private class PositionCollection
    {
        public Dictionary<string, DraggablePosition> positions { get; set; } = new Dictionary<string, DraggablePosition>();
    }

    // 저장 파일 전체 경로
    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFolderName, FileName);

    // 캐시 데이터 초기화/로드
    private static void EnsureCacheIsLoaded()
    {
        if (positionCache != null) return;

        positionCache = new Dictionary<string, DraggablePosition>();
        LoadPositionsFromDisk();
    }

    // 모든 위치 정보 불러오기
    private static void LoadPositionsFromDisk()
    {
        string filePath = SaveFilePath;
        if (!File.Exists(filePath)) return;

        try
        {
            string yaml = File.ReadAllText(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            PositionCollection collection = deserializer.Deserialize<PositionCollection>(yaml);

            if (collection.positions != null)
            {
                positionCache = collection.positions;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading draggable positions: {e.Message}");
        }
    }

    // 모든 위치 정보 저장하기
    private static void SavePositionsToDisk()
    {
        if (positionCache == null) return;

        try
        {
            PositionCollection collection = new PositionCollection
            {
                positions = positionCache
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(collection);

            string directoryPath = Path.GetDirectoryName(SaveFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(SaveFilePath, yaml);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving draggable positions: {e.Message}");
        }
    }

    // 위치 저장
    public static void SavePosition(string uniqueId, DraggablePosition position)
    {
        EnsureCacheIsLoaded();
        positionCache[uniqueId] = position;
        SavePositionsToDisk();
    }

    // 위치 불러오기
    public static bool TryLoadPosition(string uniqueId, out DraggablePosition position)
    {
        position = new DraggablePosition(0, 0);
        EnsureCacheIsLoaded();

        return positionCache.TryGetValue(uniqueId, out position);
    }

    // 특정 위치 삭제
    public static void DeletePosition(string uniqueId)
    {
        EnsureCacheIsLoaded();

        if (positionCache.ContainsKey(uniqueId))
        {
            positionCache.Remove(uniqueId);
            SavePositionsToDisk();
        }
    }

    // 모든 레이아웃 초기화
    public static void ResetAllPositions()
    {
        positionCache = new Dictionary<string, DraggablePosition>();
        SavePositionsToDisk();
        Debug.Log("All UI layouts have been reset!");
    }
}
