using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MeshExtension;

public class PointPuncher : MonoBehaviour
{
    public float Radius = 0.05f;
    public bool drawExactProjections = false;
    public Vector3 offset;

#if UNITY_EDITOR
    public Renderer selectedTransfrom => GetSelected();

    public Renderer GetSelected()
    {
        if(UnityEditor.Selection.activeGameObject == null)
        {
            return null;
        }

        if (UnityEditor.Selection.activeGameObject.GetComponent<Renderer>() == null)
        {
            return null;
        }

        return UnityEditor.Selection.activeGameObject.GetComponent<Renderer>();
    }
#endif

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var selection = GetSelected();
        if (selection == null)
        {
            return;
        }

        var targetMesh = selection.GetComponent<MeshFilter>().sharedMesh;
        if (targetMesh == null)
        {
            return;
        }

        var patternCreator = GetComponent<PatternFetcher>();

        var DrawCorners = targetMesh.Get4CornerVerticesWithOBBAndCenter(selection.transform, offset);
        DrawCornors(DrawCorners.corners);

        GetRectangleFrame(DrawCorners.corners.Select(c => c.exactCorner).ToArray(), out float width, out float height, out Vector3 center, out Vector3 xAxis, out Vector3 yAxis);
        
        transform.position = DrawCorners.center;
        patternCreator.dimensions = new Vector2(width, height);
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(yAxis, xAxis), yAxis);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(DrawCorners.center, Radius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(DrawCorners.center, DrawCorners.center + xAxis * 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(DrawCorners.center, DrawCorners.center + yAxis * 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(DrawCorners.center, DrawCorners.center + Vector3.Cross(xAxis, yAxis) * 0.5f);
    }

    int index = 0;

    public void DrawCornors(OBBResult[] corners)
    {
        index = 0;
        foreach (var item in corners)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(item.nearestMeshCorner, Radius);

            if (drawExactProjections)
            {
                UnityEditor.Handles.Label(item.exactCorner + (Vector3.one * 0.75f), $"{index}");
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(item.exactCorner, Radius);
                index++;
            }
        }
    }

    public void DrawBoundsAndSize(Renderer selection)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(selection.bounds.center, selection.bounds.size);

        Gizmos.color = Color.blue;

        Vector3 pointZ_A = selection.bounds.center + new Vector3(0, 0, -selection.bounds.extents.z);
        Vector3 pointZ_B = selection.bounds.center + new Vector3(0, 0, selection.bounds.extents.z);

        ProjectionRayData rayZ = new ProjectionRayData(pointZ_A, pointZ_B);
        Gizmos.DrawLine(pointZ_A, pointZ_B);

        Vector3 pointX_A = selection.bounds.center + new Vector3(-selection.bounds.extents.x, 0, 0);
        Vector3 pointX_B = selection.bounds.center + new Vector3(selection.bounds.extents.x, 0, 0);

        ProjectionRayData rayX = new ProjectionRayData(pointX_A, pointX_B);
        Gizmos.DrawLine(pointX_A, pointX_B);

        Vector3 pointY_A = selection.bounds.center + new Vector3(0, -selection.bounds.extents.y, 0);
        Vector3 pointY_B = selection.bounds.center + new Vector3(0, selection.bounds.extents.y, 0);

        ProjectionRayData rayY = new ProjectionRayData(pointY_A, pointY_B);
        Gizmos.DrawLine(pointY_A, pointY_B);

        List<ProjectionRayData> rays = new List<ProjectionRayData> { rayZ, rayX, rayY };
        RaycastHit? hit = GetAproxCenter(rays);

        if (hit.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.Value.point, 0.05f);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(selection.bounds.center, 0.05f);
        }
    }

    public RaycastHit? GetAproxCenter(List<ProjectionRayData> rays)
    {
        foreach (var item in rays)
        {
            if(Physics.Raycast(item.from, item.to - item.from, out RaycastHit hit, Vector3.Distance(item.from, item.to)))
            {
                return hit;
            }
        }

        return null;
    }
#endif
}

public static class MeshExtension
{
    public static void GetRectangleFrame(
    Vector3[] points,
    out float width,
    out float height,
    out Vector3 center,
    out Vector3 xAxis,
    out Vector3 yAxis)
    {
        if (points.Length != 4)
        {
            Debug.LogError("Exactly 4 points are required.");
            width = height = 0;
            center = xAxis = yAxis = Vector3.zero;
            return;
        }

        // Step 1: Compute center
        center = (points[0] + points[1] + points[2] + points[3]) / 4f;

        // Step 2: Compute plane normal
        Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;

        // Step 3: Choose an edge as X axis (stable)
        Vector3 edge = points[1] - points[0];
        xAxis = edge.normalized;

        // Step 4: Compute Y axis orthogonal to normal and xAxis
        yAxis = Vector3.Cross(normal, xAxis).normalized;

        // Re-orthogonalize xAxis in case edge wasn't perpendicular
        xAxis = Vector3.Cross(yAxis, normal).normalized;

        // Step 5: Build a transform matrix to local space
        Matrix4x4 worldToLocal = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(normal, yAxis), Vector3.one).inverse;

        // Step 6: Project all points to local XY plane
        Vector2[] local2D = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            Vector3 local = worldToLocal.MultiplyPoint3x4(points[i]);
            local2D[i] = new Vector2(local.x, local.y);
        }

        // Step 7: Compute bounds in local space
        float minX = local2D[0].x, maxX = local2D[0].x;
        float minY = local2D[0].y, maxY = local2D[0].y;

        for (int i = 1; i < 4; i++)
        {
            minX = Mathf.Min(minX, local2D[i].x);
            maxX = Mathf.Max(maxX, local2D[i].x);
            minY = Mathf.Min(minY, local2D[i].y);
            maxY = Mathf.Max(maxY, local2D[i].y);
        }

        width = maxX - minX;
        height = maxY - minY;

        if (height > width)
        {
            // Ensure width is always the larger dimension
            Vector3 temp = xAxis;
            xAxis = yAxis;
            yAxis = temp;
            float tempSize = width;
            width = height;
            height = tempSize;
        }
    }


    public static void GetProjectionAxesFromAverageNormal(Mesh mesh, out Vector3 axisX, out Vector3 axisY, out Vector3 avgNormal)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        avgNormal = Vector3.zero;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            avgNormal += normal;
        }

        avgNormal.Normalize();

        //// Step 2: Create orthonormal basis
        //// axisX: any perpendicular vector
        //axisX = Vector3.Cross(avgNormal, Vector3.up).normalized;
        //if (axisX == Vector3.zero)
        //    axisX = Vector3.Cross(avgNormal, Vector3.right).normalized;

        //axisY = Vector3.Cross(avgNormal, axisX).normalized;

        Vector3 up = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(avgNormal, up)) > 0.99f)
            up = Vector3.forward; // Use an alternative if avgNormal is almost parallel to up

        axisX = Vector3.Cross(avgNormal, up).normalized;
        axisY = Vector3.Cross(avgNormal, axisX).normalized;
    }

    public struct OBBResult
    {
        public Vector3 exactCorner;
        public Vector3 nearestMeshCorner;
    }
    /*
    public static OBBResult[] Get4CornerVerticesWithOBBAndProjection(this Mesh mesh, Transform transform)
    {
        var vertices = mesh.vertices;
        if (vertices.Length < 4)
        {
            OBBResult[] r = new OBBResult[4];
            for (int i = 0; i < 4; i++)
            {
                r[i] = new OBBResult
                {
                    exactCorner = transform.TransformPoint(vertices[i % vertices.Length]),
                    nearestMeshCorner = transform.TransformPoint(vertices[i % vertices.Length])
                };
            }
            return r;
        }

        // Step 1: Center
        Vector3 mean = Vector3.zero;
        foreach (var v in vertices) mean += v;
        mean /= vertices.Length;

        List<Vector3> centered = vertices.Select(v => v - mean).ToList();

        // Step 2: PCA
        // Covariance matrix
        float[,] cov = new float[3, 3];
        foreach (var v in centered)
        {
            cov[0, 0] += v.x * v.x; cov[0, 1] += v.x * v.y; cov[0, 2] += v.x * v.z;
            cov[1, 0] += v.y * v.x; cov[1, 1] += v.y * v.y; cov[1, 2] += v.y * v.z;
            cov[2, 0] += v.z * v.x; cov[2, 1] += v.z * v.y; cov[2, 2] += v.z * v.z;
        }

        // Use Unity's Matrix4x4 and Eigen decomposition if you want — for now, assume PCA axes as X/Y of best-fit plane
        // For this example, pick X and Y of Unity space as approximation (or plug in real eigenvectors)

        GetProjectionAxesFromAverageNormal(mesh, out Vector3 axisX, out Vector3 axisY);
        //GetPCAProjectionAxes(mesh, out Vector3 axisX, out Vector3 axisY);

        //Vector3 axisX = Vector3.right;
        //Vector3 axisY = Vector3.up;

        // Step 3: Project to 2D
        List<Vector2> projected = centered.Select(v => new Vector2(Vector3.Dot(v, axisX), Vector3.Dot(v, axisY))).ToList();

        // Step 4: Convex Hull
        List<Vector2> hull = ConvexHull2D(projected);

        // Step 5: OBB in 2D
        Vector2[] obb2D = GetOBBCornersAlignedToInput(hull);

        // Step 6: Find closest original 2D points -> map to 3D
        OBBResult[] results = new OBBResult[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 corner2D = obb2D[i];
            Vector3 exactCorner = mean + corner2D.x * axisX + corner2D.y * axisY;
            int closest = FindClosest2DIndex(corner2D, projected);
            Vector3 meshCorner = vertices[closest];

            results[i] = new OBBResult
            {
                exactCorner = transform.TransformPoint(exactCorner),
                nearestMeshCorner = transform.TransformPoint(meshCorner)
            };
        }
        return results;
    }
    */

    private static int FindClosest2DIndex(Vector2 target, List<Vector2> list)
    {
        float minDistSq = float.MaxValue;
        int closest = -1;
        for (int i = 0; i < list.Count; i++)
        {
            float distSq = (list[i] - target).sqrMagnitude;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closest = i;
            }
        }
        return closest;
    }

    // Simple Graham Scan for Convex Hull in 2D
    private static List<Vector2> ConvexHull2D(List<Vector2> points)
    {
        if (points.Count <= 3)
            return new List<Vector2>(points);

        points = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();
        List<Vector2> lower = new(), upper = new();

        foreach (var p in points)
        {
            while (lower.Count >= 2 && Cross(lower[^2], lower[^1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        for (int i = points.Count - 1; i >= 0; i--)
        {
            var p = points[i];
            while (upper.Count >= 2 && Cross(upper[^2], upper[^1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);
        return lower;
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    private static Vector2[] GetOBBCornersAlignedToInput(List<Vector2> points)
    {
        float minArea = float.MaxValue;
        Vector2[] bestRect = new Vector2[4];

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = points[(i + 1) % points.Count];

            Vector2 edge = (p2 - p1).normalized;
            Vector2 perp = new Vector2(-edge.y, edge.x);

            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            List<Vector2> projections = new();

            foreach (var p in points)
            {
                float x = Vector2.Dot(p, edge);
                float y = Vector2.Dot(p, perp);

                projections.Add(new Vector2(x, y));

                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            float area = (maxX - minX) * (maxY - minY);
            if (area < minArea)
            {
                minArea = area;

                // Reconstruct rectangle in rotated basis, then transform back to original 2D space
                Vector2 baseOrigin = edge * minX + perp * minY;
                Vector2 xVec = edge * (maxX - minX);
                Vector2 yVec = perp * (maxY - minY);

                bestRect[0] = baseOrigin;
                bestRect[1] = baseOrigin + xVec;
                bestRect[2] = baseOrigin + xVec + yVec;
                bestRect[3] = baseOrigin + yVec;
            }
        }

        return bestRect;
    }
    public struct OBBWithCenterResult
    {
        public Vector3 center;
        public OBBResult[] corners;
        public Vector3 projectedX;
        public Vector3 projectedY;
    }

    public static OBBWithCenterResult Get4CornerVerticesWithOBBAndCenter(this Mesh mesh, Transform transform, Vector3 offset)
    {
        var vertices = mesh.vertices;
        if (vertices.Length < 4)
        {
            OBBResult[] fallback = new OBBResult[4];
            for (int i = 0; i < 4; i++)
            {
                Vector3 p = transform.TransformPoint(vertices[i % vertices.Length]);
                fallback[i] = new OBBResult
                {
                    exactCorner = p,
                    nearestMeshCorner = p
                };
            }

            Vector3 meanTri = (vertices[0] + vertices[1] + vertices[2]);
            meanTri /= 3f; // Use first 3 vertices for center if not enough data

            return new OBBWithCenterResult
            {
                center = transform.TransformPoint(meanTri),
                corners = fallback
            };
        }

        // Step 1: Center
        Vector3 mean = Vector3.zero;
        foreach (var v in vertices) mean += v;
        mean /= vertices.Length;

        List<Vector3> centered = vertices.Select(v => v - mean).ToList();

        // Step 2: PCA (optional)
        GetProjectionAxesFromAverageNormal(mesh, out Vector3 axisX, out Vector3 axisY, out Vector3 avgNormal);

        // Step 3: Project to 2D
        List<Vector2> projected = centered.Select(v => new Vector2(Vector3.Dot(v, axisX), Vector3.Dot(v, axisY))).ToList();

        // Step 4: Convex Hull
        List<Vector2> hull = ConvexHull2D(projected);

        // Step 5: OBB in 2D
        Vector2[] obb2D = GetOBBCornersAlignedToInput(hull);

        // Step 6: Find closest original 2D points -> map to 3D
        OBBResult[] results = new OBBResult[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 corner2D = obb2D[i];
            Vector3 exactCorner = mean + corner2D.x * axisX + corner2D.y * axisY;
            int closest = FindClosest2DIndex(corner2D, projected);
            Vector3 meshCorner = vertices[closest];

            results[i] = new OBBResult
            {
                exactCorner = transform.TransformPoint(exactCorner),
                nearestMeshCorner = transform.TransformPoint(meshCorner)
            };
        }


        // Compute OBB center from 2D rectangle
        Vector2 center2D = Vector2.zero;
        foreach (var c in obb2D) center2D += c;
        center2D /= obb2D.Length;

        Vector3 obbCenter = mean + center2D.x * axisX + center2D.y * axisY;

        //Dictionary<int, float> distances = new Dictionary<int, float>()
        //{ 
        //    { 0, Vector3.Distance(results[0].exactCorner, results[1].exactCorner)},
        //    { 1, Vector3.Distance(results[0].exactCorner, results[2].exactCorner)},
        //    { 2, Vector3.Distance(results[0].exactCorner, results[3].exactCorner)}
        //};

        //var indexes = distances.OrderBy(x => x.Value).Select(k => k.Key).ToList();

        //Vector3 sudoright;

        //if (indexes[1] == 0)
        //{
        //    sudoright = (results[1].exactCorner - results[0].exactCorner).normalized;
        //}
        //else if (indexes[1] == 1)
        //{
        //    sudoright = (results[2].exactCorner - results[0].exactCorner).normalized;
        //}
        //else
        //{
        //    sudoright = (results[3].exactCorner - results[0].exactCorner).normalized;
        //}

        //Vector3 up = Vector3.Cross(avgNormal, sudoright).normalized;
        //Vector3 right = Vector3.Cross(up, avgNormal).normalized;

        return new OBBWithCenterResult
        {
            center = transform.TransformPoint(obbCenter),
            corners = results,
            projectedX = transform.TransformDirection(axisX),
            projectedY = transform.TransformDirection(axisY),
        };
    }
}