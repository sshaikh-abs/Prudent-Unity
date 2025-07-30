using System;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationStateMachine : SingletonMono<ApplicationStateMachine>, IStateMachine
{
    public Action OnStateChanged;
    public string vesselName;

    private IState currentState;
    private BiDictionary<string, IState> stateMap;
    public string currentStateName => GetCurrentStateName();
    public IState CurrentState => currentState;

    public List<IState> structuralModelViewStates;
    public List<IState> compartmentModelViewStates;

    private bool isWorkingOnSimpleModel = false;
    public bool IsWorkingOnSimpleModel => isWorkingOnSimpleModel;

    public IState GetState(string name)
    {
        return stateMap.GetValue(name);
    }

    private void Awake()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    private string GetCurrentStateName()
    {
        if(currentState == null)
        {
            return null;
        }
        else
        {
            return currentState.GetType().Name;
        }
    }

    public bool TryGetCurrentState<STATE>(out STATE state) where STATE : IState
    {
        try
        {
            state = (STATE)currentState;
            return true;
        }
        catch
        {
            state = null;
            return false;
        }
    }

    public STATE GetCurrentState<STATE>() where STATE : IState
    {
        try
        {
            return (STATE)currentState;
        }
        catch
        {
            return null;
        }
    }

    public void GoToLoadingViewState(string vesselName)
    {
        this.vesselName = vesselName;

        stateMap = new BiDictionary<string, IState>();
        foreach (var stateObject in (isWorkingOnSimpleModel) ? compartmentModelViewStates : structuralModelViewStates)
        {
            stateMap.Add(stateObject.GetType().Name, stateObject);
        }

        ChangeState(stateMap.GetValue(nameof(LoadingViewState)), new List<string>() { vesselName });
    }

    public void InitializeStateMachine(string vesselName, bool compartmentModel = false)
    {
        isWorkingOnSimpleModel = compartmentModel;
        this.vesselName = vesselName;

        stateMap = new BiDictionary<string, IState>();

        foreach (var stateObject in (isWorkingOnSimpleModel) ? compartmentModelViewStates : structuralModelViewStates)
        {
            stateMap.Add(stateObject.GetType().Name, stateObject);
        }

        ChangeState(stateMap.GetValue((isWorkingOnSimpleModel) ? nameof(SimpleVesselViewState) : nameof(VesselViewState)), new List<string>() { vesselName });
    }

    public void ChangeState(IState state, List<string> data)
    {
        CameraService.Instance.SetDistanceFromTarget(state.distanceFromCameraMultiplier);

        if(currentState != null)
        {
            currentState.OnStateLeave(stateMap.GetKey(state));
        }

        currentState = state;

        if (currentState != null)
        {
            currentState.OnStateEnter(data);
            CommunicationManager.Instance.HandleCurrentStatename_Extern(currentStateName);
        }
        
        OnStateChanged?.Invoke();
    }

    public void ExcuteStateTransition(string targetState, List<string> data)
    {
        List<string> transitionData = new List<string>() { vesselName };
        transitionData.AddRange(data);
      
        ChangeState(stateMap.GetValue(targetState), transitionData);
        ContextMenuManager.Instance.SetInformationObject(false);
    }

    public List<string> GetTransitionableStates()
    {
        return currentState.transitionableStates;
    }

    public void ResetStateMachine()
    {
        ContextMenuManager.Instance.SetInformationObject(false);
        InitializeStateMachine(vesselName, IsWorkingOnSimpleModel);
    }
}