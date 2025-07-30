using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Analytics;


public class ObjectLabeler3D : SingletonMono<ObjectLabeler3D>
{
    public GameObject label3DPrefab;     // Assign a prefab with TextMeshPro (3D)
    public Camera screenshotCamera;      // Assign the screenshot camera in the inspector
    public GameObject platesGaugePoint_3DPrefab;   // Assign TextMeshPro UI Prefab
    public GameObject bracketGaugePoint_3DPrefab;   // Assign TextMeshPro UI Prefab
    public GameObject stiffenerGaugePoint_3DPrefab;   // Assign TextMeshPro UI Prefab
    [SerializeField][Range(1f, 20f)] private float offsetDistance;
    public Vector2 labelSize = new Vector2(1.5f, 0.15f);

    private Dictionary<GameObject, TextMeshPro> objectLabelMap = new();       // For general labels
    private Dictionary<GameObject, TextMeshPro> gaugePointLabelMap = new();   // For gauge point labels
    public int iterations = 2;
    public float pixelMinDistance = 25f; // Minimum spacing in screen space (pixels)
    public float minDistanceFromAnchor = 5f; // pixels
    public float zValue = 30f;

    public void Create3DLabelsForAllObjects(Hullpart targetObject, float sizeMultiplier, Transform parent = null)
    {
        var subparts = targetObject.GetSubparts(SubpartType.Plate, s => s);

        int index = 0;

        foreach (var subpart in subparts)
        {
            GameObject obj = subpart.subpartObjectMeshReference;

            if (!objectLabelMap.ContainsKey(obj))
            {
                index++;
                GameObject labelInstance = Instantiate(label3DPrefab);

                Vector3 labelSizeCheck = labelSize;
                labelSizeCheck.z = -1f;

                labelInstance.name = $"PLATENAME_{index}";
                if (Mathf.Abs(Vector3.Dot(transform.forward, Vector3.forward)) > 0.1f)
                {
                    labelSizeCheck.x = -1f;
                    labelSizeCheck.y = labelSize.y;
                    labelSizeCheck.z = labelSize.x;
                }

                if(Mathf.Abs(Vector3.Dot(transform.forward, Vector3.right)) > 0.1f && Mathf.Abs(Vector3.Dot(transform.up, Vector3.forward)) > 0.1f)
                {

                    labelSizeCheck.x = labelSize.x;
                    labelSizeCheck.y = -1f;
                    labelSizeCheck.z = labelSize.y;
                }

                Bounds bounds = subpart.subpartMeshRenderer.bounds;

                if(bounds.size.x < labelSizeCheck.x ||
                    bounds.size.y < labelSizeCheck.y ||
                    bounds.size.z < labelSizeCheck.z)
                {
                    labelInstance.name += "_MOVE";
                }

                if (parent != null)
                {
                    labelInstance.transform.SetParent(parent, false);
                }
                TextMeshPro label = labelInstance.GetComponent<TextMeshPro>();
                TextMeshPro label_Child = labelInstance.transform.GetChild(0).GetComponent<TextMeshPro>();

                if (label != null)
                {
                    string str = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(obj).name;
                    string modified = Regex.Replace(str, @"^plate_?", "PL", RegexOptions.IgnoreCase);
                    label_Child.text = modified;
                    label_Child.fontSize *= sizeMultiplier;
                    label.text = modified;
                    label.fontSize *= sizeMultiplier;
                    // Initially place it at origin; well fix positions after loop
                    label.transform.position = Vector3.zero;
                    objectLabelMap.Add(obj, label);
                }
            }
        }

        UpdateLabelPositions(); // Now update positions after all labels are created
    }

    public void UpdateLabelPositions()
    {
        foreach (var kvp in objectLabelMap)
        {
            UpdatePosition(kvp);
        }
    }

    private void UpdatePosition(KeyValuePair<GameObject, TextMeshPro> kvp)
    {
        GameObject obj = kvp.Key;
        TextMeshPro label = kvp.Value;

        if (obj == null || label == null)
        {
            if (label != null)
            {
                Destroy(label.gameObject);
                return;
            }
        }

        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            label.transform.position = rend.bounds.center;
        }
        else
        {
            label.transform.position = obj.transform.position;
        }

        FaceCamera(label.transform);
    }

    //public void SpawnGaugePointUI_3D(List<KeyValuePair<GameObject, GaugePointData>> points)
    //{
    //    foreach (var point in points)
    //    {
    //        if (!objectLabelMap.ContainsKey(point.Key)) // Avoid duplicates
    //        {
    //            GameObject template = null;

    //            // Choose prefab based on subpart type
    //            switch (GroupingManager.Instance.vesselObject.GetSubpart(point.Value.uId, point.Value.frame, point.Value.plate).type)
    //            {
    //                case SubpartType.Stiffner:
    //                    template = stiffenerGaugePoint_3DPrefab;
    //                    break;
    //                case SubpartType.Bracket:
    //                    template = bracketGaugePoint_3DPrefab;
    //                    break;
    //                case SubpartType.Plate:
    //                default:
    //                    template = platesGaugePoint_3DPrefab;
    //                    break;
    //            }

    //            // Determine spawn position
    //            Vector3 spawnPos;
    //            Renderer objRenderer = point.Key.GetComponent<Renderer>();
    //            if (objRenderer != null)
    //                spawnPos = objRenderer.bounds.center;
    //            else
    //                spawnPos = point.Key.transform.position;

    //            // Instantiate prefab at world position
    //            GameObject labelInstance = Instantiate(template, spawnPos, Quaternion.identity); // optional parent

    //            // Set label text (assuming prefab has TextMeshPro, not UGUI)
    //            TextMeshPro label = labelInstance.transform.GetChild(0).GetComponent<TextMeshPro>();
    //            if (label != null)
    //            {
    //                label.text = point.Value.id.ToString();
    //            }

    //            // Add to tracking dictionary
    //            objectLabelMap.Add(point.Key, labelInstance.GetComponent<TextMeshPro>());

    //        }

    //    }

    //    UpdateLabelPositions_3D();
    //}



    public bool SpawnGaugePointUI_3D(List<KeyValuePair<GameObject, GaugePointData>> points, SubpartType filterType, Transform parent = null)
    {
        var filteredPoints = new List<KeyValuePair<GameObject, GaugePointData>>();

        // Filter points by the given SubpartType
        foreach (var point in points)
        {
            var subpart = GroupingManager.Instance.vesselObject.GetSubpart(point.Value.uId, point.Value.frame, point.Value.plate);
            if (subpart.type == filterType)
            {
                filteredPoints.Add(point);
            }
        }

        // Return false if no points of the desired type
        if (filteredPoints.Count == 0)
        {
            Debug.Log($"No points found for {filterType}");
            return false;
        }

        // Choose prefab
        GameObject prefab = platesGaugePoint_3DPrefab; // default
        switch (filterType)
        {
            case SubpartType.Bracket:
                prefab = bracketGaugePoint_3DPrefab;
                break;
            case SubpartType.Stiffener:
                prefab = stiffenerGaugePoint_3DPrefab;
                break;
            case SubpartType.Plate:
            default:
                prefab = platesGaugePoint_3DPrefab;
                break;
        }

        // Spawn filtered points
        foreach (var point in filteredPoints)
        {
            if (!gaugePointLabelMap.ContainsKey(point.Key))
            {
                Vector3 spawnPos;
                Renderer objRenderer = point.Key.GetComponent<Renderer>();
                if (objRenderer != null)
                    spawnPos = objRenderer.bounds.center;
                else
                    spawnPos = point.Key.transform.position;

                GameObject labelInstance = Instantiate(prefab, spawnPos, Quaternion.identity);

                if(parent != null)
                {
                    labelInstance.transform.SetParent(parent, false);
                }

                if (filterType == SubpartType.Bracket)
                {
                    var data = new LabelData
                    {
                        anchor = spawnPos,
                        labelGO = labelInstance.transform.GetChild(0).gameObject,
                        indicator = labelInstance,
                        screenPosition = (Vector2)screenshotCamera.WorldToScreenPoint(labelInstance.transform.GetChild(0).position)
                        //anchor = spawnPos,
                        //labelGO = labelInstance.transform.GetChild(0).gameObject,
                        //position = labelInstance.transform.GetChild(0).position,
                        //anchorScreenDepth = screenshotCamera.WorldToScreenPoint(spawnPos).z,
                        //indicator = labelInstance
                    }; 
                    var data1 = new LabelData
                    {
                        anchor = spawnPos,
                        labelGO = labelInstance.transform.gameObject,
                        indicator = labelInstance,
                        screenPosition = (Vector2)screenshotCamera.WorldToScreenPoint(labelInstance.transform.position),
                        dummy = true
                        //anchor = spawnPos,
                        //labelGO = labelInstance.transform.GetChild(0).gameObject,
                        //position = labelInstance.transform.GetChild(0).position,
                        //anchorScreenDepth = screenshotCamera.WorldToScreenPoint(spawnPos).z,
                        //indicator = labelInstance

                    };
                    labelDataList.Add(data);
                    labelDataList.Add(data1);
                }

                //.. get the lineCreation component from labelInstance and call start function if this is not a null

                TextMeshPro label = labelInstance.transform.GetChild(0).GetComponent<TextMeshPro>();
                if (label != null)
                {
                    label.text = point.Value.id.ToString();
                }

                gaugePointLabelMap.Add(point.Key, labelInstance.GetComponent<TextMeshPro>());

            }
        }

        return true; // Successfully spawned
    }
    class LabelData
    {
        //public Vector3 anchor; 
        //public float anchorScreenDepth;
        //public GameObject indicator;
        //public GameObject labelGO;
        //public Vector3 position;
        public Vector3 anchor;
        public GameObject labelGO;
        public GameObject indicator;
        public Vector2 screenPosition; // <- Add this for 2D tracking
        public bool dummy = false;
    }


    List<LabelData> labelDataList = new List<LabelData>();
    /*
    public void RelaxLabelsScreenSpace(Camera cam)
    {
        for (int it = 0; it < iterations; it++)
        {
            Dictionary<Vector2Int, List<LabelData>> grid = new();
            float cellSize = pixelMinDistance;

            // Step 1: Build spatial hash grid in screen space
            foreach (var label in labelDataList)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(label.position);
                Vector2Int cell = new Vector2Int(
                    Mathf.FloorToInt(screenPos.x / cellSize),
                    Mathf.FloorToInt(screenPos.y / cellSize)
                );

                if (!grid.ContainsKey(cell))
                    grid[cell] = new List<LabelData>();

                grid[cell].Add(label);
            }

            // Step 2: Relax overlapping labels in screen space
            foreach (var label in labelDataList)
            {
                Vector3 screenA = cam.WorldToScreenPoint(label.position);
                Vector2Int baseCell = new Vector2Int(
                    Mathf.FloorToInt(screenA.x / cellSize),
                    Mathf.FloorToInt(screenA.y / cellSize)
                );

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int cell = baseCell + new Vector2Int(dx, dy);
                        if (!grid.TryGetValue(cell, out var others)) continue;

                        foreach (var other in others)
                        {
                            if (label == other) continue;

                            Vector3 screenB = cam.WorldToScreenPoint(other.position);
                            Vector2 offset = (Vector2)(screenA - screenB);
                            float dist = offset.magnitude;

                            if (dist < pixelMinDistance && dist > 0.001f)
                            {
                                // Compute relaxed push with angle and jitter
                                Vector2 pushDir = offset.normalized;
                                Vector2 perp = new Vector2(-pushDir.y, pushDir.x);

                                float angleOffset = (it + 1) * 2.5f;
                                float radians = angleOffset * Mathf.Deg2Rad;
                                Vector2 angledPush = new Vector2(
                                    pushDir.x * Mathf.Cos(radians) - pushDir.y * Mathf.Sin(radians),
                                    pushDir.x * Mathf.Sin(radians) + pushDir.y * Mathf.Cos(radians)
                                );

                                Vector2 push = angledPush * (pixelMinDistance - dist) * 0.5f
                                               + perp * 1.0f;

                                // Apply push in screen space
                                screenA += (Vector3)push;
                                screenB -= (Vector3)push;

                                // Reproject using original anchor Z
                                label.position = cam.ScreenToWorldPoint(new Vector3(screenA.x, screenA.y, label.anchorScreenDepth));
                                other.position = cam.ScreenToWorldPoint(new Vector3(screenB.x, screenB.y, other.anchorScreenDepth));
                            }
                        }
                    }
                }
            }
        }

        // Step 3: Ensure minimum distance from anchor
        foreach (var label in labelDataList)
        {
            Vector3 anchorScreenPos = cam.WorldToScreenPoint(label.anchor);
            Vector3 labelScreenPos = cam.WorldToScreenPoint(label.position);

            Vector2 offset = (Vector2)(labelScreenPos - anchorScreenPos);
            float dist = offset.magnitude;

            if (dist < minDistanceFromAnchor)
            {
                Vector2 push = offset.normalized;
                if (push == Vector2.zero)
                    push = Random.insideUnitCircle.normalized;

                Vector2 newScreenPos = (Vector2)anchorScreenPos + push * minDistanceFromAnchor;
                label.position = cam.ScreenToWorldPoint(new Vector3(
                    newScreenPos.x,
                    newScreenPos.y,
                    label.anchorScreenDepth
                ));
            }

            label.labelGO.transform.position = label.position;
            FaceCamera(label.labelGO.transform);
            label.labelGO.transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);

            var line = label.indicator.GetComponent<LineCreation>();
            if (line != null)
                line.UpdateLine();
        }

        labelDataList.Clear();
    }
    */
    public void RelaxLabelsScreenSpace2D(Camera cam)
    {
        //float screenZ = 10f; // You can choose a fixed Z value when projecting back (e.g. 10 units from camera)

        // Step 1: Initialize screen positions for all labels from their world anchor
        foreach (var label in labelDataList)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(label.anchor);
            label.screenPosition = new Vector2(screenPos.x, screenPos.y); // New property in LabelData
        }

        // Step 2: Iterative screen-space relaxation
        for (int it = 0; it < iterations; it++)
        {
            Dictionary<Vector2Int, List<LabelData>> grid = new();
            float cellSize = pixelMinDistance;

            // Build spatial hash grid
            foreach (var label in labelDataList)
            {
                Vector2Int cell = new Vector2Int(
                    Mathf.FloorToInt(label.screenPosition.x / cellSize),
                    Mathf.FloorToInt(label.screenPosition.y / cellSize)
                );

                if (!grid.ContainsKey(cell))
                    grid[cell] = new List<LabelData>();

                grid[cell].Add(label);
            }

            // Apply separation forces
            foreach (var label in labelDataList)
            {
                Vector2Int baseCell = new Vector2Int(
                    Mathf.FloorToInt(label.screenPosition.x / cellSize),
                    Mathf.FloorToInt(label.screenPosition.y / cellSize)
                );

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int neighborCell = baseCell + new Vector2Int(dx, dy);
                        if (!grid.TryGetValue(neighborCell, out var neighbors)) continue;

                        foreach (var other in neighbors)
                        {
                            if (label == other) continue;

                            Vector2 offset = label.screenPosition - other.screenPosition;
                            float dist = offset.magnitude;

                            if (dist < pixelMinDistance && dist > 0.001f)
                            {
                                Vector2 pushDir = offset.normalized;

                                // Perpendicular jitter to break symmetry
                                Vector2 perp = new Vector2(-pushDir.y, pushDir.x);
                                float angle = (it + 1) * 2.5f * Mathf.Deg2Rad;
                                Vector2 angled = new Vector2(
                                    pushDir.x * Mathf.Cos(angle) - pushDir.y * Mathf.Sin(angle),
                                    pushDir.x * Mathf.Sin(angle) + pushDir.y * Mathf.Cos(angle)
                                );

                                Vector2 push = angled * (pixelMinDistance - dist) * 0.5f + perp * 1.0f;

                                label.screenPosition += push;
                                other.screenPosition -= push;
                            }
                        }
                    }
                }
            }
        }

        // Step 3: Final adjustment to ensure distance from anchor
        foreach (var label in labelDataList)
        {
            if (label.dummy)
            {
                continue;
            }

            Vector3 anchorScreen = cam.WorldToScreenPoint(label.anchor);
            Vector2 anchor2D = new Vector2(anchorScreen.x, anchorScreen.y);

            Vector2 offset = label.screenPosition - anchor2D;
            float dist = offset.magnitude;

            if (dist < minDistanceFromAnchor)
            {
                Vector2 dir = offset == Vector2.zero ? Random.insideUnitCircle.normalized : offset.normalized;
                label.screenPosition = anchor2D + dir * minDistanceFromAnchor;
            }

            // Project relaxed screen position back to world space at fixed depth
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(label.screenPosition.x, label.screenPosition.y, zValue));
            //worldPos.z = label.labelGO.transform.position.z;
            label.labelGO.transform.position = worldPos;

            //FaceCamera(label.labelGO.transform);
            label.labelGO.transform.rotation = Quaternion.LookRotation(cam.transform.forward, Vector3.up);

            var line = label.indicator.GetComponent<LineCreation>();
            if (line != null)
                line.UpdateLine();
        }

        labelDataList.Clear();
    }
    public void UpdateLabelPositions_3D()
    {
        foreach (var kvp in gaugePointLabelMap)
        {
            UpdatePosition(kvp);
        }
    }

    public void ClearLabels()
    {
        foreach (var kvp in objectLabelMap)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        objectLabelMap.Clear();
    }

    public void ClearGeneralLabels()
    {
        foreach (var kvp in objectLabelMap)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        objectLabelMap.Clear();
    }

    public void ClearGaugePointLabels()
    {
        foreach (var kvp in gaugePointLabelMap)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        gaugePointLabelMap.Clear();
    }



    private void FaceCamera(Transform labelTransform)
    {
        Vector3 toCamera = -screenshotCamera.transform.forward;
        labelTransform.position += toCamera * offsetDistance;

        Vector3 forwardDirection;
        if (screenshotCamera.orthographic)
        {
            forwardDirection = -screenshotCamera.transform.forward;
        }
        else
        {
            forwardDirection = (screenshotCamera.transform.position - labelTransform.position).normalized;
        }

        // Use camera's up vector instead of Vector3.up to support arbitrary angles like top view
        Vector3 upDirection = screenshotCamera.transform.up;

        labelTransform.rotation = Quaternion.LookRotation(forwardDirection, upDirection);
        labelTransform.Rotate(0f, 180f, 0f);
    }

}
