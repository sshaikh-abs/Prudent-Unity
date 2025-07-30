using System.Collections.Generic;
using System;
using UnityEngine;
using GLTFast.Custom;
using System.Linq;
using System.Threading.Tasks;

[Serializable]
public class Compartment
{
    /// <summary>
    /// Display Name of this compartment.
    /// </summary>
    public string name;

    /// <summary>
    /// Asset UID of this compartment.
    /// </summary>
    public string uid;

    /// <summary>
    /// Function of this compartment.
    /// </summary>
    public string function;

    /// <summary>
    /// from which frame this compartment starts.
    /// </summary>
    public string frameFrom;

    /// <summary>
    /// to which frame this compartment ends.
    /// </summary>
    public string frameTo;

    /// <summary>
    /// List of all roots that belong to this compartment entity.
    /// </summary>
    [NonSerialized] public List<GameObject> compartmentGameobject = new List<GameObject>();

    /// <summary>
    /// Metadata component of this compartment.
    /// </summary>
    [NonSerialized] public MetadataComponent compartmentMetaData;
    /// <summary>
    /// Mesh object reference of this compartment.
    /// </summary>
    [NonSerialized] public GameObject compartmentMeshObjectReference;

    /// <summary>
    /// data means all the hullparts.
    /// </summary>
    public List<Hullpart> data = new List<Hullpart>();

    /// <summary>
    /// Flag to know if the bracekts and Stiffeners are loaded previously.
    /// </summary>
    public bool HaveBracketsLoaded { get; private set; }

    [NonSerialized] public Dictionary<string, Hullpart> hullpartLookup = new Dictionary<string, Hullpart>();
    [NonSerialized] private Collider compartmentColliderReference;

    #region Builder Related

    public Compartment(string uid, string name, MetadataComponent compartmentMetaData, bool useDefaultLayer = false)
    {
        this.uid = uid;
        this.name = name;
        this.compartmentMetaData = compartmentMetaData;

        if (compartmentMetaData.transform.childCount == 0 && compartmentMetaData.GetComponent<MeshRenderer>())
        {
            compartmentMeshObjectReference = compartmentMetaData.transform.gameObject;
        }
        else
        {
            compartmentMeshObjectReference = compartmentMetaData.transform.GetChild(0).gameObject;
        }

        compartmentMeshObjectReference.gameObject.layer = LayerMask.NameToLayer(useDefaultLayer ? "Default" : "Compartments");
        if (!useDefaultLayer)
        {
            compartmentMeshObjectReference.GetComponent<MeshRenderer>().renderingLayerMask = 1 << 2;
        }
        compartmentMeshObjectReference.GetComponent<OutlineSelectionHandler>().Initialize();
    }

    /// +-----------------------------------------------------------------------------------------+
    /// |                                     Builder Related                                     |
    /// +-----------------------------------------------------------------------------------------+

    /// <summary>
    /// Registers the given hullpart to this compartment. <seealso cref="Hullpart"/>
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    /// <param name="hullPart">hullpart to associate with this compartment.</param>
    public void RegisterHullpart(Hullpart hullPart)
    {
        data.Add(hullPart);

        if (hullPart == null)
        {
            Debug.LogError("Something went wrong");
        }

        string name = hullPart.name;
        if (!hullpartLookup.ContainsKey(name))
        {
            hullpartLookup.Add(name, hullPart);
        }
    }

    #endregion

    /// <summary>
    /// Returns the Avg Original thickness of all the plates that belong to this Compartment. <seealso cref="Hullpart.GetAvgOriginalThickness()"/>
    /// (NOTE : Sums are pre calculated on hullpart level so we are not actully iterating through each and every subpart)
    /// </summary>
    public float GetAvgOriginalThickness()
    {
        float sum = 0f;
        foreach (var item in hullpartLookup.Values)
        {
            sum += item.GetAvgOriginalThickness();
        }
        return sum / hullpartLookup.Count;
    }

    /// <summary>
    /// Sets all the Hullparts active that are associated to this Compartment.
    /// (NOTE : This in turn will call for each subpart as it calls for the hullpart's <seealso cref="Hullpart.SetActive(bool)"/>)
    /// </summary>
    /// <param name="active">active status</param>
    public void SetActive(bool active, bool applyToGaugePoints = true)
    {
        foreach (var item in compartmentGameobject)
        {
            item.SetActive(active);
            if(applyToGaugePoints)
            {
                RunOverAllSubparts(s => s.SetGaugePointsActive(active));
            }
        }
    }

    /// <summary>
    /// Returns the Collider associated to this Compartment.
    /// </summary>
    public Collider GetCollider()
    {
        if (compartmentColliderReference == null)
        {
            compartmentColliderReference = compartmentMeshObjectReference.GetComponent<Collider>();
            if (compartmentColliderReference == null)
            {
                MeshCollider meshCollider = compartmentMeshObjectReference.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = compartmentMeshObjectReference.GetComponent<MeshFilter>().sharedMesh;
                compartmentColliderReference = meshCollider;
            }
        }
        return compartmentColliderReference;
    }


    /// <summary>
    /// Gets the hullpart with the given name from this compartment.
    /// </summary>
    /// <param name="hullPartName"></param>
    /// <returns></returns>
    public Hullpart GetHullpart(string hullPartName)
    {
        var hullpartsLookup = data.ToLookup(h => h.name);
        Hullpart hullpart = hullpartsLookup[hullPartName].FirstOrDefault();
        return hullpart;
    }

    /// <summary>
    /// Runs the given action on all the subparts of this compartment.
    /// </summary>
    /// <param name="action"></param>
    public void RunOverAllSubparts(Action<Subpart> action)
    {
        var subparts = data.SelectMany(h => h.data);
        foreach (var subpart in subparts)
        {
            action(subpart);
        }
    }

    /// <summary>
    /// Runs the given action on all the subparts of this compartment.
    /// </summary>
    /// <param name="action"></param>
    public async void RunOverAllSubpartsAsync(Action<Subpart> action)
    {
        Vessel.stopwatch.Restart();
        Vessel.stopwatch.Start();

        var subparts = data.SelectMany(h => h.data);
        foreach (var subpart in subparts)
        {
            action(subpart);
            if (Vessel.stopwatch.Elapsed.TotalMilliseconds > PointProjector.Instance.millsecondThreshold)
            {
                await Task.Yield();
                Vessel.stopwatch.Restart();
            }
        }
    }

    /// <summary>
    /// Marks the entity that the bracekts and stiffeners have loaded.
    /// </summary>
    public void MarkBracektsLoaded()
    {
        HaveBracketsLoaded = true;
    }

    /// <summary>
    /// Returns the metadata relating to this subpart for Metadata Panel.
    /// </summary>
    /// <returns>Metadata in JSON Format</returns>
    public string GetMetadata()
    {
        if(compartmentMetaData.TryGetValue("ABS_GROUP", out string compartmentType))
        {
            if (compartmentType == "***")
            {
                compartmentType = "other";
            }
        }
        else
        {
            compartmentType = "other";
        }

        if (frameFrom.IsNullOrEmpty() || frameFrom == "--")
        {
            if (compartmentMetaData.TryGetValue("FRAME_FROM", out frameFrom))
            {
                if (frameFrom.IsNullOrEmpty())
                {
                    var frames = ServiceRunner.GetService<FrameLabelFeature>().GetFrameFromAndTo(this);
                    frameTo = frames.Item1;
                    frameFrom = frames.Item2;
                }
            }
        }

        if (frameTo.IsNullOrEmpty() || frameTo == "--")
        {
            if (compartmentMetaData.TryGetValue("FRAME_TO", out frameTo))
            {
                if (frameTo.IsNullOrEmpty())
                {
                    var frames = ServiceRunner.GetService<FrameLabelFeature>().GetFrameFromAndTo(this);
                    frameTo = frames.Item1;
                    frameFrom = frames.Item2;
                }
            }
        }

        frameFrom = ConvertLine(frameFrom);
        frameTo = ConvertLine(frameTo);

        List <MetadataKeyValuePair> metadata = new List<MetadataKeyValuePair>()
        {
            new MetadataKeyValuePair("ASSET_UID", uid),
            new MetadataKeyValuePair("DISPLAY_NAME", name),
            new MetadataKeyValuePair("TYPE", compartmentType),
            new MetadataKeyValuePair("FUNCTION", function),
            new MetadataKeyValuePair("FRAME_FROM", frameFrom),
            new MetadataKeyValuePair("FRAME_TO", frameTo)
        };

        Metadata metaDataObject = new Metadata()
        {
            metaData = metadata,
        };

        return JsonUtility.ToJson(metaDataObject, true);
    }

    //public static string ConvertLine(string input)
    //{
    //    // Normalize start for comparison
    //    string trimmedInput = input.TrimStart();

    //    if (trimmedInput.StartsWith("FR ") || trimmedInput.StartsWith("Fr "))
    //    {
    //        // Match "FR " or "Fr ", followed by a number
    //        var match = System.Text.RegularExpressions.Regex.Match(trimmedInput, @"^FR\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    //        if (match.Success)
    //        {
    //            return "F " + match.Groups[1].Value;
    //        }
    //    }

    //    // If it doesn't match, return input as-is
    //    return input;
    //}
    public static string ConvertLine(string input)
    {
        string trimmedInput = input.TrimStart();

        // Case 1: "FR 63" or "Fr 63"
        var match = System.Text.RegularExpressions.Regex.Match(trimmedInput, @"^FR\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return "F " + match.Groups[1].Value;
        }

        // Case 2: "TRANS FR63" or "TRANS FR E"
        match = System.Text.RegularExpressions.Regex.Match(trimmedInput, @"^TRANS\s+FR\s*(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return "F " + match.Groups[1].Value;
        }

        // Case 3: "TRANS BHD81" or "SWASH BHD64"
        match = System.Text.RegularExpressions.Regex.Match(trimmedInput, @"^(TRANS|SWASH)\s+BHD\s*(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return "F " + match.Groups[2].Value;
        }

        // If no pattern matched, return original input
        return input;
    }

    private int gaugepoint_TrackedId_Plates = 0;
    private int gaugepoint_TrackedId_Brackets = 0;
    private int gaugepoint_TrackedId_Stiffeners = 0;

    internal int RequestGaugePointId(SubpartType type)
    {
        if (Subpart.gaugePointUniqueness == GaugePointUniqueness.HullpartLevel_Seperate)
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
        data.ForEach(h => h.ResetTrackingId());
    }

    public void MarkHighestId(int highestId_Plate, int highestId_Bracket, int highestId_Stiffener)
    {
        gaugepoint_TrackedId_Plates = highestId_Plate;
        gaugepoint_TrackedId_Brackets = highestId_Bracket;
        gaugepoint_TrackedId_Stiffeners = highestId_Stiffener;
    }
}
