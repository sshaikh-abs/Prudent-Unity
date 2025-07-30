using Cinemachine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PresetFocusedViewState : BaseCameraState
{
    private GameObject m_lookAtGOTarget;

    private CinemachineVirtualCamera m_presetCamera;
    private CinemachineVirtualCamera m_defaultCamera;

    private Transform m_presetCameraTransform;
    private Transform m_defaultCameraTransform;

    private CinemachineTransposer m_focusCameraTransposer;

    public Vector3 direction;

    public Dictionary<Vector3, Quaternion> lookAtLUT = new Dictionary<Vector3, Quaternion>()
    {
        { Vector3.up, Quaternion.LookRotation(Vector3.down, Vector3.back) },
        { Vector3.down, Quaternion.LookRotation(Vector3.up, Vector3.forward) },
        { Vector3.left, Quaternion.LookRotation(Vector3.right, Vector3.up) },
        { Vector3.right, Quaternion.LookRotation(Vector3.left, Vector3.up) },
        { Vector3.forward, Quaternion.LookRotation(Vector3.back, Vector3.up) },
        { Vector3.back, Quaternion.LookRotation(Vector3.forward, Vector3.up) },

        { new Vector3(-1, 1, 1), Quaternion.Euler(35.264f, 135f, 0f) },
        { new Vector3(-1, -1, 1), Quaternion.Euler(-35.264f, 135f, 0f) },
        { new Vector3(-1, 1, -1), Quaternion.Euler(35.264f, 45f, 0f) },
        { new Vector3(-1, -1, -1), Quaternion.Euler(-35.264f, 45f, 0f) },
        { new Vector3( 1, 1, -1), Quaternion.Euler(35.264f, 315f, 0f)},
        { new Vector3(1, -1, -1), Quaternion.Euler(-35.264f, 315f, 0f) },
        { new Vector3(1, 1, 1), Quaternion.Euler(35.264f, 225f, 0f)  },
        { new Vector3(1, -1, 1), Quaternion.Euler(-35.264f, 225f, 0f) },

        { new Vector3(0, 1, 1), Quaternion.Euler(45f, 180f, 0f) },
        { new Vector3(0, -1, 1), Quaternion.Euler(-45f, 180f, 0f) },

        { new Vector3(0, 1, -1), Quaternion.Euler(45f, 0f, 0f) },
        { new Vector3(0, -1, -1), Quaternion.Euler(-45f, 0f, 0f) },

        { new Vector3(-1, 1, 0), Quaternion.Euler(45f, 90f, 0f) },
        { new Vector3(-1, -1, 0), Quaternion.Euler(-45f, 90f, 0f) },

        { new Vector3(1, 1, 0), Quaternion.Euler(45f, 270f, 0f) },
        { new Vector3(1, -1, 0), Quaternion.Euler(-45f, 270f, 0f) },

        {new Vector3(-1, 0, -1), Quaternion.Euler(0f, 45f, 0f) },
        {new Vector3(-1, 0, 1), Quaternion.Euler(0f, 135f, 0f) },

        {new Vector3(1, 0, -1), Quaternion.Euler(0f, -45f, 0f) },
        { new Vector3(1, 0, 1), Quaternion.Euler(0f, -135f, 0f) }
    };

    public PresetFocusedViewState(CameraService cameraService)
    {
        // Cache the reference to avoid using these long names
        m_presetCamera = CameraService.Instance.PresetVirtualCamera;
        m_defaultCamera = CameraService.Instance.DefaultVirtualCamera;
        m_presetCameraTransform = m_presetCamera.transform;
        m_defaultCameraTransform = m_defaultCamera.transform;

        m_focusCameraTransposer = m_presetCamera.GetCinemachineComponent<CinemachineTransposer>();

        // Create a placeholder transform to use as our LookAt and Follow targets
        // We do this because all parts in Unity are at (0,0,0) so we cannot rely on their Transform data.
        // We rely on their Renderer.bounds to determine where the camera should focus on
        m_lookAtGOTarget = new GameObject("PresetCameraLookAtRef");
        m_lookAtGOTarget.transform.localPosition = Vector3.zero;
        m_lookAtGOTarget.transform.SetParent(m_presetCameraTransform.parent);
        m_lookAtGOTarget.transform.localPosition = Vector3.zero;
    }

    public override void Enter()
    {
        base.Enter();
        CameraService.Instance.PresetVirtualCamera.gameObject.SetActive(true);

        float objectSize = m_focusedObjectsCombinedBounds.size.magnitude;
        float desiredDistanceFromObject = objectSize * CameraService.Instance.DistanceFromTargetMultiplier;
        desiredDistanceFromObject = Mathf.Max(desiredDistanceFromObject, CameraService.Instance.MinDistanceFromTarget);
        // Position the LookAt target at the center of the bounds
        m_lookAtGOTarget.transform.position = m_focusedObjectsCombinedBounds.center;

        AssignLookAtAndFollowTarget();

        m_focusCameraTransposer.m_FollowOffset = direction.normalized * desiredDistanceFromObject;

        m_presetCamera.gameObject.SetActive(true);
        m_defaultCamera.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Exit();

        // Make sure the default view is starting where the focus view was pointing
        m_defaultCameraTransform.position = m_presetCameraTransform.position;
        m_defaultCameraTransform.rotation = m_presetCamera.transform.rotation;

        // We need the default camera's lookat to be the set to the last lookat that was focused
        m_defaultCamera.LookAt = m_lookAtGOTarget.transform;
    }

    private void AssignLookAtAndFollowTarget()
    {
        //m_presetCamera.LookAt = m_lookAtGOTarget.transform;
        m_presetCamera.Follow = m_lookAtGOTarget.transform;
        m_presetCamera.transform.rotation = lookAtLUT[direction];
    }
}
