
using Cinemachine;
using GLTFast.Loading;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for camera-related functionality
/// </summary>
public class CameraService : SingletonMono<CameraService>
{
    [SerializeField] private CinemachineBrain m_CinemachineBrain;

    [SerializeField] private CinemachineVirtualCamera m_focusedPartVirtualCamera = null;

    [SerializeField] private CinemachineVirtualCamera m_defaultVirtualCamera = null;
    [SerializeField] private CinemachineVirtualCamera m_presetVirtualCamera = null;

    [SerializeField] private GameObject m_defaultTarget = null;

    [SerializeField] private float m_distanceFromTargetMultiplier = 1.2f;
    private Camera m_mainCamera;
    private Vector3 m_currentFocusedPosition;
    private int m_currentSelectedPart;
    public Transform defaultFocusLocation;

    public float DistanceFromTargetMultiplier => m_distanceFromTargetMultiplier;
    public float MinDistanceFromTarget => 6f;
    public CinemachineVirtualCamera FocusedPartVirtualCamera => m_focusedPartVirtualCamera;
    public CinemachineVirtualCamera PresetVirtualCamera => m_presetVirtualCamera;
    public CinemachineVirtualCamera DefaultVirtualCamera => m_defaultVirtualCamera;

    public Transform cameraTransform;

    public GameObject DefaultTarget => m_defaultTarget;
    public Vector3 FocusedPosition
    {
        get => m_currentFocusedPosition;
        set => m_currentFocusedPosition = value;
    }

    public List<GameObject> itemsToFocus = new List<GameObject>();

    private Dictionary<Type, BaseCameraState> cameraStates = new Dictionary<Type, BaseCameraState>();
    protected void Awake()
    {
        m_mainCamera = Camera.main;
        cameraStates.Add(typeof(DefaultCameraViewState), new DefaultCameraViewState(this));
        cameraStates.Add(typeof(FreeFlyViewState), new FreeFlyViewState(this));
        cameraStates.Add(typeof(FocusedPartViewState), new FocusedPartViewState(this));
        cameraStates.Add(typeof(PresetFocusedViewState), new PresetFocusedViewState(this));

        ResetView();
     
    }

    private BaseCameraState currentCameraState;

    public bool IsCameraInTransit()
    {
        if(currentCameraState == null)
        {
            return false;
        }
        return (currentCameraState.GetType() != typeof(FreeFlyViewState));
    }

    public void SetDistanceFromTarget(float targetDisanceMultiplier)
    {
        m_distanceFromTargetMultiplier = targetDisanceMultiplier;
    }

    /// <inheritdoc/>
    public void ChangeState<TState>(Action<TState> OnInitialize = null) where TState : BaseCameraState
    {
        if (!cameraStates.TryGetValue(typeof(TState), out BaseCameraState newState))
        {
            throw new InvalidOperationException("Trying to go in a state not added to the State Machine.");
        }

        if (currentCameraState != null)
        {
            currentCameraState?.Exit();
        }

        currentCameraState = newState;
        OnInitialize?.Invoke(currentCameraState as TState);
        currentCameraState.Enter();
    }

    private void Start()
    {
        cameraTransform = transform;

    }

    public override void Update()
    {
        if (currentCameraState != null)
        {
            currentCameraState.Execute();
        }

        if (!m_CinemachineBrain.IsLiveInBlend(DefaultVirtualCamera))
        {
            if (currentCameraState is not FreeFlyViewState)
            {
                if (m_CinemachineBrain.ActiveVirtualCamera.Name != DefaultVirtualCamera.Name)
                {
                    ChangeState<FreeFlyViewState>();
                }
            }
        }
        float currentFOV = m_defaultVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
        m_presetVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = currentFOV;
        m_focusedPartVirtualCamera.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = currentFOV;
    }

    [ContextMenu(nameof(ResetView))]
    public void ResetView()
    {
        ChangeState<DefaultCameraViewState>();
    }

    [ContextMenu(nameof(FocusCamera))]
    public void FocusCamera()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, 1, 1);
        });
    }

    #region Camera Preset Views

    //--------------Isometric----------------------------------------------------

    //[ContextMenu(nameof(PresetViewIsometric))]
    //public void PresetViewIsometric()
    //{
     
    //    ChangeState<PresetFocusedViewState>(state =>
    //    {
    //        state.direction = new Vector3(-1, 1, 1);

    //    });
    //}

    [ContextMenu(nameof(PresetViewIsometricTSF))]
    public void PresetViewIsometricTSF()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, 1, 1);

        });
    }

    public void PresetViewIsometricBSF()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
           state.direction = new Vector3(-1, -1, 1);

        });
    }

    public void PresetViewIsometricTFP()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, 1, -1);

        });
    }

    public void PresetViewIsometricBFP()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, -1, -1);

        });
    }

    public void PresetViewIsometricTPA()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, 1, -1);
        });
    }


    public void PresetViewIsometricBPA()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, -1, -1);
        });
    }

 
    public void PresetViewIsometricTAS()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, 1, 1);
        });
    }

    public void PresetViewIsometricBAS()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, -1, 1);
        });
    }

    //---------------------------Sides-----------------------

    public void PresetViewIsometricTS()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(0, 1, 1);

        });
    }

    public void PresetViewIsometricBS()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(0, -1, 1);

        });
    }
    public void PresetViewIsometricTP()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(0, 1, -1);

        });
    }
    public void PresetViewIsometricBP()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(0, -1, -1);

        });
    }
    public void PresetViewIsometricTF()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, 1, 0);

        });
    }
    public void PresetViewIsometricBF()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, -1, 0);

        });
    }
    public void PresetViewIsometricTA()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, 1, 0);

        });
    }
    public void PresetViewIsometricBA()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, -1, 0);

        });
    }
    public void PresetViewIsometricFP()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, 0, -1);

        });
    }
    public void PresetViewIsometricFS()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(-1, 0, 1);

        });
    }
    public void PresetViewIsometricAP()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, 0, -1);

        });
    }
    public void PresetViewIsometricAS()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = new Vector3(1, 0, 1);

        });
    }


    //---------------------------Faces--------------------------


    [ContextMenu(nameof(PresetViewUp))]
    public void PresetViewUp()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = Vector3.up;
        });
    }
    [ContextMenu(nameof(PresetViewDown))]
    public void PresetViewDown()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = Vector3.down;
        });
    }
    [ContextMenu(nameof(PresetViewStarBoard))]
    public void PresetViewStarBoard()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = Vector3.forward;
        });
    }
    [ContextMenu(nameof(PresetViewPort))]
    public void PresetViewPort()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = Vector3.back;
        });
    }
    [ContextMenu(nameof(PresetViewForward))]
    public void PresetViewForward()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = Vector3.left;
        });
    }
    [ContextMenu(nameof(PresetViewAft))]
    public void PresetViewAft()
    {
        ChangeState<PresetFocusedViewState>(state =>
        {
            state.direction = Vector3.right;
        });
    }

    #endregion

    private bool IsCameraBlending()
    {
        return Vector3.Distance(m_CinemachineBrain.CurrentCameraState.RawPosition, m_CinemachineBrain.CurrentCameraState.FinalPosition) > 0.01f;
    }

    private bool HasUserMovedCamera()
    {
        // Left-mouse button to orbit
        // Right-mouse button to rotate
        // Middle-mouse button to pan
        // Scroll wheel to zoom
        return CameraUtils.IsMouseWithinViewport(m_mainCamera) && !MouseOverChecker.IsMouseOverAUIElement() && (/*Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) ||*/ Input.mouseScrollDelta.y != 0);
    } 
}
