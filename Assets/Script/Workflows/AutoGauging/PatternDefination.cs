using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Patters Config", menuName = "ScriptableObjects/Configs/Patterns Config", order = 1)]
public class PatternDefination : ScriptableObject
{
    public List<Pattern> patterns = new List<Pattern>();

    public static Dictionary<string, Pattern> lookup = null;

    public Pattern GetPattern(string pattern)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, Pattern>();
            foreach (var item in patterns)
            {
                lookup[item.name] = item;
            }
        }
        if (lookup.ContainsKey(pattern))
        {
            return lookup[pattern];
        }

        return null;
    }
}

[System.Serializable]
public class Pattern
{
    public string name;
    public List<Vector3> points;
    public Vector2 dimensions;
    public int pointsToPick = -1; // -1 means all points will be used
}