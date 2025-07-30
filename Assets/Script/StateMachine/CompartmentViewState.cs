using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(CompartmentViewState), menuName = "ScriptableObjects/Application States/" + nameof(CompartmentViewState), order = 2)]
public class CompartmentViewState : IState
{
    public CompartmentViewState(IStateMachine stateMachine) : base(stateMachine) { }
    public override List<string> transitionableStates => new List<string>()
    {
        nameof(HullpartViewState),
        nameof(VesselViewState)
    };

    //private string vesselName;
    private string compartmentName;
    //private List<GameObject> hullParts;

    public override void OnStateEnter(List<string> data)
    {
        base.OnStateEnter(data);
        //vesselName = data[0];
        compartmentName = data[1];

        var compartment = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName);

        if(compartment.HaveBracketsLoaded == false)
        {
            List<string> urls = new List<string>();

            if (GroupingManager.Instance.loadCommand.bracketsGLBLookup.Contains(compartment.uid))
            {
                urls.Add(GroupingManager.Instance.loadCommand.bracketsGLBLookup[compartment.uid].FirstOrDefault());
            }
            if (GroupingManager.Instance.loadCommand.stiffenersGLBLookup.Contains(compartment.uid))
            {
                urls.Add(GroupingManager.Instance.loadCommand.stiffenersGLBLookup[compartment.uid].FirstOrDefault());
            }

            if (urls.Count > 0)
            {
                EventBroadcaster.OnCompartmentLoadingStarted?.Invoke(compartmentName);
                GltfLoader.Instance.LoadModelsRuntime(GroupingManager.Instance.loadCommand, GroupingManager.Instance.loadCommand.vesselId, urls, true, message: "Loading Brackets and Stiffeners", OnComplete: () => 
                {
                    EventBroadcaster.OnCompartmentLoadingComplete?.Invoke(compartmentName);
                });
            }

            compartment.MarkBracektsLoaded();
        }
        else
        {
            EventBroadcaster.OnCompartmentLoadingComplete?.Invoke(compartmentName);
        }

        //hullParts = GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, h => 
        //{
        //    if(h.hullpartMeshReference != null)
        //    {
        //        return h.hullpartMeshReference.gameObject;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //});
        CameraService.Instance.itemsToFocus = new List<GameObject>()
        {
            compartment.compartmentMetaData.gameObject
        };
        CameraService.Instance.FocusCamera();

        //IsolationManager.Instance.intendedCompartment = compartment.compartmentMetaData.transform;

        GameEvents.TriggerGaugingWorkflowAvailability?.Invoke(true);

        GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c => 
        {
            c.SetActive(c.uid == compartmentName);
        });
        GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, s => s).ForEach(h => 
        {
            //h.hullpartMetadata.transform.parent.gameObject.SetActive(true);
            //h.GetCollider().enabled = true;
            h.SetActiveAllColliders(true);
        });

        //GaugingManager.Instance.IsolateGaugePoints(compartment.uid);
        IconSpawningManager.Instance.IsolateIcons(compartment.uid);

        if(CommunicationManager.Instance.bracketsActive)
        {
            CommunicationManager.Instance.SetBracketsActive_Extern(true);
        }

        if (CommunicationManager.Instance.stiffenersActive)
        {
            CommunicationManager.Instance.SetStiffenersActive_Extern(true);
        }

        if (DebugEnabled)
        {
            Debug.Log($"Entered Compartment View with compartment : {compartmentName}");
        }
        GameEvents.OnCompartmentIsolated?.Invoke(compartment);
    }

    public string GetTargettedCompartemnt()
    {
        return compartmentName;
    }

    public Compartment GetTargettedCompartemntObject()
    {
        return GroupingManager.Instance.vesselObject.GetCompartment(compartmentName);
    }

    public override void OnStateLeave(string upcomingState)
    {
        GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, s => s).ForEach(h =>
        {
            //h.GetCollider().enabled = false;
            h.SetActiveAllColliders(false);
        });

        ContextMenuManager.Instance.PerformShowAll();
    }

    //private void SetActiveHullParts(bool value)
    //{
    //    foreach (GameObject hullPart in hullParts)
    //    {
    //        hullPart.gameObject.SetActive(true);
    //        hullPart.FindChildWithSubstringInName("FULL").gameObject.SetActive(value);
    //    }
    //}
}


public static class EventBroadcaster
{
    public static System.Action<string> OnCompartmentLoadingStarted;
    public static System.Action<string> OnCompartmentLoadingComplete;
}