using System;
using UnityEngine;

public class GaugePointerLookAt : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //static float width = 0.1f;
    public GameObject pivot;

    void Update()
    {
        //width = width_;
        if(GaugingManager.Instance.gaugepointLookAtOffset >= 0f)
        {
            LookAt();
        }
    }
    public void LookAt()
    {
        float dotProd = Vector3.Dot(CameraService.Instance.cameraTransform.forward, transform.forward);

        if (dotProd < 0)
        {
            transform.rotation = Quaternion.LookRotation(-transform.forward, transform.up);
        }

        if(ApplicationStateMachine.Instance.currentStateName == nameof(HullpartViewState))
        {
            EnableChildren(Mathf.Abs(dotProd) > GaugingManager.Instance.gaugepointLookAtThreshold);
        }

        //transform.rotation = Quaternion.LookRotation(CameraService.Instance.cameraTransform.forward, CameraService.Instance.cameraTransform.up);
        //transform.position = (transform.parent.position + transform.parent.up * 0.65f) + (-transform.forward * 0.5f);

        //width = transform.parent.parent.localScale.x * 2f;
        //transform.position = (transform.parent.position + transform.parent.right * width) + (-transform.forward * 0.2f);

        transform.position = (transform.parent.position + transform.parent.right * 0) + (-transform.forward * GaugingManager.Instance.gaugepointLookAtOffset);
    }

    private void EnableChildren(bool v)
    {
     //   pivot.SetActive(v);
    }
}
