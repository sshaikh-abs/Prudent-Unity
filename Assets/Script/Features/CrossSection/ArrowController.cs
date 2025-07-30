using UnityEngine;

public class ArrowController : SingletonMono<ArrowController>
{
    [System.Serializable]
    public class DraggablePlane
    {
        public Transform plane;
        public Axis dragAxis;
        public Vector2 minMax;
        public GameObject arrow;
        public GameObject arrowSpriteGO;

        [HideInInspector] public Renderer planeRenderer;
        [HideInInspector] public Color originalPlaneColor;
        [HideInInspector] public Color originalSpriteColor;
    }

    public enum Axis { X, Y, Z }

    public DraggablePlane[] planes;

    public Color hoverPlaneColor = Color.blue;
    public Color selectedPlaneColor = Color.magenta;
    public Color hoverSpriteColor = Color.blue;
    public Color selectedSpriteColor = Color.magenta;

    private DraggablePlane selectedPlane;
    private DraggablePlane hoveredPlane;
    private float currentArrowScale = 1f;

    private Plane dragPlane;
    private Vector3 offset;
    public bool isDragging;

    void Start()
    {
        foreach (var box in planes)
        {
            if (box.plane != null)
            {
                box.planeRenderer = box.plane.GetComponent<Renderer>();
                box.originalPlaneColor = box.planeRenderer.material.color;
                box.plane.gameObject.SetActive(false);
            }

            if (box.arrow != null) box.arrow.SetActive(false);
            if (box.arrowSpriteGO != null)
            {
                var sr = box.arrowSpriteGO.GetComponent<SpriteRenderer>();
                if (sr != null) box.originalSpriteColor = sr.color;
                box.arrowSpriteGO.SetActive(false);
            }
        }

        SetArrowScale(currentArrowScale);
    }

    public override void Update()
    {
        HandleHoverAndSelection();

        if (isDragging && selectedPlane != null)
            DragSelectedPlane();

        if (Input.GetMouseButtonUp(0)) isDragging = false;

        UpdateArrowTransforms();
    }

    private void HandleHoverAndSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        DraggablePlane hitPlane = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            foreach (var box in planes)
            {
                if (hit.transform == box.plane)
                {
                    hitPlane = box;
                    break;
                }
            }
        }

        if (!isDragging && hoveredPlane != hitPlane)
        {
            if (hoveredPlane != null && hoveredPlane != selectedPlane)
                SetColors(hoveredPlane, hoveredPlane.originalPlaneColor, hoveredPlane.originalSpriteColor);

            if (hitPlane != null && hitPlane != selectedPlane)
                SetColors(hitPlane, hoverPlaneColor, hoverSpriteColor);

            hoveredPlane = hitPlane;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (hitPlane != null)
            {
                if (selectedPlane != null && selectedPlane != hitPlane)
                    SetColors(selectedPlane, selectedPlane.originalPlaneColor, selectedPlane.originalSpriteColor);

                selectedPlane = hitPlane;
                hoveredPlane = null;

                SetColors(selectedPlane, selectedPlaneColor, selectedSpriteColor);

                Vector3 normal = GetPlaneNormalForAxis(selectedPlane.dragAxis);
                dragPlane = new Plane(normal, selectedPlane.plane.position);

                if (dragPlane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);
                    offset = selectedPlane.plane.position - hitPoint;
                    isDragging = true;
                }
            }
            else
            {
                DeselectPlane();
            }
        }
    }

    private void DragSelectedPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 targetPosition = hitPoint + offset;
            Vector3 newPosition = selectedPlane.plane.position;

            switch (selectedPlane.dragAxis)
            {
                case Axis.X: newPosition.x = Mathf.Clamp(targetPosition.x, selectedPlane.minMax.x, selectedPlane.minMax.y); break;
                case Axis.Y: newPosition.y = Mathf.Clamp(targetPosition.y, selectedPlane.minMax.x, selectedPlane.minMax.y); break;
                case Axis.Z: newPosition.z = Mathf.Clamp(targetPosition.z, selectedPlane.minMax.x, selectedPlane.minMax.y); break;
            }

            selectedPlane.plane.position = newPosition;
        }
    }

    private void DeselectPlane()
    {
        if (selectedPlane != null)
        {
            SetColors(selectedPlane, selectedPlane.originalPlaneColor, selectedPlane.originalSpriteColor);
            selectedPlane = null;
        }

        if (hoveredPlane != null)
        {
            SetColors(hoveredPlane, hoveredPlane.originalPlaneColor, hoveredPlane.originalSpriteColor);
            hoveredPlane = null;
        }
    }

    private void UpdateArrowTransforms()
    {
        foreach (var box in planes)
        {
            bool isActive = box.plane.gameObject.activeInHierarchy;

            if (box.arrow != null)
            {
                box.arrow.SetActive(isActive);

                if (isActive)
                {
                    box.arrow.transform.position = box.plane.position;
                    box.arrow.transform.localScale = GetFlippedScale(box.dragAxis, currentArrowScale);
                }
            }

            if (box.arrowSpriteGO != null)
            {
                box.arrowSpriteGO.SetActive(isActive);
                if (isActive)
                    box.arrowSpriteGO.transform.position = box.plane.position;
            }
        }
    }

    private Vector3 GetFlippedScale(Axis axis, float baseScale)
    {
        float flip = 1f;
        switch (axis)
        {
            case Axis.X:
                flip = ShipBoundsComputer.Instance.xPlaneInverted ? -1f : 1f;
                break;
            case Axis.Y:
                flip = ShipBoundsComputer.Instance.yPlaneInverted ? -1f : 1f;
                break;
            case Axis.Z:
                flip = ShipBoundsComputer.Instance.zPlaneInverted ? -1f : 1f;
                break;
        }
        return new Vector3(baseScale, baseScale, baseScale * flip);
    }

    private Vector3 GetPlaneNormalForAxis(Axis axis)
    {
        return axis switch
        {
            Axis.X => Vector3.up,
            Axis.Y => Vector3.forward,
            Axis.Z => Vector3.up,
            _ => Vector3.up,
        };
    }

    public void SetArrowScale(float scale)
    {
        currentArrowScale = scale;

        foreach (var box in planes)
        {
            if (box.arrow == null) continue;
            box.arrow.transform.localScale = GetFlippedScale(box.dragAxis, scale);
        }
    }

    private void SetColors(DraggablePlane box, Color planeColor, Color spriteColor)
    {
        if (box.planeRenderer != null)
            box.planeRenderer.material.color = planeColor;

        if (box.arrowSpriteGO != null)
        {
            SpriteRenderer sr = box.arrowSpriteGO.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = spriteColor;
        }
    }

    [ContextMenu("Test FlipPlane")]
    public void flipSelectedPlane()
    {
        if (selectedPlane == null) return;
        ShipBoundsComputer.Instance.FlipPlane(selectedPlane.plane.name);
       // UpdateArrowTransforms();
    }
}