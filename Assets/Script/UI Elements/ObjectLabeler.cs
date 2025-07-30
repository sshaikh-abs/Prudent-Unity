using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectLabeler : SingletonMono<ObjectLabeler>
{
    public Camera screenshotCamera;  // Assign Screenshot Camera (for Screen Space - Camera mode)
    public Canvas labelCanvas;       // Assign UI Canvas (Screen Space - Camera)
    public GameObject labelPrefab;   // Assign TextMeshPro UI Prefab
    public GameObject platesGaugePointPrefab;   // Assign TextMeshPro UI Prefab
    public GameObject bracketGaugePointPrefab;   // Assign TextMeshPro UI Prefab
    public GameObject stiffenerGaugePointPrefab;   // Assign TextMeshPro UI Prefab
    public Transform labelParent;    // Assign an empty GameObject in the Inspector to hold labels

    private Dictionary<GameObject, TextMeshProUGUI> objectLabelMap = new Dictionary<GameObject, TextMeshProUGUI>();

    public void CreateLabelsForAllObjects(Hullpart targetObject)
    {
        var subparts = targetObject.GetSubparts(SubpartType.Plate, s => s);

        foreach (var subpart in subparts)
        {
            GameObject obj = subpart.subpartObjectMeshReference;

            if (!objectLabelMap.ContainsKey(obj)) // Avoid duplicates
            {
                GameObject labelInstance = Instantiate(labelPrefab, labelParent); // Instantiate under labelParent
                TextMeshProUGUI label = labelInstance.GetComponent<TextMeshProUGUI>();

                if (label != null)
                {
                    string str = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(obj).name;
                    label.text = str;
                    objectLabelMap.Add(obj, label);
                }
            }
        }

        UpdateLabelPositions();
    }

    public void SpawnGaugePointUI(List<KeyValuePair<GameObject, GaugePointData>> points)
    {
        foreach (var point in points)
        {
            if (!objectLabelMap.ContainsKey(point.Key)) // Avoid duplicates
            {
                GameObject template = null;

                switch (GroupingManager.Instance.vesselObject.GetSubpart(point.Value.uId, point.Value.frame, point.Value.plate).type)
                {
                    case SubpartType.Stiffener:
                        template = stiffenerGaugePointPrefab;
                        break;
                    case SubpartType.Bracket:
                        template = bracketGaugePointPrefab;
                        break;
                    case SubpartType.Plate:
                    default:
                        template = platesGaugePointPrefab;
                        break;
                }

                GameObject labelInstance = Instantiate(template, labelParent); // Instantiate under labelParent
                TextMeshProUGUI label = labelInstance.GetComponent<TextMeshProUGUI>();

                if (label != null)
                {
                    string str = point.Value.id.ToString();
                    label.text = str;
                    objectLabelMap.Add(point.Key, label);
                    UpdatePosition(point.Key, label, true);
                }
            }
        }
    }

    public void UpdateLabelPositions()
    {
        foreach (var kvp in objectLabelMap)
        {
            UpdatePosition(kvp.Key, kvp.Value);
        }
    }

    public void UpdatePosition(GameObject obj, TextMeshProUGUI label, bool useRawPos = false)
    {
        if (obj == null)
        {
            Destroy(label.gameObject);
            return;
        }

        Vector3 worldPos = Vector3.zero;

        if(useRawPos)
        {
            worldPos = obj.transform.position;
        }
        else
        {
            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer == null) return;

            worldPos = objRenderer.bounds.center;
        }
        
        Vector3 screenPos = screenshotCamera.WorldToScreenPoint(worldPos);

        if (screenPos.z > 0) // Ensure object is visible
        {
            RectTransform canvasRect = labelCanvas.GetComponent<RectTransform>();

            // Convert screen position to canvas local position
            Vector2 canvasPos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                screenshotCamera,
                out canvasPos))
            {
                label.rectTransform.anchoredPosition = canvasPos;
                label.gameObject.SetActive(true);
            }
        }
        else
        {
            label.gameObject.SetActive(false);
        }
    }

    public void ClearLabels()
    {
        foreach (var kvp in objectLabelMap)
        {
            Destroy(kvp.Value.gameObject);
        }
        objectLabelMap.Clear();
    }
}









































