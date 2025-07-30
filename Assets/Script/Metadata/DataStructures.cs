using System.Collections.Generic;

namespace GLTFast.Custom
{
    [System.Serializable]
    public class MetadataKeyValuePair
    {
        public string Key;
        public string Value;

        public MetadataKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    [System.Serializable]
    public class Metadata
    {
        public List<MetadataKeyValuePair> metaData = new List<MetadataKeyValuePair>();

        public void SetData(Dictionary<string, object> newData)
        {
            foreach (var item in newData)
            {
                metaData.Add(new MetadataKeyValuePair(item.Key, item.Value.ToString()));
            }
        }

        public void AddMetaDataEntry(string key, string value)
        {
            metaData.Add(new MetadataKeyValuePair(key, value));
        }
    }

    [System.Serializable]
    public class NodeMetaData
    {
        public Dictionary<string, object> extras;
    }

    [System.Serializable]
    public class GltfMetaDataRoot
    {
        public NodeMetaData[] nodes;
    }
}
