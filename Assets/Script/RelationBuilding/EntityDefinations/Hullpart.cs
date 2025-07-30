using GLTFast.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Hullpart
{
    /// <summary>
    /// Bounds of this hullpart
    /// </summary>
    public Bounds hullpartBounds;

    /// <summary>
    /// Global bounds of this hullpart.
    /// </summary>
    public Bounds hullpartBoundsGlobal;

    /// <summary>
    /// Name of the Hullpart.
    /// </summary>
    public string name;

    /// <summary>
    /// Type of the hullpart.
    /// </summary>
    public string hullpartType;

    /// <summary>
    /// This is deprecated as we are no longer using the metadata component for hullpart.
    /// </summary>
    [NonSerialized] public MetadataComponent hullpartMetadata;

    /// <summary>
    /// data means all the subparts.
    /// </summary>
    [NonSerialized] public List<Subpart> data = new List<Subpart>(); // remove Non Serialized if we want to send subparts as well
    [NonSerialized] public string owningCompartmentUid;
    [NonSerialized] public float originalThicknessSum;
    [NonSerialized] public GameObject hullpartMeshReference;

    [NonSerialized] private bool validHullpartMesh = false;
    [NonSerialized] private Collider hullpartMeshCollider;
    [NonSerialized] private ILookup<string, Subpart> dataLookup_name;
    [NonSerialized] private ILookup<string, Subpart> dataLookup_displayName;
    [NonSerialized] private Dictionary<string, Subpart> subpartLookup = new Dictionary<string, Subpart>();
    [NonSerialized] public List<string> associatedCompartments = new List<string>();

    private bool hasBoundsInitialized = false;

    #region Builder Related

    /// +-----------------------------------------------------------------------------------------+
    /// |                                     Builder Related                                     |
    /// +-----------------------------------------------------------------------------------------+

    public Hullpart(string name, Compartment owningCompartment)
    {
        try
        {
            this.name = name;
            owningCompartmentUid = owningCompartment.uid;
        }
        catch
        {
            Debug.LogError($"Error with compartment {name}");
        }

        validHullpartMesh = false;
    }

    /// <summary>
    /// Sets up the mesh relation (root plate) for this hullpart.
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="materialToApplyToRootPlate"></param>
    public void SetupHullpartMesh(GameObject gameObject, Material materialToApplyToRootPlate)
    {
        hullpartMeshReference = gameObject;
        validHullpartMesh = true;
        hullpartMeshReference.gameObject.layer = LayerMask.NameToLayer("Frames");
        hullpartMeshReference.GetComponent<MeshRenderer>().sharedMaterial = materialToApplyToRootPlate;
        hullpartMeshReference.GetComponent<MeshRenderer>().renderingLayerMask = 1 << 2;
        hullpartMeshReference.GetComponent<OutlineSelectionHandler>().Initialize();
    }

    private int gaugepoint_TrackedId_Plates = 0;
    private int gaugepoint_TrackedId_Brackets = 0;
    private int gaugepoint_TrackedId_Stiffeners = 0;

    public int RequestGaugePointId(SubpartType type)
    {
        if(Subpart.gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Seperate)
        {
            switch (type)
            {
                case SubpartType.Plate:
                    {
                        gaugepoint_TrackedId_Plates++;
                        return gaugepoint_TrackedId_Plates;
                    }
                case SubpartType.Bracket:
                    {
                        gaugepoint_TrackedId_Brackets++;
                        return gaugepoint_TrackedId_Brackets;
                    }
                case SubpartType.Stiffener:
                    {
                        gaugepoint_TrackedId_Stiffeners++;
                        return gaugepoint_TrackedId_Stiffeners;
                    }
                case SubpartType.All:
                case SubpartType.None:
                default:
                    return -1;
            }
        }
        else
        {
            gaugepoint_TrackedId_Plates++;
            return gaugepoint_TrackedId_Plates;
        }
    }

    public void ResetTrackingId()
    {
        gaugepoint_TrackedId_Plates = 0;
        gaugepoint_TrackedId_Brackets = 0;
        gaugepoint_TrackedId_Stiffeners = 0;
        data.ForEach(s => s.ResetTrackingId());
    }

    public void MarkHighestId(int highestId_Plate, int highestId_Bracket, int highestId_Stiffener)
    {
        gaugepoint_TrackedId_Plates = highestId_Plate;
        gaugepoint_TrackedId_Brackets = highestId_Bracket;
        gaugepoint_TrackedId_Stiffeners = highestId_Stiffener;
    }

    /// <summary>
    /// Registers the subpart to this hullpart. <seealso cref="Subpart"/>
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    /// <param name="subpart">subpart to associate with this hullpart</param>
    public void RegisterSubpart(Subpart subpart)
    {
        data.Add(subpart);
        string name = subpart.metadata.GetValue("PRT_NAME");
        if (!subpartLookup.ContainsKey(name))
        {
            subpartLookup.Add(name, subpart);
        }
        else
        {
            subpartLookup[name].connectedEntities.Add(subpart.metadata.gameObject);
        }
    }

    public void UpdateBounds(Subpart subpart)
    {
        if (!hasBoundsInitialized)
        {
            hullpartBounds = subpart.subpartMeshRenderer.bounds;
            hasBoundsInitialized = true;
        }
        else
        {
            hullpartBounds.Encapsulate(subpart.subpartMeshRenderer.bounds);
        }
    }

    private bool globalBoundsInitialized = false;

    public void RegisterGlobal(Subpart subpart)
    {
        if(!globalBoundsInitialized)
        {
            hullpartBoundsGlobal = subpart.subpartMeshRenderer.bounds;
            globalBoundsInitialized = true;
        }
        else
        {
            hullpartBoundsGlobal.Encapsulate(subpart.subpartMeshRenderer.bounds);
        }
    }

    /// <summary>
    /// Sets the lookups ready for querying at runtime.
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    public void RefreshLookup()
    {
        dataLookup_name = data.ToLookup(s => s.prt_name);
        dataLookup_displayName = data.ToLookup(s => s.name);
    }

    /// |-----------------------------------------------------------------------------------------|

    #endregion

    /// <summary>
    /// Returns the Avg Original thickness of all the plates that belong to this hullpart.
    /// (NOTE : The sum for the avg is precalculated when the relations are built to avoid unnecssary iteration over subparts)
    /// </summary>
    public float GetAvgOriginalThickness()
    {
        float result = -1f;

        int totalSum = subpartLookup.SelectMany(s => s.Value.connectedEntities).Count();
        totalSum += subpartLookup.Count;

        result = originalThicknessSum / totalSum;

        return result;
    }

    /// <summary>
    /// A selector function to select subparts or any thing specific from the subparts.
    /// </summary>
    /// <typeparam name="TResult">the requested result</typeparam>
    /// <param name="subpartType">subpart type to target</param>
    /// <param name="selector">the selection query</param>
    /// <returns></returns>
    public List<TResult> GetSubparts<TResult>(SubpartType subpartType, Func<Subpart, TResult> selector)
    {
        return data.Where(s => (subpartType & s.type) != 0).ToList().Select(selector).ToList();
    }

    /// <summary>
    /// Runs the given action on all the subparts of this hullpart.
    /// </summary>
    /// <param name="action"></param>
    public void RunOverAllSubparts(Action<Subpart> action)
    {
        foreach (var subpart in data)
        {
            action(subpart);
        }
    }

    /// <summary>
    /// A try getter that returns false if failes to fetch the intended subpart.
    /// </summary>
    /// <param name="subPartName">Subpart name to query for.</param>
    /// <param name="subpart">Result Subpart object reference.</param>
    /// <returns>status of the query (false = Failed / true = Success)</returns>
    public bool TryGetSubpart(string subPartName, out Subpart subpart)
    {
        try
        {
            subpart = dataLookup_name[subPartName].FirstOrDefault();

            if (subpart == null)
            {
                subpart = dataLookup_displayName[subPartName].FirstOrDefault();
            }
            if (subpart == null)
            {
                Debug.LogWarning($"Error with compartment {owningCompartmentUid}, hullpart {name},Subpart {subPartName}");
            }
        }
        catch
        {
            Debug.LogWarning($"Error with compartment {owningCompartmentUid}, hullpart {name},Subpart {subPartName}");
            subpart = null;
            return false;
        }
        return subpart != null;
    }

    /// <summary>
    /// Sets the colliders active or not for the hullpart itself and all the associated subparts.<seealso cref="Subpart.GetCollider()"/>
    /// </summary>
    /// <param name="enabled">enable status</param>
    public void SetActiveAllColliders(bool enabled)
    {
        if (!validHullpartMesh && data.Count > 1)
        {
            foreach (var item in data)
            {
                if (item.GetCollider() == null)
                {
                    continue;
                }
                item.GetCollider().enabled = enabled;
            }
        }
        else
        {
            GetCollider().enabled = enabled;
        }
    }

    /// <summary>
    /// Returns the Collider associated to this hullpart.
    /// </summary>
    public Collider GetCollider()
    {
        if (hullpartMeshReference == null)
        {
            hullpartMeshReference = data[0].subpartObjectMeshReference;
        }

        if (hullpartMeshCollider == null)
        {
            try
            {
                hullpartMeshCollider = hullpartMeshReference.GetComponent<Collider>();
            }
            catch
            {
                Debug.LogError($"Error with compartment {name}");
            }
            if (hullpartMeshCollider == null)
            {
                MeshCollider meshCollider = hullpartMeshReference.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = hullpartMeshReference.GetComponent<MeshFilter>().sharedMesh;
                hullpartMeshCollider = meshCollider;
            }
        }
        return hullpartMeshCollider;
    }

    /// <summary>
    /// Sets all the subparts active that are associated to this hullpart. <seealso cref="Subpart.SetActive(bool)"/>
    /// </summary>
    /// <param name="active">active status</param>
    public void SetActive(bool active, Action<Subpart> excutesOnSubparts = null)
    {
        hullpartMeshReference.gameObject.SetActive(active);
        foreach (var subpart in data)
        {
            subpart.SetActive(active);
            excutesOnSubparts?.Invoke(subpart);
            subpart.SetGaugePointsActive(active);
        }
    }

    /// <summary>
    /// Sets all the subparts active that are associated to this hullpart asynchronously. <seealso cref="Subpart.SetActive(bool)"/>
    /// </summary>
    /// <param name="active">active status</param>
    public void SetActiveAsync(bool active, Action<Subpart> excutesOnSubparts = null)
    {
        hullpartMeshReference.gameObject.SetActive(active);
        foreach (var subpart in data)
        {
            subpart.SetActive(active);
            excutesOnSubparts?.Invoke(subpart);
            subpart.SetGaugePointsActiveAsync(active);
        }
    }

    /// <summary>
    /// Sets all the subparts of a certain type active that are associated to this hullpart. <seealso cref="Subpart.SetActive(bool)"/>
    /// </summary>
    /// <param name="active">active status</param>
    /// <param name="intendedType">targeting subpart type</param>
    public void SetActiveByType(bool active, SubpartType intendedType)
    {
        hullpartMeshReference.gameObject.SetActive(active);
        foreach (var subpart in data)
        {
            if (subpart.type == intendedType)
            {
                subpart.SetActive(active);
                subpart.SetGaugePointsActive(active);
            }
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

        List<MetadataKeyValuePair> metadata = new List<MetadataKeyValuePair>()
        {
            new MetadataKeyValuePair("NAME", name),
            new MetadataKeyValuePair("TYPE", hullpartType),
            //new MetadataKeyValuePair("ASSOCIATED_COMPARTMENTS", compartments),
        };

        Metadata metaDataObject = new Metadata()
        {
            metaData = metadata,
        };

        return JsonUtility.ToJson(metaDataObject, true);
    }
}
