using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class ComplexShipManager_DataClass : DataClass
{
    public string shipName;
}

public class ComplexShipManager : BaseService<ComplexShipManager_DataClass>
{
    public override void Kill() { }

    public override void Update() { }

    public bool LoadedStructuralModel = false;

    public void Initialize(GameObject shipObject, Action OnBuildVesselComplete, bool isCompartmentModel, string vesselId)
    {
        LoadedStructuralModel = !isCompartmentModel;
        data.shipName = shipObject.name;
        CallBuildVessel(isCompartmentModel, () =>
        {
            OnBuildVesselComplete?.Invoke();
            InitializeStateMachine(isCompartmentModel);
        }, vesselId);
    }

    private async void CallBuildVessel(bool isCompartmentModel, Action OnBuildVesselComplete, string vesselId)
    {
        await GroupingManager.Instance.BuildVesselObject(isCompartmentModel, OnBuildVesselComplete, vesselId);
    }

    public void InitializeStateMachine(bool isCompartmentModel = false)
    {
        ApplicationStateMachine.Instance.InitializeStateMachine(data.shipName, isCompartmentModel);
    }

    public void EnterHullPartView(GameObject intendedPartObject)
    {
        var hullpart = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(intendedPartObject);
        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(HullpartViewState), new List<string>() { hullpart.owningCompartmentUid, hullpart.name });
        CommunicationManager.Instance.HandleIsolatedObject_Extern(hullpart.name);
    }

    public void EnterFullHullPartView(GameObject intendedPartObject)
    {
        var hullpart = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(intendedPartObject);
        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(HullpartViewState), new List<string>() { hullpart.owningCompartmentUid, hullpart.name, "_Full" });
        CommunicationManager.Instance.HandleIsolatedObject_Extern(hullpart.name);
    }

    public void EnterCompartmentView(GameObject intendedPart)
    {
        string stateName = ApplicationStateMachine.Instance.currentStateName;
        Compartment compartment = null;
        switch (stateName)
        {
            case nameof(HullpartViewState):
                Subpart subpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(intendedPart);
                compartment = GroupingManager.Instance.vesselObject.GetCompartment(ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>().compartmentUid);
                break;
            case nameof(CompartmentViewState):
                return;
            case nameof(VesselViewState):
                compartment = GroupingManager.Instance.vesselObject.ProcessCompartmentSelection(intendedPart);
                break;
            default:
                break;
        }

        CommunicationManager.Instance.HandleIsolatedObject_Extern(compartment.name);

        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentViewState), new List<string>() { compartment.uid });
    }

    public void EnterVesselView()
    {
        CommunicationManager.Instance.HandleIsolatedObject_Extern("");
        CommunicationManager.Instance.ClearGaugePoints_Extern();
        ApplicationStateMachine.Instance.ResetStateMachine();
        CommunicationManager.Instance.HandleEnterVesselView();
        
    }
}