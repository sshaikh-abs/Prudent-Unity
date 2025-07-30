using System.Collections.Generic;

/// <summary>
/// JSON Friendly Branch class with a generic type that accepts both leaf and branch class.
/// </summary>
/// <typeparam name="T">Type of Child</typeparam>
[System.Serializable]
public class Branch_JSONFO<T>
{
    /// <summary>
    /// Title
    /// </summary>
    public string name;

    /// <summary>
    /// List of children
    /// </summary>
    public List<T> data;

    public Branch_JSONFO() { }
    public Branch_JSONFO(string name)
    {
        this.name = name;
        data = new List<T>();
    }
}