using UnityEngine;
using TMPro;
public class PointerLookAt : MonoBehaviour
{
    public float offset = 0.65f;

    void Update()
    {
        LookAt();
    }
    public void LookAt()
    {
        transform.rotation = Quaternion.LookRotation(CameraService.Instance.cameraTransform.forward, CameraService.Instance.cameraTransform.up);
        transform.position = (transform.parent.position + transform.parent.up * (offset * transform.parent.parent.localScale.x)) + (-transform.forward * 0.5f);
    }
}
