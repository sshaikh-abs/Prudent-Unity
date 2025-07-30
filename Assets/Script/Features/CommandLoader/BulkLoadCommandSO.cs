using UnityEngine;

[CreateAssetMenu(fileName = "New Bulk Load Command", menuName = "ScriptableObjects/Load Command/Bulk Load", order = 1)]
public class BulkLoadCommandSO : BaseLoadCommandSO
{
    public override ILoadCommand data => loadCommand;
    public BulkLoadCommand loadCommand;
}