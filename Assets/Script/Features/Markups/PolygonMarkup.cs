using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolygonMarkup : BaseMarkup
{
    public List<Vector3> verts = new List<Vector3>();

    public GameObject vertPointTemplate;

    private List<GameObject> vertPoints = new List<GameObject>();

    //public float scaleThreshold = 0.2f;
    //private Vector3 initScale => 0.5f * Vector3.one;

    public override void ValidateShape()
    {
        Mesh strokeMesh = new Mesh();
        Mesh fillMesh = GenerateMesh(verts);

        CalculateArea();

        if (verts.Count >= 3 && MeshExtruder.Instance != null)
        {
            fillMesh = MeshExtruder.Instance.ExtrudeFace(fillMesh, 0.01f);
            fillMeshFilter.sharedMesh = fillMesh;
            gameObject.layer = LayerMask.NameToLayer("Markup");
            GetComponent<MeshCollider>().sharedMesh = fillMesh;
        }

        strokeMesh.SetVertices(verts);
        List<int> tris = Enumerable.Range(0, verts.Count).ToList();
        tris.Add(tris[0]);
        strokeMesh.SetIndices(tris, MeshTopology.LineStrip, 0);
        strokeFilter.sharedMesh = strokeMesh;

    }

    public override void ParseDimensions()
    {
        Collider col = GetComponent<Collider>();
        verts = new List<Vector3>();
        for (int i = 0; i < data.dimensions.Count; i += 3)
        {
            verts.Add(new Vector3(data.dimensions[i], data.dimensions[i + 1], data.dimensions[i + 2]));
        }
    }

    public override void CalculateArea()
    {
        data.area = fillMeshFilter.sharedMesh.CalculateSurfaceArea();
    }

    public override void UpdateData(MarkupData markupData)
    {
        base.UpdateData(markupData);
        ParseDimensions();

        if (!Application.isPlaying)
        {
            return;
        }

        foreach (var item in vertPoints)
        {
            Destroy(item);
        }
        vertPoints.Clear();
        int index = 0;

        foreach (var item in verts)
        {
            GameObject v = Instantiate(vertPointTemplate, item, Quaternion.identity, transform);
            v.transform.SetParent(transform);
            vertPoints.Add(v);
            v.name = "Vertex Point " + (index);
            index++;

            //float fovMultiplier = (2f - (CameraInputController.Instance.scrollSlider.normalizedValue * 2f));
            //float distanceMultiplierSquared = (Camera.main.transform.position - transform.position).magnitude / (60f);

            //transform.localScale = initScale * distanceMultiplierSquared * fovMultiplier;

            //if (transform.localScale.x > scaleThreshold)
            //{
            //    transform.localScale = Vector3.one * scaleThreshold;
            //}
        }
    }

    public override void ValidateColor()
    {
        base.ValidateColor();

        if (!Application.isPlaying)
        {
            return;
        }

        foreach (var item in vertPoints)
        {
            item.GetComponent<MeshRenderer>().material.color = markupColor;
        }
    }

    #region Polygon Markup

    public Mesh GenerateMesh(List<Vector3> userPoints)
    {
        float thickness = 0.1f; // Default thickness for the polygon mesh

        if (userPoints.Count < 3)
        {
            Debug.LogWarning("Need at least 3 points to form a polygon.");
            return null;
        }

        // Step 1: Compute best-fit plane
        Vector3 centroid = userPoints.Aggregate(Vector3.zero, (sum, p) => sum + p) / userPoints.Count;
        Vector3 normal = ComputeBestFitNormal(userPoints, centroid);
        GetPlaneBasis(normal, out var axisX, out var axisY);

        // Step 2: Project points to 2D
        List<Vector2> projected2D = new();
        foreach (var p in userPoints)
        {
            Vector3 d = p - centroid;
            projected2D.Add(new Vector2(Vector3.Dot(d, axisX), Vector3.Dot(d, axisY)));
        }

        if (IsPolygonClockwise(projected2D))
            projected2D.Reverse(); // Ensure CCW winding

        var edgeStripVerts = new List<Vector3>();

        foreach (var point in userPoints)
        {
            Vector3 topVertex = centroid + axisX * point.x + axisY * point.y + normal * thickness / 2f;
            Vector3 bottomVertex = centroid + axisX * point.x + axisY * point.y - normal * thickness / 2f;
            edgeStripVerts.Add(topVertex);
            edgeStripVerts.Add(bottomVertex);
        }

        // Step 3: Triangulate in 2D
        int[] triangleIndices = Triangulate(projected2D);
        int[] triangleIndices_offseted = new int[triangleIndices.Length * 2];

        for (int i = 0; i < triangleIndices.Length; i++)
        {
            int index = triangleIndices[i];
            triangleIndices_offseted[i * 2] = index * 2; // Top vertex
            triangleIndices_offseted[i * 2 + 1] = index * 2 + 1; // Bottom vertex
        }

        // Step 4: Build 3D mesh
        Mesh mesh = new();
        mesh.SetVertices(userPoints);
        mesh.SetTriangles(triangleIndices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Vector3 ComputeBestFitNormal(List<Vector3> points, Vector3 centroid)
    {
        // Basic PCA for plane fitting
        float xx = 0, xy = 0, xz = 0, yy = 0, yz = 0, zz = 0;
        foreach (var p in points)
        {
            Vector3 r = p - centroid;
            xx += r.x * r.x; xy += r.x * r.y; xz += r.x * r.z;
            yy += r.y * r.y; yz += r.y * r.z; zz += r.z * r.z;
        }

        Matrix4x4 cov = new();
        cov[0, 0] = xx; cov[0, 1] = xy; cov[0, 2] = xz;
        cov[1, 0] = xy; cov[1, 1] = yy; cov[1, 2] = yz;
        cov[2, 0] = xz; cov[2, 1] = yz; cov[2, 2] = zz;

        // Approximate least-dominant direction = normal
        Vector3 col0 = new Vector3(cov[0, 0], cov[1, 0], cov[2, 0]);
        Vector3 col1 = new Vector3(cov[0, 1], cov[1, 1], cov[2, 1]);
        Vector3 col2 = new Vector3(cov[0, 2], cov[1, 2], cov[2, 2]);

        Vector3 normal = Vector3.Cross(col0, col1).normalized +
                         Vector3.Cross(col1, col2).normalized +
                         Vector3.Cross(col2, col0).normalized;
        return normal.normalized;
    }

    void GetPlaneBasis(Vector3 normal, out Vector3 axisX, out Vector3 axisY)
    {
        // Safe way to create tangent basis
        if (Mathf.Abs(normal.y) < 0.99f)
            axisX = Vector3.Cross(normal, Vector3.up).normalized;
        else
            axisX = Vector3.Cross(normal, Vector3.forward).normalized;

        axisY = Vector3.Cross(normal, axisX).normalized;
    }

    bool IsPolygonClockwise(List<Vector2> points)
    {
        float sum = 0;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % points.Count];
            sum += (b.x - a.x) * (b.y + a.y);
        }
        return sum > 0; // True = clockwise
    }

    int[] Triangulate(List<Vector2> inputPoints)
    {
        List<Vector2> points = new List<Vector2>(inputPoints);
        List<int> indices = new List<int>();
        int n = points.Count;

        if (n < 3) return indices.ToArray();

        int[] V = new int[n];
        if (Area(points) > 0)
        {
            for (int v = 0; v < n; v++) V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++) V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2;)
        {
            if ((count--) <= 0)
            {
                Debug.LogWarning("Triangulation failed - possibly non-simple polygon");
                return new int[0];
            }

            int u = v;
            if (nv <= u) u = 0;
            v = u + 1;
            if (nv <= v) v = 0;
            int w = v + 1;
            if (nv <= w) w = 0;

            if (Snip(points, u, v, w, nv, V))
            {
                int a = V[u];
                int b = V[v];
                int c = V[w];
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);

                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        return indices.ToArray();
    }

    float Area(List<Vector2> points)
    {
        int n = points.Count;
        float A = 0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = points[p];
            Vector2 qval = points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return A * 0.5f;
    }

    bool Snip(List<Vector2> contour, int u, int v, int w, int n, int[] V)
    {
        Vector2 A = contour[V[u]];
        Vector2 B = contour[V[v]];
        Vector2 C = contour[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;

        for (int p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w)) continue;
            Vector2 P = contour[V[p]];
            if (PointInTriangle(P, A, B, C))
                return false;
        }

        return true;
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1f / (2f * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1f / (2f * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        float u = 1 - s - t;
        return s >= 0 && t >= 0 && u >= 0;
    }

    #endregion
}

public static class MeshUtility
{
    public static float CalculateSurfaceArea(this Mesh mesh)
    {
        if (mesh == null || mesh.triangles.Length == 0)
            return 0f;

        float totalArea = 0f;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            totalArea += TriangleArea(v0, v1, v2);
        }

        return totalArea;
    }

    private static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        // Heron's formula is avoided for performance and stability.
        return Vector3.Cross(b - a, c - a).magnitude * 0.5f;
    }
}
