using GLTFast.Custom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class PetrobrasDataHelper : MonoBehaviour
{
    List<Metadata> ParseInputList(string[] inputLines)
    {
        var pattern = @"^([A-Za-z0-9]+)_([A-Za-z0-9]+)x([A-Za-z0-9]+)x([A-Za-z0-9]+)x([A-Za-z0-9]+)_([A-Za-z0-9]+)-([A-Za-z0-9]+)_([^_]+)_([A-Za-z0-9]+)_([0-9]{4})$";
        var regex = new Regex(pattern);

        var keys = new[]
        {
            "TYPE", "POSITION", "GROUP", "SUB_GROUP", "SUB_TYPE",
            "ID", "SUB_TYPE_PREFIX", "DIMENSIONS", "MATERIAL", "YEAR"
        };

        var result = new List<Metadata>();

        foreach (var line in inputLines)
        {
            var match = regex.Match(line);
            if (!match.Success)
            {
                Debug.LogWarning($"Invalid line format: {line}");
                continue;
            }

            var kvList = new List<MetadataKeyValuePair>();
            for (int i = 0; i < keys.Length; i++)
            {
                kvList.Add(new MetadataKeyValuePair(keys[i], match.Groups[i + 1].Value));
            }

            result.Add(new Metadata()
            {
                metaData = kvList,
            });
        }

        return result;
    }

    [ContextMenu(nameof(Write))]
    public void Write()
    {
        var Names = GroupingManager.Instance.vesselObject.GetAllSubparts().Select(s => s.metadata.GetValue("NAME")).ToArray();
        MetadataCsvExporter.ExportToCSV_RAW(ParseInputList(Names));
    }
}
