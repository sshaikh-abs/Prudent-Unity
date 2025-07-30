using UnityEngine;

public class OutlineManager : SingletonMono<OutlineManager>
{
    public string defaultLayer = "Default";
    public string highlightLayer = "Default";
    public string focusLayer = "Default";

    //public Transform highlightObject;
    public Color uiSelectionColor = Color.red;
    public Color uiButtonDefaultColor = Color.red;

    private OutlineSelectionHandler currentSelection;
    private OutlineSelectionHandler currentHighlight;
    public GameObject CurrentSelectionGameObject => (currentSelection == null) ? null : currentSelection.gameObject;
    public GameObject CurrentHighlightGameObject => (currentHighlight == null) ? null : currentHighlight.gameObject;

    public void SetCurrentSelection(OutlineSelectionHandler selectionTarget)
    {
        if (currentSelection != selectionTarget)
        {
            if (currentSelection != null)
            {
                DisableOutline(currentSelection);
                currentSelection.isSelected = false;
            }
            currentSelection = selectionTarget;

            if(currentSelection!=null)
            {
                SelectPart(currentSelection);
            }
        }
    }
    public void HighlightPart(OutlineSelectionHandler partObject)
    {
        if (!GaugingManager.Instance.isGaugingEnabled)
        {
            partObject.gameObject.layer = LayerMask.NameToLayer(highlightLayer);
        }
        currentHighlight = partObject;
    }

    public void SelectPart(OutlineSelectionHandler partObject)
    {
        if (ApplicationStateMachine.Instance.currentStateName == null)
        {
            return;
        }

        partObject.gameObject.layer = LayerMask.NameToLayer(focusLayer);

        if (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentSelectionViewState))
        {
            CommunicationManager.Instance.HandleCompartmentSelection_Extern(partObject.transform.gameObject.name);
        }

        string compartmentUID = partObject.transform.GetComponent<MetadataComponent>().GetValue("ASSET_UID");


        if (compartmentUID != null)
        {
            CommunicationManager.Instance.HandleCompartmentSelectionUID_Extern(compartmentUID);
        }
        string compartmentName = "";
        string frameName = "";

        bool isInVesselView = ApplicationStateMachine.Instance.currentStateName.Equals(nameof(VesselViewState));
        bool isInCompartmentView = ApplicationStateMachine.Instance.currentStateName.Equals(nameof(CompartmentViewState));

        if (isInVesselView)
        {
            compartmentName = partObject.gameObject.name;
            CommunicationManager.Instance.HandlePartSelection(compartmentName);
        }
        else if(isInCompartmentView)
        {
            compartmentName = (ApplicationStateMachine.Instance.CurrentState as CompartmentViewState).GetTargettedCompartemnt();
            frameName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(partObject.gameObject).name;
            CommunicationManager.Instance.HandlePartSelection($"{compartmentName}/{frameName}");
        }
        else
        {
            compartmentName = partObject.transform.gameObject.name;
            CommunicationManager.Instance.HandlePartSelection(compartmentName);
        }
    }

    public void UnselectSelectedPart()
    {
        if (currentSelection == null)
        {
            return;
        }

        DisableOutline(currentSelection);
        currentSelection.isSelected = false;
        currentSelection = null;
    }

    public void DisableOutline(OutlineSelectionHandler partObject)
    {
        if (ApplicationStateMachine.Instance.currentStateName == null)
        {
            return;
        }

        if ((ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentSelectionViewState)) || ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            partObject.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            partObject.gameObject.layer = LayerMask.NameToLayer(partObject.initialLayer);
        }
        if (currentHighlight == partObject)
        {
            currentHighlight = null;
        }
    }


    public void HightliteCompartmentUID(string UID)
    {
        if (GroupingManager.Instance.vesselObject.TryGetCompartment(UID, out Compartment compartmentGameObject))
        {
            compartmentGameObject.compartmentMeshObjectReference.GetComponent<OutlineSelectionHandler>()?._OnMouseDown();
        }
    }
    public void HighlitePlate(string UID,string frameName,string plateName)
    {
        Subpart subpart = GroupingManager.Instance.vesselObject.GetSubpart(UID, frameName, plateName);

        //if (subpart != null)
        //{
        //    subpart.subpartMeshRenderer.GetComponent<OutlineSelectionHandler>()?._OnMouseDown();
        //}

        if (GroupingManager.Instance.vesselObject.TryGetSubpart(UID, frameName, plateName, out Subpart plateGameObject))
        {
            plateGameObject.subpartMeshRenderer.GetComponent<OutlineSelectionHandler>()?._OnMouseDown();
        }
    }

#if DEPRICATED_BUILTIN_UI // please note that this is depricated as the Hierarchy UI is outside of the visualizer
    private void HighlightUIElement()
    {
        highlightObject.gameObject.SetActive(currentSelection != null);

        if (currentSelection != null)
        {
            Vector3 targetPostion = highlightObject.transform.position;
            targetPostion.y = currentSelection.expandableNode.transform.GetChild(0).position.y;
            targetPostion.z = highlightObject.transform.position.z;
            highlightObject.transform.position = targetPostion;
        }
    }
#endif
}
