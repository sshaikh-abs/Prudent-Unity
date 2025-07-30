using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo_CameraFollow : MonoBehaviour
{
    public Camera primaryCamera;          // Reference to the primary camera
    public Transform targetObject;        // The target object to look at (e.g., static cube)
    public Vector3 positionOffset;        // Optional: Offset from target object to the secondary camera's position

    void Update()
    {
        if (primaryCamera != null && targetObject != null)
        {
            // Match the secondary camera's rotation to the primary camera's rotation
            transform.rotation = primaryCamera.transform.rotation;

            // Optional: Maintain the fixed position relative to the target object
            //transform.position = targetObject.position + positionOffset;

            // Make the secondary camera always look at the target object
            //transform.LookAt(targetObject);
        }
    }


}
