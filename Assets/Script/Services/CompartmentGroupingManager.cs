using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CompartmentGroupingManager_DataClass : DataClass { }

[System.Serializable]
public class CompartmentsUIDData
{
    public List<CompartmentUIDAssociationObject> compartmentData = new List<CompartmentUIDAssociationObject>();
}

[System.Serializable]
public class CompartmentUIDAssociationObject
{
    public string name;
    public string uId;
    public bool isDuplicatedId;

    public CompartmentUIDAssociationObject(string name, string uId)
    {
        this.name = name;
        this.uId = uId;
    }
}

[System.Serializable]
public class CompartmentTypeClass
{
    public string Type;
    public Dictionary<string, GameObject> compartments;

    public CompartmentTypeClass()
    {
        compartments = new Dictionary<string, GameObject>();
    }

    public void RegisterCompartment(string id, GameObject Object)
    {
        if(compartments.ContainsKey(id))
        {
            Debug.LogWarning($"Compartment with id:{id} already exists in the internal logical book", Object);
            Debug.LogWarning($"here is the duplicate Id Compartment pair", compartments[id]);
            return;
        }

        compartments.Add(id, Object);
    }
}

//public class CompartmentGroupingManager : BaseService<CompartmentGroupingManager_DataClass>
//{
//    public CompartmentsUIDData compartmentData = new CompartmentsUIDData();
//    private List<MetadataComponent> metadataComponents = new List<MetadataComponent>();
//    public Dictionary<string, CompartmentTypeClass> compartmentCatogrization = new Dictionary<string, CompartmentTypeClass>();

//    public List<GameObject> AllCompartments = new List<GameObject>();
//    public bool TryGetCompartmentGameobject(string uId, out GameObject outCompartmentGameObject)
//    {
//        foreach (var item in compartmentCatogrization)
//        {
//            if(item.Value.compartments.ContainsKey(uId))
//            {
//                outCompartmentGameObject = item.Value.compartments[uId];
//                return true;
//            }
//        }
//        outCompartmentGameObject = null;
//        return false;
//    }

//    public bool TryGetCompartmentName(string uId, out string compartmentName)
//    {
//        foreach (var item in compartmentCatogrization)
//        {
//            if (item.Value.compartments.ContainsKey(uId))
//            {
//                compartmentName = item.Value.compartments[uId].name;
//                return true;
//            }
//        }
//        compartmentName = "";
//        return false;
//    }

//    public void InjectObject(GameObject VesselObject)
//    {
//        metadataComponents = VesselObject.GetComponentsInChildren<MetadataComponent>().ToList();
//        CatogarizeData();
//        CompartmentGameObjects();
//    }


//    public bool TryGetCompartmenObjectBytName(string name, out GameObject CompartmentObject)
//    {
//        foreach (var item in AllCompartments)
//        {
//            if (item.name == name)
//            {
//                CompartmentObject = item;
//                return true;
//            }
//        }
//        CompartmentObject = null;
//        return false;
//    }

//    public void CompartmentGameObjects()
//    {
//        foreach (var item in metadataComponents)
//        {
//            AllCompartments.Add(item.gameObject);
//            if (item.gameObject.name.Contains("HULL"))
//            {
//                item.gameObject.SetActive(false);
//            }
//        }
//    }

//    public void CatogarizeData()
//    {
//        foreach (var item in metadataComponents)
//        {
//            string compartmentType = null;
//            string compartmentName = null;
//            string uId = "";

//            compartmentType = item.metaData.metaData.Where(p => p.Key == "COMPARTMENT_TYPE").FirstOrDefault()?.Value;
//            compartmentName = item.metaData.metaData.Where(p => p.Key == "NAME").FirstOrDefault()?.Value;
//            var idItem = item.metaData.metaData.Where(p => p.Key == "ASSET_UID").FirstOrDefault();

//            if (idItem == null || idItem.Value == "0")
//            {
//                //Debug.Log("Compartment has UID 0");
//            }
//            else
//            {
//               // Debug.Log("Compartment is correct");
//            }

//            if (compartmentType == null || compartmentName == null || idItem == null)
//            {
//                //Debug.LogWarning($"item is not a compartment type", item);
//                continue;
//            }

//            uId = idItem.Value;

//            //CompartmentUIDAssociationObject compartmentUIDAssociationObject = new CompartmentUIDAssociationObject(item.gameObject.name, uId);
//            //if(compartmentData.compartmentData.Exists(c => c.uId == uId))
//            //{
//            //    compartmentUIDAssociationObject.isDuplicatedId = true;
//            //}
//            //compartmentData.compartmentData.Add(compartmentUIDAssociationObject);

//            if(!compartmentCatogrization.ContainsKey(compartmentType))
//            {
//                compartmentCatogrization.Add(compartmentType, new CompartmentTypeClass()
//                {
//                    Type = compartmentType
//                });
//            }

//            compartmentCatogrization[compartmentType].RegisterCompartment(uId, item.gameObject);
//        }
//    }

//    public void PrintJSON()
//    {
//        string rawData = JsonUtility.ToJson(compartmentData, true);
//        Debug.Log(rawData);
//    }

//    public void ResetColorizedCompartments()
//    {
//        CompartmentsConditionData compartmentsConditionData = new CompartmentsConditionData();
//        compartmentsConditionData.compartmentsData = new List<AssetsData>();
//        foreach (var compartmentType in compartmentCatogrization)
//        {
//            foreach (var compartment in compartmentType.Value.compartments)
//            {
//                compartmentsConditionData.compartmentsData.Add(new AssetsData()
//                {
//                    uId = compartment.Key,
//                    value = -1
//                });
//                //ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartment(compartment.Key, Random.Range(-6f, 6f));
//            }
//        }
//        ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartments(compartmentsConditionData);


//    }

//    public void ColorizeCompartmentsRandomly()
//    {
//        CompartmentsConditionData compartmentsConditionData = new CompartmentsConditionData();
//        compartmentsConditionData.compartmentsData = new List<AssetsData>();
//        foreach (var compartmentType in compartmentCatogrization)
//        {
//            foreach (var compartment in compartmentType.Value.compartments)
//            {
//                compartmentsConditionData.compartmentsData.Add(new AssetsData()
//                {
//                    uId = compartment.Key,
//                    value = Random.Range(-6f, 6f)
//                });
//                //ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartment(compartment.Key, Random.Range(-6f, 6f));
//            }
//        }
       
//        ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartments(compartmentsConditionData);
//    }

//    public void ResetCompartmentsColorization()
//    {
//        foreach (var compartmentType in compartmentCatogrization)
//        {
//            foreach (var compartment in compartmentType.Value.compartments)
//            {
//                ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartment(compartment.Key, -1f);
//            }
//        }
//    }

//    //public void CheckForHighlite(string nam)
//    //{
//    //    foreach (var compartmentType in compartmentCatogrization)
//    //    {
//    //        foreach (var compartment in compartmentType.Value.compartments)
//    //        {
//    //            //ServiceRunner.GetService<CompartmentConditionStatusService>().ColorizeCompartment(compartment.Key, -1f);

//    //            GameObject LocalCompartment = compartment.Value;

                
//    //                if(LocalCompartment.name == nam)
//    //                {
//    //                    Debug.Log("Found compartment");
//    //                }
//    //                Debug.Log(LocalCompartment.name);
//    //            //}

//    //        }
//    //    }
//    //}

//    public override void Kill()
//    {
//        compartmentCatogrization.Clear();
//    }

//    public override void Update()
//    {
//    }
//}