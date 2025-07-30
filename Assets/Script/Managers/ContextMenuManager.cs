using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class ContextMenuManager : SingletonMono<ContextMenuManager>
{
    private class ContextMenuData
    {
        public List<ContextMenuButtonData> contextMenu = new List<ContextMenuButtonData>();

        public ContextMenuData()
        {
            contextMenu = new List<ContextMenuButtonData>();
        }
    }

    private class ContextMenuButtonData
    {
        public string Label;
        public string id;
        public bool IsEnable = true;
        public Action OnClick;
    }

    public RectTransform contextMenuGameObject;
    public RectTransform informationMenuGameObject;
    public TextMeshProUGUI informationText;
    public ContextMenuButton buttonPrefab;
    public RectTransform container;

    //private Dictionary<string, ContextMenuButton> spawnedButtons = new Dictionary<string, ContextMenuButton>();
    public GameObject contextedGameobject;

    private bool closeContextMenuRequest = false;
    private bool showAllEnable =false;
    public List<Hullpart> disabledHullpartsCache = new List<Hullpart>();
    private List<Subpart> disabledSubpartCache = new List<Subpart>();

    //private bool isContextMenuOpen = false;

    /// <summary>
    /// Excution lookup table for which type of isolation to perform in which state.
    /// </summary>
    private Dictionary<string, Action> excuationLookup => new Dictionary<string, Action>()
    {
        { nameof(VesselViewState), () => ServiceRunner.GetService<ComplexShipManager>().EnterVesselView() },
        { nameof(CompartmentViewState), () => ServiceRunner.GetService<ComplexShipManager>().EnterCompartmentView(contextedGameobject) },
        { nameof(HullpartViewState), () => ServiceRunner.GetService<ComplexShipManager>().EnterHullPartView(contextedGameobject) },
        { nameof(HullpartViewState) + "_Full", () => ServiceRunner.GetService<ComplexShipManager>().EnterFullHullPartView(contextedGameobject) }
    };

    #region Context Menu Button Definations.

    private ContextMenuButtonData VesselViewStateButton => new ContextMenuButtonData()
    {
        Label =  "Asset View", //ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState) ? "Reset View" :
        id = "AssetView",
        IsEnable = !(ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState)),
        OnClick = () => PerformIsolation(nameof(VesselViewState))
    };

    private ContextMenuButtonData CompartmentViewStateButton => new ContextMenuButtonData()
    {
        Label = "Compartment View",
        id = "CompartmentView",
        IsEnable = !(ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState)),
        OnClick = () => PerformIsolation(nameof(CompartmentViewState))
    };

    private ContextMenuButtonData CompartmentViewStateBackButton => new ContextMenuButtonData()
    {
        Label = "Compartment View",
        id = "CompartmentViewBack",
        OnClick = () => PerfromIsolationFromHullpartToCompartment()
    };

    private ContextMenuButtonData HullPartViewButton => new ContextMenuButtonData()
    {
        Label = "Hullpart View",
        id = "HullpartView",
        IsEnable = !(ApplicationStateMachine.Instance.currentStateName == nameof(HullpartViewState) || contextedGameobject == null),
        OnClick = () => PerformIsolation(nameof(HullpartViewState))
    };

    private ContextMenuButtonData FullHullPartViewButton => new ContextMenuButtonData()
    {
        Label = "Full Hullpart View",
        id = "FullHullpartView",
        IsEnable = !(ApplicationStateMachine.Instance.currentStateName == nameof(HullpartViewState) || contextedGameobject == null),
        OnClick = () => PerformIsolation(nameof(HullpartViewState)+"_Full")
    };

    private ContextMenuButtonData HullPartViewButtonBlank => new ContextMenuButtonData()
    {
        Label = "Hullpart View",
        id = "HullpartViewBlank",
        IsEnable = false
    };

    private ContextMenuButtonData HideButton => new ContextMenuButtonData()
    {
        Label = "Hide",
        id = "Hide",
        OnClick = PerformHide
    };

    private ContextMenuButtonData ShowAllButton => new ContextMenuButtonData()
    {
        Label = "Show All",
        id = "ShowAll",
        OnClick = PerformShowAll
    };

    private ContextMenuButtonData RemoveGaugePointButton => new ContextMenuButtonData()
    {
        Label = "Remove",
        id = "RemoveGaugePoint",
        OnClick = RemoveGaugePoint
    };

    private ContextMenuButtonData CreateAnomalyButton => new ContextMenuButtonData()
    {
        Label = "Create Anomaly",
        id = "CreateAnomaly",
        OnClick = CreateAnomaly
    };

    private ContextMenuButtonData CreateAnomalyWithAllButton => new ContextMenuButtonData()
    {
        Label = "Create Anomaly With All",
        id = "CreateAnomalyWithAll",
        OnClick = CreateAnomalyWithAllMarkups
    };

    private ContextMenuButtonData DeleteAnomalyButton => new ContextMenuButtonData()
    {
        Label = "Delete Anomaly",
        id = "DeleteAnomaly",
        OnClick = DeleteAnomaly
    };

    private ContextMenuButtonData AttachIconButton => new ContextMenuButtonData()
    {
        Label = "Attach",
        id = "AttachIcon",
        OnClick = AttachIcon
    };
    private ContextMenuButtonData DiminutionGraphButton => new ContextMenuButtonData()
    {
        Label = "Diminution Graph",
        id = "DiminutionGraph",
        OnClick = DiminutionGraph
    };
    private ContextMenuButtonData FlipButton => new ContextMenuButtonData()
    {
        Label = " Flip Plane",
        id = "FlipPlane",
        OnClick = Flip
    };

    private ContextMenuButtonData RemoveAttachmentButton => new ContextMenuButtonData()
    {
        Label = "Remove",
        id = "RemoveAttachment",
        OnClick = RemoveAttachment 
    };

    #endregion

    public override void Update()
    {
        //HandleInputs();
        base.Update();
        ShowInformation();
    }

    /// <summary>
    /// Gets the object thats underneath the cursor.
    /// </summary>
    /// <returns>Object under the cursor</returns>
    public GameObject GetObjectUnderCursorAndInIsolation()
    {
        if(ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState) || ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            if (IsolationManager.Instance.CaptureRayCastHitOnCompartments(out RaycastHit hit))
            {
                return hit.collider.gameObject;
            }
        }
        else if(ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
        {
            if (IsolationManager.Instance.CaptureRayCastHitOnHullParts(out RaycastHit hit) && OutlineManager.Instance.CurrentSelectionGameObject == hit.collider.gameObject)
            {
                return hit.collider.gameObject;
            }
            else
            {
                if(IsolationManager.Instance.CaptureRayCastHit(out hit))
                {
                    return hit.collider.gameObject;
                }
            }
        }
        else if (ApplicationStateMachine.Instance.currentStateName == nameof(HullpartViewState))
        {
            if (IsolationManager.Instance.CaptureRayCastHit(out RaycastHit hit))
            {
                return hit.collider.gameObject;
            }
        }
        else
        {
            return null;
        }

        //if (IsolationManager.Instance.CaptureRayCastHit(out RaycastHit hit))
        //{
        //    return hit.collider.gameObject;
        //}

        return null;
    }

    Stopwatch stopwatch = new Stopwatch();
    private bool firstClickFinished = false;

    public void HandleInputs()
    {
        if (!ApplicationStateMachine.Instance.currentStateName.IsNullOrEmpty() || ApplicationStateMachine.Instance.currentStateName != nameof(CompartmentSelectionViewState) || ApplicationStateMachine.Instance.currentStateName != nameof(SimpleVesselViewState))
        {
            if (Input.GetMouseButtonDown(1))
            {
                firstClickFinished = false;

                if (GetObjectUnderCursorAndInIsolation())
                    {
                        contextedGameobject = GetObjectUnderCursorAndInIsolation();
                        ;
                    if ((GaugingManager.Instance.isGaugingEnabled) && contextedGameobject.GetComponent<GaugingPointIndicator>())
                    {
                        ShowContextMenuOnGaugePoint();
                    }
                    else if (LayerMask.LayerToName(contextedGameobject.layer) == "Markup")
                    {
                        #if UNITY_EDITOR
                        ShowContextMenuOnMarkup();
                        #endif
                    }
                    else if (contextedGameobject.GetComponent<IconIndicator>())
                    {
                        Debug.Log("IconIndicator");
                        //ShowContextMenuOnAttachment();
                    }
                    else
                    {
                        ShowContextMenuOnPart();
                    }
                   
                }
                else
                {

                    ShowContextMenuOnEmptySpace();
                }
               
            }
          
        }

        if (Input.GetMouseButtonDown(0))// && !GetObjectUnderCursorAndInIsolation())
        {
            RequestToCloseContextMenu();
            //CloseContextMenu();

            if (!GaugingManager.Instance.isGaugingEnabled && !MarkupCreator.Instance.enableMarking)
            {
                if (!firstClickFinished && !MouseOverChecker.IsMouseOverAUIElement())
                {
                    contextedGameobject = GetObjectUnderCursorAndInIsolation();
                    firstClickFinished = true;
                    stopwatch.Restart();
                }
                else
                {
                    bool isSecondClick = stopwatch.Elapsed.TotalMilliseconds <= 500 && contextedGameobject == GetObjectUnderCursorAndInIsolation();
                    Debug.Log("failed second click by millseconds : " + stopwatch.Elapsed.TotalMilliseconds);
                    firstClickFinished = false;

                    if (isSecondClick && !MouseOverChecker.IsMouseOverAUIElement())
                    {
                        if (GetObjectUnderCursorAndInIsolation() && (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState) || ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState)))
                        {
                            contextedGameobject = GetObjectUnderCursorAndInIsolation();
                            string targetStateName = nameof(CompartmentViewState);

                            if (ApplicationStateMachine.Instance.TryGetCurrentState(out CompartmentViewState targetStateObject))
                            {
                                targetStateName = nameof(HullpartViewState);
                            }

                            PerformIsolation(targetStateName);
                        }
                    }
                }
            }

            if (!GetObjectUnderCursorAndInIsolation() && ApplicationStateMachine.Instance.currentStateName != nameof(CompartmentSelectionViewState))
            {
                firstClickFinished = false;
                OutlineManager.Instance.UnselectSelectedPart();
                OutlineSelectionHandler.OnClickedOnEmptySpace?.Invoke();
            }
            if (!GetObjectUnderCursorAndInIsolation())
            {
                firstClickFinished = false;
                CommunicationManager.Instance.HandleEmptySpaceSelection();
            }
        }
    }

    /// <summary>
    /// Function responsible for showig the context menu that belongs to the type of part.
    /// </summary>
    public void ShowContextMenuOnPart()
    {

        if (contextedGameobject.layer == LayerMask.NameToLayer("Plane"))
        {
            List<ContextMenuButtonData> contextMenuButtonData = new List<ContextMenuButtonData>()
                    {
                        FlipButton
                        //HullPartViewButton
                    };

            ShowContextMenu(new ContextMenuData()
            {
                contextMenu = contextMenuButtonData
            });
        }
        else
        {
            List<ContextMenuButtonData> contextMenuButtonData = new List<ContextMenuButtonData>();
            if (!MarkupCreator.Instance.enableMarking)
            {
                if (!GaugingManager.Instance.isGaugingEnabled && !PointProjector.Instance.autoGauging)
                {
                    contextMenuButtonData.Add(VesselViewStateButton);
                }

                contextMenuButtonData.Add(CompartmentViewStateButton);

                if (ApplicationStateMachine.Instance.currentStateName != nameof(VesselViewState))
                {
                    contextMenuButtonData.Add(HullPartViewButton);
                    //contextMenuButtonData.Add(FullHullPartViewButton);

                    if (ApplicationStateMachine.Instance.currentStateName != nameof(HullpartViewState))
                    {
                        if (showAllEnable)
                            contextMenuButtonData.Add(ShowAllButton);
                        contextMenuButtonData.Add(HideButton);
                    }
                    if (!GaugingManager.Instance.isGaugingEnabled)
                    {
                        contextMenuButtonData.Add(AttachIconButton);
                        contextMenuButtonData.Add(DiminutionGraphButton);
                    }

                }
            }
            else
            {
                if (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
                {
                    contextMenuButtonData.Add(HullPartViewButton);
                
                    contextMenuButtonData.Add(HideButton);

                }
                else if(ApplicationStateMachine.Instance.currentStateName == nameof(HullpartViewState)){
                    contextMenuButtonData.Add(CompartmentViewStateButton);
                   
                }
                if (showAllEnable)
                    contextMenuButtonData.Add(ShowAllButton);
            }

                ShowContextMenu(new ContextMenuData()
                {
                    contextMenu = contextMenuButtonData
                });
            

        }
    }

    /// <summary>
    /// Function responsible for showing the context menu that belongs to the Gauge Point.
    /// </summary>
    public void ShowContextMenuOnGaugePoint()
    {
        if (LayerMask.LayerToName(contextedGameobject.layer).GetHashCode() == ("GaugePoint").GetHashCode())
        {
            ShowContextMenu(new ContextMenuData()
            {
                contextMenu = new List<ContextMenuButtonData>()
                {
                    RemoveGaugePointButton
                }
            });
        }
    }

    public void ShowContextMenuOnAttachment()
    {
        if (LayerMask.LayerToName(contextedGameobject.layer).GetHashCode() == ("DocumentIcon").GetHashCode())
        {
            ShowContextMenu(new ContextMenuData()
            {
                contextMenu = new List<ContextMenuButtonData>()
                {
                   RemoveAttachmentButton
                }
            });
        }
    }

    /// <summary>
    /// Function responsible for showing the context menu that belongs to the Gauge Markups.
    /// </summary>
    public void ShowContextMenuOnMarkup()
    {
        if (LayerMask.LayerToName(contextedGameobject.layer).GetHashCode() == ("Markup").GetHashCode())
        {
            ShowContextMenu(new ContextMenuData()
            {
                contextMenu = new List<ContextMenuButtonData>()
                {
                    CreateAnomalyButton,
                  //  CreateAnomalyWithAllButton,
                    DeleteAnomalyButton
                }
            });
        }
    }

    /// <summary>
    /// Function responsible for showing the context menu that belongs to the open area. 
    /// </summary>
    public void ShowContextMenuOnEmptySpace()
    {
        ContextMenuData menuData = new ContextMenuData();

        if (!GaugingManager.Instance.isGaugingEnabled && !PointProjector.Instance.autoGauging && !MarkupCreator.Instance.enableMarking)
        {
            menuData.contextMenu.Add(VesselViewStateButton);
        }
        if (ApplicationStateMachine.Instance.currentStateName != nameof(HullpartViewState))
        {
            if (showAllEnable)
            {
                menuData.contextMenu.Add(ShowAllButton);
            }

            if(ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
            {
                menuData.contextMenu.Add(CompartmentViewStateButton);
                menuData.contextMenu.Add(HullPartViewButtonBlank);
            }
        }
        else
        {
            menuData.contextMenu.Add(CompartmentViewStateBackButton);
            menuData.contextMenu.Add(HullPartViewButton);
            //menuData.contextMenu.Add(FullHullPartViewButton);
        }

        //if (ServiceRunner.GetService<MarkupManager>().GetMarkupCount() >= 1)
        //{
        //    menuData.contextMenu.Add(CreateAnomalyWithAllButton);
        //}

        ShowContextMenu(menuData);
    }

    /// <summary>
    /// function that requests the Manager to close the context menu.
    /// </summary>
    private void RequestToCloseContextMenu()
    {
        closeContextMenuRequest = true;
    }

    private void LateUpdate()
    {       
        if (closeContextMenuRequest)
        {
            CloseContextMenu();
        }

       
        //HandleInputs();
    }

    public void ShowOrHideExtern(string currentCompartment, string hullpartName, bool visible)
    {
        Hullpart hullpart = GroupingManager.Instance.vesselObject.GetHullpart(currentCompartment, hullpartName);
        string HullPartName = hullpart.name;
        hullpart.SetActive(visible);
        GaugingManager.Instance.SetGaugePointsActive(currentCompartment, HullPartName, visible);
        IconSpawningManager.Instance.SetGaugePointsActive(currentCompartment, HullPartName, visible);
        if (!visible)
        {
            disabledHullpartsCache.Add(hullpart);
            showAllEnable = true;
        }
        else
        {
            disabledHullpartsCache.Remove(hullpart);
            if(disabledHullpartsCache.Count == 0)
            {
                showAllEnable = false;
            }
        }
    }

    /// <summary>
    /// Function that hids the current contexted object of the context menu
    /// </summary>
    private void PerformHide()
    {
        if (contextedGameobject != null)
        {
            switch (ApplicationStateMachine.Instance.currentStateName)
            {
                case nameof(VesselViewState):
                    return;
                case nameof(CompartmentViewState):
                    string currentCompartment = ApplicationStateMachine.Instance.CurrentState.keyData[1];
                    Hullpart hullpart = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(contextedGameobject);
                    disabledHullpartsCache.Add(hullpart);
                    string HullPartName = hullpart.name;
                    hullpart.SetActive(false);
                    GaugingManager.Instance.SetGaugePointsActive(currentCompartment, HullPartName, false);
                    IconSpawningManager.Instance.SetGaugePointsActive(currentCompartment, HullPartName, false);
                    CommunicationManager.Instance.HandlePartHide_Extern(currentCompartment +"/" +HullPartName);
                    showAllEnable = true;
                    GameEvents.OnSetHullPartActive?.Invoke(hullpart,false);

                    break;
                case nameof(HullpartViewState):
                    string currentCompartment_ = ApplicationStateMachine.Instance.CurrentState.keyData[1];
                    string currentFrame = ApplicationStateMachine.Instance.CurrentState.keyData[2];
                    Subpart subpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(contextedGameobject);
                    disabledSubpartCache.Add(subpart);
                    subpart.SetActive(false);
                    //GroupingManager.Instance.vesselObject.SetActivePlate(currentCompartment_, currentFrame, contextedGameobject, false);
                    break;
                default:
                    break;
            }
        }
        RequestToCloseContextMenu();
    }

    /// <summary>
    /// Removes the contexted gauge point.
    /// </summary>
    private void RemoveGaugePoint()
    {
        GaugingManager.Instance.RemoveGaugePoint(contextedGameobject);
#if FULL_GAUGEPOINTS
        GaugingManager.Instance.GenerateJSON(true);
#endif
        RequestToCloseContextMenu();
    }


    public void RemoveAttachment()
    {
        if (contextedGameobject != null)
        {
            IconSpawningManager.Instance.RemoveIcon(contextedGameobject);
        }
        RequestToCloseContextMenu();
    }

    /// <summary>
    /// Triggers Anomaly workflow
    /// </summary>
    private void CreateAnomaly()
    {
        var markup = contextedGameobject.GetComponent<BaseMarkup>();
        string jsonData = markup.GetJsonData();
#if UNITY_EDITOR
        Debug.Log($"Area : {markup.data.area}");
        Debug.Log(jsonData);
#endif
        CommunicationManager.Instance.HandleCreateAnomaly(jsonData);
    }

    private void DeleteAnomaly()
    {
        var markup = contextedGameobject.GetComponent<BaseMarkup>();
        if (markup != null)
        {
            ServiceRunner.GetService<MarkupManager>().RemoveMarkup(markup);
        }
        Destroy(markup.gameObject);
    }

    private void CreateAnomalyWithAllMarkups()
    {
        string jsonData = ServiceRunner.GetService<MarkupManager>().GetAllMarkupDataAsJson();

#if UNITY_EDITOR
        Debug.Log(jsonData);
#endif

        CommunicationManager.Instance.HandleCreateAnomaly(jsonData);
    }


    /// <summary>
    /// Simply Refereshes all the visibility states current view state.
    /// </summary>
    public void PerformShowAll()
    {
        switch (ApplicationStateMachine.Instance.currentStateName)
        {
            case nameof(VesselViewState):
            case nameof(CompartmentViewState):

                var disabledHullpartsCacheDuplicates = disabledHullpartsCache.Where(h=>true).ToList();
                disabledHullpartsCache.Clear();
                foreach (var item in disabledHullpartsCacheDuplicates)
                {
                    item.SetActive(true);
                    GaugingManager.Instance.SetGaugePointsActive(item.owningCompartmentUid, item.name, true);
                    IconSpawningManager.Instance.SetGaugePointsActive(item.owningCompartmentUid, item.name, true);
                    //GroupingManager.Instance.vesselObject.SetActiveFrame(currentCompartment, item, true, true);
                    GameEvents.OnSetHullPartActive?.Invoke(item, true);
                }
              
                CommunicationManager.Instance.HandleShowAll_Extern();
                showAllEnable = false;
                break;
            case nameof(HullpartViewState):
                string currentCompartment_ = ApplicationStateMachine.Instance.CurrentState.keyData[1];
                string currentFrame = ApplicationStateMachine.Instance.CurrentState.keyData[2];
                foreach (var item in disabledSubpartCache)
                {
                    item.SetActive(true);
                    //GroupingManager.Instance.vesselObject.SetActivePlate(currentCompartment_, currentFrame, item, true);
                }

                CommunicationManager.Instance.HandleShowAll_Extern();
                disabledSubpartCache.Clear();
                break;
            default:
                break;

        }

        RequestToCloseContextMenu();
    }

    /// <summary>
    /// Isolation function call based on the isolation function lookup table.
    /// </summary>
    /// <param name="isolationType">state name</param>
    public void PerformIsolation(string isolationType)
    {
        PerformShowAll();

        if (contextedGameobject != null || isolationType.Equals(nameof(VesselViewState)))
        {
            if (ServiceRunner.GetService<ComplexShipManager>().LoadedStructuralModel)
            {
                excuationLookup[isolationType]();
            }
        }

        RequestToCloseContextMenu();
    }

    public void PerfromIsolationFromHullpartToCompartment()
    {
        PerformShowAll();
        string compUid = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>().compartmentUid;
        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentViewState), new List<string>() { compUid });
        RequestToCloseContextMenu();
    }

    public Dictionary<string, ContextMenuButton> contextMenuPool = new Dictionary<string, ContextMenuButton>();

    /// <summary>
    /// Function to show the context menu on screen
    /// </summary>
    /// <param name="contextMenuData">data of which buttons and what should be performed when clicked</param>
    private void ShowContextMenu(ContextMenuData contextMenuData)
    {
        if (ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            CloseContextMenu();
            return;
        }      


        if (contextMenuData.contextMenu.Count == 0)
        {
            CloseContextMenu();
            return;
        }

        OpenPanelAtCursor();
        //isContextMenuOpen = true;

        foreach (var contextMenuButtonData in contextMenuData.contextMenu)
        {
            if (contextMenuButtonData != null)
            {
                ContextMenuButton contextMenuButton = null;
                if (!contextMenuPool.ContainsKey(contextMenuButtonData.id))
                {
                    contextMenuButton = Instantiate(buttonPrefab.gameObject, container).GetComponent<ContextMenuButton>();
                    contextMenuButton.Initialized(contextMenuButtonData.Label, contextMenuButtonData.IsEnable, contextMenuButtonData.OnClick);
                    contextMenuPool[contextMenuButtonData.id] = contextMenuButton;
                }
                else
                {
                    contextMenuButton = contextMenuPool[contextMenuButtonData.id];
                    contextMenuButton.Initialized(contextMenuButtonData.Label, contextMenuButtonData.IsEnable, contextMenuButtonData.OnClick);
                }

                //spawnedButtons.Add(contextMenuButtonData.Label, contextMenuButton);
            }
        }

        CloseContextMenu(contextMenuData.contextMenu.Select(s => s.id).ToList());

        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
    }

    /// <summary>
    /// Function to set the location of the context menu on screen
    /// </summary>
    private void OpenPanelAtCursor()
    {
        if (MouseOverChecker.IsMouseOverAUIElement())
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(contextMenuGameObject.transform.parent as RectTransform, Input.mousePosition, null, out Vector2 localPoint);

        contextMenuGameObject.anchoredPosition = localPoint;
        contextMenuGameObject.gameObject.SetActive(true);
    }

    /// <summary>
    /// Function to set the location of the context menu on screen
    /// </summary>
    private void ShowInformation()
    {
        if (MouseOverChecker.IsMouseOverAUIElement())
        {
            return;
        }

        //RectTransformUtility.ScreenPointToLocalPointInRectangle(contextMenuGameObject.transform.parent as RectTransform, Input.mousePosition, null, out Vector2 localPoint);

        informationMenuGameObject.position = Input.mousePosition;
        //Debug

    }

    public void SetInformationObject(bool value)
    {
        informationMenuGameObject.gameObject.SetActive(value);
    }

    public void SetInfromationText(string data)
    {
        informationText.text = data;
        Canvas.ForceUpdateCanvases();
        informationText.transform.parent.gameObject.SetActive(false);
        informationText.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Helps to close the current context menu
    /// </summary>
    public void CloseContextMenu(List<string> itemsNeeded = null)
    {
        closeContextMenuRequest = false;
        //isContextMenuOpen = false;

        if(itemsNeeded == null)
        {
            foreach (ContextMenuButton child in contextMenuPool.Values)
            {
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (string child in contextMenuPool.Keys)
            {
                if(itemsNeeded.Contains(child))
                {
                    contextMenuPool[child].gameObject.SetActive(true);
                }
                else
                {
                    contextMenuPool[child].gameObject.SetActive(false);
                }
            }
        }

        //foreach (ContextMenuButton child in contextMenuPool.Values)
        //{
        //    Destroy(child.gameObject);
        //}

        //spawnedButtons.Clear();
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);

        if(itemsNeeded == null)
        {
            contextMenuGameObject.gameObject.SetActive(false);
        }
    }

    public void AttachIcon()
    {
        //GroupingManager.Instance.vesselObject.SetActivePlate(currentCompartment_, currentFrame, item, true);
        IconSpawningManager.Instance.SpawnIcon();                
    }
    public void DiminutionGraph()
    {
        IconSpawningManager.Instance.DiminutionGraphPositionHit();
    }

    public void Flip()
    {
        //string Planename = contextedGameobject.name;
        //ShipBoundsComputer.Instance.FlipPlane(Planename);
        ArrowController.Instance.flipSelectedPlane();
    }
    //... function for controlling scale of the informationMenuGameObject 
    public void SetInformationMenuScale(string scale)
    {
        if (!float.TryParse(scale, out float scaleValue))
        {
            Debug.LogError("Invalid scale value: " + scale);
            return;
        }
        informationMenuGameObject.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
    }
}