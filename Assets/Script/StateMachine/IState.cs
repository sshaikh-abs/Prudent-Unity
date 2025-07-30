using System.Collections.Generic;
using UnityEngine;

public abstract class IState : ScriptableObject
{
    public float distanceFromCameraMultiplier;
    public static bool DebugEnabled;

    /// <summary>
    /// Currently not being used as the state change might happen from external source as well.
    /// </summary>
    public virtual List<string> transitionableStates => new List<string>();
    public IStateMachine StateMachine;
    public List<string> keyData;

    public IState(IStateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public virtual void OnStateEnter(List<string> data)
    {
        keyData = data;
        
    }
    public abstract void OnStateLeave(string upcomingState);
    public virtual void ResetMacine() { }
}
