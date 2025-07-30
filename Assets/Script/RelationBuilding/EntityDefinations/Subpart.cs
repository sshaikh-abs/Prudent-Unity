using GLTFast.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public enum GaugePointUniqueness
{
    SubpartLevel,
    HullpartLevel_Seperate,
    HullpartLevel_Unison,
    CompartmentLevel_Seperate,
    CompartmentLevel_Unison
}

[Serializable]
public class Subpart
{
    public const GaugePointUniqueness gaugePointUniqueness = GaugePointUniqueness.HullpartLevel_Unison;

    /// <summary>
    /// Display Name of the subpart
    /// </summary>
    public string name;

    /// <summary>
    /// Unique Name of the subpart
    /// </summary>
    [NonSerialized] public string prt_name;

    /// <summary>
    /// The build thickness of the subpart.
    /// </summary>
    [NonSerialized] public string buildThickness;

    /// <summary>
    /// The subpart type.
    /// </summary>
    [NonSerialized] public SubpartType type;

    /// <summary>
    /// Raw metadata associated with this subpart.
    /// </summary>
    [NonSerialized] public MetadataComponent metadata;

    /// <summary>
    /// subpart gameobject reference.
    /// </summary>
    [NonSerialized] public GameObject subpartObjectMeshReference;

    /// <summary>
    /// subpart's collider reference.
    /// </summary>
    [NonSerialized] private Collider subpartObjectColliderReference;

    /// <summary>
    /// entites with same signature and parameters within same sectioned hullpart.
    /// </summary>
    public List<GameObject> connectedEntities = new List<GameObject>();

    /// <summary>
    /// shader entires that pan accross multiple compartments with same signature and parameters.
    /// </summary>
    [NonSerialized] public List<Subpart> connectedSubparts = new List<Subpart>();

    /// <summary>
    /// owning sectioned hullpart reference.
    /// </summary>
    [NonSerialized] public Hullpart owningHullpart;

    /// <summary>
    /// owning compartment reference.
    /// </summary>
    [NonSerialized] public Compartment owningCompartment;

    /// <summary>
    /// list of all associated compartment uids.
    /// </summary>
    [NonSerialized] public List<string> associatedCompartments = new List<string>();

    /// <summary>
    /// assocaited gaugepoints.
    /// </summary>
    public BiDictionary<int, GaugingPointIndicator> gaugePoints = new BiDictionary<int, GaugingPointIndicator>();

    public MeshRenderer subpartMeshRenderer => subpartObjectMeshReference.GetComponent<MeshRenderer>();

    /// <summary>
    /// a colorCache to revert back when coloring for dimunation value.
    /// </summary>
    private Color colorCache;

    #region Builder Related

    /// +-----------------------------------------------------------------------------------------+
    /// |                                     Builder Related                                     |
    /// +-----------------------------------------------------------------------------------------+

    public Subpart(MetadataComponent plateGameObject, Hullpart owingHullpart, Compartment owningCompartment)
    {
        this.metadata = plateGameObject;
        this.owningHullpart = owingHullpart;
        subpartObjectMeshReference = plateGameObject.gameObject;
        colorCache = subpartMeshRenderer.material.color;
        this.owningCompartment = owningCompartment;
    }

    /// <summary>
    /// Sets the subpart type.
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    /// <param name="type"></param>
    public void SetType(SubpartType type)
    {
        this.type = type;
        switch (type)
        {
            case SubpartType.Plate:
                //GameEvents.SetPlatesActive += SetActiveEntity;
                break;
            case SubpartType.Bracket:
                GameEvents.SetBracketsActive += SetVisibility;
                break;
            case SubpartType.Stiffener:
                GameEvents.SetStiffnersActive += SetVisibility;
                break;
            default:
                break;
        }
    }

    #endregion

    /// <summary>
    /// trackedId for gaugepoints to keep uniqueness of the gaugepoints.
    /// </summary>
    private int gaugepoint_TrackedId = 0;

    public void AssociateGaugePoint(GaugePointData gaugePoint, GaugingPointIndicator gaugingPointIndicator, bool generateId = true)
    {
        int resultId = -1;

        if (generateId)
        {

            if (gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Unison || gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Seperate)
            {
                resultId = owningHullpart.RequestGaugePointId(type);
            }
            else if (gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Unison || gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Seperate)
            {
                resultId = owningCompartment.RequestGaugePointId(type);
            }
            else
            {
                gaugepoint_TrackedId++;
                resultId = gaugepoint_TrackedId;
            }
            gaugePoint.id = resultId;
        }
        else
        {
            resultId = gaugePoint.id;
        }
        gaugePoints.Add(resultId, gaugingPointIndicator, $"{owningCompartment.name}/{owningHullpart.name}/{name}");
    }

    public void ResetTrackingId()
    {
        gaugepoint_TrackedId = 0;
        gaugePoints.Clear();
    }

    public void MarkHighestId(int highestId_Plate, int highestId_Bracket, int highestId_Stiffener)
    {
        if (gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Unison || gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Seperate)
        {
            owningHullpart.MarkHighestId(highestId_Plate, highestId_Bracket, highestId_Stiffener);
        }
        else if (gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Unison || gaugePointUniqueness == GaugePointUniqueness.CompartmentLevel_Seperate)
        {
            owningCompartment.MarkHighestId(highestId_Plate, highestId_Bracket, highestId_Stiffener);
        }
        else
        {
            gaugepoint_TrackedId = highestId_Plate;
        }
    }

    /// <summary>
    /// Sets active all gaugepoints associated to this subpart 
    /// </summary>
    /// <param name="active"></param>
    public void SetGaugePointsActive(bool active)
    {
        gaugePoints.Foreach(g =>
        {
            g.Value.gameObject.SetActive(active);
        });
    }

    /// <summary>
    /// Sets active all gaugepoints associated to this subpart asynchronously.
    /// </summary>
    /// <param name="active"></param>
    public async void SetGaugePointsActiveAsync(bool active)
    {
        foreach (var g in gaugePoints.GetAllValues())
        {
            if (Vessel.stopwatch.ElapsedMilliseconds > PointProjector.Instance.millsecondThreshold)
            {
                await Task.Yield(); // Yield back to main thread
                Vessel.stopwatch.Restart();
            }
            g.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// Make the entity active or inactive based on the parameter.
    /// (NOTE : Doest NOT Considerer the hullparts status [disabled or isolated])
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active)
    {
        if (active)
        {
            if (type == SubpartType.Bracket && !CommunicationManager.Instance.bracketsActive)
            {
                return;
            }

            if (type == SubpartType.Stiffener && !CommunicationManager.Instance.stiffenersActive)
            {
                return;
            }
        }

        subpartObjectMeshReference.SetActive(active);
    }

    /// <summary>
    /// changes only the visibility of the entity, might look highly similar to SetActive but this is only for visibility not of logical.
    /// (NOTE : Considerers the hullparts status [disabled or isolated])
    /// </summary>
    /// <param name="visible">visible status to update to</param>
    public void SetVisibility(bool visible)
    {
        if (visible)
        {
            if (ContextMenuManager.Instance.disabledHullpartsCache.Contains(owningHullpart))
            {
                return;
            }

            var viewState = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>();

            if (viewState != null)
            {
                if (viewState.hullpartName != owningHullpart.name)
                {
                    return;
                }
            }
            else
            {
                var viewState_Comp = ApplicationStateMachine.Instance.GetCurrentState<CompartmentViewState>();
                if(viewState_Comp != null)
                {
                    if (viewState_Comp.GetTargettedCompartemnt() != owningCompartment.uid)
                    {
                        return;
                    }
                }
            }
        }

        gaugePoints.Foreach(g =>
        {
            g.Value.gameObject.SetActive(visible);
        });

        subpartObjectMeshReference.SetActive(visible);
    }

    /// <summary>
    /// Returns the Collider associated to this subpart.
    /// </summary>
    public Collider GetCollider()
    {
        if (subpartObjectColliderReference == null)
        {
            subpartObjectColliderReference = subpartObjectMeshReference.GetComponent<Collider>();
            if (subpartObjectColliderReference == null)
            {
                if (subpartObjectMeshReference.GetComponent<MeshFilter>() == null)
                {
                    Debug.Log($"{prt_name} has failed to build", subpartObjectMeshReference);
                    return null;
                }

                MeshCollider meshCollider = subpartObjectMeshReference.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = subpartObjectMeshReference.GetComponent<MeshFilter>().sharedMesh;
                subpartObjectColliderReference = meshCollider;
                meshCollider.enabled = false;
            }
        }
        return subpartObjectColliderReference;
    }

    /// <summary>
    /// Applies the passed in color to this subpart and caches the actual color to reset later.
    /// </summary>
    /// <param name="dimunationColor"></param>
    public void ShowDimunation(Color dimunationColor)
    {
        subpartMeshRenderer.material.color = dimunationColor;
        foreach (var support in connectedEntities)
        {
            support.GetComponent<MeshRenderer>().material.color = dimunationColor;
        }
    }

    /// <summary>
    /// Resets the color of the subpart back to the cache.
    /// </summary>
    public void ResetDimunation()
    {
        subpartMeshRenderer.material.color = colorCache;
        foreach (var support in connectedEntities)
        {
            support.GetComponent<MeshRenderer>().material.color = colorCache;
        }
    }

    /// <summary>
    /// Returns the metadata relating to this subpart for Metadata Panel.
    /// </summary>
    /// <returns>Metadata in JSON Format</returns>
    public string GetMetadata()
    {
        var compartmentNames = associatedCompartments.Select(c => GroupingManager.Instance.vesselObject.GetCompartment(c).name).ToList();
        string compartments = string.Join(",", compartmentNames);

        string ruleThickness = this.metadata.GetValue("RULE_THICKNESS");
        if (ruleThickness == null)
        {
            ruleThickness = "---";
        }
        string area = this.metadata.GetValue("AREA");
        if (area == null)
        {
            area = "---";
        }
        string allowableCorrosion = this.metadata.GetValue("ALLOWABLE_CORROSION");
        if (allowableCorrosion == null)
        {
            allowableCorrosion = "0";
        }
        string material = this.metadata.GetValue("MATERIAL");
        if (material == null)
        {
            material = "---";
        }

        List<MetadataKeyValuePair> metadata = new List<MetadataKeyValuePair>()
        {
            new MetadataKeyValuePair("NAME", name),
            new MetadataKeyValuePair("TYPE", type.ToString()),
            new MetadataKeyValuePair("ASSOCIATED_HULLPART", owningHullpart.name),
            //new MetadataKeyValuePair("ASSOCIATED_COMPARTMENTS", compartments),
            new MetadataKeyValuePair("BUILD_THICKNESS", buildThickness),
            new MetadataKeyValuePair("RULE_THICKNESS", ruleThickness),
            new MetadataKeyValuePair("AREA", area),
            new MetadataKeyValuePair("ALLOWABLE_CORROSION", allowableCorrosion),
            new MetadataKeyValuePair("MATERIAL", material),
        };

        Metadata metaDataObject = new Metadata()
        {
            metaData = metadata,
        };

        return JsonUtility.ToJson(metaDataObject, true);
    }
}

public static class SubpartTypeHelper
{
    public static bool HasFlag(this SubpartType value, SubpartType flag)
    {
        return (value & flag) == flag;
    }
}

[System.Flags]
public enum SubpartType
{
    None = 0,
    Plate = 1 << 0,
    Bracket = 1 << 1,
    Stiffener = 1 << 2,
    All = Plate | Bracket | Stiffener
}