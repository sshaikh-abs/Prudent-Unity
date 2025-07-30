using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WebGLSupport.Detail;
using Random = UnityEngine.Random;

#region JSON SPECIFIC TO REACT

[System.Serializable]
public class GaugePointsData_Plate_Flat_External
{
    public string assetName;
    public string assetUID;
    public string frameName;
    public string frame_Id;
    public string plateName;
    public string plate_Id;
    public List<GaugePointData_External> gaugingPoints = new List<GaugePointData_External>();
}

[System.Serializable]
public class GaugePointsData_External
{
    public List<GaugePointsData_Compartment_External> compartments = new List<GaugePointsData_Compartment_External>();
}

[System.Serializable]
public class GaugePointsData_Compartment_External
{
    public string assetName;
    public string assetUID;
    public bool isDeleted;
    public List<GaugePointsData_Frame_External> frames = new List<GaugePointsData_Frame_External>();
}

[System.Serializable]
public class GaugePointsData_Frame_External
{
    public string frameName;
    public string frame_Id;
    public string frameImage = "";
    public bool isDeleted;
    public List<GaugePointsData_Plate_External> plates = new List<GaugePointsData_Plate_External>();
}

[System.Serializable]
public class GaugePointsData_Plate_External
{
    public string plateName;
    public string plate_Id;
    public bool isDeleted;
    public List<GaugePointData_External> gaugingPoints = new List<GaugePointData_External>();
}

[System.Serializable]
public class GaugePointData_External
{
    public string id;
    public string point_Id;
    public bool isClass = true;
    public bool isRepresentative = false;
    public float originalThickness = 15f;
    public float measuredThickness = -1f;
    public float lifeRemaining = 0f;
    public float ruledThickness = 15f;
    public StringyVector3 location;
    public StringyVector3 normal;
    public bool isDeleted;
}

[System.Serializable]
public class GaugePoint_External
{
    public string assetName;
    public string assetUID;
    public string frameName;
    public string frame_Id;
    public string plateName;
    public string plate_Id;
    public string id;
    public string point_Id;
    public bool isClass = true;
    public bool isRepresentative = false;
    public float originalThickness = 15f;
    public float measuredThickness = -1f;
    public float lifeRemaining = 0f;
    public float ruledThickness = 15f;
    public StringyVector3 location;
    public StringyVector3 normal;
}

[System.Serializable]
public class StringyVector3
{
    public string x;
    public string y;
    public string z;

    public Vector3 GetVector()
    {
        return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
    }
}

#endregion

[System.Serializable]
public class GaugePointData
{
    public int id;
    public string plate;
    public string frame;
    public string comparment;
    public string uId; // metaData
    public Vector3 location;
    public Vector3 normal;
    public float originalThickness = 15f;// metaData
    public float ruledThickness = 15f;// metaData
    public float measuredThickness = 15f;
    public string fullName;

    public void Randomize()
    {
        measuredThickness = Random.Range(0, (int)originalThickness);
    }

    public string GetJSON()
    {
        return "";
    }
}

public class GaugingManager : SingletonMono<GaugingManager>
{
    public float gaugepointLookAtOffset = 0.1f;
    public float gaugepointLookAtThreshold = 0.3f;
    public float visualSize = 5f;
    public GameObject MainHUD;
    public GameObject GaugingHUD;
    public GaugingPointIndicator gaugePointRepresentation;
    public bool isGaugingEnabled = false;
    public TextMeshProUGUI gaugingToggleButtonText;
    public bool enableDebug = false;
    public bool enableDebugDrawing = false;
    public Gradient colorScale;

    public Texture2D gaugingCursor;
    public bool preLoadRandomDataOnPlacement = false;

    //private int trackedId = 0;
    public Expectation subpartExpectation;

    public GaugePointInfoHandler gaugePointInfoHandler;
    public GameObject tempObject;
    //public RectTransform GaugePointInfoPanel;
    //public TextMeshProUGUI infoText;
    ////public float fadeDuration = 0.5f;
    //public float tiltAngle = 90f;
    ////public float yOffset = 35;

    //// public Color defaultPointColor;
    //// public Color selectedPointColor;
    //// public Color hoveredPointColor;
    #region GaugePointNumericRepresentation

    private GaugePointNumericRepresentation gaugePointNumericRepresentation;
    public GaugePointNumericRepresentation GaugePointNumericRepresentation
    {
        get
        {
            if (gaugePointNumericRepresentation == null)
            {
                gaugePointNumericRepresentation = GetComponent<GaugePointNumericRepresentation>();
            }

            return gaugePointNumericRepresentation;
        }
    } 

    #endregion

    void Awake()
    {

#if UNITY_WEBGL == true && UNITY_EDITOR == false && REACT_BUILD
    MainHUD.SetActive(false);
#endif
        gaugePointInfoHandler.gameObject.SetActive(false);
    }

    public void OnHover(GaugingPointIndicator obj, string id, string measuredThickness, string originalThickness, Color clr)
    {
        Vector3 localPoint = Camera.main.WorldToScreenPoint(obj.transform.position);

        gaugePointInfoHandler.ShowInfoGraphic(localPoint, id, measuredThickness, originalThickness, clr);
    }

    public void OnExit(GaugingPointIndicator obj)
    {
        gaugePointInfoHandler.HideInfoGraphic();
    }

    public void OnToggleGauging()
    {
        isGaugingEnabled = !isGaugingEnabled;
        gaugingToggleButtonText.text = isGaugingEnabled ? "Exit" : "Enter Markup";
        SetGaugingActive(isGaugingEnabled);

        if(isGaugingEnabled)
        {
            Cursor.SetCursor(gaugingCursor, new Vector2(8f, 8f), CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnSetGauging(bool value)
    {
        isGaugingEnabled = value;
        gaugingToggleButtonText.text = isGaugingEnabled ? "Exit" : "Enter Markup";
        SetGaugingActive(isGaugingEnabled);
    }

    private void SetGaugingActive(bool value)
    {
        GameEvents.OnGaugingModeSet?.Invoke(value);
    }

    public override void Update()
    {
        if (isGaugingEnabled && !MouseOverChecker.IsMouseOverAUIElement() && Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (IsolationManager.Instance.CaptureRayCastHit(out RaycastHit hit))
            {
                if (LayerMask.LayerToName(hit.collider.gameObject.layer).GetHashCode() != ("GaugePoint").GetHashCode() && LayerMask.LayerToName(hit.collider.gameObject.layer).GetHashCode() != ("DocumentIcon").GetHashCode() &&
                    LayerMask.LayerToName(hit.collider.gameObject.layer).GetHashCode() != ("Markup").GetHashCode() && LayerMask.LayerToName(hit.collider.gameObject.layer).GetHashCode() != ("Plane").GetHashCode())
                {
                    CaptureGaugePoint(hit);
                }
            }
        }
        ContextMenuManager.Instance.HandleInputs();
        if (MouseOverChecker.IsMouseOverAUIElement() && Input.GetKeyDown(KeyCode.Mouse0))
        {

        }
    }
    public void LateUpdate()
    {
        
    }

    public string tempDataRead;

    [ContextMenu(nameof(TestLoad))]
    public void TestLoad()
    {
        LoadAndSpawnGaugePoints(tempDataRead);
    }

    private GaugePointsData_External deserializedData;

    public GaugePointsData_External GetDeserializedData()
    {
        if(deserializedData == null)
        {
            deserializedData = GetGaugePointsData_External();
        }
        return deserializedData;
    }

    public void OnCallLoadGaugePoints(string rawData)
    {
        ClearGaugePoints();
        deserializedData = JsonUtility.FromJson<GaugePointsData_External>(rawData);
        EventBroadcaster.OnCompartmentLoadingComplete += OnCompartmentLoadedCallback;
        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentViewState), new List<string>() { deserializedData.compartments[0].assetUID });
        rawData = null;
    }

    private void OnCompartmentLoadedCallback(string compartmentUid)
    {
        SpawnLoadedGaugePoints(deserializedData);
        EventBroadcaster.OnCompartmentLoadingComplete -= OnCompartmentLoadedCallback;
    }

    public void LoadAndSpawnGaugePoints(string rawData)
    {
        deserializedData = JsonUtility.FromJson<GaugePointsData_External>(rawData);
        SpawnLoadedGaugePoints(deserializedData);
        rawData = null;
    }

    public async void SpawnLoadedGaugePoints(GaugePointsData_External gaugePointsData, Action OnComplate = null)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        int highestId_Plate = 0;
        int highestId_Bracket = 0;
        int highestId_Stiffener = 0;

        Subpart currentSubpart = null;

        foreach (var compartmentData in gaugePointsData.compartments)
        {
            foreach (var frameData in compartmentData.frames)
            {
                foreach (var plateData in frameData.plates)
                {
                    if(!GroupingManager.Instance.vesselObject.TryGetSubpart(compartmentData.assetUID, frameData.frameName, plateData.plateName, out currentSubpart))
                    {
                        continue;
                    }

                    foreach (var pointData in plateData.gaugingPoints)
                    {
                        var currentId = int.Parse(pointData.point_Id);

                        SpawnGaugePoint(compartmentData.assetUID, frameData.frameName, plateData.plateName, new GaugePointData()
                        {
                            id = currentId,
                            plate = plateData.plateName,
                            frame = frameData.frameName,
                            comparment = compartmentData.assetName,
                            uId = compartmentData.assetUID,
                            location = pointData.location.GetVector(),
                            normal = pointData.normal.GetVector(),
                            originalThickness = pointData.originalThickness,
                            ruledThickness = pointData.ruledThickness,
                            measuredThickness = pointData.measuredThickness,
                            fullName = plateData.plate_Id
                        }, currentSubpart);

                        if (Subpart.gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Seperate || Subpart.gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Seperate)
                        {
                            switch (currentSubpart.type)
                            {
                                case SubpartType.Plate:
                                    {
                                        if (currentId > highestId_Plate)
                                        {
                                            highestId_Plate = currentId;
                                        }
                                        break;
                                    }
                                case SubpartType.Bracket:
                                    {
                                        if (currentId > highestId_Bracket)
                                        {
                                            highestId_Bracket = currentId;
                                        }
                                        break;
                                    }
                                case SubpartType.Stiffener:
                                    {
                                        if (currentId > highestId_Stiffener)
                                        {
                                            highestId_Stiffener = currentId;
                                        }
                                        break;
                                    }
                                case SubpartType.None:
                                case SubpartType.All:
                                default:
                                    break;
                            }
                        }
                        else if (Subpart.gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Unison || Subpart.gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Unison)
                        {
                            if (currentId > highestId_Plate)
                            {
                                highestId_Plate = highestId_Bracket = highestId_Stiffener = currentId;
                            }
                        }

                        currentSubpart.MarkHighestId(highestId_Plate, highestId_Bracket, highestId_Stiffener);

                        if (stopwatch.Elapsed.TotalMilliseconds > PointProjector.Instance.millsecondThreshold)
                        {
                            await Task.Yield();
                            stopwatch.Restart();
                        }
                    }
                }

                if (Subpart.gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Unison || Subpart.gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Seperate)
                {
                    highestId_Plate = 0;
                    highestId_Bracket = 0;
                    highestId_Stiffener = 0;
                }
            }

            if (Subpart.gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Unison || Subpart.gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Seperate)
            {
                highestId_Plate = 0;
                highestId_Bracket = 0;
                highestId_Stiffener = 0;
            }
        }

        OnComplate?.Invoke();
    }

    private void SpawnGaugePoint(string compartmentuid, string frameDataName, string plateDataName, GaugePointData pointData, Subpart subpart)
    {
        if (subpart.gaugePoints.ContainsKey(pointData.id))
        {
            CommunicationManager.Instance.HandleError_Extern($"Duplicate Id {pointData.id} detected at {compartmentuid}/{frameDataName}/{plateDataName} : Skipping duplicate");
            return;
        }

        GameObject indicatorobject = Instantiate(gaugePointRepresentation.gameObject, pointData.location, Quaternion.identity);

        indicatorobject.transform.forward = pointData.normal;
        indicatorobject.transform.SetParent(transform);


        //....................................................................................................................................................
        var subpartMetadataComponent = GroupingManager.Instance.vesselObject.GetSubpart(compartmentuid, frameDataName, plateDataName).metadata;
 
        if (!subpartMetadataComponent.TryGetValue("NAME", out string PlatefullName))
        {
            PlatefullName = $"{compartmentuid}/{frameDataName}/{plateDataName}".GetHashCode().ToString();
        }
 
        pointData.originalThickness = subpart.buildThickness.IsNullOrEmpty() ? 0f : float.Parse(subpart.buildThickness);
 
        pointData.originalThickness *= pointData.originalThickness < 1 ? 1000f : 1f;

        //....................................................................................................................................................

        pointData.frame = OldToNewFrameNameAdaptor.GetNewFrameName(frameDataName);
        pointData.uId = compartmentuid;
        pointData.comparment = GroupingManager.Instance.vesselObject.GetCompartment(compartmentuid).name;

        GaugingPointIndicator gaugingPointIndicator = indicatorobject.GetComponent<GaugingPointIndicator>();
        gaugingPointIndicator.Initialize(pointData, subpart);
        subpart.AssociateGaugePoint(pointData, gaugingPointIndicator, false);

        indicatorobject.GetComponent<GaugingPointIndicator>().Initialize(pointData, subpart);

        if (ContextMenuManager.Instance.disabledHullpartsCache.Exists(h => h.name.Equals(frameDataName)))
        {
            indicatorobject.SetActive(false);
        }
        var compartmentViewState = ApplicationStateMachine.Instance.GetCurrentState<CompartmentViewState>();
        if (compartmentViewState != null)
        {
            if (compartmentViewState.GetTargettedCompartemnt() != compartmentuid)
            {
                indicatorobject.SetActive(false);
            }
        }
        else
        {
            var hullpartViewState = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>();
            if (hullpartViewState != null && (hullpartViewState.compartmentUid != compartmentuid || hullpartViewState.hullpartName != frameDataName))
            {
                indicatorobject.SetActive(false);
            }
        }

        indicatorobject.GetComponentInChildren<TextMeshPro>().text = pointData.id.ToString();
    }
   
    public void RemoveGaugePoint(GameObject gaugePointGameobject)
    {
        GaugingPointIndicator gaugingPointIndicator = gaugePointGameobject.GetComponent<GaugingPointIndicator>();

        if(GroupingManager.Instance.vesselObject.TryGetSubpart(gaugingPointIndicator.pointData.uId, gaugingPointIndicator.pointData.frame, gaugingPointIndicator.pointData.plate, out Subpart subpart))
        {
            if(!subpart.gaugePoints.ContainsValue(gaugingPointIndicator))
            {
                Debug.LogWarning("Object you are trying to remove is not part of the gauge point data", gaugePointGameobject);
                return;
            }

            CommunicationManager.Instance.HandleGaugePointRemoval_Extern(GetGaugePointInJSONFormat_Nested(gaugingPointIndicator.pointData, true));
            CommunicationManager.Instance.HandleGaugePointRemovalFlat_Extern(GetGaugePointInJSONFormat(gaugingPointIndicator.pointData, true));

            subpart.gaugePoints.RemoveByValue(gaugingPointIndicator);

            Destroy(gaugingPointIndicator.gameObject);
        }
    }

    public void ClearGaugePoints(bool resetId = false)
    {
        GroupingManager.Instance.vesselObject.RunOverAllSubparts(s => 
        {
            s.gaugePoints.Foreach(g =>
            {
                Destroy(g.Value.gameObject);
            });
        });

        GroupingManager.Instance.vesselObject.RunOverCompartments(c => c.ResetTrackingId());

        GaugePointNumericRepresentation.Clear();
        GaugingPointIndicator.ClearIndicators();
       
    }

    public RaycastHit QueryForDataOnPoint(GameObject indicatorobject, RaycastHit hit, string vesselName, string compartmentUid, ref string hullpartName, ref string plateName, ref Subpart plateMetadataComponent)
    {
        List<Collider> frames = GroupingManager.Instance.vesselObject.GetHullparts(compartmentUid, h => h.GetCollider());
        List<GameObject> collidingFrames = new List<GameObject>();

        frames.ForEach(p => p.enabled = true);

        collidingFrames = Physics.OverlapSphere(hit.point, 0.1f).Select(c => c.gameObject).ToList();

        frames.ForEach(p => p.enabled = false);

        collidingFrames.Remove(hit.collider.gameObject);
        collidingFrames.Remove(indicatorobject);

        hullpartName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(collidingFrames[0]).name;

        return QueryForDataOnPoint(indicatorobject, hit, vesselName, compartmentUid, hullpartName, ref plateName, ref plateMetadataComponent);
    }

    public RaycastHit QueryForDataOnPoint(GameObject indicatorobject, RaycastHit hit, string vesselName, string compartmentName, string hullpartName, ref string plateName, ref Subpart plateMetadataComponent)
    {
        RaycastHit[] hits = new RaycastHit[0];
        List<GameObject> collidingPlates = new List<GameObject>();

        Subpart subpart = null;
        Hullpart hullpart = GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName);
        var plateCollection = hullpart.GetSubparts(SubpartType.Plate, s => s);
        if (plateCollection.Count() == 1)
        {
            subpart = plateCollection[0];

            if (subpart != null)
            {
                plateMetadataComponent = subpart;
                plateName = subpart.prt_name;
            }

            return hit;
        }
        else
        {
            List<Collider> plates = GroupingManager.Instance.vesselObject.GetSubparts(compartmentName, hullpartName, SubpartType.All, s => s.GetCollider());

            List<Collider> disabledColliders = new List<Collider>();
            plates.ForEach(p => 
            {
                if (!p.enabled)
                {
                    disabledColliders.Add(p);
                    p.enabled = true;
                }
            });

            Ray ray = CameraService.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray);

            hits = hits.ToList().OrderBy(h => h.distance).ToArray();

            if (plateMetadataComponent != null && plateMetadataComponent.metadata.gameObject == hits[1].collider.gameObject)
            {
                return hit;
            }

            foreach (var hited in hits)
            {
                Debug.DrawLine(hited.point, hited.point + Vector3.up, Color.red, 30f);
            }
            collidingPlates = hits.Select(c => c.collider.gameObject).ToList();

            disabledColliders.ForEach(p => p.enabled = false);

            collidingPlates.Remove(hit.collider.gameObject);
            collidingPlates.Remove(indicatorobject);

            if(collidingPlates.Count>0)
            {
                subpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(collidingPlates[0]);
            }

            if (subpart != null)
            {
                plateMetadataComponent = subpart;
                plateName = subpart.prt_name;
            }

            return hits.Where(tempHit => tempHit.collider.gameObject == collidingPlates[0]).FirstOrDefault();
        }
    }

    public void UpdateGaugePointBasedId(string data)
    {
        var dataArray = data.Split('/');
        string compartment = dataArray[0];
        string frameName = dataArray[1];
        string plateName = dataArray[2];
        int id = int.Parse(dataArray[3]);
        float measuredThickness = float.Parse(dataArray[4]);

        if(GroupingManager.Instance.vesselObject.TryGetSubpart(compartment, frameName, plateName, out Subpart subpart))
        {
            if(subpart.gaugePoints.TryGetValue(id, out GaugingPointIndicator gaugePointIndicator))
            {
                gaugePointIndicator.UpdateThickness(measuredThickness);
            }
        }
    }

    public void CaptureGaugePointIndependent(CastHit castHit)
    {
        GameObject indicatorobject = Instantiate(gaugePointRepresentation.gameObject, castHit.hit.point, Quaternion.identity);

        indicatorobject.transform.forward = castHit.hit.normal;
        indicatorobject.transform.SetParent(transform);
        var plateMetadataComponent = castHit.subpartReference.metadata;

        string plateName = plateMetadataComponent.GetValue("PRT_NAME");
        string frameName = castHit.subpartReference.owningHullpart.name;
        string compartmentName = castHit.subpartReference.owningHullpart.owningCompartmentUid;
        if (!plateMetadataComponent.TryGetValue("NAME", out string PlatefullName))
        {
            PlatefullName = $"{compartmentName}/{frameName}/{plateName}".GetHashCode().ToString();
        }

        GaugePointData pointData = new GaugePointData()
        {
            plate = plateName,
            frame = frameName,
            uId = compartmentName,
            comparment = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName).name,
            location = castHit.hit.point,
            normal = castHit.hit.normal,
            fullName = PlatefullName,
            ruledThickness = 15f,
            measuredThickness = -1f
        };
        if (plateMetadataComponent.ContainsKey("THICKNESS"))
        {
            pointData.originalThickness = float.Parse(plateMetadataComponent.GetValue("THICKNESS")) * 1000f;// as the value is coming in meters not mm.
        }

        pointData.uId = compartmentName;

        if (preLoadRandomDataOnPlacement)
        {
            pointData.Randomize();
        }

        GaugingPointIndicator gaugingPointIndicator = indicatorobject.GetComponent<GaugingPointIndicator>();
        gaugingPointIndicator.Initialize(pointData, castHit.subpartReference);

        castHit.subpartReference.AssociateGaugePoint(pointData, gaugingPointIndicator);
        //castHit.subpartReference.gaugePoints.Add(pointData.id, gaugingPointIndicator);
        indicatorobject.GetComponentInChildren<TextMeshPro>().text = pointData.id.ToString();
    }

    private void CaptureGaugePoint(RaycastHit hit)
    {
        GameObject indicatorobject = Instantiate(gaugePointRepresentation.gameObject, hit.point, Quaternion.identity);

        indicatorobject.transform.forward = hit.normal;

        string plateName = "";
        string frameName = "";
        string compartmentName = "";
        string PlatefullName = "";

        MetadataComponent compartmentDataComponent = null;
        Subpart targetedSubpart = null;

        switch (ApplicationStateMachine.Instance.currentStateName)
        {
            case nameof(VesselViewState):
                // Caution Vessel level gauging is incomplete.
                compartmentName = GroupingManager.Instance.vesselObject.ProcessCompartmentSelection(hit.collider.gameObject).uid;
                hit = QueryForDataOnPoint(indicatorobject, hit, ApplicationStateMachine.Instance.vesselName, compartmentName, ref frameName, ref plateName, ref targetedSubpart);
                indicatorobject.transform.position = hit.point;
                indicatorobject.transform.forward = hit.normal;
                break;
            case nameof(CompartmentViewState):
                if (subpartExpectation.AreExpectationsMet(hit.collider.transform.gameObject))
                {
                    CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                    compartmentName = compartmentViewState.GetTargettedCompartemnt();
                    targetedSubpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(hit.collider.transform.gameObject);
                    frameName = targetedSubpart.owningHullpart.name;
                    plateName = targetedSubpart.metadata.GetValue("PRT_NAME");
                    hit = QueryForDataOnPoint(indicatorobject, hit, ApplicationStateMachine.Instance.vesselName, compartmentName, frameName, ref plateName, ref targetedSubpart);
                    indicatorobject.transform.position = hit.point;
                    indicatorobject.transform.forward = hit.normal;
                    if (!targetedSubpart.metadata.TryGetValue("NAME", out PlatefullName))
                    {
                        PlatefullName = $"{compartmentName}/{frameName}/{plateName}".GetHashCode().ToString();
                    }
                    if (plateName == "")
                    {
                        Destroy(indicatorobject.gameObject);
                        return;
                    }
                }
                else
                {
                    CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                    compartmentName = compartmentViewState.GetTargettedCompartemnt();
                    frameName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(hit.collider.transform.gameObject).name;
                    hit = QueryForDataOnPoint(indicatorobject, hit, ApplicationStateMachine.Instance.vesselName, compartmentName, frameName, ref plateName, ref targetedSubpart);
                    indicatorobject.transform.position = hit.point;
                    indicatorobject.transform.forward = hit.normal;
                    if (!targetedSubpart.metadata.TryGetValue("NAME", out PlatefullName))
                    {
                        PlatefullName = $"{compartmentName}/{frameName}/{plateName}".GetHashCode().ToString();
                    }
                    if (plateName == "")
                    {
                        Destroy(indicatorobject.gameObject);
                        return;
                    }
                }
                break;
            case nameof(HullpartViewState):
                HullpartViewState hullpartViewState = ApplicationStateMachine.Instance.CurrentState as HullpartViewState;
                targetedSubpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(hit.collider.gameObject);
                plateName = targetedSubpart.metadata.GetValue("PRT_NAME");
                if (!targetedSubpart.metadata.TryGetValue("NAME", out PlatefullName))
                {
                    PlatefullName = $"{compartmentName}/{frameName}/{plateName}".GetHashCode().ToString();
                }
                compartmentName = hullpartViewState.compartmentUid;
                frameName = hullpartViewState.hullpartName;
                break;
            default:
                break;
        }

        indicatorobject.transform.SetParent(transform);

        var compartment = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName);
        compartmentDataComponent = compartment.compartmentMetaData;

        string ActualCompartmentName;
        if (compartmentDataComponent.TryGetValue("NAME", out string CompartmentNameParam))
        {
            ActualCompartmentName = CompartmentNameParam;
        }
        else
        {
            ActualCompartmentName = compartmentName;
        }

        GaugePointData pointData = new GaugePointData()
        {
            plate = plateName,
            frame = frameName,
            comparment = ActualCompartmentName,
            uId = compartment.uid,
            location = hit.point,
            normal = hit.normal,
            measuredThickness = -1f,
            fullName = PlatefullName
        };

        pointData.originalThickness = targetedSubpart.buildThickness.IsNullOrEmpty() ? 0f : float.Parse(targetedSubpart.buildThickness);
        pointData.originalThickness *= pointData.originalThickness < 1 ? 1000f : 1f;
        if (targetedSubpart.metadata.TryGetValue("RULE_THICKNESS", out string ruleThickness))
        {
            pointData.ruledThickness = float.Parse(ruleThickness);
        }
        else
        {
            pointData.ruledThickness = 0f;
        }
        if (compartmentDataComponent.ContainsKey("ASSET_UID"))
        {
            pointData.uId = compartmentDataComponent.GetValue("ASSET_UID");
        }

        if (preLoadRandomDataOnPlacement)
        {
            pointData.Randomize();
        }

        GaugingPointIndicator gaugingPointIndicator = indicatorobject.GetComponent<GaugingPointIndicator>();
        gaugingPointIndicator.Initialize(pointData, targetedSubpart);
        //targetedSubpart.gaugePoints.Add(pointData.id, gaugingPointIndicator);
        targetedSubpart.AssociateGaugePoint(pointData, gaugingPointIndicator);

        CommunicationManager.Instance.HandleSingleGaugePointCreation_Extern(GetGaugePointInJSONFormat_Nested(pointData));
        CommunicationManager.Instance.HandleSingleGaugePointCreation_Flat_Extern(GetGaugePointInJSONFormat(pointData));
        //CommunicationManager.Instance.HandleSingleGaugePointCreation_Extern(GetGaugePointsPlateInJSONFormat(targetedSubpart, pointData));

#if FULL_GAUGEPOINTS
        GenerateJSON(true);
#endif

        deserializedData = null;
        indicatorobject.GetComponentInChildren<TextMeshPro>().text = pointData.id.ToString() ;
    }

    public void GenerateJSON(bool isManualgauging = false)
    {
        string rawData = GetGaugePointsInJSONFormat();

        CommunicationManager.Instance.HandleGaugePointCreation_Extern(rawData);
        Image_Manager.Instance.data_testFpso = rawData;
        if (enableDebug)
        {
            Debug.Log(rawData);
        }

        if (!isManualgauging)
        {
#if UNITY_EDITOR
            JsonFileWriter.WriteJsonToFile(rawData, "Heavy Gauge Plan");
#endif
        }
    }

    public string GetGaugePointsPlateInJSONFormat(Subpart subpart, GaugePointData pointData)
    {
        GaugePointsData_Plate_Flat_External gaugePointsData = new GaugePointsData_Plate_Flat_External();

        gaugePointsData.assetName = pointData.comparment;
        gaugePointsData.assetUID = pointData.uId;
        gaugePointsData.frameName = pointData.frame;
        gaugePointsData.frame_Id = (pointData.uId + "/" + pointData.frame).GetHashCode().ToString();
        gaugePointsData.plateName = pointData.plate;
        gaugePointsData.plate_Id = pointData.fullName;

        subpart.gaugePoints.Foreach(item =>
        {
            GaugePointData_External gaugePointData_External = new GaugePointData_External();
            gaugePointData_External.point_Id = item.Value.pointData.id.ToString();
            gaugePointData_External.location = item.Value.pointData.location.GetStringyVector();
            gaugePointData_External.normal = item.Value.pointData.normal.GetStringyVector();
            gaugePointData_External.originalThickness = item.Value.pointData.originalThickness;
            gaugePointData_External.measuredThickness = item.Value.pointData.measuredThickness;
            gaugePointsData.gaugingPoints.Add(gaugePointData_External);
        });

        string rawData = JsonUtility.ToJson(gaugePointsData, true);

        if (enableDebug)
        {
            Debug.Log(rawData);
        }

        return rawData;
    }

    public string GetGaugePointInJSONFormat(GaugePointData pointData, bool deleted = false)
    {
        GaugePoint_External gaugePointData = new GaugePoint_External();

        gaugePointData.assetName = pointData.comparment;
        gaugePointData.assetUID = pointData.uId;
        gaugePointData.frameName = pointData.frame;
        gaugePointData.frame_Id = (pointData.uId + "/" + pointData.frame).GetHashCode().ToString();
        gaugePointData.plateName = pointData.plate;
        gaugePointData.plate_Id = pointData.fullName;
        gaugePointData.point_Id = pointData.id.ToString();
        gaugePointData.location = pointData.location.GetStringyVector();
        gaugePointData.normal = pointData.normal.GetStringyVector();
        gaugePointData.originalThickness = pointData.originalThickness;
        gaugePointData.measuredThickness = pointData.measuredThickness;

        string rawData = JsonUtility.ToJson(gaugePointData, true);

        if (enableDebug)
        {
            Debug.Log(rawData);
        }

        return rawData;
    }

    public string GetGaugePointInJSONFormat_Nested(GaugePointData pointData, bool deleted = false)
    {
        GaugePointsData_External gaugePointsData = new GaugePointsData_External();

        GaugePointsData_Compartment_External compartmentData = new GaugePointsData_Compartment_External();
        compartmentData.assetName = pointData.comparment;
        compartmentData.assetUID = pointData.uId;
        compartmentData.isDeleted = false;
        gaugePointsData.compartments.Add(compartmentData);

        GaugePointsData_Frame_External frameData = new GaugePointsData_Frame_External();
        frameData.frameName = pointData.frame;
        frameData.frame_Id = (compartmentData.assetUID + "/" + frameData.frameName).GetHashCode().ToString();
        frameData.isDeleted = false;
        compartmentData.frames.Add(frameData);

        GaugePointsData_Plate_External plateData = new GaugePointsData_Plate_External();
        plateData.plateName = pointData.plate;
        plateData.plate_Id = pointData.fullName;
        plateData.isDeleted = false;
        frameData.plates.Add(plateData);

        GaugePointData_External pointData_External = new GaugePointData_External()
        {
            point_Id = pointData.id.ToString(),
            location = pointData.location.GetStringyVector(),
            normal = pointData.normal.GetStringyVector(),
            originalThickness = pointData.originalThickness,
            measuredThickness = pointData.measuredThickness,
            isDeleted = deleted
        };
        plateData.gaugingPoints.Add(pointData_External);

        string rawData = JsonUtility.ToJson(gaugePointsData, true);

        if (enableDebug)
        {
          //  Debug.Log(rawData);
        }

        return rawData;
    }

    public GaugePointsData_External GetGaugePointsData_External()
    {
        GaugePointsData_External gaugePointsData = new GaugePointsData_External();

        GroupingManager.Instance.vesselObject.RunOverAllSubparts(s => s.gaugePoints.Foreach(pointData =>
        {
            var compartmentData = gaugePointsData.compartments.Where(c => c.assetUID == pointData.Value.pointData.uId).FirstOrDefault();
            if (compartmentData == null)
            {
                compartmentData = new GaugePointsData_Compartment_External();
                compartmentData.assetName = pointData.Value.pointData.comparment.ToPascalCase();
                compartmentData.assetUID = pointData.Value.pointData.uId;
                compartmentData.isDeleted = false;
                gaugePointsData.compartments.Add(compartmentData);
            }

            var frameData = compartmentData.frames.Where(f => f.frameName == pointData.Value.pointData.frame).FirstOrDefault();
            if (frameData == null)
            {
                frameData = new GaugePointsData_Frame_External();
                frameData.frameName = pointData.Value.pointData.frame;
                frameData.frame_Id = (compartmentData.assetUID + "/" + frameData.frameName).GetHashCode().ToString();
                frameData.isDeleted = false;
                compartmentData.frames.Add(frameData);
            }

            var plateData = frameData.plates.Where(p => p.plateName == pointData.Value.pointData.plate).FirstOrDefault();
            if (plateData == null)
            {
                plateData = new GaugePointsData_Plate_External();
                plateData.plateName = pointData.Value.pointData.plate;
                plateData.plate_Id = pointData.Value.pointData.fullName;
                plateData.isDeleted = false;
                frameData.plates.Add(plateData);
            }

            GaugePointData_External pointData_External = new GaugePointData_External()
            {
                point_Id = pointData.Value.pointData.id.ToString(),
                location = pointData.Value.pointData.location.GetStringyVector(),
                normal = pointData.Value.pointData.normal.GetStringyVector(),
                originalThickness = pointData.Value.pointData.originalThickness,
                measuredThickness = pointData.Value.pointData.measuredThickness,
                isDeleted = false
            };
            plateData.gaugingPoints.Add(pointData_External);
        }));

        return gaugePointsData;
    }

    public string GetGaugePointsInJSONFormat()
    {
        return JsonUtility.ToJson(GetGaugePointsData_External(), true);
    }

    public string GetGaugePointsInJSONFormat(string compartmentuid)
    {
        GaugePointsData_External gaugePointsData = new GaugePointsData_External();

        if (GroupingManager.Instance.vesselObject.TryGetCompartment(compartmentuid, out Compartment compartment))
        {
            compartment.RunOverAllSubparts(s => s.gaugePoints.Foreach(pointData =>
            {
                var compartmentData = gaugePointsData.compartments.Where(c => c.assetUID == pointData.Value.pointData.uId).FirstOrDefault();
                if (compartmentData == null)
                {
                    compartmentData = new GaugePointsData_Compartment_External();
                    compartmentData.assetName = pointData.Value.pointData.comparment;
                    compartmentData.assetUID = pointData.Value.pointData.uId;
                    compartmentData.isDeleted = false;
                    gaugePointsData.compartments.Add(compartmentData);
                }

                var frameData = compartmentData.frames.Where(f => f.frameName == pointData.Value.pointData.frame).FirstOrDefault();
                if (frameData == null)
                {
                    frameData = new GaugePointsData_Frame_External();
                    frameData.frameName = pointData.Value.pointData.frame;
                    frameData.frame_Id = (compartmentData.assetUID + "/" + frameData.frameName).GetHashCode().ToString();
                    frameData.isDeleted = false;
                    compartmentData.frames.Add(frameData);
                }

                var plateData = frameData.plates.Where(p => p.plateName == pointData.Value.pointData.plate).FirstOrDefault();
                if (plateData == null)
                {
                    plateData = new GaugePointsData_Plate_External();
                    plateData.plateName = pointData.Value.pointData.plate;
                    plateData.plate_Id = pointData.Value.pointData.fullName;
                    plateData.isDeleted = false;
                    frameData.plates.Add(plateData);
                }

                GaugePointData_External pointData_External = new GaugePointData_External()
                {
                    point_Id = pointData.Value.pointData.id.ToString(),
                    location = pointData.Value.pointData.location.GetStringyVector(),
                    normal = pointData.Value.pointData.normal.GetStringyVector(),
                    originalThickness = pointData.Value.pointData.originalThickness,
                    measuredThickness = pointData.Value.pointData.measuredThickness,
                    isDeleted = false
                };
                plateData.gaugingPoints.Add(pointData_External);
            }));
        }
        return JsonUtility.ToJson(gaugePointsData, true);
    }

    public string GetGaugePointsInJSONFormat(string compartmentuid, string hullpartName)
    {
        GaugePointsData_External gaugePointsData = new GaugePointsData_External();

        if (GroupingManager.Instance.vesselObject.TryGetHullpart(compartmentuid, hullpartName, out Hullpart hullpart))
        {
            hullpart.RunOverAllSubparts(s => s.gaugePoints.Foreach(pointData =>
            {
                var compartmentData = gaugePointsData.compartments.Where(c => c.assetUID == pointData.Value.pointData.uId).FirstOrDefault();
                if (compartmentData == null)
                {
                    compartmentData = new GaugePointsData_Compartment_External();
                    compartmentData.assetName = pointData.Value.pointData.comparment;
                    compartmentData.assetUID = pointData.Value.pointData.uId;
                    compartmentData.isDeleted = false;
                    gaugePointsData.compartments.Add(compartmentData);
                }

                var frameData = compartmentData.frames.Where(f => f.frameName == pointData.Value.pointData.frame).FirstOrDefault();
                if (frameData == null)
                {
                    frameData = new GaugePointsData_Frame_External();
                    frameData.frameName = pointData.Value.pointData.frame;
                    frameData.frame_Id = (compartmentData.assetUID + "/" + frameData.frameName).GetHashCode().ToString();
                    frameData.isDeleted = false;
                    compartmentData.frames.Add(frameData);
                }

                var plateData = frameData.plates.Where(p => p.plateName == pointData.Value.pointData.plate).FirstOrDefault();
                if (plateData == null)
                {
                    plateData = new GaugePointsData_Plate_External();
                    plateData.plateName = pointData.Value.pointData.plate;
                    plateData.plate_Id = pointData.Value.pointData.fullName;
                    plateData.isDeleted = false;
                    frameData.plates.Add(plateData);
                }

                GaugePointData_External pointData_External = new GaugePointData_External()
                {
                    point_Id = pointData.Value.pointData.id.ToString(),
                    location = pointData.Value.pointData.location.GetStringyVector(),
                    normal = pointData.Value.pointData.normal.GetStringyVector(),
                    originalThickness = pointData.Value.pointData.originalThickness,
                    measuredThickness = pointData.Value.pointData.measuredThickness,
                    isDeleted = false
                };
                plateData.gaugingPoints.Add(pointData_External);
            }));
        }
        return JsonUtility.ToJson(gaugePointsData, true);
    }

    public string GetGaugePointsInJSONFormat(string compartmentuid, string hullpartName, string subpartName)
    {
        GaugePointsData_External gaugePointsData = new GaugePointsData_External();

        if (GroupingManager.Instance.vesselObject.TryGetSubpart(compartmentuid, hullpartName, subpartName, out Subpart subpart))
        {
            subpart.gaugePoints.Foreach(pointData =>
            {
                var compartmentData = gaugePointsData.compartments.Where(c => c.assetUID == pointData.Value.pointData.uId).FirstOrDefault();
                if (compartmentData == null)
                {
                    compartmentData = new GaugePointsData_Compartment_External();
                    compartmentData.assetName = pointData.Value.pointData.comparment;
                    compartmentData.assetUID = pointData.Value.pointData.uId;
                    compartmentData.isDeleted = false;
                    gaugePointsData.compartments.Add(compartmentData);
                }

                var frameData = compartmentData.frames.Where(f => f.frameName == pointData.Value.pointData.frame).FirstOrDefault();
                if (frameData == null)
                {
                    frameData = new GaugePointsData_Frame_External();
                    frameData.frameName = pointData.Value.pointData.frame;
                    frameData.frame_Id = (compartmentData.assetUID + "/" + frameData.frameName).GetHashCode().ToString();
                    frameData.isDeleted = false;
                    compartmentData.frames.Add(frameData);
                }

                var plateData = frameData.plates.Where(p => p.plateName == pointData.Value.pointData.plate).FirstOrDefault();
                if (plateData == null)
                {
                    plateData = new GaugePointsData_Plate_External();
                    plateData.plateName = pointData.Value.pointData.plate;
                    plateData.plate_Id = pointData.Value.pointData.fullName;
                    plateData.isDeleted = false;
                    frameData.plates.Add(plateData);
                }

                GaugePointData_External pointData_External = new GaugePointData_External()
                {
                    point_Id = pointData.Value.pointData.id.ToString(),
                    location = pointData.Value.pointData.location.GetStringyVector(),
                    normal = pointData.Value.pointData.normal.GetStringyVector(),
                    originalThickness = pointData.Value.pointData.originalThickness,
                    measuredThickness = pointData.Value.pointData.measuredThickness,
                    isDeleted = false
                };
                plateData.gaugingPoints.Add(pointData_External);
            });
        }
        return JsonUtility.ToJson(gaugePointsData, true);
    }

    public void SetAllGaugePointsActive(bool value)
    {
        GroupingManager.Instance.vesselObject.RunOverAllSubparts(s => s.SetGaugePointsActive(value));
    }

    public void SetGaugePointsActive(string compartmentUID, bool value)
    {
        GroupingManager.Instance.vesselObject.GetCompartment(compartmentUID).RunOverAllSubparts(s => s.SetGaugePointsActive(value));
        //GroupingManager.Instance.vesselObject.GetCompartment(compartmentUID).RunOverAllSubpartsAsync(s => s.SetGaugePointsActive(value));
    }

    public void SetGaugePointsActive(string compartmentUID, string frameName, bool value)
    {
        GroupingManager.Instance.vesselObject.GetHullpart(compartmentUID, frameName).RunOverAllSubparts(s => s.SetGaugePointsActive(value));
    }

    public List<KeyValuePair<GameObject, GaugePointData>> GetGaugePoints(string compartmentUID, string frameName)
    {
        var res = new List<KeyValuePair<GameObject, GaugePointData>>();

        if(GroupingManager.Instance.vesselObject.TryGetHullpart(compartmentUID, frameName, out Hullpart hullpart))
        {
            hullpart.RunOverAllSubparts(s => s.gaugePoints.Foreach(p => 
            {
                res.Add(new KeyValuePair<GameObject, GaugePointData>(p.Value.gameObject, p.Value.pointData));
            }));
        }

        return res;
    }

    private void OnDrawGizmos()
    {
        if(!enableDebugDrawing)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        GroupingManager.Instance.vesselObject.RunOverAllSubparts(s => s.gaugePoints.Foreach(pointData =>
        {
            Gizmos.DrawWireSphere(pointData.Value.pointData.location, visualSize);
        }));
    }
}