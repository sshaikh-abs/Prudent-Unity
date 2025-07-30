using Cinemachine;
using UnityEngine;

/// <summary>
/// State that's used by default
/// Responsible for panning, orbiting, zooming and rotating
/// </summary>
public class FreeFlyViewState : BaseCameraState
{
    protected CinemachineVirtualCamera m_defaultVirtualCamera;

    private GameObject m_lookAtGOTarget;
    private Camera m_mainCamera;
    private bool m_isOrbiting;

    public FreeFlyViewState(CameraService cameraService)
    {
        m_mainCamera = Camera.main;
        m_defaultVirtualCamera = CameraService.Instance.DefaultVirtualCamera;
    }

    public override void Enter()
    {
        base.Enter();

        m_lookAtGOTarget = new GameObject("DefaultCameraLookAtRef");
        m_lookAtGOTarget.transform.localPosition = Vector3.zero;
        m_lookAtGOTarget.transform.SetParent(m_defaultVirtualCamera.transform.parent);
        m_lookAtGOTarget.transform.localPosition = Vector3.zero;

        if (m_defaultVirtualCamera.LookAt == null)
        {
            PlaceLookAtTargetInFrontOfCamera();
        }
        else
        {
            // Position the LookAt target at the current LookAt target
            m_lookAtGOTarget.transform.position = m_defaultVirtualCamera.LookAt.position;
        }

        m_defaultVirtualCamera.LookAt = m_lookAtGOTarget.transform;
        m_defaultVirtualCamera.Follow = m_lookAtGOTarget.transform;

        CameraService.Instance.FocusedPartVirtualCamera.gameObject.SetActive(false);
        CameraService.Instance.PresetVirtualCamera.gameObject.SetActive(false);
        CameraService.Instance.DefaultVirtualCamera.gameObject.SetActive(true);
    }

    private void PlaceLookAtTargetInFrontOfCamera()
    {
        Ray ray = m_mainCamera.ScreenPointToRay(Input.mousePosition);

        //// this distance is between the camera and the focused part
        //float distanceFromDefaultCamToTarget = (CameraService.Instance.FocusedPosition - m_defaultVirtualCamera.transform.position).magnitude;
        //m_lookAtGOTarget.transform.position = ray.GetPoint(distanceFromDefaultCamToTarget);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Renderer renderer = hit.transform.GetComponent<Renderer>();
            // && (ApplicationStateMachine.Instance.currentStateName != nameof(HullpartViewState))
            if (renderer != null)
            {
                Vector3 boundsCenter = renderer.bounds.center;
                m_lookAtGOTarget.transform.position = boundsCenter;
            }           
        }
        RotateLookAtTargetAwayFromCamera();
    }

    public override void Execute()
    {
        if (CameraUtils.IsMouseWithinViewport(m_mainCamera))
        {
            if (Input.GetMouseButton(0) && m_defaultVirtualCamera.gameObject.activeInHierarchy)
            {
                if (!m_isOrbiting)
                {
                    m_isOrbiting = true;

                    m_defaultVirtualCamera.LookAt = null;
                    m_defaultVirtualCamera.Follow = null;

                    PlaceLookAtTargetInFrontOfCamera();

                    m_defaultVirtualCamera.LookAt = m_lookAtGOTarget.transform;
                    m_defaultVirtualCamera.Follow = m_lookAtGOTarget.transform;
                }
            }
            else
            {
                m_isOrbiting = false;
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        CameraService.Instance.DefaultVirtualCamera.gameObject.SetActive(false);
        GameObject.Destroy(m_lookAtGOTarget);
    }

    private void RotateLookAtTargetAwayFromCamera()
    {
        // Rotate the object to point in the direction the camera is pointing
        Quaternion cameraRotation = m_defaultVirtualCamera.transform.rotation;
        Quaternion onlyConsiderYAxis = Quaternion.Euler(0f, cameraRotation.eulerAngles.y, 0f);
        m_lookAtGOTarget.transform.rotation = onlyConsiderYAxis;
    }
}
