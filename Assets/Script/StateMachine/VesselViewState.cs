using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(VesselViewState), menuName = "ScriptableObjects/Application States/"+nameof(VesselViewState), order = 1)]
public class VesselViewState : IState
{
    public VesselViewState(IStateMachine stateMachine) : base(stateMachine) { }
    public override List<string> transitionableStates => new List<string>()
    {
        nameof(CompartmentViewState)
    };

    private string vesselName;
    private List<GameObject> comparments;

    public override void OnStateEnter(List<string> data)
    {
        base.OnStateEnter(data);

        GaugingManager.Instance.OnSetGauging(false);
        GameEvents.TriggerGaugingWorkflowAvailability?.Invoke(false);
        vesselName = data[0];

        IsolationManager.Instance.ResetIsolation();
        comparments = GroupingManager.Instance.vesselObject.GetCompartments(c => c.compartmentMetaData.gameObject).ToList();
        GaugingManager.Instance.SetAllGaugePointsActive(true);

        CameraService.Instance.itemsToFocus = comparments;
        CameraService.Instance.FocusCamera();

        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c => 
        {
            c.SetActive(true);
            //c.data.ForEach(h => h.hullpartMetadata.transform.parent.gameObject.SetActive(true));
        });

        GroupingManager.Instance.vesselObject.GetCompartments(c => c.GetCollider()).ForEach(c => c.enabled = true);

        GaugingManager.Instance.SetAllGaugePointsActive(true);
        IconSpawningManager.Instance.SetAllIconsActive(IconSpawningManager.Instance.IconEnabler);

        CommunicationManager.Instance.SetBracketsActive_Extern(false);
        CommunicationManager.Instance.SetStiffenersActive_Extern(false);
        CommunicationManager.Instance.ClearGaugePoints_Extern();

        if (DebugEnabled)
        {
            Debug.Log($"Entered Vessel View with Vessel : {vesselName}");
        }
        GameEvents.OnVesselView?.Invoke();
    }

    public override void OnStateLeave(string upcomingState)
    {
        GroupingManager.Instance.vesselObject.GetCompartments(c => c.GetCollider()).ForEach(c => c.enabled = false);
    }

    private void SetActiveHullParts(bool value)
    {
        foreach (GameObject compartment in comparments)
        {
            compartment.SetActive(value);
        }
    }
}