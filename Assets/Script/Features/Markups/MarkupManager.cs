using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MarkupManager_DataClass : DataClass
{
    public BaseMarkup circleMarkup;
    public BaseMarkup sphereMarkup;
    public BaseMarkup cubeMarkup;
    public BaseMarkup squareMarkup;
    public BaseMarkup polygonMarkup;
    public Expectation subpartExpectation;
}


[System.Serializable]
public class AnomalySet
{
    public List<Markups> anomalySet = new List<Markups>();
}

[System.Serializable]
public class Markups
{
    public string AnomalyCode;
    public List<MarkupData> markups = new List<MarkupData>();
}

[System.Serializable]
public struct VesselLookUpdata
{
    public List<CompartmentLookUpdata> compartments;
}

[System.Serializable]
public struct CompartmentLookUpdata
{
    public string uid;
    public List<HullpartLookUpdata> hullparts;
}

[System.Serializable]
public struct HullpartLookUpdata
{
    public string hullpart;
    public List<string> subparts;
}

[System.Serializable]
public struct MarkupData
{
    public string AnomalyCode; // Unique identifier for the markup
    public string markUpName;
    public StringyVector3 location;
    public Quaternion rotation;
    public string color;
    public string markupType;
    public List<float> dimensions;
    public float area;
    public string image;
    public VesselLookUpdata assocationHierarchy;

    public void Reset()
    {
       
        location = new StringyVector3();
        rotation = Quaternion.identity;
        //color = "#FF0000"; // Default to red
        //markupType = "square"; // Default type
        dimensions.Clear();
    }
}

public class MarkupManager : BaseService<MarkupManager_DataClass>
{
    public string GetAllMarkupDataAsJson()
    {
        Markups markupsData = new Markups();

        if (markups.Count > 0)
        {
            foreach (var markup in markupsLookup.GetAllValues())
            {
                markup.CalculateArea();
                //markup.BuildAssociations();
                markupsData.markups.Add(markup.data);
            }
        }

        return JsonUtility.ToJson(markupsData, true);
    }

    public int GetMarkupCount() => markupsLookup.Count;

    public int trackId = 1;
    public void RegisterMarkup(BaseMarkup markupObject, bool sendOver = true)
    {
        if (!markupsLookup.ContainsValue(markupObject))
        {
            markupsLookup.Add(trackId.ToString(), markupObject);
            trackId++;
            Debug.Log(trackId.ToString());
        }

        if (sendOver)
        {
            SendMarkup(markupObject);
        }
    }

    public void SendMarkup(BaseMarkup markup)
    {
        markup.BuildAssociations();

        string jsonData = markup.GetJsonData();

#if UNITY_EDITOR
        Debug.Log($"Area : {markup.data.area}");
        Debug.Log(jsonData);
#endif
        CommunicationManager.Instance.HandleCreateAnomaly(jsonData);
    }

    private BiDictionary<string, BaseMarkup> markupsLookup = new BiDictionary<string, BaseMarkup>();

    internal string currentShapeType;

    public bool areAnomaliesEnabled = false;

    private Dictionary<string, BaseMarkup> markups => new Dictionary<string, BaseMarkup>()
    {
        { "square", data.squareMarkup },
        { "rectangle", data.squareMarkup },
        { "cuboid", data.cubeMarkup },
        { "cube", data.cubeMarkup },
        { "sphere", data.sphereMarkup },
        { "circle", data.circleMarkup },
        { "polygon", data.polygonMarkup }
    };

    public override void Kill() { }

    public override void Update() { }

    public void PingMarkup(string id)
    {
        if (markupsLookup.TryGetValue(id, out BaseMarkup markup))
        {
            if (markup != null)
            {
                markup.PingMarkup();
            }
        }
    }

    public void SpawnAllAnomalySets(AnomalySet anomalySet)
    {
        foreach (var item in anomalySet.anomalySet)
        {
            SpawnAllMarkups(item);
        }
    }

    public void SpawnAllMarkups(Markups markupsData)
    {
        foreach (MarkupData markup in markupsData.markups)
        {
            markupsLookup.Add($"{markupsData.AnomalyCode}/{markup.AnomalyCode}", SpawnOrUpdateMarkUp(markup, null, markupsData.AnomalyCode));
        }
    }

    public BaseMarkup SpawnOrUpdateMarkUp(MarkupData markupData, GameObject markUp = null, string anomalyCode = null)
    {
        BaseMarkup markupComponent = null;

        if (markUp == null)
        {
            markUp = GameObject.Instantiate(markups[markupData.markupType].gameObject);
            markUp.transform.SetParent(MarkupCreator.Instance.transform);
        }

        if (markUp != null && ColorUtility.TryParseHtmlString(markupData.color, out Color pureColor))
        {
            if (markupData.markupType != "polygon")
            {
                markUp.transform.position = markupData.location.GetVector();
                markUp.transform.rotation = markupData.rotation;
            }
            markupComponent = markUp.GetComponent<BaseMarkup>();
            markupComponent.anomalyCode = anomalyCode;
            markupComponent.UpdateData(markupData);
            markupComponent.markupColor = pureColor;
            markupComponent.Validate();
            markupComponent.Initalize();

            if(ApplicationStateMachine.Instance.TryGetCurrentState(out CompartmentViewState compartmentViewState))
            {
                markupComponent.OnCompatmentIsolated(compartmentViewState.GetTargettedCompartemntObject());
            }
            else if (ApplicationStateMachine.Instance.TryGetCurrentState(out HullpartViewState hullpartViewState))
            {
                markupComponent.OnHullpartIsolated(hullpartViewState.GetTargettedHullpartObject());
            }
        }

        return markupComponent;
    }

    public void SpawnMarkup(MarkupData markupData)
    {
        BaseMarkup markup = SpawnOrUpdateMarkUp(markupData);
        if (markup != null)
        {
            markupsLookup.Add(markupData.AnomalyCode, markup);
        }
    }

    public void ToggleMarkupVisibility(bool isVisible)
    {
        areAnomaliesEnabled = isVisible;

        foreach (var item in markupsLookup.GetAllValues())
        {
            if (item != null)
            {
                if (!isVisible)
                {
                    item.gameObject.SetActive(false);
                }
                else
                {
                    if (ApplicationStateMachine.Instance.TryGetCurrentState(out VesselViewState vesselVS))
                    {
                        item.gameObject.SetActive(true);
                    }
                    else if (ApplicationStateMachine.Instance.TryGetCurrentState(out CompartmentViewState compartmentVS))
                    {
                        Compartment c = GroupingManager.Instance.vesselObject.GetCompartment(compartmentVS.GetTargettedCompartemnt());
                        item.GetComponent<BaseMarkup>().OnCompatmentIsolated(c);
                    }
                    else if (ApplicationStateMachine.Instance.TryGetCurrentState(out HullpartViewState hullpartVS))
                    {
                        Hullpart h = GroupingManager.Instance.vesselObject.GetHullpart(hullpartVS.compartmentUid, hullpartVS.hullpartName);
                        item.GetComponent<BaseMarkup>().OnHullpartIsolated(h);
                    }
                }
            }
        }
    }

    public void RemoveAllMarkups()
    {
        ServiceRunner.GetService<MarkupManager>().trackId = 1;
        foreach (var item in markupsLookup.GetAllKeys())
        {
            RemoveMarkup(item);
        }
        ContextMenuManager.Instance.SetInformationObject(false);
    }

    public void RemoveMarkup(string markupId)
    {
        if (markupsLookup.TryGetValue(markupId, out BaseMarkup markup))
        {
            markupsLookup.RemoveByKey(markupId);
            GameObject.Destroy(markup.gameObject);
        }
    }

    public void RemoveMarkup(BaseMarkup markup)
    {
        if (markup != null && markupsLookup.ContainsValue(markup))
        {
            markupsLookup.RemoveByValue(markup);
        }
    }
}
