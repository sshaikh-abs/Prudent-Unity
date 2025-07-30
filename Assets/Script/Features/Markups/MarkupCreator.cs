using EasyButtons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

public class MarkupCreator : SingletonMono<MarkupCreator>
{
    public bool enableMarking = false;
    public bool clickAndDrag = false;

    public MarkupData markupData;

    private bool begunDrawing = false;
    private RaycastHit initialPoint;
    private Vector3 currentPoint = Vector3.zero;
    private BaseMarkup currentTargetMarkup;

    private List<Vector3> verts = new List<Vector3>();
    public float polygonDistanceThreshold = 5f;

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.B))
        {
            enableMarking = !enableMarking;
        }

        if (!enableMarking)
        {
            return;
        }

        if (markupData.markupType == "polygon")
        {
            HandlePolygonShape();
        }
        else
        {
            
            HandlePrimitiveShapes();
        }
    }
    public void SetMarkupType_Extern( string shape)
    {
        if (currentTargetMarkup != null && currentTargetMarkup.data.markupType == "polygon")
        {
            ServiceRunner.GetService<MarkupManager>().SendMarkup(currentTargetMarkup);           
        }
        ResetMarkupData();
       markupData.markupType = shape.ToLower();
       enableMarking = true;
    }

    [Button("Reset Markup Data")]
    public void ResetMarkupData(bool disableMarking = false)
    {
        //if(currentTargetMarkup != null)
        //{
        //    currentTargetMarkup.BuildAssociations();
        //}
      
        markupData.Reset();
        verts.Clear();
        currentTargetMarkup = null;
        begunDrawing = false;
        if(disableMarking)
        {
            enableMarking = false;
        }
    }

    void HandlePolygonShape()
    {
        Ray ray = CameraService.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !MouseOverChecker.IsMouseOverAUIElement())
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                if (verts.Count > 1 && Vector3.Distance(verts[0], hitInfo.point) < polygonDistanceThreshold)
                {
                    ServiceRunner.GetService<MarkupManager>().SendMarkup(currentTargetMarkup);
                    ResetMarkupData();
                    return;
                }

                verts.Add(hitInfo.point);
                markupData.dimensions.Add(hitInfo.point.x);
                markupData.dimensions.Add(hitInfo.point.y);
                markupData.dimensions.Add(hitInfo.point.z);
            }

            if (verts.Count < 2)
            {
                return; // Need at least two points to draw a polygon
            }
            else
            {
                bool isUpdating = currentTargetMarkup != null;

                if (!isUpdating)
                {
                    markupData.AnomalyCode = ServiceRunner.GetService<MarkupManager>().trackId.ToString();
                }

                currentTargetMarkup = ServiceRunner.GetService<MarkupManager>().SpawnOrUpdateMarkUp(markupData, isUpdating ? currentTargetMarkup.gameObject : null);
                ServiceRunner.GetService<MarkupManager>().RegisterMarkup(currentTargetMarkup, false);
            }
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            ServiceRunner.GetService<MarkupManager>().SendMarkup(currentTargetMarkup);
            ResetMarkupData();
        }
    }

    void HandlePrimitiveShapes()
    {
        Ray ray = CameraService.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && !begunDrawing && !MouseOverChecker.IsMouseOverAUIElement())
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                currentTargetMarkup = ServiceRunner.GetService<MarkupManager>().SpawnOrUpdateMarkUp(markupData);
                begunDrawing = true;
                initialPoint = hitInfo;
            }
        }
        else if (!clickAndDrag && Input.GetMouseButtonDown(0) && begunDrawing && !MouseOverChecker.IsMouseOverAUIElement())
        {
            //ServiceRunner.GetService<MarkupManager>().markupObjects.Add(currentTargetMarkup);
            //trackId++;
            ServiceRunner.GetService<MarkupManager>().RegisterMarkup(currentTargetMarkup);

            ResetMarkupData();
            return;
        }

        if (begunDrawing)
        {
            Vector3 up = Vector3.up;

            if ("square,rectangle,circle".Contains(markupData.markupType))
            {
                up = initialPoint.normal;
            }

            var m_Plane = new Plane(up, initialPoint.point);
            if (m_Plane.Raycast(ray, out float enter))
            {
                currentPoint = ray.GetPoint(enter);
            }
        }

        if (clickAndDrag)
        {
            if (Input.GetMouseButtonUp(0))
            {
                ServiceRunner.GetService<MarkupManager>().RegisterMarkup(currentTargetMarkup);

                ResetMarkupData();
            }
        }

        bool useDiagonal = (markupData.markupType == "rectangle");

        if (useDiagonal)
        {
            markupData.location = ((initialPoint.point + currentPoint) / 2f).GetStringyVector();
            var corners = GetRectangleCorners(initialPoint.point, currentPoint, initialPoint.normal);
            markupData.dimensions.Clear();

            float width = Vector3.Distance(corners[0], corners[1]);
            float height = Vector3.Distance(corners[1], corners[2]);

            markupData.dimensions.Add(height); // Heights
            markupData.dimensions.Add(width); // Width

            markupData.rotation = Quaternion.LookRotation((corners[1] - corners[0]).normalized, initialPoint.normal);
            markupData.AnomalyCode = ServiceRunner.GetService<MarkupManager>().trackId.ToString();
        }
        else
        {
            markupData.location = initialPoint.point.GetStringyVector();
            markupData.dimensions.Clear();
            markupData.dimensions.Add(Vector3.Distance(initialPoint.point, currentPoint) * 2f);

            if ("square,circle".Contains(markupData.markupType))
            {
                PatternFetcher.GetOrthonormalBasis(initialPoint.normal, "up", out Vector3 xAxis, out Vector3 yAxis);
                markupData.rotation = Quaternion.LookRotation(xAxis, initialPoint.normal);
            }
            else
            {
                markupData.rotation = Quaternion.identity;
            }
            markupData.AnomalyCode = ServiceRunner.GetService<MarkupManager>().trackId.ToString();
        }

        if (currentTargetMarkup != null)
        {
            ServiceRunner.GetService<MarkupManager>().SpawnOrUpdateMarkUp(markupData, currentTargetMarkup.gameObject);
        }
    }

    Vector3[] GetRectangleCorners(Vector3 initial, Vector3 current, Vector3 planeNormal)
    {
        // Vector from initial to current
        Vector3 diagonal = current - initial;

        // Any vector not parallel to the planeNormal
        Vector3 arbitrary = Vector3.up;
        if (Vector3.Dot(arbitrary, planeNormal) > 0.99f)
            arbitrary = Vector3.right;

        // Compute two perpendicular directions on the plane
        Vector3 dir1 = Vector3.Cross(planeNormal, arbitrary).normalized;
        Vector3 dir2 = Vector3.Cross(planeNormal, dir1).normalized;

        // Project the diagonal onto these directions
        float length1 = Vector3.Dot(diagonal, dir1);
        float length2 = Vector3.Dot(diagonal, dir2);

        // Compute the 4 corners
        Vector3 corner1 = initial;
        Vector3 corner2 = corner1 + dir1 * length1;
        Vector3 corner3 = corner2 + dir2 * length2;
        Vector3 corner4 = corner1 + dir2 * length2;

        return new Vector3[] { corner1, corner2, corner3, corner4 };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var corners = GetRectangleCorners(initialPoint.point, currentPoint, initialPoint.normal);
        if (corners != null && corners.Length == 4)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(corners[3], corners[0]);
        }
        else
        {
            Gizmos.DrawSphere(initialPoint.point, 0.1f);
            Gizmos.DrawSphere(currentPoint, 0.1f);
        }
    }
}
