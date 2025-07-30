using Cinemachine;
using UnityEngine;

/// <summary>
/// State that's used when a part is focused on
/// </summary>
public class FocusedPartViewState : BaseCameraState
{
    private GameObject m_lookAtGOTarget;

    private CinemachineVirtualCamera m_focusCamera;
    private CinemachineVirtualCamera m_defaultCamera;

    private Transform m_focusedCameraTransform;
    private Transform m_defaultCameraTransform;

    private CinemachineTransposer m_focusCameraTransposer;

    public FocusedPartViewState(CameraService cameraService)
    {
        // Cache the reference to avoid using these long names
        m_focusCamera = CameraService.Instance.FocusedPartVirtualCamera;
        m_defaultCamera = CameraService.Instance.DefaultVirtualCamera;
        m_focusedCameraTransform = m_focusCamera.transform;
        m_defaultCameraTransform = m_defaultCamera.transform;

        m_focusCameraTransposer = m_focusCamera.GetCinemachineComponent<CinemachineTransposer>();

        // Create a placeholder transform to use as our LookAt and Follow targets
        // We do this because all parts in Unity are at (0,0,0) so we cannot rely on their Transform data.
        // We rely on their Renderer.bounds to determine where the camera should focus on
        m_lookAtGOTarget = new GameObject("FocusedCameraLookAtRef");
        m_lookAtGOTarget.transform.localPosition = Vector3.zero;
        m_lookAtGOTarget.transform.SetParent(m_focusedCameraTransform.parent);
        m_lookAtGOTarget.transform.localPosition = Vector3.zero;
    }

    public override void Enter()
    {
        base.Enter();
        CameraService.Instance.FocusedPartVirtualCamera.gameObject.SetActive(true);

        float objectSize = m_focusedObjectsCombinedBounds.size.magnitude;
        float desiredDistanceFromObject = objectSize * CameraService.Instance.DistanceFromTargetMultiplier;
        desiredDistanceFromObject = Mathf.Max(desiredDistanceFromObject, CameraService.Instance.MinDistanceFromTarget);
        // Position the LookAt target at the center of the bounds
        m_lookAtGOTarget.transform.position = m_focusedObjectsCombinedBounds.center;

        RotateLookAtTargetAwayFromCamera();
        AssignLookAtAndFollowTarget();

        // Set the follow offset distance
        m_focusCameraTransposer.m_FollowOffset =
            m_focusCameraTransposer.m_FollowOffset.normalized * desiredDistanceFromObject;

        m_focusCamera.gameObject.SetActive(true);
        m_defaultCamera.gameObject.SetActive(false);
    }

    private void RotateLookAtTargetAwayFromCamera()
    {
        // Rotate the object to point in the direction the camera is pointing
        Quaternion cameraRotation = m_defaultCameraTransform.rotation;
        Quaternion onlyConsiderYAxis = Quaternion.Euler(0f, cameraRotation.eulerAngles.y, 0f);
        m_lookAtGOTarget.transform.rotation = onlyConsiderYAxis;
    }

    public override void Exit()
    {
        base.Exit();

        // Make sure the default view is starting where the focus view was pointing
        m_defaultCameraTransform.position = m_focusedCameraTransform.position;
        m_defaultCameraTransform.rotation =
            Quaternion.LookRotation(m_lookAtGOTarget.transform.position - m_focusedCameraTransform.position);

        // We need the default camera's lookat to be the set to the last lookat that was focused
        m_defaultCamera.LookAt = m_lookAtGOTarget.transform;
    }

    private void AssignLookAtAndFollowTarget()
    {
        SetFocusedCameraToDefaultCameraView();
        m_focusCamera.LookAt = m_lookAtGOTarget.transform;
        m_focusCamera.Follow = m_lookAtGOTarget.transform;
    }

    private void SetFocusedCameraToDefaultCameraView()
    {
        m_focusedCameraTransform.position = m_defaultCameraTransform.position;
        m_focusedCameraTransform.rotation =
            Quaternion.LookRotation(m_lookAtGOTarget.transform.position - m_defaultCameraTransform.position);
    }
}
