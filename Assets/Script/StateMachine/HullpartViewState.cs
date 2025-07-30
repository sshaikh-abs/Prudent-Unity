using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(HullpartViewState), menuName = "ScriptableObjects/Application States/" + nameof(HullpartViewState), order = 3)]
public class HullpartViewState : IState
{
    public HullpartViewState(IStateMachine stateMachine) : base(stateMachine) { }
    public override List<string> transitionableStates => new List<string>()
    {
        nameof(CompartmentViewState),
        nameof(VesselViewState)
    };

    public string vesselName { get; private set; }
    public string compartmentUid { get; private set; }
    public string hullpartName { get; private set; }
    public bool isViewingFullHullpart { get; private set; }

    private List<GameObject> plates = new List<GameObject>();

    public Hullpart GetTargettedHullpartObject()
    {
        return GroupingManager.Instance.vesselObject.GetHullpart(compartmentUid, hullpartName);
    }

    public override void OnStateEnter(List<string> data)
    {
        base.OnStateEnter(data);

        vesselName = data[0];
        compartmentUid = data[1];
        hullpartName = data[2];

        if(data.Count > 3)
        {
            isViewingFullHullpart = data[3] == "_Full";
        }
        else
        {
            isViewingFullHullpart = false;
        }

#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        isViewingFullHullpart = false;
#endif

        var targetHullpart = GroupingManager.Instance.vesselObject.GetHullpart(compartmentUid, hullpartName);
        plates = GroupingManager.Instance.vesselObject.GetSubparts(compartmentUid, hullpartName, SubpartType.Plate, s => s.subpartObjectMeshReference);

        Vessel.stopwatch.Start();

        if (isViewingFullHullpart == false)
        {
            GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
            {
                c.SetActive(c.uid == compartmentUid, false);
            });

            GroupingManager.Instance.vesselObject.GetHullparts(compartmentUid, h => h).ForEach(h =>
            {
                //h.SetActiveAsync(h.name == hullpartName, Vessel.isHullpartLessVessel ? null : s => 
                h.SetActive(h.name == hullpartName,s => 
                {
                    s.GetCollider().enabled = h.name == hullpartName;
                });
            });

            //GroupingManager.Instance.vesselObject.GetSubparts(compartmentUid, hullpartName, SubpartType.All, s => s.GetCollider()).ForEach(s =>
            //{
            //    s.enabled = true;
            //});
        }
        else
        {
            GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
            {
                bool show = targetHullpart.associatedCompartments.Contains(c.uid);
                if(show)
                {
                    LoadBracketsAndStiffeners(c);
                }
                c.SetActive(show);
            });

            foreach (var item in targetHullpart.associatedCompartments)
            {

                GroupingManager.Instance.vesselObject.GetHullparts(item, h => h).ForEach(h =>
                {
                    h.SetActive(h.name == hullpartName);
                });

                GroupingManager.Instance.vesselObject.GetSubparts(item, hullpartName, SubpartType.All, s => s.GetCollider()).ForEach(s =>
                {
                    s.enabled = true;
                });
            }
        }

        Vessel.stopwatch.Stop();
        Vessel.stopwatch.Reset();

        CameraService.Instance.itemsToFocus = plates;
        CameraService.Instance.FocusCamera();

        //GaugingManager.Instance.IsolateGaugePoints(compartmentUid, hullpartName);
        //IconSpawningManager.Instance.IsolateIcons(compartmentUid, hullpartName);

        //GaugingManager.Instance.IsolateGaugePoints(compartmentUid, hullpartName);
        IconSpawningManager.Instance.IsolateIcons(compartmentUid, hullpartName);
        GameEvents.TriggerGaugingWorkflowAvailability?.Invoke(true);

        if (DebugEnabled)
        {
            Debug.Log($"Entered Hullpart View with Hull part : {hullpartName}");
        }

        GameEvents.OnHullpartIsolated?.Invoke(targetHullpart);
    }

    public void LoadBracketsAndStiffeners(Compartment compartment)
    {
        if (compartment.HaveBracketsLoaded == false)
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
                EventBroadcaster.OnCompartmentLoadingStarted?.Invoke(compartment.name);
                GltfLoader.Instance.LoadModelsRuntime(GroupingManager.Instance.loadCommand, GroupingManager.Instance.loadCommand.vesselId, urls, true, OnComplete: () =>
                {
                    EventBroadcaster.OnCompartmentLoadingComplete?.Invoke(compartment.name);
                    Debug.Log($"Compartment Name : {compartment.name} || Thickness {compartment.GetAvgOriginalThickness()}");
                });
            }

            compartment.MarkBracektsLoaded();
        }
    }

    public override void OnStateLeave(string upcomingState)
    {
        if (isViewingFullHullpart == false)
        {
            GroupingManager.Instance.vesselObject.GetHullparts(compartmentUid, h => h).ForEach(h =>
            {
                h.SetActive(true);
            });
            GroupingManager.Instance.vesselObject.GetSubparts(compartmentUid, hullpartName, SubpartType.All, s => s.GetCollider()).ForEach(s =>
            {
                s.enabled = false;
            });
        }
        else
        {
            var targetHullpart = GroupingManager.Instance.vesselObject.GetHullpart(compartmentUid, hullpartName);

            foreach (var item in targetHullpart.associatedCompartments)
            {
                GroupingManager.Instance.vesselObject.GetHullparts(item, h => h).ForEach(h =>
                {
                    h.SetActive(true);
                });

                GroupingManager.Instance.vesselObject.GetCompartments(c => c).ForEach(c =>
                {
                    c.SetActive(c.uid == compartmentUid);
                });

                GroupingManager.Instance.vesselObject.GetSubparts(item, hullpartName, SubpartType.All, s => s.GetCollider()).ForEach(s =>
                {
                    s.enabled = false;
                });
            }
        }

        //GroupingManager.Instance.vesselObject.GetHullparts(compartmentUid, h => h).ForEach(h =>
        //{
        //    h.SetActive(true);
        //});
        ContextMenuManager.Instance.PerformShowAll();
    }
}