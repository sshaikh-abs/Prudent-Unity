using UnityEngine;
using GLTFast.Custom;
using System.Collections.Generic;

public class MetadataComponent : MonoBehaviour
{
    public Metadata metaData = new Metadata();
    public Dictionary<string, string> metadatalookUp = new Dictionary<string, string>();

    /// <summary>
    /// Sets the metadata for the component.
    /// </summary>
    /// <param name="metaData">metadata to inject</param>
    public void SetMetadata(Metadata metaData)
    {
        this.metaData = metaData;
        foreach (var item in metaData.metaData)
        {
            metadatalookUp.Add(item.Key, item.Value);
        }
    }

    /// <summary>
    /// Gets the value of a specific key from the metadata.
    /// </summary>
    /// <param name="Key">Key to look for</param>
    /// <param name="isCaseSensitive">should the check be case sensitive</param>
    /// <returns>data</returns>
    public string GetValue(string Key, bool isCaseSensitive = true)
    {
        if (metadatalookUp.ContainsKey(Key))
        {
            return isCaseSensitive ? metadatalookUp[Key] : metadatalookUp[Key].ToLower();
        }
        return null;
    }

    /// <summary>
    /// Tries to get the value of a specific key from the metadata.
    /// </summary>
    /// <param name="Key">Key to look for</param>
    /// <param name="value">The out value if found</param>
    /// <param name="isCaseSensitive">should the check be case sensitive</param>
    /// <returns>has found the value in the metadata</returns>
    public bool TryGetValue(string Key, out string value, bool isCaseSensitive = true)
    {
        if (metadatalookUp.ContainsKey(Key))
        {
            value = isCaseSensitive ? metadatalookUp[Key] : metadatalookUp[Key].ToLower();
            return true;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Checks if the metadata contains a specific key.
    /// </summary>
    /// <param name="Key">Key to check for</param>
    /// <returns>Result</returns>
    public bool ContainsKey(string Key)
    {
        return metadatalookUp.ContainsKey(Key);
    }

    /// <summary>
    /// Returns the metadata table in JSON format.
    /// </summary>
    /// <returns></returns>
    public string  DisplayMetadataTable()
    {
        return JsonUtility.ToJson(metaData);
    }
}