using UnityEngine;

public class LineCreation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform pointB;

    public Transform lineCylinder;

    public void UpdateLine()
    {
        lineCylinder.position = (transform.position + pointB.position) / 2;
        lineCylinder.LookAt(pointB.position);
        lineCylinder.localScale = new Vector3(1f, .05f, Vector3.Distance(transform.position, pointB.position));
    }
}
