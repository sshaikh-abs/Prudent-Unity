using UnityEngine;

public class BaseLoadCommandSO : ScriptableObject
{
    public new string name;
    public virtual ILoadCommand data { get; }
}