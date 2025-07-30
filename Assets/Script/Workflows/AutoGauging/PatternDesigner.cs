using EasyButtons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatternDesigner : MonoBehaviour
{
    public List<Vector3> points = null;

    [Button]
    public void RegisterPoints()
    {
        points = new List<Vector3>();
        foreach (Transform item in transform)
        {
            points.Add(item.position);
        }
        Debug.Log("Points registered: " + points.Count);
        ClearChildren();
    }

    public void ClearChildren()
    {
        var items = transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform item in items)
        {
            if(item != transform)
            {
                DestroyImmediate(item.gameObject);
            }
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var item in points)
        {
            Gizmos.DrawSphere(item + transform.position, 0.1f);
        }

        Gizmos.color = Color.blue;
        foreach (Transform item in transform)
        {
            Gizmos.DrawSphere(item.position, 0.1f);
        }
    }
}
