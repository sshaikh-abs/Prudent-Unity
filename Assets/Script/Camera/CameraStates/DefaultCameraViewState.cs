using UnityEngine;

public class DefaultCameraViewState : FreeFlyViewState
{
    private Vector3 m_defaultCameraPosition;
    private Quaternion m_defaultCameraRotation;

    public DefaultCameraViewState(CameraService cameraService) : base(cameraService)
    {
        m_defaultCameraPosition = CameraService.Instance.DefaultVirtualCamera.gameObject.transform.position;
        m_defaultCameraRotation = CameraService.Instance.DefaultVirtualCamera.gameObject.transform.rotation;
    }

    public override void Enter()
    {
        // Reset to default view
        m_defaultVirtualCamera.LookAt = CameraService.Instance.DefaultTarget.transform;
        m_defaultVirtualCamera.transform.position = m_defaultCameraPosition;
        m_defaultVirtualCamera.transform.rotation = m_defaultCameraRotation;

        base.Enter();
    }
}
