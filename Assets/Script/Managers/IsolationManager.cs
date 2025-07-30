using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IsolationManager : SingletonMono<IsolationManager>
{
    public bool enableDebug = false;
    //public bool isIsolationEnabled => intendedCompartment != null;

    //public Vector3 sizeOffset;
    public Vector3 cubePosition = Vector3.zero;
    public Vector3 cubeScale = new Vector3(400, 50, 60);

    //public Transform intendedCompartment = null;

    //public void FocusCompartment()
    //{
    //    if (intendedCompartment == null)
    //    {
    //        return;
    //    }

    //    intendedCompartment = intendedCompartment.transform.GetChild(0);

    //    if (intendedCompartment.GetComponent<MeshRenderer>() == null)
    //    {
    //        return;
    //    }

    //    cubePosition = intendedCompartment.GetComponent<MeshRenderer>().bounds.center;
    //    cubeScale = intendedCompartment.GetComponent<MeshRenderer>().bounds.size + sizeOffset;
    //}

    private void Start()
    {
        Physics.queriesHitBackfaces = true;
    }

    private void OnDrawGizmos()
    {
        int index = 0;
        foreach (var item in compartmentHits)
        {
            if(index == 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(item.point, 1f);
            }
            else
            {

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(item.point, 0.5f);
            }
            index++;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if (enableDebug)
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawWireCube(cubePosition, cubeScale);
    //    }

    //    if (!Application.isPlaying)
    //    {
    //        ExcuteIsolate();
    //    }
    //}

    //public void ExcuteIsolate()
    //{
    //    FocusCompartment();
    //}

    [ContextMenu(nameof(ResetIsolation))]
    public void ResetIsolation()
    {
        //intendedCompartment = null;
        cubePosition = Vector3.zero;
        cubeScale = new Vector3(1300f, 1300f, 1300f);
    }

    private List<Collider> currentlyDisabledObjects = new List<Collider>();
    private List<Collider> currentlyEnabledObjects = new List<Collider>();

    public void ResetPartActivation()
    {
        currentlyDisabledObjects.ForEach(collider => { collider.gameObject.SetActive(true); });
        currentlyDisabledObjects.Clear();
        currentlyEnabledObjects.Clear();
    }

    private RaycastHit[] hits = new RaycastHit[0];
    private RaycastHit[] compartmentHits = new RaycastHit[0];
    private RaycastHit[] hullpartHits = new RaycastHit[0];
    private RaycastHit[] subPartHits = new RaycastHit[0];

    private GameObject previousIntendedGameObject = null;

    public List<GameObject> hitObjects;

    private void UpdateHits(Ray ray)
    {

        hits = Physics.RaycastAll(ray);
        hits = hits.OrderBy(h => Vector3.Distance(h.point, ray.origin)).ToArray();
        //hits = hits.OrderBy(h => h.distance).ToArray();

        //int planesLayerMask = LayerMask.GetMask("Plane");
        //if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, planesLayerMask))
        //{
        //    //ray = new Ray(hit.point, ray.direction);
        //    compartmentHits = new RaycastHit[0];
        //    hullpartHits = new RaycastHit[0];
        //    subPartHits = new RaycastHit[0];
        //    //return;
        //}

        Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.green);

        int hullpartLayerMask = LayerMask.GetMask("Frames", "Focus", "GaugePoint", "Markup", "DocumentIcon", "Plane");
        int compartmentLayerMask = -1;
        if (ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState))
        {
            compartmentLayerMask = LayerMask.GetMask("Default");
        }
        else
        {
            compartmentLayerMask = LayerMask.GetMask("Compartments", "Focus", "GaugePoint", "Markup", "DocumentIcon", "Plane");
        }
        int subPartLayerMask = LayerMask.GetMask("Default", "Focus", "GaugePoint", "Markup", "DocumentIcon","Plane");

        compartmentHits = Physics.RaycastAll(ray, Mathf.Infinity, compartmentLayerMask);
        //compartmentHits = compartmentHits.OrderBy(h => Vector3.Distance(h.point, ray.origin)).ToArray();
        compartmentHits = compartmentHits.OrderBy(h => h.distance).ToArray();

        hullpartHits = Physics.RaycastAll(ray, Mathf.Infinity, hullpartLayerMask);
        hullpartHits = hullpartHits.OrderBy(h => h.distance).ToArray();
        //hullpartHits = hullpartHits.OrderBy(h => h.distance).ToArray();


        subPartHits = Physics.RaycastAll(ray, Mathf.Infinity, subPartLayerMask);
        subPartHits = subPartHits.OrderBy(h => h.distance).ToArray();
        //subPartHits = subPartHits.OrderBy(h => h.distance).ToArray();

        //Debug.Log("Compartment Hits : " + compartmentHits.Length);
        //if (hullpartHits.Count() > 0)
        //{
        //    Debug.Log("Hullparts hit count : " + hullpartHits.Count());
        //}
        //if (compartmentHits.Count() > 0)
        //{
        //    string hits = "";

        //    foreach (var item in compartmentHits)
        //    {
        //        hits += item.collider.gameObject.name + " || ";
        //    }
        //    Debug.Log("Compartment hit count : " + compartmentHits.Count() + " || " + hits);
        //}
    }

    public override void Update()
    {
        Ray ray = CameraService.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        UpdateHits(ray);
        ManageMouseEvents();
    }

    private void ManageMouseEvents()
    {
        GameObject currentIntendedGameObject = GetIntendedGameObject();

        if (previousIntendedGameObject != currentIntendedGameObject)
        {
            if (previousIntendedGameObject != null)
            {
                previousIntendedGameObject.GetComponent<OutlineSelectionHandler>()?._OnMouseExit();
            }

            if (currentIntendedGameObject != null)
            {
                currentIntendedGameObject.GetComponent<OutlineSelectionHandler>()?._OnMouseEnter();
            }

            previousIntendedGameObject = currentIntendedGameObject;
        }

        bool InputRecieve = Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0);

        if (InputRecieve)
        {
            if (previousIntendedGameObject != null)
            {
                previousIntendedGameObject.GetComponent<OutlineSelectionHandler>()?._OnMouseDown(isFromRightMouse: Input.GetKeyDown(KeyCode.Mouse1));
            }
        }
    }

    public GameObject GetIntendedGameObject()
    {
        if (CaptureRayCastHit(out RaycastHit capturedHit) && !MouseOverChecker.IsMouseOverAUIElement())
        {
            if (LayerMask.LayerToName(capturedHit.collider.gameObject.layer).GetHashCode() != ("GaugePoint").GetHashCode())
            {
                return capturedHit.collider.gameObject;
            }
        }

        return null;
    }

    public bool CaptureRayCastHitOnCompartments(out RaycastHit raycastHit)
    {
        return CaptureRayCastHitFromCollection(compartmentHits, out raycastHit);
    }

    public bool CaptureRayCastHitOnHullParts(out RaycastHit raycastHit)
    {
        return CaptureRayCastHitFromCollection(hullpartHits, out raycastHit);
    }

    public bool CaptureRayCastHitOnSubParts(out RaycastHit raycastHit)
    {
        return CaptureRayCastHitFromCollection(subPartHits, out raycastHit);
    }

    public bool CaptureRayCastHitFromCollection(RaycastHit[] collection, out RaycastHit capturedHit)
    {
        foreach (var hit in collection)
        {
            Bounds bounds = new Bounds()
            {
                center = cubePosition,
                extents = cubeScale / 2f
            };

            bool isInBounds = bounds.Contains(hit.point);
            //bool isInBounds = ShipBoundsComputer.Instance.IsVectorOnTheVisibleSide(hit.point);
            if (isInBounds)
            {
                capturedHit = hit;
                return true;
            }
        }
        capturedHit = new RaycastHit();
        return false;
    }

    public bool CaptureRayCastHit(List<string> layersFilter, out RaycastHit capturedHit)
    {
        foreach (var hit in hits)
        {
            Bounds bounds = new Bounds()
            {
                center = cubePosition,
                extents = cubeScale / 2f
            };

            //bool isInBounds = bounds.Contains(hit.point);

            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            bool shouldBeFilterdOut = layersFilter.Contains(layerName);
            if (!shouldBeFilterdOut)
            {
                bool isInBounds = ShipBoundsComputer.Instance.IsVectorOnTheVisibleSide(hit.point);
                if (isInBounds)
                {
                    capturedHit = hit;
                    return true;
                }
            }
        }

        capturedHit = new RaycastHit();
        return false;
    }

    public bool CaptureRayCastHit(out RaycastHit capturedHit)
    {
        foreach (var hit in hits)
        {
            Bounds bounds = new Bounds()
            {
                center = cubePosition,
                extents = cubeScale / 2f
            };

            //bool isInBounds = bounds.Contains(hit.point);
            bool isInBounds = ShipBoundsComputer.Instance.IsVectorOnTheVisibleSide(hit.point);
            if (isInBounds)
            {
                capturedHit = hit;
                return true;
            }
        }

        capturedHit = new RaycastHit();
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        int index = 0;

        foreach (var hit in hits)
        {
            Bounds bounds = new Bounds()
            {
                center = cubePosition,
                extents = cubeScale / 2f
            };
            bool isInBounds = bounds.Contains(hit.point);
            //bool isInBounds = ShipBoundsComputer.Instance.IsVectorOnTheVisibleSide(hit.point);

            Gizmos.color = (isInBounds ? ((index < 1) ? Color.blue : Color.green) : Color.red);
            Gizmos.DrawSphere(hit.point, 1f);

            index++;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(cubePosition, cubeScale);
    }
}