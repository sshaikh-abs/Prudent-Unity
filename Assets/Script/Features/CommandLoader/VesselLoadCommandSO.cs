using UnityEngine;

[CreateAssetMenu(fileName = "New Vessel Load Command", menuName = "ScriptableObjects/Load Command/Vessel Load", order = 1)]
public class VesselLoadCommandSO : BaseLoadCommandSO
{
    public VesselLoadCommand loadCommand;
    public override ILoadCommand data => loadCommand;
}
