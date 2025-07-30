using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using static MeshExtension;

public class PatternFetcher : MonoBehaviour
{
    public PatternDefination PatternDefination;
    public string PatternName;
    [ReadOnly] public Vector2 dimensions;
    public float pushBack = 2f;
    public float factor = 0f; // Used to scale the pattern dimensions
    public Vector2 offset = Vector2.zero; // Used to scale the pattern dimensions

    public Vector2[] GetPoints()
    {
        Vector2[] corners = new Vector2[0];
        return corners;
    }
#if UNITY_EDITOR
    public Renderer GetSelected()
    {
        if (UnityEditor.Selection.activeGameObject == null)
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

    public static void GetOrthonormalBasis(Vector3 input, string directionType, out Vector3 dir1, out Vector3 dir2)
    {
        Vector3 primary = input.normalized;
        Vector3 arbitrary;

        // Choose a good arbitrary vector to avoid parallel edge cases
        if (Mathf.Abs(Vector3.Dot(primary, Vector3.up)) < 0.99f)
            arbitrary = Vector3.up;
        else
            arbitrary = Vector3.right;

        if (directionType == "up")
        {
            dir1 = Vector3.Cross(arbitrary, primary).normalized; // right
            dir2 = Vector3.Cross(primary, dir1).normalized;       // forward
        }
        else if (directionType == "forward")
        {
            dir1 = Vector3.Cross(arbitrary, primary).normalized; // right
            dir2 = Vector3.Cross(primary, dir1).normalized;       // up
        }
        else if (directionType == "right")
        {
            dir1 = Vector3.Cross(primary, arbitrary).normalized; // up
            dir2 = Vector3.Cross(dir1, primary).normalized;       // forward
        }
        else
        {
            throw new System.ArgumentException("Invalid directionType. Use 'up', 'forward', or 'right'.");
        }
    }

    public List<Vector3> GetPatternPositionsOnTarget(Renderer selection, SubpartType subpartType, AutoGaugingPatternDefination defination, out int pointsAllowed)
    {
        pointsAllowed = 0;

        if (defination.bracketPattern == "0" && subpartType == SubpartType.Bracket ||
            defination.platePattern == "0" && subpartType == SubpartType.Plate ||
            defination.stiffenerPattern == "0" && subpartType == SubpartType.Stiffener)
        {
            return new List<Vector3>();
        }

        if (selection == null)
        {
            return null;
        }

        var targetMesh = selection.GetComponent<MeshFilter>().sharedMesh;
        if (targetMesh == null)
        {
            return null;
        }

        List<Vector3> results = new List<Vector3>();

        var DrawCorners = targetMesh.Get4CornerVerticesWithOBBAndCenter(selection.transform, Vector3.zero);
        MeshExtension.GetRectangleFrame(DrawCorners.corners.Select(c => c.exactCorner).ToArray(), out float width, out float height, out Vector3 center, out Vector3 xAxis, out Vector3 yAxis);
        dimensions = new Vector2(width, height);

        switch (subpartType)
        {
            case SubpartType.Plate:

                PatternName = "Plates_" + defination.platePattern;
                break;
            case SubpartType.Bracket:
                PatternName = "Brackets_" + defination.bracketPattern;
                break;
            case SubpartType.Stiffener:
                PatternName = "Stiffeners_" + defination.stiffenerPattern;
                break;
            case SubpartType.None:
            case SubpartType.All:
            default:
                break;
        }

        var pattern = PatternDefination.GetPattern(PatternName);

        if (pattern == null)
        {
            return new List<Vector3>();
        }

        Vector3 right = xAxis;
        Vector3 forward = Vector3.Cross(yAxis, xAxis);
        Vector3 up = yAxis;

        pointsAllowed = pattern.pointsToPick < 1 ? pattern.points.Count : pattern.pointsToPick;

        foreach (var corner in pattern.points)
        {
            Vector3 actualPosition = corner;
            actualPosition = RotatePositionByDirections(actualPosition, right, forward, pattern.dimensions, dimensions, factor) + DrawCorners.center;

            results.Add(actualPosition + (forward * pushBack));
            results.Add(actualPosition - (forward * pushBack));
        }

        transform.position = DrawCorners.center;
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(yAxis, xAxis), yAxis);
        return results;
    }

    public List<Vector3> GetPatternPositionsOnTarget_Debug(Renderer selection, SubpartType subpartType, AutoGaugingPatternDefination defination, out OBBWithCenterResult DrawCorners, out int pointsAllowed)
    {
        pointsAllowed = 0;

        DrawCorners = new OBBWithCenterResult();

        if (defination.bracketPattern == "0" && subpartType == SubpartType.Bracket ||
            defination.platePattern == "0" && subpartType == SubpartType.Plate ||
            defination.stiffenerPattern == "0" && subpartType == SubpartType.Stiffener)
        {
            return new List<Vector3>();
        }

        if (selection == null)
        {
            return null;
        }

        var targetMesh = selection.GetComponent<MeshFilter>().sharedMesh;
        if (targetMesh == null)
        {
            return null;
        }

        List<Vector3> results = new List<Vector3>();

        DrawCorners = targetMesh.Get4CornerVerticesWithOBBAndCenter(selection.transform, Vector3.zero);
        MeshExtension.GetRectangleFrame(DrawCorners.corners.Select(c => c.exactCorner).ToArray(), out float width, out float height, out Vector3 center, out Vector3 xAxis, out Vector3 yAxis);
        dimensions = new Vector2(width, height);

        switch (subpartType)
        {
            case SubpartType.Plate:

                PatternName = "Plates_" + defination.platePattern;
                break;
            case SubpartType.Bracket:
                PatternName = "Brackets_" + defination.bracketPattern;
                break;
            case SubpartType.Stiffener:
                PatternName = "Stiffeners_" + defination.stiffenerPattern;
                break;
            case SubpartType.None:
            case SubpartType.All:
            default:
                break;
        }

        var pattern = PatternDefination.GetPattern(PatternName);

        if (pattern == null)
        {
            return new List<Vector3>();
        }

        pointsAllowed = pattern.pointsToPick < 1 ? pattern.points.Count : pattern.pointsToPick;

        Vector3 right = xAxis;
        Vector3 forward = Vector3.Cross(yAxis, xAxis);
        Vector3 up = yAxis;


        foreach (var corner in pattern.points)
        {
            Vector3 actualPosition = corner;
            actualPosition = RotatePositionByDirections(actualPosition, right, forward, pattern.dimensions, dimensions, factor) + DrawCorners.center;

            results.Add(actualPosition + (forward * pushBack));
            results.Add(actualPosition - (forward * pushBack));
        }

        transform.position = DrawCorners.center;
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(yAxis, xAxis), yAxis);
        return results;
    }

    public static Vector3 RotatePositionByDirections(Vector3 position, Vector3 dir1, Vector3 dir2, Vector2 patternDimensions, Vector2 dimensions, float factor = 0f)
    {
        // Ensure the input directions are normalized
        Vector3 xAxis = dir1.normalized;
        Vector3 yAxis = dir2.normalized;

        // Compute the third orthogonal vector using cross product
        Vector3 zAxis = Vector3.Cross(xAxis, yAxis).normalized;

        float xScale = dimensions.x / ((patternDimensions.x <= 0f) ? 1f : patternDimensions.x);
        float yScale = dimensions.y / ((patternDimensions.y <= 0f) ? 1f : patternDimensions.y);

        xAxis *= (xScale * factor * 0.5f);
        zAxis *= (yScale * factor * 0.5f);

        // Create a rotation matrix from the new basis
        Matrix4x4 rotationMatrix = new Matrix4x4();

        // Each column of the matrix is a basis vector
        rotationMatrix.SetColumn(0, xAxis);  // X direction
        rotationMatrix.SetColumn(1, yAxis);  // Y direction
        rotationMatrix.SetColumn(2, zAxis);  // Z direction
        rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1)); // Homogeneous coordinate

        // Rotate the position
        return rotationMatrix.MultiplyPoint3x4(position);
    }

    public static Vector3 RotatePositionByDirections(Vector3 position, Vector3 dir1, Vector3 dir2)
    {
        // Ensure the input directions are normalized
        Vector3 xAxis = dir1.normalized;
        Vector3 yAxis = dir2.normalized;

        // Compute the third orthogonal vector using cross product
        Vector3 zAxis = Vector3.Cross(xAxis, yAxis).normalized;

        // Create a rotation matrix from the new basis
        Matrix4x4 rotationMatrix = new Matrix4x4();

        // Each column of the matrix is a basis vector
        rotationMatrix.SetColumn(0, xAxis);  // X direction
        rotationMatrix.SetColumn(1, yAxis);  // Y direction
        rotationMatrix.SetColumn(2, zAxis);  // Z direction
        rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1)); // Homogeneous coordinate

        // Rotate the position
        return rotationMatrix.MultiplyPoint3x4(position);
    }

    public List<RaycastHit> GetProjectedPoints(Renderer renderer, List<Vector3> points, int pointsAllowed, bool byPassCount)
    {
        var collider = renderer.GetComponent<Collider>();
        if (collider == null)
        {
            return new List<RaycastHit>();
        }

        List<RaycastHit> results = new List<RaycastHit>();

        for (int i = 0; i < points.Count; i += 2)
        {
            Ray ray = new Ray(points[i], (points[(i + 1) % points.Count] - points[i]).normalized);

            if (collider.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity))
            {
                results.Add(hitInfo);
                if ((!byPassCount) && results.Count >= pointsAllowed)
                {
                    break; // If we are bypassing the count, we stop after reaching the limit
                }
            }
        }

        return results;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //if (true /*Application.isPlaying*/)
        //{
        //    return; // Gizmos should only be drawn in play mode
        //}

        var points = GetPatternPositionsOnTarget_Debug(GetSelected(), PointProjector.Instance.subpartType_Debug, PointProjector.Instance.defination, out OBBWithCenterResult drawCorners, out int pointsAllowed);

        if (points == null || points.Count == 0)
        {
            return;
        }

        var results = GetProjectedPoints(GetSelected(), points, pointsAllowed, true);
        for (int i = 0; i < points.Count; i += 2)
        {
            Debug.DrawLine(points[i], points[(i + 1) % points.Count], Color.blue, 0.1f);
        }

        Gizmos.color = Color.blue;
        foreach (var item in points)
        {
            Gizmos.DrawSphere(item, 0.05f); // Adjust radius if needed
        }

        Gizmos.color = Color.cyan;
        foreach (var item in drawCorners.corners)
        {
            Gizmos.DrawSphere(item.exactCorner, 0.05f); // Adjust radius if needed
        }

        Gizmos.color = Color.grey;
        Gizmos.DrawSphere(drawCorners.center, 0.05f); // Adjust radius if needed

        int index = 0;

        Gizmos.color = Color.green;
        foreach (var item in results)
        {
            if (index >= pointsAllowed)
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.DrawSphere(item.point, 0.05f); // Adjust radius if needed
            index++;
        }
    }
#endif
}
