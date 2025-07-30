using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Bson;

public class CommunicationManager : SingletonMono<CommunicationManager>
{
    [DllImport("__Internal")]
    private static extern void handleGaugePoints(string data);

    [DllImport("__Internal")]
    private static extern void handleCompartmentSelection(string data);

    [DllImport("__Internal")]
    private static extern void handleCompartmentSelectionUID(string data);

    [DllImport("__Internal")]
    private static extern void handlePartSelection(string data);

    [DllImport("__Internal")]
    private static extern void handleMetadataInformation(string data);

    [DllImport("__Internal")]
    private static extern void handleCurrentStatename(string data);

    [DllImport("__Internal")]
    private static extern void postVesselLoad(string data);

    [DllImport("__Internal")]
    private static extern void handleAttachDocument(string data);

    [DllImport("__Internal")]
    private static extern void handleError(string data);
    [DllImport("__Internal")]
    private static extern void handleClickOnAttachIcon(string data);

    [DllImport("__Internal")]
    private static extern void handleDiminutionUnity(string data);

    [DllImport("__Internal")]
    private static extern void handleShowDocumentMetadata(string data);

    [DllImport("__Internal")]
    private static extern void handlePartHide(string data);

    [DllImport("__Internal")]
    private static extern void handleCapturedImagesForExport(string data);

    [DllImport("__Internal")]
    private static extern void handleAvgOriginalThickness(string data);

    [DllImport("__Internal")]
    private static extern void handleShowAll();

    [DllImport("__Internal")]
    private static extern void handleZoomScrollValue(string data);

    [DllImport("__Internal")]
    private static extern void handleIsolatedObject(string data);

    [DllImport("__Internal")]
    private static extern void handleSingleGaugePoint(string data);

    [DllImport("__Internal")]
    private static extern void handleSingleGaugePointFlat(string data);

    [DllImport("__Internal")]
    private static extern void handleGaugePointRemoval(string data);

    [DllImport("__Internal")]
    private static extern void handleGaugePointRemovalFlat(string data);

    [DllImport("__Internal")]
    private static extern void handleCompartmentLoaded(string data);

    [DllImport("__Internal")]
    private static extern void handlePlateName(string data);

    [DllImport("__Internal")]
    private static extern void handleCreateAnomaly(string data);


    [DllImport("__Internal")]
    private static extern void handleAnomalyClicked(string data);

    [DllImport("__Internal")]
    private static extern void handleOnAutoGaugeCompleted();
    [DllImport("__Internal")]
    private static extern void handleEnterVesselView();

    [DllImport("__Internal")]
    private static extern void handleRemoveAttachment(string data);
    [DllImport("__Internal")]
    private static extern void handleEmptySpaceClick();

    public static Action<string, bool> OnSetVisability;
    public static Action<string> OnToggleVisability;

    [SerializeField] private Image targetPanel;
    [SerializeField] private Image loadingBar;
    [SerializeField] private Image contextmenuBG;
    [SerializeField] private TextMeshProUGUI loadingBarText;

    [SerializeField] private GameObject GizmoPanel;

    public string sampleHidepart = "";
    public string samplePlate = "";
    public string sampleGaugepoints = "";
    public string docs = "";
    bool documentIconsActive = false;

    [HideInInspector] public bool platesActive = true;
    [HideInInspector] public bool stiffenersActive = true;
    [HideInInspector] public bool bracketsActive = true;
    bool Gizmo= false;

    #region Plates

    public void SetPlatesActive_Extern(string active)
    {
        platesActive = active == "true";
        GameEvents.SetPlatesActive?.Invoke(platesActive);
    }
    public void TogglePlates_Extern()
    {
        if (ShaderUpdater.Instance.DisplayMode != DisplayMode.Wireframe)
        {
            platesActive = !platesActive;
            GameEvents.SetPlatesActive?.Invoke(platesActive);
        }
    }
    public void ShowPlates_Extern()
    {
        platesActive = true;
        GameEvents.SetPlatesActive?.Invoke(platesActive);
    }
    public void HidePlates_Extern()
    {
        platesActive = false;
        GameEvents.SetPlatesActive?.Invoke(platesActive);
    } 

    #endregion

    #region Stiffeners

    public void SetStiffenersActive_Extern(string active)
    {
        if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
        {
            return;
        }

        stiffenersActive = active == "true";
        GameEvents.SetStiffnersActive?.Invoke(stiffenersActive);
    }

    public void SetStiffenersActive_Extern(bool active)
    {
        GameEvents.SetStiffnersActive?.Invoke(active);
    }

    public void ToggleStiffeners_Extern()
    {
        if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
        {
            return;
        }

        stiffenersActive = !stiffenersActive;
        GameEvents.SetStiffnersActive?.Invoke(stiffenersActive);
    }
    public void ShowStiffeners_Extern()
    {
        stiffenersActive = true;
        GameEvents.SetStiffnersActive?.Invoke(stiffenersActive);
    }
    public void HideStiffeners_Extern()
    {
        stiffenersActive = false;
        GameEvents.SetStiffnersActive?.Invoke(stiffenersActive);
    }

    #endregion

    #region Brackets

    public void SetBracketsActive_Extern(string active)
    {
        if(ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
        {
            return;
        }

        bracketsActive = active == "true";
        GameEvents.SetBracketsActive?.Invoke(bracketsActive);
    }

    public void SetBracketsActive_Extern(bool active)
    {
        GameEvents.SetBracketsActive?.Invoke(active);
    }

    public void ToggleBrackets_Extern()
    {
        if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
        {
            return;
        }

        bracketsActive = !bracketsActive;
        GameEvents.SetBracketsActive?.Invoke(bracketsActive);
    }
    public void ShowBrackets_Extern()
    {
        bracketsActive = true;
        GameEvents.SetBracketsActive?.Invoke(bracketsActive);
    }
    public void HideBrackets_Extern()
    {
        bracketsActive = false;
        GameEvents.SetBracketsActive?.Invoke(bracketsActive);
    }

    #endregion

    private void Start()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        Debug.unityLogger.logEnabled = false;
#endif
        EventBroadcaster.OnCompartmentLoadingComplete += OnCompartmentLoaded;
        //   SetGaugingMode_Extern("true");

    }

    public override void Update()
    {

        if (Input.GetKeyDown(KeyCode.O))
        {
            
            if (Gizmo == true)
            {
                Gizmo = false;
                HandleGizmoPosition_Extern("-100/-100/150/150");
            }
            else
            {
                Gizmo = true;
                HandleGizmoPosition_Extern("-400/-100/150/150");
            }
                

        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            //hh = !hh;

            //if (hh)
            //{
            //    Hide_Extern(sampleHidepart);
            //}
            //else
            //{
            //    Show_Extern(sampleHidepart);
            //}
        }

        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    GetMetadataTable(CompartmentUID);
        //}
        if (Input.GetKeyDown(KeyCode.D))
        {
            documentIconsActive = !documentIconsActive;

             AttahcIcon_Extern(docs);

            HandleDocuments_Extern(documentIconsActive ? "true" : "false");
           
          // ToggleMarkup_Extern(Markupbool);
          //GetAvgOriginalThickness_Extern(CompartmentUID);
          //SpawnGaugePoints_Extern(sampleGaugepoints);
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
          //  ShowPlateCorrosion_Extern("{\r\n    \"compartments\": [\r\n        {\r\n            \"assetName\": \"FORE PEAK TANK\",\r\n            \"assetUID\": \"13878122\",\r\n            \"frames\": [\r\n                {\r\n                    \"frameName\": \"ShipHull\",\r\n                    \"frame_Id\": \"\",\r\n                    \"plates\": [\r\n                        {\r\n                            \"plateName\": \"PLATE_6299_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 415.6666564941406\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6742_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 415.6666564941406\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6292_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 416.1666564941406\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6322_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 458.1836853027344\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6297_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 415.6666564941406\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6250_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 138.55555725097657\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6748_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 415.6666564941406\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_6735_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 415.6666564941406\r\n                        },\r\n                        {\r\n                            \"plateName\": \"PLATE_5011_7370181\",\r\n                            \"plate_Id\": \"\",\r\n                            \"corrosionValue\": 419.0\r\n                        }\r\n                    ]\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
            //SpawnGaugePoints_Extern(sampleGaugepoints);
            //AutoGaugeCompartment_Extern("{\"compartmentUid\":\"C0000000972\",\"platePattern\":\"5\",\"bracketPattern\":\"3\",\"stiffenerPattern\":\"2\"}");
            //HandleGaugePointUpdate_Extern(Gaugedata);
            //HandleFocusOnPlate_Extern(samplePlate);
            //Clicked_Extern("14044360");
            //Isolate_Extern("14044360");
        }
    }

    public string Gaugedata = "";
    public bool switched = false;

    public void PreVesselLoad()
    {
        // Write your code here to do anything before loading the vessel.
    }

    private bool vesselLoaded = false;

    public bool ThrowVesselNeedsToLoadWarning()
    {
        if(!vesselLoaded)
        {
            HandleError_Extern("Incorrect data input");
            return true;
        }
        return false;
    }

    public void PostVesselLoad(string data)
    {
       // Debug.Log("Vessel Loaded callback from unity !");
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            postVesselLoad (data);
#endif
        ShaderUpdater.Instance.CollectAllRenderers();
        vesselLoaded = true;
    }

    void CompartmentSelection_Extern(string name)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ServiceRunner.GetService<CompartmentConditionStatusService>().HighlightName(name);
    }

    void CompartmentSelectionUID_Extern(string UID)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        OutlineManager.Instance.HightliteCompartmentUID(UID);
    }

    public void ShowConditionStatus_Extern(string conditionData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        Debug.Log("Show : " + conditionData);
        CompartmentsConditionData compartmentsConditionData = JsonUtility.FromJson<CompartmentsConditionData>(conditionData);
        EnterConditionStatusFlow_Extern();
        GroupingManager.Instance.vesselObject.ResetColorizedCompartments();
        ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartments(compartmentsConditionData);
    }

    void ExitConditionStatus_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        if (ServiceRunner.GetService<ComplexShipManager>().LoadedStructuralModel)
        {
            ApplicationStateMachine.Instance.ResetStateMachine();
        }
        GroupingManager.Instance.vesselObject.ResetColorizedCompartments();
    }

    void LoadModels_Extern(string data)
    {
        if(data.IsNullOrEmpty())
        {
            return;
        }

        VesselLoadCommand vesselLoadCommand = null;

        try
        {
            vesselLoadCommand = JsonUtility.FromJson<VesselLoadCommand>(data);
            GltfLoader.Instance.ExcuteLoadModels(vesselLoadCommand);
        }
        catch
        {
            HandleError_Extern("Incorrect data input");
        }
    }

    public void SpawnGaugePoints_Extern(string gaugePointsData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GaugingManager.Instance.OnCallLoadGaugePoints(gaugePointsData);
        //GaugingManager.Instance.ClearGaugePoints();
        //GaugingManager.Instance.LoadAndSpawnGaugePoints(gaugePointsData);
        //gaugePointsData = null;
    }

    public void ShowCorrosion_Extern(string gaugedPointsData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ServiceRunner.GetService<CorrosionStatusService>().LoadGaugedPoints(gaugedPointsData);
        gaugedPointsData = null;
    }

    public void ShowPlateCorrosion_Extern(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        CorrosionData corrosionData = JsonUtility.FromJson<CorrosionData>(rawData);
        ServiceRunner.GetService<CorrosionStatusService>().ShowCorrosionData(corrosionData);
    }   

    public void HideCorrosion_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ServiceRunner.GetService<CorrosionStatusService>().ResetCorrosionData();
    }

    public void ClearGaugePoints_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GaugingManager.Instance.ClearGaugePoints();
        PointProjector.Instance.autoGauging = false;
    }

    public void HandleGaugePointCreation_Extern(string rawData)
    {
       // Debug.Log(rawData);
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleGaugePoints (rawData);
#endif
    }

    public void HandleCompartmentSelection_Extern(string data)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleCompartmentSelection (data);
#endif
    }

    public void HandleCompartmentSelectionUID_Extern(string data)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleCompartmentSelectionUID (data);
#endif
    }

    public void HandleMetadataInformation_Extern(string data)
    {
     
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
           handleMetadataInformation (data);
#endif

    }

    public void HandleCurrentStatename_Extern(string data)
    {
        //Debug.Log("Current State in unity "+data);
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
           handleCurrentStatename (data);
#endif

    }

    /// <summary>
    /// NON FUNCTIONAL
    /// </summary>
    /// <param name="input"></param>
    public void SetVisibility_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        var dataArray = input.Split(',');
        string path = dataArray[0];
        int isVisible = int.Parse(dataArray[1]);

        OnSetVisability(path, isVisible == 1);
    }

    /// <summary>
    /// NON FUNCTIONAL
    /// </summary>
    /// <param name="input"></param>
    public void VisibilityToggled_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        OnToggleVisability(input);
    }

    public void Clicked_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        var dataArray = input.Split('/');
        string compartment = dataArray[0];
        string frame = "";
        if (dataArray.Length > 1)
        {
            frame = dataArray[1];
            GroupingManager.Instance.vesselObject.GetHullpart(compartment, frame).hullpartMeshReference.GetComponent<OutlineSelectionHandler>()._OnMouseDown(true);
        }
        else
        {
            GroupingManager.Instance.vesselObject.GetCompartment(compartment).compartmentMeshObjectReference.GetComponent<OutlineSelectionHandler>()._OnMouseDown(true);
        }

        Debug.Log($"Clicked Data : {input}");
    }

    public void Isolate_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        var dataArray = input.Split('/');
        string compartment = dataArray[0];
        string hullPart = dataArray.Length <= 1 ? null : dataArray[1];
        if (!GroupingManager.Instance.vesselObject.HasCompartment(compartment))
        {
            compartment = "";
            return;
        }

        if (!hullPart.IsNullOrEmpty())
        {
            ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(HullpartViewState), dataArray.ToList());
        }
        else
        {
            ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentViewState), new List<string>() { compartment });
        }
    }

    //.................Hide unhide hullparts or frames......................

    public void SetVisibility(string input, bool visible)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        var dataArray = input.Split('/');
        string compartment = dataArray[0];
        string compartmentName = dataArray[0];
        string hullPart = dataArray.Length <= 1 ? null : dataArray[1];
        string plate = dataArray.Length <= 2 ? null : dataArray[2];


        if (plate.IsNullOrEmpty())
        {
            ContextMenuManager.Instance.ShowOrHideExtern(compartmentName, dataArray[1], visible);


        }
        else
        {
            GroupingManager.Instance.vesselObject.GetSubpart(compartmentName, dataArray[1], dataArray[2]).SetActive(visible);
        }
    }

    public void Hide_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        SetVisibility(input, false); // input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>
    }

    public void Show_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        SetVisibility(input, true);// input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>
    }

    public void HandlePartSelection(string input)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handlePartSelection (input);
#endif
    }

    public void EnterCompartmentSelectionMode_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentSelectionViewState), new List<string>() { ApplicationStateMachine.Instance.vesselName });
    }

    public void ToggleConditionManagerFlow_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        OutlineManager.Instance.UnselectSelectedPart();

        if (ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            ApplicationStateMachine.Instance.ResetStateMachine();
        }
        else
        {
            EnterConditionStatusFlow_Extern();
        }

#if UNITY_EDITOR
          GroupingManager.Instance.vesselObject.ColorizeCompartmentsRandomly();
#endif
    }

    public void EnterConditionStatusFlow_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        if (ApplicationStateMachine.Instance.IsWorkingOnSimpleModel || ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            return;
        }
        OutlineManager.Instance.UnselectSelectedPart();

        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(SimpleVesselViewState), new List<string>() { ApplicationStateMachine.Instance.vesselName });
    }

    public void EnterGaugingMode_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GaugingManager.Instance.OnToggleGauging();
    }

    public void SetGaugingMode_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GaugingManager.Instance.OnSetGauging(input == "true");
        bool value = input.Equals("true");
    }

    public void ExitCompartmentSelectionMode_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ApplicationStateMachine.Instance.ResetStateMachine();
    }

    public void GetMetadataJson(string input)
    {
        if (GroupingManager.Instance.vesselObject.TryGetCompartment(input, out Compartment compartment))
        {

            // Try to get the MetadataComponent attached to the GameObject
            MetadataComponent metadataComponent = compartment.compartmentMetaData;

            if (metadataComponent != null)
            {
                // Convert the metadata into JSON format
                string json = JsonUtility.ToJson(metadataComponent.metadatalookUp, true);

                //HandleMetadataInformation_Extern(json);
                Debug.Log(json);
            }
            else
            {
                Debug.LogError("No MetadataComponent found on this GameObject.");
                // return null;
            }
        }
    }

    public void ChangeUIColors_Extrn(string HexColor)
    {
        var dataArray = HexColor.Split('/');
        string panelHexColor = dataArray[0];
        string loadingBarHexColor = dataArray[1];

        if (targetPanel == null || loadingBar == null || loadingBarText == null || contextmenuBG == null)
        {
            Debug.LogError("One or more UI elements are not assigned.");
            return;
        }

        if (ColorUtility.TryParseHtmlString(panelHexColor, out Color panelColor))
        {
            targetPanel.color = panelColor;
            Debug.Log($"Panel color changed to: {panelHexColor}");
        }
        else
        {
            Debug.LogError($"Invalid panel hex color code: {panelHexColor}");
        }

        if (ColorUtility.TryParseHtmlString(loadingBarHexColor, out Color loadingBarColor))
        {
            loadingBar.color = loadingBarColor;
            loadingBarText.color = loadingBarColor;
            contextmenuBG.color = loadingBarColor;

            Debug.Log($"Loading bar and text color changed to: {loadingBarHexColor}");
        }
        else
        {
            Debug.LogError($"Invalid loading bar hex color code: {loadingBarHexColor}");
        }
    }

    [ContextMenu("Test Change Colors")]
    private void TestChangeColors()
    {
        ChangeUIColors_Extrn("#FFFFFF/#0C294D");
    }

    public void HandleClickOnAttachIcon_Extern(string id)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
           handleClickOnAttachIcon (id);
#endif
    }



    //...........................To get the Metadata of Compartments or Frames......................

    public void GetMetadataTable_Extern(string input)
    {
        var dataArray = input.Split('/');
        string compartmentName = dataArray[0];
        string hullPart = dataArray.Length <= 1 ? null : dataArray[1];
        string metadataJson;

        if (hullPart.IsNullOrEmpty())
        {
            metadataJson = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName).compartmentMetaData.DisplayMetadataTable();
        }
        else
        {
            metadataJson = GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullPart).hullpartMetadata.GetComponent<MetadataComponent>().DisplayMetadataTable();
        }
        HandleMetadataInformation_Extern(metadataJson);
    }

    public void GetDisplayMetadataTable_Extern(string input)
    {
        var dataArray = input.Split('/');
        string compartmentName = dataArray[0];
        string hullPart = dataArray.Length <= 1 ? null : dataArray[1];
        string metadataJson;

        if (hullPart.IsNullOrEmpty())
        {
            metadataJson = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName).GetMetadata();
        }
        else
        {
            metadataJson = GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullPart).GetMetadata();
        }
        HandleMetadataInformation_Extern(metadataJson);
    }

    public Dictionary<string, Action> bottomBarExcutationLUT => new Dictionary<string, Action>()
    {
        { "Select", CameraInputController.Instance.SetCursorToDefault },
        { "Pan", CameraInputController.Instance.SetToolToPan },
        { "Rotate", CameraInputController.Instance.SetToolToOrbit },
        { "Zoom In Out", CameraInputController.Instance.SetToolToZoom },
        { "Walk Around", CameraInputController.Instance.SetToolToLookAround },
        { "Focus", CameraService.Instance.PresetViewIsometricTSF },
        { "Front View", CameraService.Instance.PresetViewForward },
        { "Aft View", CameraService.Instance.PresetViewAft },
        { "Top View", CameraService.Instance.PresetViewUp },
        { "Bottom View", CameraService.Instance.PresetViewDown },
        { "Starboard View", CameraService.Instance.PresetViewStarBoard },
        { "Port View", CameraService.Instance.PresetViewPort },
        { "Isometric View", CameraService.Instance.PresetViewIsometricTSF },
    };

    public static bool isTransparent = false;

    public void ExcuteBottomBarButton_Extern(string Input)
    {
        bottomBarExcutationLUT[Input]();
    }
    public void TransparencyHandler_Extern(string valu)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ShaderUpdater.Instance.DisplayMode = DisplayMode.Transparency;
        isTransparent = true;
        Shader.SetGlobalFloat("_SimpleLitAlpha", float.Parse(valu));
        Shader.SetGlobalFloat("_Isolation_apha_Value", float.Parse(valu));
        Shader.SetGlobalFloat("_Isolation_apha_Value_Lit", float.Parse(valu));
       // Shader.SetGlobalFloat("_Isolation_apha_Value2", 0.65f);

    }

    public void OpaqueMode_Extern(string valu)
    {
        ShaderUpdater.Instance.DisplayMode = DisplayMode.Opaque;
        isTransparent = false;
        Shader.SetGlobalFloat("_SimpleLitAlpha", float.Parse(valu + 1));
        Shader.SetGlobalFloat("_Isolation_apha_Value", float.Parse(valu));
       // Shader.SetGlobalFloat("_Isolation_apha_Value2", float.Parse(valu));
    }

    public void WireframeMode_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        if (platesActive)
        {
            HideBrackets_Extern();
            HideStiffeners_Extern();
            ShaderUpdater.Instance.DisplayMode = DisplayMode.Wireframe;
            isTransparent = true;
            Shader.SetGlobalFloat("_Isolation_apha_Value", 0);
        }
    }

    public void HandleAttahcIcon_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleAttachDocument (rawData);
#endif
    }

    public void AttahcIcon_Extern(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        IconSpawningManager.Instance.LoadAndSpawnIcons(rawData);
    }

    public void HandleError_Extern(string rawData)
    {
        Debug.Log(rawData);
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleError (rawData);
#endif
    }

    public void Handlediminution_Extern(string rawData) {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleDiminutionUnity (rawData);
#endif
    }

    public void ShowDocumentMetadata_Extern(string data)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleShowDocumentMetadata (data);
#endif
    }

    public void HandlePartHide_Extern(string rawData) {
        Debug.Log("Hide : "+rawData);
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handlePartHide (rawData);
#endif
    }



    public void ZoomScroll_Extern(string val)
    {
        float newVal = float.Parse(val);
        CameraInputController.Instance.ZoomSlider(newVal);
    }

    public void ZoomButton_Extern(string val)
    {
        float newVal = float.Parse(val);        
       CameraInputController.Instance.ZoomSlider(80-CameraInputController.Instance.currentZoomValue+newVal);
        CameraInputController.Instance.scrollSlider.value = 80 - CameraInputController.Instance.currentZoomValue + newVal;
    }



    public void GetAvgOriginalThickness_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        var dataArray = input.Split('/');
        string compartmentName = dataArray[0];
        string hullPart = dataArray.Length <= 1 ? null : dataArray[1];

        float data = -1f;

        if (hullPart == null)
        {
            data = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName).GetAvgOriginalThickness();
        }
        else
        {
            data = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName).hullpartLookup[hullPart].GetAvgOriginalThickness();
        }

        Debug.Log("Àverage Value"+data);

        HandleAvgOriginalThickness_Extern(data.ToString());
    }





    public void HandleAvgOriginalThickness_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleAvgOriginalThickness(rawData);
#endif
    }
    public void HandleShowAll_Extern()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleShowAll();
#endif
    }

    public void HandleZoomScrollValue_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleZoomScrollValue(rawData);
#endif
    }

    public void HandleColors_Extern(string colors)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ColorCodeHandler.Instance.updateColors(colors);
    }

    public void HandleDocuments_Extern(string active)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool t = active.Equals("true");
        IconSpawningManager.Instance.IconEnabler = t;
        IconSpawningManager.Instance.SetAllIconsActive(t);
    }
    public void HandleIsolatedObject_Extern(string data)
    {
       // Debug.Log("HandleIsolatedObject_Extern : " + data);
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleIsolatedObject(data);
#endif
    }

    /// <summary>
    /// Screenshot Capture
    /// </summary>
    public void HandleCaptureIamges_Extern(/*string rawData*/)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        Image_Manager.Instance.CaptureHullPartScreenshot(/*rawData*/);
    }

    public void HandleCapturedImagesForExport_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleCapturedImagesForExport (rawData);
#endif
    }


    /// <summary>
    /// Handle Section Tool Controller
    /// </summary>
    public void HandleSectionTool_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool[] flags = input.Select(c => c == '1').ToArray();

        ShipBoundsComputer.Instance.controlSectionTool(
            flags[0], flags[1], flags[2]
       
        );
    }

    public void HandleSectionToolReset_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ShipBoundsComputer.Instance.ResetPlanes();
    }

    public void HandleSectionToolFlip_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        ArrowController.Instance.flipSelectedPlane();
    }

    /// <summary>
    /// Transverse Label Controller
    /// </summary>
    public void HandleTransverseLables_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool label = (input == "1");  // true if input is "1", false otherwise
        ServiceRunner.GetService<FrameLabelFeature>().SetTransverseLabelsActive(label);
    }
    public void HandleTransverseLablesDensity_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }
        float value = float.Parse(input);
        ServiceRunner.GetService<FrameLabelFeature>().SetTransverseLabelsDensity(value);
    }

    /// <summary>
    /// Longitudinal Label Controller
    /// </summary>
    public void HandleLongitudinalLables_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool label = (input == "1");  // true if input is "1", false otherwise
        ServiceRunner.GetService<FrameLabelFeature>().SetLongitudinalLabelsActive(label);
    }
    public void HandleLongitudinalLablesDensity_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }
        float value = float.Parse(input);
        ServiceRunner.GetService<FrameLabelFeature>().SetLongitudinalLabelsDensity(value);
    }

    /// <summary>
    /// Longitudinal Label Controller
    /// </summary>
    public void HandleDeckLables_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool label = (input == "1");  // true if input is "1", false otherwise
        ServiceRunner.GetService<FrameLabelFeature>().SetDeckLabelsActive(label);
    }
    public void HandleDeckLablesDensity_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }
        float value = float.Parse(input);
        ServiceRunner.GetService<FrameLabelFeature>().SetDeckLabelsDensity(value);
    }

    /// <summary>
    /// Frame Label Controller
    /// </summary>
    public void HandleFrameLables_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool label = (input == "1");  // true if input is "1", false otherwise
        ServiceRunner.GetService<FrameLabelFeature>().SetLabelsActive(label);
    }
    public void HandleFrameLablesDensity_Extern(string input)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        float value = float.Parse(input);

        ServiceRunner.GetService<FrameLabelFeature>().SetLabelsDensity(value);
    }

    /// <summary>
    /// Gizmo Controller
    /// </summary>
    public void HandleGizmoPanelSize_Extern(string value)
    {
        float scaleValue = float.Parse(value);
        GizmoPanel.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);  
    }

    public void HandleGizmoPosition_Extern(string input)
    {
        var dataArray = input.Split('/');

        float posX = float.Parse(dataArray[0]);
        float posY = float.Parse(dataArray[1]);
        float width = float.Parse(dataArray[2]);
        float height = float.Parse(dataArray[3]);

        GizmoHandler.Instance.SetPanelProperties(posX, posY, width, height);
    }

    public void HandleSingleGaugePointCreation_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleSingleGaugePoint (rawData);
#endif
    }

    public void HandleSingleGaugePointCreation_Flat_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
           handleSingleGaugePointFlat (rawData);
#endif
    }

    public void HandleGaugePointRemoval_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleGaugePointRemoval (rawData);
#endif
    }

    public void HandleGaugePointRemovalFlat_Extern(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleGaugePointRemovalFlat (rawData);
#endif
    }

    public void AutoGaugeCompartment_Extern(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        PointProjector.Instance.OnCallAutoGauge(rawData);
    }

    public void OnCompartmentLoaded(string data)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleCompartmentLoaded (data);
#endif
    }
    public void HandleFocusOnPlate_Extern(string data)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        var dataArray = data.Split('/');
        string CompartmentUID = dataArray[0];
        string FrameName = dataArray[1];
        string PlateName = dataArray[2];

        OutlineManager.Instance.HighlitePlate(CompartmentUID, FrameName, PlateName);
    }

    public void HandleGaugePointMThicknessUpdate_Extern(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GaugingManager.Instance.UpdateGaugePointBasedId(rawData);
    }

    public void SetCorrosionPercent(string data)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        CorrosionStatusService.corrosionPercent = float.Parse(data);
    }

    public void SetGaugePointLabelsActive(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GameEvents.SetGaugePointLabelActive?.Invoke(rawData == "true");
    }

    public void SetGaugePointViewActive(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GameEvents.SetGaugePointViewActive?.Invoke(rawData == "true");
    }

    public void SetGaugePointViewActive_Extern(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        GameEvents.SetGaugePointViewActive?.Invoke(rawData == "true");
    }

    public void GetPlateName(string data)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handlePlateName (data);
#endif
    }

    public void BeginMarkupDrawing_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        MarkupCreator.Instance.enableMarking = true;
    }

    public void SetMarkupShapeType_Extern(string shape)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        MarkupCreator.Instance.SetMarkupType_Extern(shape);
    }

    public void EndMarkupDrawing_Extern()
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        MarkupCreator.Instance.ResetMarkupData(true);
    }

    public void ToggleMarkup_Extern(string visibility)
    {
        if(ThrowVesselNeedsToLoadWarning())
        {
            return;
        }

        bool isVisible;

        switch (visibility.Trim().ToLower())
        {
            case "true":
                isVisible = true;
                break;
            case "false":
                isVisible = false;
                break;
            default:
                Debug.LogWarning($"Invalid boolean-like value: {visibility}");
                return;
        }

        ServiceRunner.GetService<MarkupManager>().ToggleMarkupVisibility(isVisible);
    }

    public void HandleCreateAnomaly(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleCreateAnomaly(rawData);
#endif
    }

    public void LoadMarkups_Extern(string rawData)
    {
        if (ThrowVesselNeedsToLoadWarning())
        {
            return;
        }
        ServiceRunner.GetService<MarkupManager>().SpawnAllAnomalySets(JsonUtility.FromJson<AnomalySet>(rawData));
    }

    public void HandleOnAutoGaugeCompleted()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleOnAutoGaugeCompleted ();
#endif
    }

    public void HandleAnomalyClicked(string rawData)
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleAnomalyClicked(rawData);
#endif
    }

    public void DeleteMarkup(string rawData)
    {
        ServiceRunner.GetService<MarkupManager>().RemoveMarkup(rawData);
    }

    public void DeleteAllMarkups()
    {
        ServiceRunner.GetService<MarkupManager>().RemoveAllMarkups();
    }

    public void PingMarkpup(string id)
    {
        ServiceRunner.GetService<MarkupManager>().PingMarkup(id);
    }

    public void HandleEnterVesselView()
    {
    #if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
            handleEnterVesselView();
    #endif
    }

    public void HandleRemoveAttachment(string rawData)
    {
        Debug.Log("Remove Attachment Id: " + rawData);
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleRemoveAttachment(rawData);
#endif
    }
    public void HandleEmptySpaceSelection()
    {
        Debug.Log("Empty Space");
#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
        handleEmptySpaceClick();
#endif
    }
    // Controlling informationGameObject Scale from React
    public void SetInformationGameObjectScale_Extern(string scale)
    {       
       ContextMenuManager.Instance.SetInformationMenuScale(scale);
    }
}