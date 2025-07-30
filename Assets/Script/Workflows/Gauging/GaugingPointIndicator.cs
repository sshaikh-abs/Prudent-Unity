using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GaugingPointIndicator : MonoBehaviour
{
    //public Material overDrawMaterial;
    public GaugePointData pointData;
    public TextMeshPro text;
    public Transform indicatorMesh;
    public bool isMeasured = false;
    public Color defaultColor;
    private Vector3 initScale;
    public float scaleThreshold = 0.2f;

    private Transform camerTransfrom;

    public void Start()
    {
        camerTransfrom = Camera.main.transform;
        initScale = transform.localScale;

        GameEvents.SetGaugePointLabelActive += SetLabelActive;
        GameEvents.SetGaugePointViewActive += SetPointViewActive;
    }

    public void OnMouseEnter()
    {
        //transform.localScale = initScale + Vector3.one * 0.25f;

        string measuredThickness = "--";

        if (pointData.measuredThickness != -1)
        {
            measuredThickness = pointData.measuredThickness.ToString();
        }
        //string information = text.text + " " + measuredThickness + "/" + pointData.originalThickness;
        //string information = $"<color=red>{text.text}</color> <color=blue>{measuredThickness}</color>/<color=green>{pointData.originalThickness}</color>";
        GaugingManager.Instance.OnHover(this, text.text, measuredThickness, pointData.originalThickness.ToString(), defaultColor);
        //GaugingManager.Instance.SetInfromationText(information);
    }
    public void OnMouseExit()
    {
        //transform.localScale = initScale;
        GaugingManager.Instance.OnExit(this);
    }

    private void Update()
    {
        float fovMultiplier = (2f - (CameraInputController.Instance.scrollSlider.normalizedValue * 2f));
        float distanceMultiplierSquared = (camerTransfrom.position - transform.position).magnitude / (60f);

        transform.localScale = initScale * distanceMultiplierSquared * fovMultiplier;

        if(transform.localScale.x > scaleThreshold)
        {
            transform.localScale = Vector3.one * scaleThreshold;
        }
    }

    private void OnDestroy()
    {
        GameEvents.SetGaugePointLabelActive -= SetLabelActive;
        GameEvents.SetGaugePointViewActive -= SetPointViewActive;

        targetSubpart.gaugePoints.RemoveByValue(this);
    }

    private Subpart targetSubpart;

    public void UpdateThickness(float mesuredThickness)
    {
        float CalculatedColorValue = CorrosionStatusService.GetCorrosionValue(pointData.originalThickness, mesuredThickness);

        // Declare the color to be assigned later
        Color clr;
        isMeasured = true;
        if (CalculatedColorValue < 75f)
        {
            clr = Color.green; // Set to GREEN if CalculatedColorValue is less than 75%
        }
        else if (CalculatedColorValue >= 75f && CalculatedColorValue <= 100f)
        {
            clr = Color.yellow; // Set to YELLOW if CalculatedColorValue is between 75% and 100%
        }
        else
        {
            clr = Color.red; // Set to RED if CalculatedColorValue is greater than 100%
        }
        Debug.Log("Color : " + CalculatedColorValue);
        ChangeIndicatorColor(clr);
        pointData.measuredThickness = mesuredThickness;
    }

    public void Initialize(GaugePointData pointData, Subpart targetSubpart = null)
    {
        if(this.targetSubpart == null)
        {
            this.targetSubpart = targetSubpart;
        }
        this.pointData = pointData;
        ChangeTextColor(Color.white);
        if (pointData.measuredThickness > 0f)
        {
            UpdateThickness(pointData.measuredThickness);
        }
        else
        {
            //new Color(0.2653199f, 0.03404237f, 0.4245283f, 1)
            ChangeIndicatorColor(defaultColor);
        }

        GaugingPointIndicator.indicatorCollection.Add(this);
    }

    public void LookAtCamera(Camera camera)
    {
        indicatorMesh.transform.LookAt(camera.transform.position, camera.transform.up);
    }

    public void ResetLookAt()
    {
        indicatorMesh.transform.forward = pointData.normal;
    }

    public void ChangeIndicatorColor(Color color)
    {
        //indicatorMesh.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(r =>
        //{
        //    r.material.SetColor("_BaseColor", color);
        //});

        indicatorMesh.GetComponentsInChildren<MeshRenderer>().ToList().ForEach(r =>
        {
            if (r.name.Contains("BG"))
                r.material.SetColor("_BaseColor", color);
        });

        defaultColor = color;
    }

    public void SetLabelActive(bool active)
    {
        text.gameObject.SetActive(active);
    }

    public void SetPointViewActive(bool active)
    {
        indicatorMesh.gameObject.SetActive(active);
        // indicatorMesh.GetChild(0).gameObject.SetActive(active);
        //  indicatorMesh.GetChild(1).gameObject.SetActive(active);
    }

    public void ChangeIndicatorColor(ColoringData coloringData)
    {
        SubpartType type = GroupingManager.Instance.vesselObject.GetSubpart(pointData.uId, pointData.frame, pointData.plate).type;
        Color color;

        switch (type)
        {
            case SubpartType.Plate:
                color = coloringData.colorForPlates;
                break;

            case SubpartType.Bracket:
                color = coloringData.colorForBrackets;
                break;

            case SubpartType.Stiffener:
                color = coloringData.colorForStiffeners;
                break;

            default:
                color = Color.black;
                break;
        }

        ChangeIndicatorColor(color);
    }

    //public void SetupForOverDraw()
    //{
    //    GetComponentsInChildren<MeshRenderer>().ToList().ForEach(r => r.material = overDrawMaterial);
    //}

    public void ShowOnlyPlateIndiactors()
    {
        SubpartType type = SubpartType.Plate;
        try
        {
            type = GroupingManager.Instance.vesselObject.GetSubpart(pointData.uId, pointData.frame, pointData.plate).type;
        }
        catch
        {
            Debug.LogError($"Error with gauge point at : {pointData.uId}/{pointData.frame}/{pointData.plate}");
        }
        if (type != SubpartType.Plate)
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeTextColor(Color color)
    {
        text.faceColor = color;
    }
    public void ChangeTextColor(ColoringData coloringData)
    {
        SubpartType type = SubpartType.Plate;
        try
        {
            type = GroupingManager.Instance.vesselObject.GetSubpart(pointData.uId, pointData.frame, pointData.plate).type;
        }
        catch
        {
            Debug.LogError($"Error with gauge point at : {pointData.uId}/{pointData.frame}/{pointData.plate}");
        }
        Color color;

        switch (type)
        {
            case SubpartType.Plate:
                color = coloringData.colorForPlates;
                break;

            case SubpartType.Bracket:
                color = coloringData.colorForBrackets;
                break;

            case SubpartType.Stiffener:
                color = coloringData.colorForStiffeners;
                break;

            default:
                color = Color.black;
                break;
        }

        ChangeTextColor(color);
    }

    public static List<GaugingPointIndicator> indicatorCollection = new List<GaugingPointIndicator>();

    public static void ClearIndicators()
    {
        indicatorCollection.Clear();
    }

    public static void ChangeColor(Color color)
    {
        foreach (var item in indicatorCollection)
        {
            item.ChangeTextColor(color);
            if (!item.isMeasured)
            {
                item.ChangeIndicatorColor(color);
            }
        }
    }

    public static void LookAt(Camera camera)
    {
        foreach (var item in indicatorCollection)
        {
            item.LookAtCamera(camera);
        }
    }

    public static void ResetLookAtAll()
    {
        foreach (var item in indicatorCollection)
        {
            item.ResetLookAt();
        }
    }

    public static void ChangeColor(ColoringData coloringData)
    {
        foreach (var item in indicatorCollection)
        {
            item.ChangeTextColor(coloringData);
            if (!item.isMeasured)
            {
                item.ChangeIndicatorColor(coloringData);
            }
        }
    }

    public static void ShowPlateIndiactors()
    {
        foreach (var item in indicatorCollection)
        {
            item.ShowOnlyPlateIndiactors();
        }
    }

    //public static void SetAllGaugePointsForOverDraw()
    //{
    //    foreach (var item in indicatorCollection)
    //    {
    //        item.SetupForOverDraw();
    //    }
    //}
}

public struct ColoringData
{
    public Color colorForPlates;
    public Color colorForBrackets;
    public Color colorForStiffeners;
}
