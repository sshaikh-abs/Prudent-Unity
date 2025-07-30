using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu(fileName = nameof(CompartmentSelectionViewState), menuName = "ScriptableObjects/Application States/" + nameof(CompartmentSelectionViewState), order = 1)]
public class CompartmentSelectionViewState : VesselViewState
{
    public Material CompartmentVisualizeMaterial;
    public Material CompartmentDefaultMaterial;

    public CompartmentSelectionViewState(IStateMachine stateMachine) : base(stateMachine) { }

    public override void OnStateEnter(List<string> data)
    {
        base.OnStateEnter(data);
        var rendererObjects = new List<Renderer>();
        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c => c.data.ForEach(h => h.data.ForEach(s => s.metadata.gameObject.SetActive(false))));
        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c => 
        {
            var renderer = c.compartmentMeshObjectReference.GetComponent<MeshRenderer>();
            rendererObjects.Add(renderer);
            renderer.material = CompartmentVisualizeMaterial;
            c.compartmentMeshObjectReference.layer = LayerMask.NameToLayer("Default");
            c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().renderingLayerMask = 1 << 0;
        });
        ShaderUpdater.Instance.UpdateShadersBasedOnDisplatMode(rendererObjects);

        //GroupingManager.Instance.RunOverPlates(plate => plate.SetActive(false));
        //GroupingManager.Instance.RunOverCompartments(compartment => 
        //{
        //    compartment.GetComponent<MeshRenderer>().sharedMaterial = CompartmentVisualizeMaterial;
        //    compartment.layer = LayerMask.NameToLayer("Default");
        //});
    }

    public override void OnStateLeave(string upcomingState)
    {
        base.OnStateLeave(upcomingState);
        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c => c.data.ForEach(h => h.data.ForEach(s => s.metadata.gameObject.SetActive(true))));
        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
        {
            c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().material = CompartmentDefaultMaterial;
            c.compartmentMeshObjectReference.GetComponent<MeshRenderer>().renderingLayerMask = 1 << 2;
            c.compartmentMeshObjectReference.layer = LayerMask.NameToLayer("Compartments");
        });
        //GroupingManager.Instance.RunOverPlates(plate => plate.SetActive(true));
        //GroupingManager.Instance.RunOverCompartments(compartment =>
        //{
        //    compartment.GetComponent<MeshRenderer>().sharedMaterial = CompartmentDefaultMaterial;
        //    compartment.layer = LayerMask.NameToLayer("Compartments");
        //});
    }
}