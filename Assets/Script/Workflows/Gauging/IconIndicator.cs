using System.Linq;
using UnityEngine;

public class IconIndicator : MonoBehaviour
{
    public IconData pointData;
    private float lastClickTime = 0f;  // Time of the last click
    private float doubleClickThreshold = 0.3f;  // Maximum time interval (in seconds) to consider it a double-click
    string rawDataToSend;
    private Vector3 initScale;
    public float scaleThreshold = 0.2f;

    private Transform camerTransfrom;

    void Start()
    {
        camerTransfrom = Camera.main.transform;
        initScale = transform.localScale;      
    }

    public void Initialize(IconData pointData)
    {
        this.pointData = pointData;
       
        
    }
    private void OnMouseDown()
    {
        rawDataToSend = JsonUtility.ToJson(pointData, true);
        float currentTime = Time.time;
       
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            Debug.Log("Double click\n"+ rawDataToSend);
            CommunicationManager.Instance.HandleClickOnAttachIcon_Extern(rawDataToSend);
        }
        else
        {
            CommunicationManager.Instance.ShowDocumentMetadata_Extern(rawDataToSend);
            Debug.Log("Single click\n" + rawDataToSend);
        }


        lastClickTime = currentTime;
    }
    public void OnMouseEnter()
    {
        string name = GroupingManager.Instance.vesselObject.GetCompartment(pointData.compartmentUID).name;

        ContextMenuManager.Instance.SetInformationObject(true);
        ContextMenuManager.Instance.SetInfromationText($"Attachment : {pointData.documentName}\nAssociated Hull Part : {pointData.hullPartName}\nAssociated Compartment : {name}");
    }
    private void OnMouseExit()
    {
        ContextMenuManager.Instance.SetInformationObject(false);
    }

    private void Update()
    {
        float fovMultiplier = (2f - (CameraInputController.Instance.scrollSlider.normalizedValue * 2f));
        float distanceMultiplierSquared = (camerTransfrom.position - transform.position).magnitude / (60f);

        transform.localScale = initScale * distanceMultiplierSquared * fovMultiplier;

        if (transform.localScale.x > scaleThreshold)
        {
            transform.localScale = Vector3.one * scaleThreshold;
        }
    }

}
