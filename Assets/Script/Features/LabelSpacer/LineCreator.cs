using EasyButtons;
using UnityEngine;

public class LineCreator : MonoBehaviour
{
    public float buffer = 0.1f;
    public Transform lineScale;

    [Button]
    public void UpdateLine()
    {
        Vector3 zClampedPosition = Vector3.zero;
        zClampedPosition.z = transform.GetChild(0).transform.localPosition.z;
        zClampedPosition = transform.TransformPoint(zClampedPosition);

        Vector3 up = (transform.GetChild(0).transform.position - zClampedPosition).normalized;
        Vector3 forward = transform.forward;
        Quaternion rotation = Quaternion.LookRotation(forward, up);
        Vector3 position = (zClampedPosition + transform.GetChild(0).position) * 0.5f;

        lineScale.rotation = rotation;
        lineScale.localScale = new Vector3(1, Vector3.Distance(zClampedPosition, transform.GetChild(0).transform.position) - buffer, 1);
        lineScale.position = position;
    }

    private void OnDrawGizmos()
    {
        UpdateLine();
    }
}
