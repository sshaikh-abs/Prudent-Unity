using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DeckFrameLabelManager : MonoBehaviour
{
    public Transform parent;
    public bool showLabels = true; // Set this to false to hide labels
    public float threshold = 10f;
    public DeckLabel frameLable3D; // Assign your prefab

    public Dictionary<string, DeckLabel> frameLabels = new Dictionary<string, DeckLabel>();
    public Dictionary<DeckLabel, Hullpart> frameNameLookup = new Dictionary<DeckLabel, Hullpart>();

    private Compartment targetedCompartment = null;
    private List<DeckLabel> transforms = new List<DeckLabel>();
    public List<Transform> isolatedList = new List<Transform>();

    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            SetLabelsActive(showLabels);
            UpdateVisibility();
        }
    }

    public void SetLabelsActive(bool active)
    {
        parent.gameObject.SetActive(active);
    }

    public void SetLabelsDensity(float value)
    {
        threshold = value;
        UpdateVisibility();
    }
    private void Start()
    {
        GameEvents.OnCompartmentIsolated += OnCompartmentIsolated;
        GameEvents.OnVesselView += OnVesselView;
        GameEvents.OnHullpartIsolated += OnHullpartIsolated;
    }

    private void OnHullpartIsolated(Hullpart hullpart)
    {
        targetedCompartment = null;
        UpdateVisibility();
    }

    private void OnVesselView()
    {
        targetedCompartment = null;
        UpdateVisibility();
    }

    private void OnCompartmentIsolated(Compartment compartment)
    {
        targetedCompartment = compartment;
        UpdateVisibility();
    }

    public List<DeckLabel> GetFrameLabelsOfCompartment(Compartment compartment)
    {
        var frames = new List<DeckLabel>();

        foreach (var item in frameLabels)
        {
            if (compartment.hullpartLookup.ContainsKey(item.Key))
            {
                frames.Add(item.Value);
            }
        }

        return frames;
    }

    public List<string> GetFramesOfCompartment(Compartment compartment)
    {
        var frames = new List<string>();

        foreach (var item in transforms)
        {
            if(compartment.hullpartLookup.ContainsKey(item.name))
            {
                frames.Add(item.name);
            }
        }

        return frames;
    }

    public (string, string) GetFrameFromAndTo(Compartment compartment)
    {
        var frames = GetFramesOfCompartment(compartment);
        return (frames.LastOrDefault(), frames.FirstOrDefault());
    }

    public void UpdateVisibility()
    {
        Transform lastTransfrom = null;
        isolatedList = new List<Transform>();
        foreach (var item in transforms)
        {
            if (lastTransfrom == null)
            {
                lastTransfrom = item.transform;
                bool condition = targetedCompartment == null ? ApplicationStateMachine.Instance.currentStateName != nameof(HullpartViewState) : targetedCompartment.hullpartLookup.ContainsKey(item.gameObject.name);

                if (targetedCompartment != null && condition)
                {
                    ReclibrateFrameLabel(item, targetedCompartment.hullpartLookup[item.gameObject.name]);
                }
                else
                {
                    ReclibrateFrameLabel(item, null);
                }

                item.gameObject.SetActive(condition);
            }
            else
            {
                //float distance = Vector3.Distance(item.transform.position, lastTransfrom.position);
                float distance = Mathf.Abs(item.transform.position.x - lastTransfrom.position.x);
                if (distance < threshold)
                {
                    item.transform.gameObject.SetActive(false);
                }
                else
                {
                    bool condition = targetedCompartment == null ? ApplicationStateMachine.Instance.currentStateName != nameof(HullpartViewState) : targetedCompartment.hullpartLookup.ContainsKey(item.gameObject.name);
                    item.gameObject.SetActive(condition);
                    if(condition)
                    {
                        lastTransfrom = item.transform;
                    }
                    if (targetedCompartment != null && condition)
                    {
                        ReclibrateFrameLabel(item, targetedCompartment.hullpartLookup[item.gameObject.name]);
                    }
                    else
                    {
                        ReclibrateFrameLabel(item, null);
                    }

                    item.gameObject.SetActive(condition);
                }
            }

            if (item.gameObject.activeSelf)
            {
                isolatedList.Add(item.transform);
            }
        }
    }

    public void ReclibrateFrameLabel(DeckLabel frameLabel, Hullpart hullpart = null)
    {
        Vector3 position = GetPosition(frameLabel, hullpart);
        frameLabel.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        frameLabel.transform.position = position;
        frameLabel.bounds = GetSize(frameLabel, hullpart);
        frameLabel.UpdatePosition(); // Call OnValidate to update the label
    }

    public static Vector3 GetPosition(DeckLabel frameLabel, Hullpart hullpart)
    {
        Vector3 pos;
        if (hullpart == null)
        {
            pos = RelationBuilder.Instance.hullpartRegistry[frameLabel.gameObject.name].hullpartBoundsGlobal.center;
            pos.z = ShipBoundsComputer.Instance.shipBounds.center.z;
        }
        else
        {
            pos = hullpart.hullpartBounds.center;
            pos.z = GroupingManager.Instance.vesselObject.GetCompartment(hullpart.owningCompartmentUid).
            compartmentMeshObjectReference.GetComponent<MeshRenderer>().bounds.center.z;
        }

        return pos;
    }


    public static Vector2 GetSize(DeckLabel frameLabel, Hullpart hullpart)
    {
        if(hullpart == null)
        {
            return new Vector2(RelationBuilder.Instance.hullpartRegistry[frameLabel.gameObject.name].hullpartBoundsGlobal.size.x, ShipBoundsComputer.Instance.shipBounds.size.z);
        }
        else
        {
            Vector2 size = hullpart.hullpartBounds.size;
            size.y = hullpart.hullpartBounds.size.z;
            //size.y = GroupingManager.Instance.vesselObject.GetCompartment(hullpart.owningCompartmentUid).
            //compartmentMeshObjectReference.GetComponent<MeshRenderer>().bounds.size.z;
            return size;
        }
    }

    public void SpawnLabels()
    {
        RelationBuilder.Instance.hullpartRegistry.ToList().ForEach((hullPart) =>
        {
            if (hullPart.Value.hullpartType.ToLower().Contains("deck") ||
                hullPart.Value.hullpartType.ToLower().Contains("girder") ||
                hullPart.Value.hullpartType.ToLower().Contains("stringer") ||
                hullPart.Value.hullpartType.ToLower().Contains("hor"))
            {
                Vector3 position = hullPart.Value.hullpartBoundsGlobal.center;
                //position.x = hullPart.Value.hullpartBoundsGlobal.center.x;
                //position.y = ServiceRunner.GetService<FrameLabelFeature>().data.yOffset;
                //position.z = ShipBoundsComputer.Instance.shipBounds.center.z;

                DeckLabel frameLabel = Instantiate(frameLable3D, parent);
                frameLabel.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                frameLabel.transform.position = position;
                frameLabel.SetLabel(Compartment.ConvertLine(hullPart.Value.name));
                
                frameLabel.bounds = new Vector2(hullPart.Value.hullpartBoundsGlobal.size.x, ShipBoundsComputer.Instance.shipBounds.size.z);
                //frameLabel.size = (ShipBoundsComputer.Instance.shipBounds.size.z) + ServiceRunner.GetService<FrameLabelFeature>().data.sizeOffset; // Set the size as needed
                frameLabel.UpdatePosition(); // Call OnValidate to update the label
                frameLabels[hullPart.Value.name] = frameLabel; // Store the label in the dictionary
                frameNameLookup[frameLabel] = hullPart.Value;
                transforms.Add(frameLabel);
                frameLabel.gameObject.name = hullPart.Value.name;
            }
        });
        transforms = transforms.OrderBy(t => t.transform.localPosition.x).ToList();
    }
}