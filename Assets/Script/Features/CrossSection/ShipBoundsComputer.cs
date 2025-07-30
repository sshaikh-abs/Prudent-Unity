using System.Collections.Generic;
using UnityEngine;

public class ShipBoundsComputer : SingletonMono<ShipBoundsComputer>
{
    //public Transform offset;
    public bool crossSectionEnabled = true;
    public bool xPlaneEnabled = true;
    public bool yPlaneEnabled = true;
    public bool zPlaneEnabled = true;
    public bool xPlaneInverted = false;
    public bool yPlaneInverted = false;
    public bool zPlaneInverted = false;
    public Vector3 offset;
    public float padding = 15f;
    public float Dispadding = 20f;
    public float boundsPadding;
    public Bounds shipBounds;
    public Transform xPlane;
    public Transform yPlane;
    public Transform zPlane;

    public ArrowController arrowController;
    public Bounds currentBounds;

    void Start()
    {
        GameEvents.OnPostVesselLoaded += OnPostVesselLoaded;
        GameEvents.OnCompartmentIsolated += OnCompartmentIsolated;
        GameEvents.OnVesselView += OnVesselView;
    }

    public void controlSectionTool(bool posz,bool posy, bool posx)
    {
        zPlaneEnabled = posz;
        yPlaneEnabled  = posy;
        xPlaneEnabled = posx;
    }

    public void FlipPlane(string name)
    {
        switch (name)
        {
            case "X":
                zPlaneInverted = !zPlaneInverted;
                break;

            case "Y":
                yPlaneInverted = !yPlaneInverted;
                break;

            case "Z":
                xPlaneInverted = !xPlaneInverted;
                break;
            default:
                Debug.Log("Error: Unknown Plane");
                break;
        }
    }

    private void OnVesselView()
    {
        currentBounds = shipBounds;
        SetCuttersToBounds(shipBounds);
        ArrowController.Instance.SetArrowScale(1f);
    }

    private void OnCompartmentIsolated(Compartment compartment)
    {
        Bounds compartmentBounds = compartment.compartmentMeshObjectReference.GetComponent<Renderer>().bounds;
        float sizeRatio = compartmentBounds.size.magnitude * (boundsPadding / 100f);
        compartmentBounds.Expand(sizeRatio);
        currentBounds = compartmentBounds;
        SetCuttersToBounds(compartmentBounds);
        ArrowController.Instance.SetArrowScale(0.4f);
    }

    private void OnPostVesselLoaded(bool loadedComplexShip)
    {
        var renderers = GroupingManager.Instance.vesselObject.GetCompartments(c => c.compartmentMeshObjectReference.GetComponent<Renderer>());
        shipBounds = CombineBounds(renderers);
        shipBounds.Expand(boundsPadding);
    }

    public void SetCuttersToBounds(Bounds bounds)
    {
        offset = bounds.center;

        xPlane.localScale = new Vector3(bounds.size.z + padding, bounds.size.y + padding, 0.01f);
        yPlane.localScale = new Vector3(bounds.size.x + padding, bounds.size.z + padding, 0.01f);
        zPlane.localScale = new Vector3(bounds.size.x + padding, bounds.size.y + padding, 0.01f);

        arrowController.planes[0].minMax = new Vector2(bounds.center.x - (bounds.size.x / 2f) - Dispadding, bounds.center.x + (bounds.size.x / 2f) + Dispadding);
        arrowController.planes[1].minMax = new Vector2(bounds.center.y - (bounds.size.y / 2f) - Dispadding, bounds.center.y + (bounds.size.y / 2f) + Dispadding);
        arrowController.planes[2].minMax = new Vector2(bounds.center.z - (bounds.size.z / 2f) - Dispadding, bounds.center.z + (bounds.size.z / 2f) + Dispadding);

        ResetPlanes();
    }

    public void ResetPlanes()
    {
        xPlaneInverted = false;
        yPlaneInverted = false;
        zPlaneInverted = false;

        Vector3 xPlanePosition = currentBounds.center;
        xPlanePosition.x = arrowController.planes[0].minMax.x;

        Vector3 yPlanePosition = currentBounds.center;
        yPlanePosition.y = arrowController.planes[1].minMax.y;

        Vector3 zPlanePosition = currentBounds.center;
        zPlanePosition.z = arrowController.planes[2].minMax.y;

        xPlane.position = xPlanePosition;
        yPlane.position = yPlanePosition;
        zPlane.position = zPlanePosition;

    }

    public override void Update()
    {
        offset.x = xPlane.position.x;
        offset.y = yPlane.position.y;
        offset.z = zPlane.position.z;

        Shader.SetGlobalVector("_CubePos", -offset);
        Shader.SetGlobalFloat("_xPlaneEnabled", xPlaneEnabled ? (crossSectionEnabled ? 1.0f : 0.0f) : 0.0f);
        Shader.SetGlobalFloat("_xPlaneInverted", xPlaneInverted ? 1.0f : 0.0f);
        Shader.SetGlobalFloat("_yPlaneEnabled", yPlaneEnabled ? (crossSectionEnabled ? 1.0f : 0.0f) : 0.0f);
        Shader.SetGlobalFloat("_yPlaneInverted", yPlaneInverted ? 0.0f : 1.0f);
        Shader.SetGlobalFloat("_zPlaneEnabled", zPlaneEnabled ? (crossSectionEnabled ? 1.0f : 0.0f) : 0.0f);
        Shader.SetGlobalFloat("_zPlaneInverted", zPlaneInverted ? 0.0f : 1.0f);
        Shader.SetGlobalVector("_BoundsMin", shipBounds.min);
        Shader.SetGlobalVector("_BoundsMax", shipBounds.max);

        xPlane.gameObject.SetActive(xPlaneEnabled && crossSectionEnabled);
        yPlane.gameObject.SetActive(yPlaneEnabled && crossSectionEnabled);
        zPlane.gameObject.SetActive(zPlaneEnabled && crossSectionEnabled);
    }

    public bool IsVectorOnTheVisibleSide(Vector3 vector)
    {
        Vector3 localPos = transform.InverseTransformPoint(vector);
        Vector3 localBoundsMin = transform.InverseTransformPoint(shipBounds.min);
        Vector3 localBoundsMax = transform.InverseTransformPoint(shipBounds.max);

        bool isInBounds = (localPos.x >= localBoundsMin.x && localPos.x <= localBoundsMax.x) &&
               (localPos.y >= localBoundsMin.y && localPos.y <= localBoundsMax.y) &&
               (localPos.z >= localBoundsMin.z && localPos.z <= localBoundsMax.z);

        bool isOnVisibleSideOfXPlane = IsOnNormalSide(vector, xPlane.position, Vector3.right * (xPlaneInverted ? -1f : 1f));
        bool isOnVisibleSideOfYPlane = IsOnNormalSide(vector, yPlane.position, Vector3.up * (yPlaneInverted ? 1f : -1f));
        bool isOnVisibleSideOfZPlane = IsOnNormalSide(vector, zPlane.position, Vector3.forward * (zPlaneInverted ? 1f : -1f));

        isOnVisibleSideOfXPlane |= !xPlaneEnabled;
        isOnVisibleSideOfYPlane |= !yPlaneEnabled;
        isOnVisibleSideOfZPlane |= !zPlaneEnabled;

        return isInBounds && isOnVisibleSideOfXPlane && isOnVisibleSideOfYPlane && isOnVisibleSideOfZPlane;
    }

    public static bool IsOnNormalSide(Vector3 point, Vector3 planeOrigin, Vector3 normal)
    {
        Vector3 toPoint = point - planeOrigin;
        return Vector3.Dot(toPoint, normal) >= 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(shipBounds.center, shipBounds.size);
    }

    private static Bounds CombineBounds(List<Renderer> renderers)
    {
        Bounds combinedBounds = new Bounds();
        bool boundsInitialized = false;

        foreach (Renderer r in renderers)
        {
            if (r != null && r.bounds.size != Vector3.zero)
            {
                if (!boundsInitialized)
                {
                    combinedBounds = r.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(r.bounds);
                }
            }
        }

        return combinedBounds;
    }
}