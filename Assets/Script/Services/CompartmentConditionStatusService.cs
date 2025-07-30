using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

[System.Serializable]
public class CompartmentConditionStatusService_DataClass : DataClass
{
    public Gradient colorGradiant;
}

public class CompartmentConditionStatusService : BaseService<CompartmentConditionStatusService_DataClass>
{
    private CompartmentsConditionData compartmentsConditionData;
    public string Criteria = "Weighted Score";
    //private 

    public override void Kill() { }

    public override void Update() { }


    Color clr = Color.grey;
    public void ColorizeCompartment(string uId, float value)
    {
        if (GroupingManager.Instance.vesselObject.TryGetCompartment(uId, out Compartment compartmentGameObject))
        {
            MeshRenderer renderer = compartmentGameObject.compartmentMeshObjectReference.GetComponent<MeshRenderer>();

            // Color clr = value < 0 ? Color.grey : data.colorGradiant.Evaluate(Mathf.Clamp(value / 6f, 0f, 1f));

            if(value < 0)
            {
                clr = Color.grey;
            }
            else if (value < 2f && value >0f)
            {
                clr = Color.green; // Set to GREEN if CalculatedColorValue is less than 75%
            }
            else if (value >= 2f && value < 4f)
            {
                clr = Color.yellow; // Set to YELLOW if CalculatedColorValue is between 75% and 100%
            }
            else 
            {
                clr = Color.red; // Set to RED if CalculatedColorValue is greater than 100%
            }

            renderer.material.SetColor("_BaseColor", clr);
            renderer.material.SetColor("baseColorFactor", clr);
            renderer.material.SetColor("_Color", clr);
            renderer.material.SetColor("_EmissionColor", clr * 0.25f);
           
        }
        else
        {
            Debug.LogWarning($"There is no Compartment found with the UID : {uId}");
        }
       
       
    }

    public void HighlightName(string name)
    {
        if (GroupingManager.Instance.vesselObject.TryGetCompartmenObjectByName(name, out Compartment compartmentGameObject))
        {          
            Debug.Log("Found the compartment");
            compartmentGameObject.compartmentMeshObjectReference.GetComponent<OutlineSelectionHandler>()?._OnMouseDown();
        }
        else
        {
          //  Debug.LogWarning($"There is no Compartment found with the UID : {uId}");
        }
    }

    public void HightliteCompartmentUID(string UID)
    {
        if (GroupingManager.Instance.vesselObject.TryGetCompartment(UID, out Compartment compartment))
        {
            Debug.Log("Found the compartment");
            compartment.compartmentMeshObjectReference.GetComponent<OutlineSelectionHandler>()?._OnMouseDown();
        }
        else
        {
            //  Debug.LogWarning($"There is no Compartment found with the UID : {uId}");
        }
    }
 
    public void ColorizeCompartments(CompartmentsConditionData compartmentsConditionData)
    {
        this.compartmentsConditionData = compartmentsConditionData;
        if (!compartmentsConditionData.criteria.IsNullOrEmpty())
        {
            Criteria = compartmentsConditionData.criteria;
        }
        else
        {
            Criteria = "Weighted Score";
        }

        if (compartmentsConditionData.compartmentsData.Count <= 0)
        {
            //GroupingManager.Instance.vesselObject.ResetColorizedCompartments();
            return;
        }

        foreach (var compartment in compartmentsConditionData.compartmentsData)
        {
            ColorizeCompartment(compartment.uId, compartment.value);
        }
    }

    public float GetValue(string uId)
    {
        if(uId == null)
        {
            return -1f;
        }

        foreach (var item in compartmentsConditionData.compartmentsData)
        {
           
            if(item.uId != null)
                if (item.uId == uId)
                {
                    return item.value;
                }            
        }
        return -1f;
    }
    
}

[System.Serializable]
public class CompartmentsConditionData
{
    public string criteria;
    public List<AssetsData> compartmentsData;
}

[System.Serializable]
public class AssetsData
{
    public string uId;
    public float value;
}