using System;
using System.Collections.Generic;
using System.Linq;

public class BiDictionary<TKey, TValue>
{
    private Dictionary<TKey, TValue> forward = new Dictionary<TKey, TValue>();
    private Dictionary<TValue, TKey> reverse = new Dictionary<TValue, TKey>();

    public void Add(TKey key, TValue value, string data = "")
    {
        if (forward.ContainsKey(key) || reverse.ContainsKey(value))
        {
            throw new ArgumentException($"Duplicate key or value at {data}");
        }

        forward.Add(key, value);
        reverse.Add(value, key);
    }

    public List<TValue> GetAllValues()
    {
        return forward.Values.ToList();
    }

    public List<TKey> GetAllKeys()
    {
        return forward.Keys.ToList();
    }

    public bool RemoveByKey(TKey key)
    {
        if (!forward.TryGetValue(key, out TValue value))
            return false;

        forward.Remove(key);
        reverse.Remove(value);
        return true;
    }

    public bool RemoveByValue(TValue value)
    {
        if (!reverse.TryGetValue(value, out TKey key))
            return false;

        reverse.Remove(value);
        forward.Remove(key);
        return true;
    }

    public bool ContainsValue(TValue value)
    {
        return reverse.ContainsKey(value);
    }

    public bool ContainsKey(TKey key)
    {
        return forward.ContainsKey(key);
    }

    public void Foreach(Action<KeyValuePair<TKey, TValue>> action)
    {
        foreach (var item in forward)
        {
            action.Invoke(item);
        }
    }

    public TValue GetValue(TKey key) => forward[key];
    public TKey GetKey(TValue value) => reverse[value];

    public bool TryGetValue(TKey key, out TValue value) => forward.TryGetValue(key, out value);
    public bool TryGetKey(TValue value, out TKey key) => reverse.TryGetValue(value, out key);

    public int Count => forward.Count;

    public void Clear()
    {
        forward.Clear();
        reverse.Clear();
    }
}
