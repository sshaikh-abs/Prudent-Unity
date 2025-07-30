using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransverseFrameLabelManager : MonoBehaviour
{
    public Transform parent;
    public bool showLabels = true; // Set this to false to hide labels
    public float threshold = 10f;
    public FrameLabel frameLable3D; // Assign your prefab

    public Dictionary<string, FrameLabel> frameLabels = new Dictionary<string, FrameLabel>();
    public Dictionary<FrameLabel, Hullpart> frameNameLookup = new Dictionary<FrameLabel, Hullpart>();

    private Compartment targetedCompartment = null;
    private List<FrameLabel> transforms = new List<FrameLabel>();
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

    public List<FrameLabel> GetFrameLabelsOfCompartment(Compartment compartment)
    {
        var frames = new List<FrameLabel>();

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

                if(targetedCompartment != null && condition)
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

    public void ReclibrateFrameLabel(FrameLabel frameLabel, Hullpart hullpart = null)
    {
        Vector3 position = GetPosition(frameLabel, hullpart);
        frameLabel.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        frameLabel.transform.position = position;
        frameLabel.size = GetSize(frameLabel, hullpart);
        frameLabel.OnValidate(); // Call OnValidate to update the label
    }

    public static Vector3 GetPosition(FrameLabel frameLabel, Hullpart hullpart)
    {
        Vector3 position;
        position.x = hullpart == null ? RelationBuilder.Instance.hullpartRegistry[frameLabel.gameObject.name].hullpartBoundsGlobal.center.x : hullpart.hullpartBounds.center.x;
        position.y = ServiceRunner.GetService<FrameLabelFeature>().data.yOffset;
        position.z = hullpart == null ? ShipBoundsComputer.Instance.shipBounds.center.z :
            GroupingManager.Instance.vesselObject.GetCompartment(hullpart.owningCompartmentUid).
            compartmentMeshObjectReference.GetComponent<MeshRenderer>().bounds.center.z;
        return position;
    }

    public static float GetSize(FrameLabel frameLabel, Hullpart hullpart)
    {
        return (hullpart == null ? ShipBoundsComputer.Instance.shipBounds.size.z :
            GroupingManager.Instance.vesselObject.GetCompartment(hullpart.owningCompartmentUid).
            compartmentMeshObjectReference.GetComponent<MeshRenderer>().bounds.size.z) + ServiceRunner.GetService<FrameLabelFeature>().data.sizeOffset; // Set the size as needed
    }

    public void SpawnLabels()
    {
        RelationBuilder.Instance.hullpartRegistry.ToList().ForEach((hullPart) =>
        {
            if (hullPart.Value.hullpartType.ToLower().Contains("trans"))
            {
                Vector3 position;
                position.x = hullPart.Value.hullpartBoundsGlobal.center.x;
                position.y = ServiceRunner.GetService<FrameLabelFeature>().data.yOffset;
                position.z = ShipBoundsComputer.Instance.shipBounds.center.z;

                FrameLabel frameLabel = Instantiate(frameLable3D, parent);
                frameLabel.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                frameLabel.transform.position = position;
                frameLabel.label = Compartment.ConvertLine(hullPart.Value.name);
                frameLabel.size = (ShipBoundsComputer.Instance.shipBounds.size.z) + ServiceRunner.GetService<FrameLabelFeature>().data.sizeOffset; // Set the size as needed
                frameLabel.OnValidate(); // Call OnValidate to update the label
                frameLabels[hullPart.Value.name] = frameLabel; // Store the label in the dictionary
                frameNameLookup[frameLabel] = hullPart.Value;
                transforms.Add(frameLabel);
                frameLabel.gameObject.name = hullPart.Value.name;
            }
        });
        transforms = transforms.OrderBy(t => t.transform.localPosition.x).ToList();
    }
}