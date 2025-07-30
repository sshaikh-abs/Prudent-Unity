using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoundsComputer : SingletonMono<BoundsComputer>
{
    //public Bounds Bounds;
    //public float threshold = 0.0001f;
    //public Vector3 offset;

    //private void OnValidate()
    //{
    //    Bounds.center = transform.position;
    //}

    public List<ProjectionRayData> GetPoints(Bounds Bounds, float spacing, float rayLength = 0.5f, float threshold = 0.1f, float areaThreshold = 0.1f)
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireCube(Bounds.center, Bounds.size);
        var axises = GetAxisDomination(Bounds, threshold, areaThreshold);

        Color[] colors = new Color[] { Color.green, Color.green, Color.yellow, Color.yellow, Color.red, Color.red };

        List<ProjectionRayData> results = new List<ProjectionRayData>();

        for (int i = 0; i < axises.Count; i++)
        {
            Gizmos.color = colors[i];
            var ray = GetRayFromPoint(Bounds.center + Vector3.Scale(Bounds.size / 2f, axises[i]), axises[i], rayLength);
            Vector3 canvasVector = RemapVectorByDirection(Bounds.center, axises[i]);
            List<Vector3> vectors = GetFivePointPattern(canvasVector, RemapVectorByDirection(Bounds.size, axises[i]), spacing);
            foreach (var item in vectors)
            {
                var vec = RemapVectorByDirection(item, axises[i]);
                var items = GetRayFromPoint(vec + Vector3.Scale(Bounds.size / 2f, axises[i]), axises[i], rayLength);
                results.Add(new ProjectionRayData(items.Item1, items.Item2));
            }
        }

        return results;
    }

    public (Vector3, Vector3) GetRayFromPoint(Vector3 point, Vector3 direction, float rayLength = 0.5f)
    {
        Vector3 from = point - (direction * rayLength);
        Vector3 to = point + (direction * rayLength);

        return (from, to);
    }

    public List<Vector3> GetAxisDomination(Bounds bounds, float threshold, float areaThreshold)
    {
        List<(Vector3, float)> axisDomination = new List<(Vector3, float)>();

        float xyArea = bounds.size.x * bounds.size.y;
        //if(xyArea > threshold)
        if((bounds.size.x / bounds.size.y) > areaThreshold && (bounds.size.y / bounds.size.x) > areaThreshold)
        {
            axisDomination.Add((Vector3.forward, xyArea));
            if(bounds.size.z > threshold)
            {
                axisDomination.Add((Vector3.back, xyArea));
            }
        }
        float xzArea = bounds.size.x * bounds.size.z;
        //if (xzArea > threshold)
        if ((bounds.size.x / bounds.size.z) > areaThreshold && (bounds.size.x / bounds.size.z) > areaThreshold)
        {
            axisDomination.Add((Vector3.up, xzArea));
            if (bounds.size.y > threshold)
            {
                axisDomination.Add((Vector3.down, xzArea));
            }
        }
        float yzArea = bounds.size.y * bounds.size.z;
        //if (yzArea > threshold)
        if ((bounds.size.y / bounds.size.z) > areaThreshold && (bounds.size.y / bounds.size.z) > areaThreshold)
        {
            axisDomination.Add((Vector3.right, yzArea));
            if (bounds.size.x > threshold)
            {
                axisDomination.Add((Vector3.left, yzArea));
            }
        }

        return axisDomination.OrderByDescending(a => a.Item2).Select(a => a.Item1).ToList();
    }

    Vector3 RemapVectorByDirection(Vector3 input, Vector3 direction)
    {
        int[] mapping = GetComponentPermutation(direction);
        float[] inputArray = new float[] { input.x, input.y, input.z };

        return new Vector3(
            inputArray[mapping[0]],
            inputArray[mapping[1]],
            inputArray[mapping[2]]
        );
    }

    int[] GetComponentPermutation(Vector3 dir)
    {
        if (dir == Vector3.forward) return new int[] { 0, 1, 2 }; // identity
        if (dir == Vector3.back) return new int[] { 0, 1, 2 }; // identity
        if (dir == Vector3.up) return new int[] { 0, 2, 1 }; // swap y and z
        if (dir == Vector3.down) return new int[] { 0, 2, 1 }; // swap y and z
        if (dir == Vector3.right) return new int[] { 2, 1, 0 }; // x <-> z
        if (dir == Vector3.left) return new int[] { 2, 1, 0 }; // x <-> z

        throw new ArgumentException("Unsupported direction: " + dir);
    }

    public List<Vector3> GetFivePointPattern(Vector3 point, Vector2 dimensions, float spacing)
    {
        List<Vector3> points = new List<Vector3>();

        if(dimensions.y > (spacing * 3) && dimensions.x > (spacing * 3))
        {
            points.Add(point + new Vector3(1f * ((dimensions.x / 2f) - spacing), 1f * ((dimensions.y / 2f) - spacing)));
            points.Add(point + new Vector3(1f * ((dimensions.x / 2f) - spacing), -1f * ((dimensions.y / 2f) - spacing)));
            points.Add(point + new Vector3(-1f * ((dimensions.x / 2f) - spacing), -1f * ((dimensions.y / 2f) - spacing)));
            points.Add(point + new Vector3(-1f * ((dimensions.x / 2f) - spacing), 1f * ((dimensions.y / 2f) - spacing)));
        }
        else
        {
            if (dimensions.y > (spacing * 3))
            {
                points.Add(point + new Vector3(0f, 1f * ((dimensions.y / 2f) - spacing)));
                points.Add(point - new Vector3(0f, 1f * ((dimensions.y / 2f) - spacing)));
            }
            if (dimensions.x > (spacing * 3))
            {
                points.Add(point + new Vector3(1f * ((dimensions.x / 2f) - spacing),0f));
                points.Add(point - new Vector3(1f * ((dimensions.x / 2f) - spacing),0f));
            }
        }

        points.Add(point);
        return points;
    }

//    if (dimensions.y > (spacing* 3) && dimensions.x > (spacing* 3))
//        {
//            results.Add(new ProjectionRayData(transform.TransformPoint(pointA - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointA + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//            results.Add(new ProjectionRayData(transform.TransformPoint(pointB - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointB + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//            results.Add(new ProjectionRayData(transform.TransformPoint(pointC - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointC + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//            results.Add(new ProjectionRayData(transform.TransformPoint(pointD - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointD + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//        }
//        else
//{
//    if (dimensions.y > (spacing * 3))
//    {
//        results.Add(new ProjectionRayData(transform.TransformPoint(pointUp - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointUp + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//        results.Add(new ProjectionRayData(transform.TransformPoint(pointDown - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointDown + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//    }
//    if (dimensions.x > (spacing * 3))
//    {
//        results.Add(new ProjectionRayData(transform.TransformPoint(pointRight - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointRight + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//        results.Add(new ProjectionRayData(transform.TransformPoint(pointLeft - new Vector3(0f, 0f, dimensions.z + 1f) + offset), transform.TransformPoint(pointLeft + new Vector3(0f, 0f, dimensions.z + 1f) + offset)));
//    }
//}
}
