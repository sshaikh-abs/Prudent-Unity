using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Vessel
{
    public static bool isHullpartLessVessel = true;
    public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    /// <summary>
    /// Name of the Vessel
    /// </summary>
    public string name;

    /// <summary>
    /// data means all the compartment groups.
    /// </summary>
    public List<CompartmentGroup> data = null;

    [NonSerialized] public Material transparentMaterial;
    [NonSerialized] public Material opaqueMat;

    public List<Renderer> renderersCollection = new List<Renderer>();

    private ILookup<string, Compartment> compartmentsLookup;
    private ILookup<GameObject, Compartment> compartmentsMeshLookup;
    private ILookup<GameObject, Hullpart> hullpartsMeshLookup;
    private ILookup<GameObject, Subpart> subpartsMeshLookup;

    [NonSerialized] private List<SubpartHelper> subpartHelpers = new List<SubpartHelper>();
    [NonSerialized] private List<HullpartHelper> hullPartHelpers = new List<HullpartHelper>();

    private Dictionary<string, CompartmentGroup> compartmentGroupsLookup = new Dictionary<string, CompartmentGroup>();

    #if UNITY_EDITOR
    [NonSerialized] public GameObject selection;
    #endif

    #region Builder Related

    /// +-----------------------------------------------------------------------------------------+
    /// |                                     Builder Related                                     |
    /// +-----------------------------------------------------------------------------------------+

    /// <summary>
    /// Used to Inject the helpers and lookup necessary for the vessel to be query ready.
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    /// <param name="compartmentGroups">The compartment groups lookup.</param>
    /// <param name="subpartHelpers">The subpart master lookup.</param>
    /// <param name="hullPartHelpers">The hullpart master lookup.</param>
    /// <param name="renderersCollection">The master renderer collection.</param>
    public void InjectHelpers(Dictionary<string, CompartmentGroup> compartmentGroups, List<SubpartHelper> subpartHelpers, List<HullpartHelper> hullPartHelpers, List<Renderer> renderersCollection)
    {
        compartmentGroupsLookup = compartmentGroups;
        data = compartmentGroups.Values.ToList();
        RunOverAllHullparts(h =>
        {
            if (h.data.Count == 1)
            {
                hullPartHelpers.Add(new HullpartHelper(h.data[0].subpartObjectMeshReference, h));
            }
            h.RefreshLookup();
        });
        this.subpartHelpers = subpartHelpers;
        this.hullPartHelpers = hullPartHelpers;
        this.renderersCollection = renderersCollection;
        Buildlookups();
    }

    /// <summary>
    /// Builds the lookup tables using the helpers for querying.
    /// (CAUTION: This method should strictly be used when relation building only)
    /// </summary>
    public void Buildlookups()
    {
        compartmentsLookup = compartmentGroupsLookup.SelectMany(g => g.Value.data).ToLookup(c => c.uid);
        data = compartmentGroupsLookup.Select(g => g.Value).ToList();
        compartmentsMeshLookup = compartmentGroupsLookup.SelectMany(g => g.Value.data).ToLookup(c => c.compartmentMeshObjectReference.gameObject);
        hullpartsMeshLookup = hullPartHelpers.Select(h => h.link).ToLookup(h => h.hullpartMeshReference);
        subpartsMeshLookup = subpartHelpers.Select(s => s.link).ToLookup(h => h.subpartObjectMeshReference);
    }

    #endregion

    /// <summary>
    /// Returns the full loaded vessel's built relations in a JSON format.
    /// </summary>
    /// <returns>relation data JSON</returns>
    public string GetJSON()
    {
        return JsonUtility.ToJson(this);
    }

    #region Materials and Coloring

    /// <summary>
    /// Initializes the required material referecnes that would be applied to the relevent meshes.
    /// </summary>
    /// <param name="transparentMaterial">transparent or invisible material.</param>
    /// <param name="opaqueMat">Opaque or the Shaded material.</param>
    public void SetupMaterials(Material transparentMaterial, Material opaqueMat)
    {
        this.transparentMaterial = transparentMaterial;
        this.opaqueMat = opaqueMat;
    }

    /// <summary>
    /// Resets the Colorization of Compartments. (colorization that typically happnes from condition status)
    /// </summary>
    public void ResetColorizedCompartments()
    {
        CompartmentsConditionData compartmentsConditionData = new CompartmentsConditionData();
        compartmentsConditionData.compartmentsData = new List<AssetsData>();
        foreach (var compartmentType in data)
        {
            foreach (var compartment in compartmentType.data)
            {
                compartmentsConditionData.compartmentsData.Add(new AssetsData()
                {
                    uId = compartment.uid,
                    value = -1
                });
            }
        }
        ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartments(compartmentsConditionData);
    } 

    #endregion

    #region Selectors

    /// <summary>
    /// A selector function to select compartments or any thing specific from the compartments.
    /// </summary>
    /// <typeparam name="TResult">the requested result</typeparam>
    /// <param name="selector">the selection query</param>
    /// <returns></returns>
    public List<TResult> GetCompartments<TResult>(Func<Compartment, TResult> selector)
    {
        return compartmentGroupsLookup.SelectMany(g => g.Value.data).Select(selector).ToList();
    }

    /// <summary>
    /// A selector function to select hullparts or any thing specific from the hullparts.
    /// </summary>
    /// <typeparam name="TResult">the requested result</typeparam>
    /// <param name="compartment">targetting compartment</param>
    /// <param name="selector">the selection query</param>
    /// <returns></returns>
    public List<TResult> GetHullparts<TResult>(string compartment, Func<Hullpart, TResult> selector)
    {
        var results = new List<TResult>();
        Compartment cmp = compartmentsLookup[compartment].FirstOrDefault();
        results = cmp.data.Select(selector).ToList();
        return results;
    }

    /// <summary>
    /// A selector function to select subparts or any thing specific from the subparts. <seealso cref="Hullpart.GetSubparts{TResult}(SubpartType, Func{Subpart, TResult})"/>
    /// </summary>
    /// <typeparam name="TResult">the requested result</typeparam>
    /// <param name="compartment">targetting compartment</param>
    /// <param name="hullPartName">targetting hullpart</param>
    /// <param name="selector">the selection query</param>
    /// <returns></returns>
    public List<TResult> GetSubparts<TResult>(string compartment, string hullPartName, SubpartType subpartType, Func<Subpart, TResult> selector)
    {
        var results = new List<TResult>();
        var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().data.ToLookup(h => h.name);
        Hullpart hullpart = hullpartsLookup[hullPartName].FirstOrDefault();
        results = hullpart.GetSubparts(subpartType , selector);
        return results;
    }

    #endregion

    #region Looping Functions

    /// <summary>
    /// A looping function that excutes the defined action over all hulparts in a given compartment.
    /// </summary>
    /// <param name="compartment">targeting compartment</param>
    /// <param name="action">action to excute on hullparts</param>
    public void RunOverHullparts(string compartment, Action<Hullpart> action)
    {
        var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().data;
        foreach (var hullpart in hullpartsLookup)
        {
            action(hullpart);
        }
    }

    /// <summary>
    /// A looping function that excutes the defined action over all hulparts.
    /// </summary>
    /// <param name="action">action to excute on hullparts</param>
    public void RunOverAllHullparts(Action<Hullpart> action)
    {
        var hullpartsLookup = compartmentGroupsLookup.SelectMany(g => g.Value.data).SelectMany(c => c.data);
        foreach (var hullpart in hullpartsLookup)
        {
            action(hullpart);
        }
    }

    /// <summary>
    /// A looping function that excutes the defined action over all subparts in a given compartment and hullpart.
    /// </summary>
    /// <param name="compartment">targetting compartment</param>
    /// <param name="hullPartName">targetting hullpart</param>
    /// <param name="action">action to excute on subparts</param>
    public void RunOverSubparts(string compartment, string hullPartName, Action<Subpart> action)
    {
        var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().data.ToLookup(h => h.name);
        Hullpart hullpart = hullpartsLookup[hullPartName].FirstOrDefault();
        foreach (var plate in hullpart.data)
        {
            action(plate);
        }
    }

    /// <summary>
    /// Returns all the subparts in the vessel.
    /// </summary>
    /// <returns>List of all subparts</returns>
    public List<Subpart> GetAllSubparts()
    {
        var subparts = compartmentGroupsLookup.SelectMany(g => g.Value.data).SelectMany(c => c.data).SelectMany(h => h.data);
        return subparts.ToList();
    }

    /// <summary>
    /// A looping function that excutes the defined action over all subparts.
    /// </summary>
    /// <param name="action">action to excute on subparts</param>
    public void RunOverAllSubparts(Action<Subpart> action)
    {
        var subparts = compartmentGroupsLookup.SelectMany(g => g.Value.data).SelectMany(c => c.data).SelectMany(h => h.data);
        foreach (var subpart in subparts)
        {
            action(subpart);
        }
    }

    /// <summary>
    /// A looping function that excutes the defined action over all subparts.
    /// </summary>
    /// <param name="action">action to excute on subparts</param>
    public async void RunOverAllSubpartsAsync(Action<Subpart> action)
    {
        var subparts = compartmentGroupsLookup.SelectMany(g => g.Value.data).SelectMany(c => c.data).SelectMany(h => h.data);
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
    /// A looping function that excutes the defined action over all compartments.
    /// </summary>
    /// <param name="action">action to excute on compartments</param>
    public void RunOverCompartments(Action<Compartment> action)
    {
        foreach (var compartmentGroup in compartmentGroupsLookup)
        {
            foreach (var compartment in compartmentGroup.Value.data)
            {
                action(compartment);
            }
        }
    }

    #endregion

    #region Getters

    #region Sub part

    /// <summary>
    /// Being Deprecated, please use TryGetSubpart instead.
    /// </summary>
    /// <param name="compartment"></param>
    /// <param name="hullPartName"></param>
    /// <param name="subPartName"></param>
    /// <returns></returns>
    public Subpart GetSubpart(string compartment, string hullPartName, string subPartName)
    {
        Subpart subpart = null;
        try
        {
            var compartmentsLookup = compartmentGroupsLookup.SelectMany(g => g.Value.data).ToLookup(c => c.uid);
            var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().hullpartLookup;
            Hullpart hullpart = null;
            hullpart = hullpartsLookup[hullPartName];

            if(hullpart.TryGetSubpart(subPartName, out subpart))
            {
                return subpart;
            }
            else
            {
                Debug.LogWarning($"Error with compartment {compartment}, hullpart {hullPartName},Subpart {subPartName}");
            }
        }
        catch
        {
#if UNITY_EDITOR
            Debug.LogError($"Error with compartment {compartment}, hullpart {hullPartName},Subpart {subPartName}");
#endif

        }
        return subpart;
    }

    public bool TryGetSubpart(string compartment, string hullPartName, string subPartName, out Subpart subpart)
    {
        try
        {
            var compartmentsLookup = compartmentGroupsLookup.SelectMany(g => g.Value.data).ToLookup(c => c.uid);
            var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().hullpartLookup;
            Hullpart hullpart = null;
            hullpart = hullpartsLookup[hullPartName];
            return hullpart.TryGetSubpart(subPartName, out subpart);
        }
        catch
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Error with compartment {compartment}, hullpart {hullPartName},Subpart {subPartName}");
#endif
            subpart = null;
            return false;
        }
    }

    #endregion

    #region Hull part

    public Hullpart GetHullpart(string compartment, string hullPartName)
    {
        var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().data.ToLookup(h => h.name);
        Hullpart hullpart = hullpartsLookup[hullPartName].FirstOrDefault();
        return hullpart;
    }

    public bool TryGetHullpart(string compartment, string hullPartName, out Hullpart hullpart)
    {
        try
        {
            var hullpartsLookup = compartmentsLookup[compartment].FirstOrDefault().data.ToLookup(h => h.name);
            hullpart = hullpartsLookup[hullPartName].FirstOrDefault();
        }
        catch
        {
            hullpart = null;
            return false;
        }
        return hullpart != null;
    }

    #endregion

    #region Compartment

    public bool HasCompartment(string uid)
    {
        return compartmentsLookup.Contains(uid);
    }

    public string GetCompartmentName(string uid)
    {
        return compartmentsLookup[uid].FirstOrDefault().name;
    }

    public string GetCompartmentUid(string name)
    {
        return compartmentGroupsLookup.SelectMany(g => g.Value.data).FirstOrDefault(c => c.name == name).uid;
    }

    public bool TryGetCompartmenObjectByName(string name, out Compartment CompartmentObject)
    {
        foreach (var item in compartmentGroupsLookup.SelectMany(g => g.Value.data).Select(c => c).ToList())
        {
            if (item.name == name)
            {
                CompartmentObject = item;
                return true;
            }
        }
        CompartmentObject = null;
        return false;
    }

    public bool TryGetCompartment(string uid, out Compartment compartmentGameObject)
    {
        compartmentGameObject = compartmentsLookup[uid].FirstOrDefault();
        return compartmentGameObject != null;
    }

    public Compartment GetCompartment(string uid)
    {
        Compartment compartmentObject = compartmentsLookup[uid].FirstOrDefault();
        return compartmentObject;
    }

    #endregion

    #endregion

    #region Getters based on gameobject (Selection).

    #if UNITY_EDITOR
    [ContextMenu(nameof(ProcessSelection_Editor))]
    public void ProcessSelection_Editor()
    {
        string compartment = null;
        string subpart = null;

        subpart = selection.transform.GetComponent<MetadataComponent>().GetValue("PRT_NAME");
        var hullpart = subpartsMeshLookup[selection].FirstOrDefault().owningHullpart.hullpartMeshReference;
        compartment = hullpartsMeshLookup[hullpart].FirstOrDefault().owningCompartmentUid;

        var compartmentRef = compartmentsLookup[compartment].FirstOrDefault().compartmentMetaData.gameObject;
        var hullpartRef = subpartsMeshLookup[selection].FirstOrDefault().owningHullpart.hullpartMeshReference;
        var subpartRef = selection.transform;

        Debug.Log($"Path of the Subpart is : {compartment}/{hullpart}/{subpart}");
        Debug.Log($"Compartment Ref", compartmentRef);
        Debug.Log($"Hullpart Ref", hullpartRef);
        Debug.Log($"Subpart Ref", subpartRef);
    }
    #endif

    public Hullpart ProcessHullpartSelection(GameObject gameObject)
    {
        Hullpart hullpart = hullpartsMeshLookup[gameObject].FirstOrDefault();
        if (hullpart == null)
        {
            Subpart subpart = subpartsMeshLookup[gameObject].FirstOrDefault();
            return subpart.owningHullpart;
        }
        return hullpart;
    }

    public Subpart ProcessSubpartSelection(GameObject gameObject)
    {
        Subpart subpart = subpartsMeshLookup[gameObject].FirstOrDefault();
        return subpart;
    }

    public Compartment ProcessCompartmentSelection(GameObject gameObject)
    {
        Compartment compartment = compartmentsMeshLookup[gameObject].FirstOrDefault();
        return compartment;
    } 

    #endregion

#if UNITY_EDITOR

    /// <summary>
    /// Colorizes Compartments with random values.
    /// (CAUTION : This function doesnt go to the build, this is an editor only function to test condition status)
    /// </summary>
    public void ColorizeCompartmentsRandomly()
    {
        string[] options = { "corrosion", "pitting", "fracture" };
        string randomType = options[UnityEngine.Random.Range(0, options.Length)];
        CompartmentsConditionData compartmentsConditionData = new CompartmentsConditionData();
        compartmentsConditionData.compartmentsData = new List<AssetsData>();
        compartmentsConditionData.criteria = randomType;
        foreach (var compartmentType in data)
        {
            foreach (var compartment in compartmentType.data)
            {
                compartmentsConditionData.compartmentsData.Add(new AssetsData()
                {
                    uId = compartment.uid,
                    value = UnityEngine.Random.Range(-6f, 6f)
                });
            }
        }

        ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartments(compartmentsConditionData);
    }
#endif
}
