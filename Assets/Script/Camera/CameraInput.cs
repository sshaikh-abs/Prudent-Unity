
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Largely based on <see cref="FreeCamera"/> including modifications to cater for custom interactions
/// Removed support for Input System Package as is currently not required
/// </summary>
public class CameraInputController : SingletonMono<CameraInputController>
{
    /// <summary>
    /// Rotation speed when using the mouse.
    /// </summary>
    [SerializeField] private float m_LookSpeedMouse = 4.0f;

    /// <summary>
    /// Movement speed.
    /// </summary>
    [SerializeField] private float m_MoveSpeed = 10.0f;

    /// <summary>
    /// Value added to the speed when incrementing.
    /// </summary>
    [SerializeField] private float m_MoveSpeedIncrement = 2.5f;

    [SerializeField] private float m_ScrollSpeedMultiplier = 1f;
    //[SerializeField] private float m_MousePanningMultiplier = 40f;

    /// <summary>
    /// Scale factor of the turbo mode.
    /// </summary>
    public float m_Turbo = 10.0f;

    private static string kMouseX = "Mouse X";
    private static string kMouseY = "Mouse Y";
    private static string kMouseScrollWheel = "Mouse ScrollWheel";
    //private static string kRightStickX = "Controller Right Stick X";
    //private static string kRightStickY = "Controller Right Stick Y";
    private static string kVertical = "Vertical";
    private static string kHorizontal = "Horizontal";

    //private static string kYAxis = "YAxis";
    //private static string kXAxis = "XAxis";
    //private static string kSpeedAxis = "Speed Axis";

    float inputRotateAxisX, inputRotateAxisY;
    float inputChangeSpeed;
    float inputVertical, inputHorizontal, inputYAxis;
    bool leftShiftBoost, leftShift, fire1;

    private Camera m_camera;

    private CinemachineVirtualCamera m_cinemachineOrbitCamera;

    public enum Tool { Default, Pan, Orbit, Zoom, LookAround }
    public Tool currentTool = Tool.Orbit;


    public Texture2D defaultCursor; // Default cursor
    public Texture2D panCursor;     // Pan cursor
    public Texture2D zoomCursor;    // Zoom cursor
    public Texture2D orbitCursor;   // Orbit cursor

    public Slider scrollSlider;
    public float currentZoomValue;


    public float panSpeed = 1f; // Tweak this based on zoom level or scene scale
    private Camera cam;

    private Vector3 lastMousePos;
    private bool isDragging = false;
    private void Awake()
    {
        m_camera = Camera.main;
        m_cinemachineOrbitCamera = GetComponent<CinemachineVirtualCamera>();

        cam = Camera.main;
        //Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    void UpdateInputs()
    {
        if (CameraService.Instance.IsCameraInTransit() || ArrowController.Instance.isDragging || (MarkupCreator.Instance.enableMarking && MarkupCreator.Instance.clickAndDrag))
        {
            return;
        }

        inputRotateAxisX = 0.0f;
        inputRotateAxisY = 0.0f;
        leftShiftBoost = false;
        fire1 = false;
        inputYAxis = 0;
        float dist = Vector3.Distance(CameraService.Instance.defaultFocusLocation.transform.position, transform.position);
        float scrollDelta = Input.GetAxis(kMouseScrollWheel);

        switch (currentTool)
        {
            case Tool.Default:
                break;
            case Tool.Orbit:

                // Left mouse button click for orbiting
                if (Input.GetMouseButton(0))
                {
                    if (m_cinemachineOrbitCamera != null && m_cinemachineOrbitCamera.LookAt != null)
                    {
                        Vector3 direction = m_cinemachineOrbitCamera.transform.position -
                                                   m_cinemachineOrbitCamera.LookAt.transform.position;

                        Vector3 horizontalVector = Vector3.Cross(m_cinemachineOrbitCamera.transform.up, direction);

                        // Horizontal orbit
                        m_cinemachineOrbitCamera.transform.RotateAround(m_cinemachineOrbitCamera.LookAt.transform.position, Vector3.up, GetMouseX() * m_LookSpeedMouse);

                        // Vertical orbit
                        m_cinemachineOrbitCamera.transform.RotateAround(m_cinemachineOrbitCamera.LookAt.transform.position, horizontalVector.normalized, GetMouseY() * m_LookSpeedMouse);
                    }
                }

                break;

            case Tool.Pan:
                if (Input.GetMouseButton(0)) // Left mouse button for panning
                {
                    //float fovMultiplier = m_cinemachineOrbitCamera.m_Lens.FieldOfView;


                    //if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
                    //{
                    //    m_MoveSpeed = 1;
                    //}
                    //else if (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
                    //{
                    //    m_MoveSpeed = 0.5f;
                    //}
                    //else
                    //{
                    //    m_MoveSpeed = 0.5f;
                    //}


                    //float inputHorizontal = GetMouseX() * m_MousePanningMultiplier * fovMultiplier;
                    //float inputVertical = GetMouseY() * m_MousePanningMultiplier * fovMultiplier;

                    //Vector3 movement = new Vector3(inputHorizontal, inputVertical, 0);

                    //// Adjust the movement based on the camera's field of view and distance
                    //movement = m_camera.transform.TransformDirection(movement); // Adjust the movement by camera rotation

                    //// Use the camera's position for panning
                    //float moveSpeed = Time.deltaTime * m_MoveSpeed;
                    //transform.position += movement * moveSpeed;


                    if (Input.GetMouseButtonDown(0)) // Middle mouse
                    {
                        lastMousePos = Input.mousePosition;
                        isDragging = true;
                    }

                    // End drag
                    if (Input.GetMouseButtonUp(0))
                    {
                        isDragging = false;
                    }

                    // While dragging
                    if (isDragging)
                    {
                        Vector3 mouseDelta = Input.mousePosition - lastMousePos;

                        // Convert screen movement to world movement
                        Vector3 move = -cam.transform.right * mouseDelta.x - cam.transform.up * mouseDelta.y;

                        // Scale it down to be reasonable across resolutions and FOV
                        float distance = GetStablePanDistance();
                        float scale = panSpeed * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;

                        transform.position += move * scale;

                        lastMousePos = Input.mousePosition;
                    }
                }

                else
                {
                    inputHorizontal = Input.GetAxis(kHorizontal) * m_MoveSpeed;
                    inputVertical = Input.GetAxis(kVertical) * m_MoveSpeed;
                }

                break;

            case Tool.Zoom:
                if (Input.GetMouseButton(0)) // Left mouse button for zooming
                {
                    float zoomAmount = GetMouseX() * m_ScrollSpeedMultiplier * Time.deltaTime;
                    float zoomAmountVerticle = GetMouseY() * m_ScrollSpeedMultiplier * Time.deltaTime;
                    //transform.position += transform.forward * (zoomAmount + zoomAmountVerticle)* (dist/100);

                    // Calculate the total zoom based on horizontal and vertical mouse movement
                    float totalZoom = zoomAmount + zoomAmountVerticle;

                    float newZoomVal = 80.5f - this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView - totalZoom * m_ScrollSpeedMultiplier;

                    // Adjust the camera's field of view (FOV) for zooming
                    // scrollSlider.value = totalZoom;
                    //this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Clamp(this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView - totalZoom*m_ScrollSpeedMultiplier, 2f, 60f);
                    scrollSlider.value = newZoomVal;

                }
                break;
            case Tool.LookAround:

                //Look Around
                if (Input.GetMouseButton(0))
                {
                    inputRotateAxisX = GetMouseX() * m_LookSpeedMouse;
                    inputRotateAxisY = GetMouseY() * m_LookSpeedMouse;
                }

                //Walk Around
                inputHorizontal = Input.GetAxis(kHorizontal) * m_MoveSpeed;
                inputVertical = Input.GetAxis(kVertical) * m_MoveSpeed;

                leftShiftBoost = Input.GetKey(KeyCode.LeftShift);
                break;
        }

        if (Input.GetMouseButton(2)) // Left mouse button for panning
        {
            // float fovMultiplier = -(CameraInputController.Instance.scrollSlider.normalizedValue * 1f);

            // float fovMultiplier = m_cinemachineOrbitCamera.m_Lens.FieldOfView;


            //if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
            //{
            //    m_MoveSpeed = 1;
            //}
            //else if(ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
            //{
            //    m_MoveSpeed = 0.5f;
            //}
            //else
            //{
            //    m_MoveSpeed = 0.5f;
            //}


            //float inputHorizontal = GetMouseX() * m_MousePanningMultiplier * fovMultiplier;
            //float inputVertical = GetMouseY() * m_MousePanningMultiplier * fovMultiplier;

            //Vector3 movement = new Vector3(inputHorizontal, inputVertical, 0);

            //// Adjust the movement based on the camera's field of view and distance
            //movement = m_camera.transform.TransformDirection(movement); // Adjust the movement by camera rotation

            //// Use the camera's position for panning
            //float moveSpeed = Time.deltaTime * m_MoveSpeed;
            //transform.position += movement ;


            if (Input.GetMouseButtonDown(2)) // Middle mouse
            {
                lastMousePos = Input.mousePosition;
                isDragging = true;
            }

            // End drag
            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }

            // While dragging
            if (isDragging)
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePos;

                // Convert screen movement to world movement
                Vector3 move = -cam.transform.right * mouseDelta.x - cam.transform.up * mouseDelta.y;

                // Scale it down to be reasonable across resolutions and FOV
                float distance = GetStablePanDistance();
                float scale = panSpeed * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;

                transform.position += move * scale;

                lastMousePos = Input.mousePosition;
            }
        }

        else
        {
            inputHorizontal = Input.GetAxis(kHorizontal) * m_MoveSpeed;
            inputVertical = Input.GetAxis(kVertical) * m_MoveSpeed;
        }

        float newScrollDelta = 80.5f - this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView + scrollDelta * m_ScrollSpeedMultiplier;
        // scrollDelta;

        if (Mathf.Abs(scrollDelta) > 0.01f) // Only zoom if there's a significant scroll input
        {
            // Adjust the camera's field of view (FOV) based on scroll wheel input
            //this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Clamp(newScrollDelta, 2f, 80f); // Adjust min and max FOV range
            //scrollSlider.value = this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
            scrollSlider.value = newScrollDelta;
            CommunicationManager.Instance.HandleZoomScrollValue_Extern(newScrollDelta.ToString());
        }


        //Debug.Log(newScrollData);

        //transform.position += transform.forward * newScrollData;
        currentZoomValue = this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;


    }


    float GetStablePanDistance()
    {
        Plane groundPlane = new Plane(-cam.transform.forward, m_cinemachineOrbitCamera.LookAt.transform.position);
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (groundPlane.Raycast(ray, out float hitDist))
            return hitDist;

        return 10f;
    }

    public void OnSliderChange()
    {
        ZoomSlider(scrollSlider.value);
    }

    public void ZoomSlider(float val)
    {
        //val = scrollSlider.value;  // The slider value will control the zoom
        this.gameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = 80.5f - val;  // Adjust the min and max FOV range as per your requirement
        //}
    }


    bool focus = false;

    private void OnApplicationFocus(bool focus)
    {
        this.focus = focus;
        if (!focus)
        {
            keepInput0 = true;
        }
    }

    bool keepInput0 = false;

    private float GetMouseX()
    {
        if (keepInput0)
        {
            return 0;
        }

        return Mathf.Clamp(Input.GetAxisRaw(kMouseX), -2, 2);
    }
    private float GetMouseY()
    {
        if (keepInput0)
        {
            return 0;
        }
        return Mathf.Clamp(Input.GetAxisRaw(kMouseY), -2, 2);
    }

    void LateUpdate()
    {
        if (keepInput0 && focus)
        {
            keepInput0 = false;
        }

        if (!CameraUtils.IsMouseWithinViewport(m_camera))
        {
            return;
        }

        //if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.Escape))
        //{
        //    SetCursorToDefault();
        //}

        if (Input.GetKey(KeyCode.Escape))
        {
            SetCursorToDefault();
        }

        UpdateInputs();

        if (inputChangeSpeed != 0.0f)
        {
            m_MoveSpeed += inputChangeSpeed * m_MoveSpeedIncrement;
            if (m_MoveSpeed < m_MoveSpeedIncrement) m_MoveSpeed = m_MoveSpeedIncrement;
        }


        bool moved = inputRotateAxisX != 0.0f || inputRotateAxisY != 0.0f || inputVertical != 0.0f ||
                     inputHorizontal != 0.0f || inputYAxis != 0.0f;
        if (moved)
        {
            float rotationX = transform.localEulerAngles.x;
            float newRotationY = transform.localEulerAngles.y + inputRotateAxisX;

            // Weird clamping code due to weird Euler angle mapping...
            float newRotationX = (rotationX - inputRotateAxisY);
            if (rotationX <= 90.0f && newRotationX >= 0.0f)
                newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
            if (rotationX >= 270.0f)
                newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

            transform.localRotation =
                Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

            float moveSpeed = Time.deltaTime * m_MoveSpeed;
            if (fire1 || leftShiftBoost && leftShift)
            {
                moveSpeed *= m_Turbo;
            }

            transform.position += transform.forward * moveSpeed * inputVertical;
            transform.position += transform.right * moveSpeed * inputHorizontal;
            transform.position += Vector3.up * moveSpeed * inputYAxis;
        }
    }

    public void SetToolToPan()
    {
        currentTool = Tool.Pan;
        //Cursor.SetCursor(panCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetToolToLookAround()
    {
        currentTool = Tool.LookAround;
    }

    public void SetToolToOrbit()
    {
        currentTool = Tool.Orbit;
        //Cursor.SetCursor(orbitCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetToolToZoom()
    {
        currentTool = Tool.Zoom;
        //Cursor.SetCursor(zoomCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetCursorToDefault()
    {
        currentTool = Tool.Default;
        //Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }



}

/// <summary>
/// Useful camera utilities
/// </summary>
public static class CameraUtils
{
    /// <summary>
    /// Returns true if the mouse is within the cameras viewport
    /// </summary>
    /// <param name="camera"></param>
    public static bool IsMouseWithinViewport(Camera camera)
    {
        Vector3 viewportPosition = camera.ScreenToViewportPoint(Input.mousePosition);
        return viewportPosition.x is >= 0f and <= 1f && viewportPosition.y is >= 0f and <= 1f;
    }
}

