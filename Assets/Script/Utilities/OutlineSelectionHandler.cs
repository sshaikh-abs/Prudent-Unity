using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class OutlineSelectionHandler : MonoBehaviour
{
    public static bool CLICK_DEBUG = true;

    public bool isSelected;

    public Action OnSelectedCallback;

    public string initialLayer;

    public static Action<Subpart, bool> OnSelectedSubpart;
    public static Action<Hullpart, bool> OnSelectedHullpart;
    public static Action<Compartment, bool> OnSelectedCompartment;
    public static Action OnClickedOnEmptySpace;
    public GameObject TempObject;
    public void Initialize()
    {
        initialLayer = LayerMask.LayerToName(gameObject.layer);
        OutlineManager.Instance.DisableOutline(this);
    }


    private MetadataComponent metadataComponent;
    private MetadataComponent MetaDataComponent
    {
        get
        {
            if (metadataComponent == null)
            {
                metadataComponent = transform.GetComponent<MetadataComponent>();
            }
            if (metadataComponent == null)
            {
                metadataComponent = transform.parent.GetComponent<MetadataComponent>();
            }
            return metadataComponent;
        }
    }

    float value = 0;
    string criteria = "Weighted Score";
    public void _OnMouseEnter()
    {
        if (ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            try
            {
                value = ServiceRunner.GetService<CompartmentConditionStatusService>().GetValue(MetaDataComponent.GetValue("ASSET_UID"));
                criteria = ServiceRunner.GetService<CompartmentConditionStatusService>().Criteria;

                if (value < 0)
                {
                    return;
                }
                string nam = metadataComponent.GetValue("NAME");

                ContextMenuManager.Instance.SetInformationObject(true);
                ContextMenuManager.Instance.SetInfromationText($"Asset Name : {nam}\n{criteria} : {value}");

                return;
            }
            catch (Exception ex)
            {
                // Log the exception message and stack trace for debugging purposes
                Debug.Log($"An error occurred: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    public void _OnMouseDown(bool external = false, bool isFromRightMouse = false)
    {
        if (!MouseOverChecker.IsMouseOverAUIElement() || external)
        {
            isSelected = true;
            OutlineManager.Instance.SetCurrentSelection(this);

            string metadataJson = "";
            string name = "";

            switch (ApplicationStateMachine.Instance.currentStateName)
            {
                case nameof(SimpleVesselViewState):
                case nameof(VesselViewState):
                    {
                        var compartmentObject = GroupingManager.Instance.vesselObject.ProcessCompartmentSelection(MetaDataComponent.gameObject);
                        metadataJson = compartmentObject.GetMetadata();
                        name = compartmentObject.uid + "/Compartment";
                        OnSelectedCompartment?.Invoke(compartmentObject, isFromRightMouse);
                        break;
                    }
                case nameof(CompartmentViewState):
                    {
                        var hullpartObject = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(MetaDataComponent.gameObject);
                        metadataJson = hullpartObject.GetMetadata();
                        name = hullpartObject.name + "/Hullpart";
                        OnSelectedHullpart?.Invoke(hullpartObject, isFromRightMouse);
                        break;
                    }
                case nameof(HullpartViewState):
                    {
                        var subpartObject = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(MetaDataComponent.gameObject);
                        metadataJson = subpartObject.GetMetadata();
                        name = MetaDataComponent.GetValue("PRT_NAME") + "/Subpart";
                        OnSelectedSubpart?.Invoke(subpartObject, isFromRightMouse);
                        break;
                    }
                default:
                    break;
            }

            CommunicationManager.Instance.HandleMetadataInformation_Extern(metadataJson);
            if (CLICK_DEBUG)
            {
                Debug.Log($"{MetaDataComponent.gameObject.name}'s Meta Data-  \n{metadataJson}", transform);
            }

            if (name != null)
            {
                Debug.Log("PRT_NAME: " + name);
                CommunicationManager.Instance.GetPlateName(name);
            }

            OnSelectedCallback?.Invoke();
        }
    }

    public void _OnMouseExit()
    {
        //ContextMenuManager.Instance.SetInformationObject(false);
        if (ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            ContextMenuManager.Instance.SetInformationObject(false);
            return;
        }
    }
}
