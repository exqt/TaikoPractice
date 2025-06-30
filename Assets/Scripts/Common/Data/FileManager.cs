using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    private static FileManager instance = null;
    public static FileManager Instance
    {
        get
        {
            instance ??= new FileManager();
            return instance;
        }
    }

    private readonly string directoryPath;

    public FileManager()
    {
        directoryPath = Application.dataPath;
        directoryPath = Path.GetFullPath(Path.Combine(directoryPath, @"..\"));
    }

    public string ReadFile(string path)
    {
        path = Path.Combine(directoryPath, path);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string fileContent = File.ReadAllText(path);
            return fileContent;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading file: {ex.Message}");
            return null;
        }
    }

    public bool ExistsFile(string path)
    {
        path = Path.Combine(directoryPath, path);

        try
        {
            if (File.Exists(path)) return true;
            else return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error checking file existence: {ex.Message}");
            return false;
        }
    }

    public void WriteFile(string path, string content)
    {
        Debug.Log($"Writing file: {path}");
        path = Path.Combine(directoryPath, path);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            Debug.Log($"File written successfully: {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error writing file: {ex.Message}");
        }
    }

    public List<string> GetFiles(string path)
    {
        List<string> fileList = new();

        path = Path.Combine(directoryPath, path);

        try
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                fileList.Add(Path.GetFileName(file));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting files: {ex.Message}");
        }

        return fileList;
    }

    public List<string> GetDirectories(string path)
    {
        List<string> dirList = new();

        path = Path.Combine(directoryPath, path);

        try
        {
            string[] directories = Directory.GetDirectories(path);
            foreach (string dir in directories)
            {
                dirList.Add(Path.GetFileName(dir));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting files: {ex.Message}");
        }

        return dirList;
    }
}
