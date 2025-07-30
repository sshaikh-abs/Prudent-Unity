using UnityEngine;

public class Gizmo_Camera : MonoBehaviour
{
    // Reference to the primary camera
    public Camera primaryCamera;

    // Target object to stay in the center of the view
    public Transform target;

    // Distance from the target
    public float distance = 10f;

    // Smooth follow speeds
    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;

    // Update the position and rotation in LateUpdate to avoid jittering
    private void LateUpdate()
    {
        if (primaryCamera != null && target != null)
        {
            // Keep the secondary camera positioned behind the target, relative to the primary camera's rotation
            Vector3 targetPosition = target.position - primaryCamera.transform.forward * distance;

            // Smoothly move the gizmo camera to the desired position
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);

            // Smoothly rotate the gizmo camera to match the primary camera's rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, primaryCamera.transform.rotation, rotationSmoothSpeed * Time.deltaTime);
        }
        else
        {
            // Debug log if the primary camera or target is not assigned
            if (primaryCamera == null)
                Debug.LogWarning("Primary Camera is not assigned!");
            if (target == null)
                Debug.LogWarning("Target is not assigned!");
        }
    }

    // Optional: Draw a Gizmo for the camera position (visualize in Scene view)
    private void OnDrawGizmos()
    {
        if (target != null)
        {
            //// Draw a line from the camera to the target
            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(transform.position, target.position);

            //// Optionally, draw a sphere to visualize the target position
            //Gizmos.color = Color.green;
            //Gizmos.DrawSphere(target.position, 0.5f);
        }
    }
}

