using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Camera targetCamera;
    private Vector3 initScale;
    public float scaleThreshold = 0.2f;

    public void Start()
    {
        initScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                         targetCamera.transform.rotation * Vector3.up);

        float fovMultiplier = (2f - (CameraInputController.Instance.scrollSlider.normalizedValue * 2f));
        float distanceMultiplier = (targetCamera.transform.position - transform.position).magnitude / 60f;

        Vector3 parentScale = transform.parent.localScale;

        // Use current signs from ArrowController-applied scale
        float signX = Mathf.Sign(parentScale.x);
        float signY = Mathf.Sign(parentScale.y);
        float signZ = Mathf.Sign(parentScale.z);

        Vector3 newScale = initScale * distanceMultiplier * fovMultiplier * 0.25f;

        // Apply original signs
        newScale.x = Mathf.Min(Mathf.Abs(newScale.x), scaleThreshold) * signX;
        newScale.y = Mathf.Min(Mathf.Abs(newScale.y), scaleThreshold) * signY;
        newScale.z = Mathf.Min(Mathf.Abs(newScale.z), scaleThreshold) * signZ;

        transform.parent.localScale = newScale;
    }
}
