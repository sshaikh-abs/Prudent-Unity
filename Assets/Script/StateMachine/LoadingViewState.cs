using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LoadingViewState), menuName = "ScriptableObjects/Application States/"+nameof(LoadingViewState), order = 1)]
public class LoadingViewState : IState
{
    public LoadingViewState(IStateMachine stateMachine) : base(stateMachine) { }
    public override List<string> transitionableStates => new List<string>()
    {
        nameof(VesselViewState)
    };

    private string vesselName;

    public override void OnStateEnter(List<string> data)
    {
        base.OnStateEnter(data);

        vesselName = data[0];

        IsolationManager.Instance.ResetIsolation();

        CameraService.Instance.itemsToFocus = new List<GameObject>() { CameraService.Instance.defaultFocusLocation.gameObject };
        CameraService.Instance.FocusCamera();

        if (DebugEnabled)
        {
            Debug.Log($"Loading View with Vessel : {vesselName}");
        }
    }

    public override void OnStateLeave(string upcomingState) { }
}