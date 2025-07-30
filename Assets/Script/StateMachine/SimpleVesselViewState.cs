using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(SimpleVesselViewState), menuName = "ScriptableObjects/Application States/"+nameof(SimpleVesselViewState), order = 1)]
public class SimpleVesselViewState : IState
{
    public Material CompartmentConditionVisualizeMaterial;
    public Material CompartmentDefaultMaterial;
    public Material seemLineMaterial;
    private List<Renderer> rendererObjects;
    public SimpleVesselViewState(IStateMachine stateMachine) : base(stateMachine) { }
    public override List<string> transitionableStates => new List<string>()
    {
        nameof(CompartmentViewState)
    };

    private string vesselName;

    private List<GameObject> comparments = new List<GameObject>();

    public override void OnStateEnter(List<string> data)
    {
        base.OnStateEnter(data);

        vesselName = data[0];
        comparments = GroupingManager.Instance.vesselObject.GetCompartments(c => c.compartmentMetaData.gameObject).ToList();
        rendererObjects = new List<Renderer>();
        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
        {
            if(!GroupingManager.Instance.isCompartmentModel)
            {
                rendererObjects.AddRange(c.compartmentMeshObjectReference.GetComponentsInChildren<Renderer>().ToList());
                c.SetActive(true);
                c.compartmentMeshObjectReference.layer = LayerMask.NameToLayer("Default");
                c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().sharedMaterial = GroupingManager.Instance.opaqueMat;
                c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().renderingLayerMask = 1 << 0;
            }
            c.hullpartLookup.Values.ToList().ForEach(h => 
            {
                h.SetActive(false);
            });
        });
        ShaderUpdater.Instance.UpdateShadersBasedOnDisplatMode(rendererObjects);
        CameraService.Instance.itemsToFocus = comparments;
        CameraService.Instance.FocusCamera();
        GroupingManager.Instance.vesselObject.GetCompartments(c => c.GetCollider()).ForEach(c => c.enabled = true);
        GaugingManager.Instance.SetAllGaugePointsActive(false);

        if (DebugEnabled)
        {
            Debug.Log($"Entered Simple Vessel View with Vessel : {vesselName}");
        }
    }

    public override void OnStateLeave(string upcomingState)
    {
        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
        {
            if (!GroupingManager.Instance.isCompartmentModel)
            {
                c.compartmentMeshObjectReference.layer = LayerMask.NameToLayer("Compartments");
                c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().renderingLayerMask = 1 << 2;
                c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().sharedMaterial = GroupingManager.Instance.transparentMaterial;
            }
            c.hullpartLookup.Values.ToList().ForEach(h =>
            {
                h.SetActive(true);
            });
        });

        ShaderUpdater.Instance.UpdateShadersBasedOnDisplatMode(rendererObjects);
    }
}