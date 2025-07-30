using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public enum PatternType
{
    None,
    Star,
    Grid,
    ZigZag,
    Circle,
    OutterMost
}

public struct CastHit
{
    public Subpart subpartReference;
    public RaycastHit hit;

    public CastHit(Subpart subpartReference, RaycastHit hit)
    {
        this.subpartReference = subpartReference;
        this.hit = hit;
    }
}

[System.Serializable]
public class AutoGaugingPatternDefination
{
    public string compartmentUid;
    public string platePattern;
    public string bracketPattern;
    public string stiffenerPattern;
}

public class PointProjector : SingletonMono<PointProjector>
{
    public List<Subpart> selectedSubparts = new List<Subpart>();

    public Vector3 position;
    public Vector3 size;
    public int millsecondThreshold = 300;

    public List<CastHit> raycastHits = new List<CastHit>();
    public PatternType patternType;
    public Vector3 offset = Vector3.zero;
    public SubpartType queryType;
    public int iterations = 3;
    public float spacing = 0.25f;
    public float threshold = 0.1f;
    public float rayCheckLength = 0.5f;
    public float areaThreshold = 0.1f;
    public float radius = 5f;

    public bool hullpartLevel = false;
    public bool compartmentLevel = false;
    public bool autoGauging = false;

    public AutoGaugingPatternDefination defination;
    public SubpartType subpartType_Debug = SubpartType.Bracket;

    [ContextMenu(nameof(PrintDefination))]
    public void PrintDefination()
    {
        string json = JsonUtility.ToJson(defination);
        JsonFileWriter.WriteJsonToFile(json, "AutoGaugingPatternDefination");
    }


    private void Start()
    {
        OutlineSelectionHandler.OnSelectedSubpart += OnSelectedSubpart;
        OutlineSelectionHandler.OnSelectedHullpart += OnSelectedHullpart;
        OutlineSelectionHandler.OnSelectedCompartment += OnSelectedCompartment;
        //OutlineSelectionHandler.OnClickedOnEmptySpace += OnClickedOnEmptySpace; // INPROGRESS : This is the culprit that stops you from capturing the gaugepoints 
    }

    private void OnClickedOnEmptySpace()
    {
        if (selectedSubparts.Count > 0)
        {
            selectedSubparts.Clear();
            raycastHits.Clear();
        }
    }

    private void OnSelectedCompartment(Compartment compartment, bool isFromRightMouse)
    {
        if (!autoGauging || isFromRightMouse)
        {
            return;
        }

        if (compartmentLevel)
        {
            selectedSubparts.Clear();
            selectedSubparts.AddRange(compartment.data.SelectMany(h => h.GetSubparts(queryType, s => s)).ToList());
            CastRays();
        }
    }

    void OnCompartmentLoadedCallback(string compartmentUid)
    {
        autoGauging = true;
        selectedSubparts.Clear();
        selectedSubparts.AddRange(GroupingManager.Instance.vesselObject.GetCompartment(compartmentUid).data.SelectMany(h => h.GetSubparts(queryType, s => s)).ToList());
        CastRays();

        OnCaptureButtonClicked(compartmentUid);
        EventBroadcaster.OnCompartmentLoadingComplete -= OnCompartmentLoadedCallback;
    }

    public void OnCallAutoGauge(string rawData)
    {
        defination = JsonUtility.FromJson<AutoGaugingPatternDefination>(rawData);
        EventBroadcaster.OnCompartmentLoadingComplete += OnCompartmentLoadedCallback;
        ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(CompartmentViewState), new List<string>() { defination.compartmentUid });
    }

    private void OnSelectedHullpart(Hullpart hullpart, bool isFromRightMouse)
    {
        if (!autoGauging || isFromRightMouse)
        {
            return;
        }

        if (hullpartLevel)
        {
            selectedSubparts.Clear();
            selectedSubparts.AddRange(hullpart.GetSubparts(queryType, s => s));
            CastRays();
        }
    }

    private void OnDestroy()
    {
        OutlineSelectionHandler.OnSelectedSubpart -= OnSelectedSubpart;
    }

    private void OnSelectedSubpart(Subpart subpart, bool isFromRightMouse)
    {
        if (!autoGauging || isFromRightMouse)
        {
            return;
        }

        if (!queryType.HasFlag(subpart.type))
        {
            return;
        }

        if (selectedSubparts.Contains(subpart))
        {
            selectedSubparts.Remove(subpart);
        }
        else
        {
            selectedSubparts.Add(subpart);
        }

        CastRays();
    }

    public void OnCaptureButtonClicked(string compartmentUid = null)
    {
        OnCaptureButtonPressed_Async(compartmentUid);
    }

    public async void OnCaptureButtonPressed_Async(string compartmentUid)
    {
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        foreach (var castHit in raycastHits)
        {
            GaugingManager.Instance.CaptureGaugePointIndependent(castHit);
            
            if(stopwatch.Elapsed.TotalMilliseconds > millsecondThreshold)
            {
                await Task.Yield();
                stopwatch.Restart();
            }
        }

        if(compartmentUid.IsNullOrEmpty())
        {
            return;
        }

        if (compartmentUid.IsNullOrEmpty())
        {
            GaugingManager.Instance.GenerateJSON();
        }
        else
        {
            Compartment compartment = GroupingManager.Instance.vesselObject.GetCompartment(compartmentUid);
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFolderPanel("Select Folder", Application.dataPath, "");
#endif

            foreach (var item in compartment.data)
            {
                string rawData = GaugingManager.Instance.GetGaugePointsInJSONFormat(compartmentUid, item.name);
#if UNITY_EDITOR
                JsonFileWriter.WriteJsonToFileAtPath(rawData, $"{compartmentUid}_{item.name}_AutogaugedPlane", path);
#endif
                CommunicationManager.Instance.HandleGaugePointCreation_Extern(rawData);
                await Task.Yield();
            }

            CommunicationManager.Instance.HandleOnAutoGaugeCompleted();
        }

        OnClickedOnEmptySpace();
    }

    private PatternFetcher _PatternFetcher;
    public PatternFetcher PatternFetcher
    {
        get
        {
            if (_PatternFetcher == null)
            {
                _PatternFetcher = GetComponent<PatternFetcher>();
            }
            return _PatternFetcher;
        }
    }

    void CastRays()
    {
        raycastHits.Clear();

        foreach (var subpart in selectedSubparts)
        {
            Collider selectedCollider = subpart.GetCollider();
            if (selectedCollider == null)
            {
                return;
            }

            bool activeCahce = subpart.subpartObjectMeshReference.activeSelf;
            bool enabledCahce = selectedCollider.enabled;

            if (!activeCahce)
            {
                subpart.SetActive(true);
            }

            if (!enabledCahce)
            {
                selectedCollider.enabled = true;
            }
            var renderer = subpart.subpartMeshRenderer;

            var hits = PatternFetcher.GetProjectedPoints(renderer, PatternFetcher.GetPatternPositionsOnTarget(renderer, subpart.type, defination, out int pointsAllowed), pointsAllowed, false);

            foreach (var item in hits)
            {
                raycastHits.Add(new CastHit(subpart, item));
            }

            selectedCollider.enabled = enabledCahce;
            subpart.SetActive(activeCahce);
        }
    }
}

public class ProjectionRayData
{
    public Vector3 from;
    public Vector3 to;

    public ProjectionRayData(Vector3 from, Vector3 to)
    {
        this.from = from;
        this.to = to;
    }
}