using System;

public static class GameEvents
{
    public static Action<bool> TriggerGaugingWorkflowAvailability;
    public static Action OnHullpartViewDeactivated;
    public static Action<bool> OnGaugingModeSet;
    public static Action<bool> OnPostVesselLoaded;
    public static Action<Hullpart> OnHullpartIsolated;
    public static Action<Hullpart,bool> OnSetHullPartActive;

    public static Action<Compartment> OnCompartmentIsolated;
    public static Action OnVesselView;

    public static Action<bool> SetPlatesActive;
    public static Action<bool> SetStiffnersActive;
    public static Action<bool> SetBracketsActive;
    public static Action<bool> SetGaugePointLabelActive;
    public static Action<bool> SetGaugePointViewActive;
}
