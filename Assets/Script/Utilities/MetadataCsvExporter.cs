using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR == true
using UnityEditor;
#endif
using GLTFast.Custom;

public static class MetadataCsvExporter
{
	//[MenuItem("Tools/Export Metadata to CSV")]
	public static void ExportToCSV(List<MetadataComponent> components, string name = "", string rootPath = "")
	{
		// Step 1: Collect all unique keys
		HashSet<string> allKeysSet = new HashSet<string>();
		foreach (var comp in components)
		{
			foreach (var pair in comp.metaData.metaData)
			{
				allKeysSet.Add(pair.Key);
			}
		}

		List<string> allKeys = new List<string>(allKeysSet);
		allKeys.Sort(); // Optional for consistent ordering

		// Step 2: Write header
		List<string> lines = new List<string> { string.Join(",", allKeys) };

		// Step 3: Write rows
		foreach (var comp in components)
		{
			Dictionary<string, string> localMap = new Dictionary<string, string>();
			foreach (var pair in comp.metaData.metaData)
			{
				localMap[pair.Key] = pair.Value;
			}

			List<string> row = new List<string>();
			foreach (var key in allKeys)
			{
				localMap.TryGetValue(key, out string val);
				row.Add(EscapeCsv(val));
			}

			lines.Add(string.Join(",", row));
		}

		// Step 4: Save
		string path = "";
#if UNITY_EDITOR == true
		var targetRoot = string.IsNullOrEmpty(rootPath) ? Application.dataPath : rootPath;
        path = EditorUtility.SaveFilePanel("Save Metadata CSV", targetRoot, name.IsNullOrEmpty() ? "metadata" : name, "csv");
#endif

        if (!string.IsNullOrEmpty(path))
		{
			File.WriteAllLines(path, lines);
			Debug.Log($"Metadata CSV saved to: {path}");
		}
	}

    public static void ExportToCSV_RAW(List<Metadata> metadata)
    {
        // Step 1: Collect all unique keys
        HashSet<string> allKeysSet = new HashSet<string>();
        foreach (var comp in metadata)
        {
            foreach (var pair in comp.metaData)
            {
                allKeysSet.Add(pair.Key);
            }
        }

        List<string> allKeys = new List<string>(allKeysSet);
        allKeys.Sort(); // Optional for consistent ordering

        // Step 2: Write header
        List<string> lines = new List<string> { string.Join(",", allKeys) };

        // Step 3: Write rows
        foreach (var comp in metadata)
        {
            Dictionary<string, string> localMap = new Dictionary<string, string>();
            foreach (var pair in comp.metaData)
            {
                localMap[pair.Key] = pair.Value;
            }

            List<string> row = new List<string>();
            foreach (var key in allKeys)
            {
                localMap.TryGetValue(key, out string val);
                row.Add(EscapeCsv(val));
            }

            lines.Add(string.Join(",", row));
        }

		// Step 4: Save
		string path = "";
#if UNITY_WEBGL == false && UNITY_EDITOR == true
        path = EditorUtility.SaveFilePanel("Save Metadata CSV", Application.dataPath, "metadata", "csv");
#endif
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllLines(path, lines);
            Debug.Log($"Metadata CSV saved to: {path}");
        }
    }

    private static string EscapeCsv(string input)
	{
		if (string.IsNullOrEmpty(input)) return "";
		if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
		{
			input = input.Replace("\"", "\"\"");
			return $"\"{input}\"";
		}
		return input;
	}
}
