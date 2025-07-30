using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RootVessel
{
    public Vesseldata vessel;
}
[System.Serializable]
public class Vesseldata
{
    public List<CompartmentData> compartments;
}
[System.Serializable]
public class CompartmentData
{
    public string compartmentUID;
    public List<Documents> documents;
}
[System.Serializable]
public class Documents
{
    public string documentId;
    public string hullPartName;
    public string plateName;
    public string documentName;
    public Files file;
    public string description;
    public string doucmentType;
    public string documentCategory;
    public AttachmentPoint attachmentPoint;
}
[System.Serializable]
public class Files
{
    public string fileId;
    public string fileName;
    public string fileType;
    public double fileSizeInBytes;
}
[System.Serializable]
public class IconData
{
    public string imoNumber;
    public string documentId;
    public string compartmentUID;
    public string compartmentName;
    public string hullPartName;
    public string plateName;
    public string documentName;
    public Files file;
    public string description;
    public string doucmentType;
    public string documentCategory;
    public AttachmentPoint attachmentPoint;
}
[System.Serializable]
public class AttachmentPoint
{
    public string point_Id;
    public StringyVector3 location;
    public StringyVector3 normal;
}


public class IconSpawningManager : SingletonMono<IconSpawningManager>
{
    public IconIndicator IconRepresentation;
    private RaycastHit selectedHitPoint;

    private Dictionary<GameObject, IconData> IconObjects = new Dictionary<GameObject, IconData>();
    private int trackedId = 0;
    private Dictionary<GameObject, IconData> CurrentIconObject = new Dictionary<GameObject, IconData>();
    public Expectation subpartExpectation;
    public bool IconEnabler = true;
    public override void Update()
    {
        if (!MouseOverChecker.IsMouseOverAUIElement() && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1)) &&
          IsolationManager.Instance.CaptureRayCastHit(out RaycastHit hit2))
        {
            selectedHitPoint = hit2;
        }

        //ContextMenuManager.Instance.HandleInputs();
    }
    public void SpawnIcon()
    {
       // Debug.Log("Icon spawn requested at " + selectedHitPoint);
        if (selectedHitPoint.collider != null && !selectedHitPoint.collider.name.Contains("ImageIcon_Indicator") && !selectedHitPoint.collider.name.Contains("GaugePoint_Indicator"))  // Ensure there's a valid hit point stored
        {
            CaptureIcon(selectedHitPoint);
        }
    }
    public void SetSelectedHitPoint(RaycastHit hit)
    {
        selectedHitPoint = hit;
    }

    public void CaptureIcon(RaycastHit hit)
    {
      //  Debug.Log("Instantiate icon at " + hit.collider.name);
        GameObject indicatorObject = Instantiate(IconRepresentation.gameObject, hit.point, Quaternion.identity);
        indicatorObject.transform.forward = hit.normal;
        indicatorObject.transform.SetParent(transform);

        string plateName = "";
        string frameName = "";
        string CompartmentUID = "";

        MetadataComponent plateMetadataComponent = null;

        switch (ApplicationStateMachine.Instance.currentStateName)
        {
            case nameof(VesselViewState):
                QueryForDataOnPoint(indicatorObject, hit, ApplicationStateMachine.Instance.vesselName, hit.collider.transform.gameObject.name, ref frameName, ref plateName, ref plateMetadataComponent);
                break;
            case nameof(CompartmentViewState):
                if (subpartExpectation.AreExpectationsMet(hit.collider.transform.gameObject))
                {
                    CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                    CompartmentUID = compartmentViewState.GetTargettedCompartemnt();
                    plateMetadataComponent = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(hit.collider.transform.gameObject).metadata;
                    plateName = plateMetadataComponent.GetValue("PRT_NAME");
                    frameName = plateMetadataComponent.GetValue("STRUCTURE");
                }
                else
                {
                    CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                    CompartmentUID = compartmentViewState.GetTargettedCompartemnt();
                    frameName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(hit.collider.transform.gameObject).name;
                    QueryForDataOnPoint(indicatorObject, hit, ApplicationStateMachine.Instance.vesselName, CompartmentUID, frameName, ref plateName, ref plateMetadataComponent);
                }
                //CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                //compartmentName = compartmentViewState.GetTargettedCompartemnt();
                ////frameName = GroupingManager.Instance.GetHullPart(ApplicationStateMachine.Instance.vesselName, compartmentName, hit.collider.transform.parent.gameObject.name).name;
                //frameName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(hit.collider.transform.gameObject).name;
                //QueryForDataOnPoint(indicatorObject, hit, ApplicationStateMachine.Instance.vesselName, compartmentName, frameName, ref plateName, ref plateMetadataComponent);
                break;
            case nameof(HullpartViewState):
                HullpartViewState hullpartViewState = ApplicationStateMachine.Instance.CurrentState as HullpartViewState;
                //plateName = hit.collider.gameObject.name;
                plateMetadataComponent = hit.collider.transform.GetComponent<MetadataComponent>();
                plateName = plateMetadataComponent.GetValue("PRT_NAME");
                CompartmentUID = hullpartViewState.compartmentUid;
                //frameName = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName).GetEntryName(hullpartViewState.hullpartName);
                frameName = hullpartViewState.hullpartName;
                break;
            default:
                break;
        }

        trackedId++;
        IconData pointData = new IconData()
        {
            plateName = plateName,
            hullPartName = frameName,
            compartmentUID = CompartmentUID,
            compartmentName = GroupingManager.Instance.vesselObject.GetCompartmentName(CompartmentUID),
            attachmentPoint = new AttachmentPoint()
            {
                point_Id = trackedId.ToString(),
                location = hit.point.GetStringyVector(),
                normal = hit.normal.GetStringyVector()
            }
            
        };

        CurrentIconObject.Clear();
        indicatorObject.GetComponent<IconIndicator>().Initialize(pointData);
        CurrentIconObject.Add(indicatorObject, pointData);
        //string rawData = GetInJSONFormatCurrentObject();
        //Debug.Log(rawData);     
        string rawDataToSend = JsonUtility.ToJson(pointData, true);
        Debug.Log("rawDataToSend \n"+rawDataToSend);

        indicatorObject.transform.localScale = Vector3.zero;

        CommunicationManager.Instance.HandleAttahcIcon_Extern(rawDataToSend);

        CurrentIconObject.Clear();
        Destroy(indicatorObject);
    }

    public void LoadAndSpawnIcons(string rawData)
    {
        RootVessel deserializedData = JsonUtility.FromJson<RootVessel>(rawData);
        SpawnLoadedIcons(deserializedData);

    }

    private void SpawnLoadedIcons(RootVessel rootVessel)
    {
        var vessel = rootVessel.vessel;

        foreach (var compartmentData in vessel.compartments)
        {
            foreach (var documentData in compartmentData.documents)
            {
                SpawnGaugePoint(documentData.documentName, documentData.documentId, compartmentData.compartmentUID, documentData.hullPartName, documentData.plateName, new IconData()
                {
                    documentName = documentData.documentName,
                    documentId = documentData.documentId,
                    plateName = documentData.plateName,
                    hullPartName = documentData.hullPartName,
                    compartmentUID = compartmentData.compartmentUID,
                    attachmentPoint = documentData.attachmentPoint,
                    file = documentData.file,
                    description = documentData.description,
                    doucmentType = documentData.doucmentType,
                    documentCategory = documentData.documentCategory,
                    compartmentName = GroupingManager.Instance.vesselObject.GetCompartmentName(compartmentData.compartmentUID)
                });
            }
        }
    }

    private void SpawnGaugePoint(string docName, string docId, string compartmentUid, string frameDataName, string plateDataName, IconData iconData)
    {
        GameObject indicatorobject = Instantiate(IconRepresentation.gameObject, iconData.attachmentPoint.location.GetVector(), Quaternion.identity);

        indicatorobject.transform.forward = iconData.attachmentPoint.normal.GetVector();
        Vector3 scale = Vector3.one * 0.5f;
        //scale.z = indicatorThickness;
        //indicatorobject.transform.localScale = scale;
        indicatorobject.transform.SetParent(transform);
     
        indicatorobject.GetComponent<IconIndicator>().Initialize(iconData);
        IconObjects.Add(indicatorobject, new IconData()
        {
            documentName = docName,
            documentId = docId,
            plateName = plateDataName,
            hullPartName = frameDataName,
            compartmentUID = compartmentUid,
            attachmentPoint = iconData.attachmentPoint,
            file = iconData.file,
            compartmentName = GroupingManager.Instance.vesselObject.GetCompartmentName(compartmentUid)
        });

        if (ContextMenuManager.Instance.disabledHullpartsCache.Exists(h => h.name.Equals(frameDataName)))
        {
            indicatorobject.SetActive(false);
        }
        var compartmentViewState = ApplicationStateMachine.Instance.GetCurrentState<CompartmentViewState>();
        if (compartmentViewState != null)
        {
            if (compartmentViewState.GetTargettedCompartemnt() != compartmentUid)
            {
                indicatorobject.SetActive(false);
            }
        }
        else
        {
            var hullpartViewState = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>();
            if (hullpartViewState != null && (hullpartViewState.compartmentUid != compartmentUid || hullpartViewState.hullpartName != frameDataName))
            {
                indicatorobject.SetActive(false);
            }
        }
    }

    public void QueryForDataOnPoint(GameObject indicatorobject, RaycastHit hit, string vesselName, string compartmentName, ref string hullpartName, ref string plateName, ref MetadataComponent plateMetadataComponent)
    {
        List<Collider> frames = GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, h => h.GetCollider());
        List<GameObject> collidingFrames = new List<GameObject>();

        frames.ForEach(p => p.enabled = true);

        collidingFrames = Physics.OverlapSphere(hit.point, 0.1f).Select(c => c.gameObject).ToList();

        frames.ForEach(p => p.enabled = false);

        collidingFrames.Remove(hit.collider.gameObject);
        collidingFrames.Remove(indicatorobject);

        hullpartName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(collidingFrames[0]).name;

        QueryForDataOnPoint(indicatorobject, hit, vesselName, compartmentName, hullpartName, ref plateName, ref plateMetadataComponent);
    }

    public void QueryForDataOnPoint(GameObject indicatorobject, RaycastHit hit, string vesselName, string compartmentName, string hullpartName, ref string plateName, ref MetadataComponent plateMetadataComponent)
    {
        Subpart subpart = null;
        Hullpart hullpart = GroupingManager.Instance.vesselObject.GetHullpart(compartmentName, hullpartName);
        var plateCollection = hullpart.GetSubparts(SubpartType.Plate, s => s);
        if (plateCollection.Count() == 1)
        {
            subpart = plateCollection[0];
        }
        else
        {
            List<Collider> plates = GroupingManager.Instance.vesselObject.GetSubparts(compartmentName, hullpartName, SubpartType.All, s => s.GetCollider());
            List<GameObject> collidingPlates = new List<GameObject>();

            plates.ForEach(p => p.enabled = true);

            collidingPlates = Physics.OverlapSphere(hit.point, 0.1f).Select(c => c.gameObject).ToList();

            plates.ForEach(p => p.enabled = false);

            collidingPlates.Remove(hit.collider.gameObject);
            collidingPlates.Remove(indicatorobject);

            subpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(collidingPlates[0]);
        }

        //plateName = collidingPlates[0].gameObject.name;
        plateMetadataComponent = subpart.metadata;
        plateName = subpart.prt_name;
    }
    //public string GetInJSONFormatCurrentObject()
    //{
    //    IconsData_External iconsData = new IconsData_External();

    //    // Assuming CurrentIconObject has only one item, access the first element directly
    //    var pointData = CurrentIconObject.FirstOrDefault(); // Or CurrentIconObject[0] if it's an array or list

    //    if (pointData.Key != null) // Assuming pointData is a KeyValuePair, check if the key exists
    //    {
    //        var compartmentData = iconsData.compartments.Where(c => c.assetUID == pointData.Value.compartmentUID).FirstOrDefault();
    //        if (compartmentData == null)
    //        {
    //            compartmentData = new IconsData_Compartment_External();
    //            //compartmentData.assetName = pointData.Value.compartmentUID;
    //            compartmentData.assetUID = pointData.Value.compartmentUID;
    //            iconsData.compartments.Add(compartmentData);
    //        }

    //        var frameData = compartmentData.frames.Where(f => f.frameName == pointData.Value.hullpartName).FirstOrDefault();
    //        if (frameData == null)
    //        {
    //            frameData = new IconsData_Frame_External();
    //            frameData.frameName = pointData.Value.hullpartName;
    //            compartmentData.frames.Add(frameData);
    //        }

    //        var plateData = frameData.plates.Where(p => p.plateName == pointData.Value.plateName).FirstOrDefault();
    //        if (plateData == null)
    //        {
    //            plateData = new IconsData_External_Plate_External();
    //            plateData.plateName = pointData.Value.plateName;
    //            frameData.plates.Add(plateData);
    //        }

    //        AttachmentPoint pointData_External = new AttachmentPoint()
    //        {
    //            point_Id = pointData.Value.attachmentPoint.point_Id,
    //            location = pointData.Value.attachmentPoint.location,
    //            normal = pointData.Value.attachmentPoint.normal,
    //        };
    //        plateData.IconPoints.Add(pointData_External);
    //    }

    //    return JsonUtility.ToJson(iconsData, true);
    //}


    public void SetAllIconsActive(bool value)
    {
        var compartmentViewState = ApplicationStateMachine.Instance.GetCurrentState<CompartmentViewState>();
        var hullpartViewState = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>();
        string compartmentUid = "";
        string frameDataName = "";

        foreach (var item in IconObjects)
        {
            compartmentUid = item.Key.GetComponent<IconIndicator>().pointData.compartmentUID;
            frameDataName = item.Key.GetComponent<IconIndicator>().pointData.hullPartName;
            if (compartmentViewState != null)
            {
                bool valu3 = (compartmentViewState.GetTargettedCompartemnt() != compartmentUid);
                item.Key.SetActive(!valu3 && IconEnabler);
            }
            else if (hullpartViewState != null)
            {
                bool value2 = (hullpartViewState.compartmentUid != compartmentUid || hullpartViewState.hullpartName != frameDataName);
                item.Key.SetActive(!value2 && IconEnabler);
            }
            else
            {
                item.Key.SetActive(IconEnabler);
            }
        }
    }

    public void IsolateIcons(string compartmentUID)
    {
        foreach (var item in IconObjects)
        {
            bool value = item.Value.compartmentUID == compartmentUID;
            item.Key.SetActive(value && IconEnabler);
        }
    }

    public void IsolateIcons(string compartmentUID, string frameName)
    {
        foreach (var item in IconObjects)
        {
            bool value = item.Value.hullPartName == frameName && item.Value.compartmentUID == compartmentUID;
            item.Key.SetActive(value && IconEnabler);
        }
    }
    public void SetGaugePointsActive(string compartmentUID, bool value)
    {
        foreach (var item in IconObjects)
        {
            if (item.Value.compartmentUID == compartmentUID)
            {
                item.Key.SetActive(value && IconEnabler);
            }
        }
    }

    public void SetGaugePointsActive(string compartmentUID, string frameName, bool value)
    {
        foreach (var item in IconObjects)
        {
            if (item.Value.hullPartName == frameName && item.Value.compartmentUID == compartmentUID)
            {
                item.Key.SetActive(value && IconEnabler);
            }
        }
    }

    public void RemoveIcon(GameObject obj)
    {
        if (IconObjects.ContainsKey(obj))
        {
            CommunicationManager.Instance.HandleRemoveAttachment(obj.GetComponent<IconIndicator>().pointData.documentId);
            IconObjects.Remove(obj);
            Destroy(obj);
        }
        else
        {
            Debug.LogWarning("Icon object not found in IconObjects dictionary.");
        }
    }

    //............................................Diminution graph Path to React page..................................................

    public void DiminutionGraphPositionHit()
    {
       // Debug.Log("Icon spawn requested at " + selectedHitPoint.collider.name);
        if (selectedHitPoint.collider != null)
        {
            DiminutionGraphPath(selectedHitPoint);
        }
    }
    public void DiminutionGraphPath(RaycastHit hit)
    {
       // Debug.Log("Instantiate icon at " + hit.collider.name);
        GameObject indicatorObject = Instantiate(IconRepresentation.gameObject, hit.point, Quaternion.identity);
        indicatorObject.transform.forward = hit.normal;
        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localScale = Vector3.zero;

        string plateName = "";
        string frameName = "";
        string compartmentuid = "";
        string compartmentName = "";
        string UID = "";
        float data = -1f;

        MetadataComponent plateMetadataComponent = null;

        switch (ApplicationStateMachine.Instance.currentStateName)
        {
            case nameof(VesselViewState):
                UID = GroupingManager.Instance.vesselObject.ProcessCompartmentSelection(hit.collider.gameObject).uid;
                QueryForDataOnPoint(indicatorObject, hit, ApplicationStateMachine.Instance.vesselName, UID, ref frameName, ref plateName, ref plateMetadataComponent);
                CommunicationManager.Instance.Handlediminution_Extern(UID + "/" + compartmentuid);
                break;
            case nameof(CompartmentViewState):
                if (subpartExpectation.AreExpectationsMet(hit.collider.transform.gameObject))
                {
                    CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                    compartmentuid = compartmentViewState.GetTargettedCompartemnt();
                    compartmentName = GroupingManager.Instance.vesselObject.GetCompartment(compartmentuid).name;
                    plateMetadataComponent = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(hit.collider.transform.gameObject).metadata;
                    plateName = plateMetadataComponent.GetValue("PRT_NAME");
                    frameName = plateMetadataComponent.GetValue("STRUCTURE");
                   
                }
                else
                {
                    CompartmentViewState compartmentViewState = ApplicationStateMachine.Instance.CurrentState as CompartmentViewState;
                    compartmentuid = compartmentViewState.GetTargettedCompartemnt();
                    compartmentName = GroupingManager.Instance.vesselObject.GetCompartment(compartmentuid).name;
                    frameName = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(hit.collider.transform.gameObject).name;
                    QueryForDataOnPoint(indicatorObject, hit, ApplicationStateMachine.Instance.vesselName, compartmentuid, frameName, ref plateName, ref plateMetadataComponent);
                }
               

                Hullpart frameItself = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(hit.collider.transform.gameObject);
                data = frameItself.GetAvgOriginalThickness();              
                CommunicationManager.Instance.Handlediminution_Extern(compartmentuid + "/" + compartmentName + "--" + data + "/" + frameName);
                //Debug.Log(compartmentuid + "/" + compartmentName + "--" + data + "/" + frameName);
                break;
            case nameof(HullpartViewState):
                HullpartViewState hullpartViewState = ApplicationStateMachine.Instance.CurrentState as HullpartViewState;
                plateMetadataComponent = hit.collider.GetComponent<MetadataComponent>();
                plateName = plateMetadataComponent.GetValue("PRT_NAME");
                compartmentuid = hullpartViewState.compartmentUid;
                compartmentName = GroupingManager.Instance.vesselObject.GetCompartment(compartmentuid).name;
                frameName = hullpartViewState.hullpartName;
                if (plateMetadataComponent.GetValue("THICKNESS") != null)
                {
                    data = float.Parse(plateMetadataComponent.GetValue("THICKNESS"));
                }
                else if(plateMetadataComponent.GetValue("WEB_THICKNESS") != null)
                {
                    data = float.Parse(plateMetadataComponent.GetValue("WEB_THICKNESS"));
                }
                else if (plateMetadataComponent.GetValue("BUILD_THICKNESS") != null)
                {
                    data = float.Parse(plateMetadataComponent.GetValue("BUILD_THICKNESS"))/1000;
                }
                else
                {
                   // Debug.LogWarning($"Failed to fetch thickness for the part : {compartmentName}/{frameName}/{plateName}");
                }

                CommunicationManager.Instance.Handlediminution_Extern(compartmentuid + "/" + compartmentName + "--" + data + "/" + frameName + "/" + plateName);
               // Debug.Log(compartmentuid + "/" + compartmentName+"--"+data + "/" + frameName + "/" + plateName);
                break;
            default:
                break;
        }

        Destroy(indicatorObject);
    }
}
