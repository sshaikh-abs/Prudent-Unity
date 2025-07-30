using System.IO;
using UnityEditor;
using UnityEngine;

public static class JsonFileWriter
{
    public static void WriteJsonToFile(string jsonString, string fileName)
    {
#if UNITY_EDITOR

        string path = EditorUtility.SaveFilePanel("Save JSON", fileName, ".json", "json");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            // Write the JSON string to the specified file
            File.WriteAllText(path, jsonString);
            Debug.Log($"JSON written to file: {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to write JSON to file: " + ex.Message);
        }

        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public static void WriteJsonToFileAtPath(string jsonString, string fileName, string path)
    {
#if UNITY_EDITOR

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            // Write the JSON string to the specified file
            File.WriteAllText($"{path}/{fileName}.json", jsonString);
            Debug.Log($"JSON written to file: {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to write JSON to file: " + ex.Message);
        }

        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
