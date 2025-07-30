using DG.Tweening;
using EasyButtons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BaseMarkup : MonoBehaviour
{
    public string anomalyCode = null;

    public List<MeshRenderer> fillRenders = new List<MeshRenderer>();
    public List<MeshRenderer> strokeRenderers = new List<MeshRenderer>();

    public MarkupData data;
    public Color markupColor = Color.red;

    public MeshFilter fillMeshFilter => transform.GetChild(1).GetComponent<MeshFilter>();
    public MeshFilter strokeFilter => transform.GetChild(0).GetComponent<MeshFilter>();

    public virtual void CalculateArea() { }

    public virtual void ParseDimensions() { }

    private void OnValidate()
    {
        Validate();
    }

    public bool hasInitialized = false;

    public void Initalize()
    {
        if(hasInitialized)
        {
            return;
        }

        GameEvents.OnCompartmentIsolated += OnCompatmentIsolated;
        GameEvents.OnHullpartIsolated += OnHullpartIsolated;
        GameEvents.OnSetHullPartActive += OnSetHullpartActive;
        GameEvents.OnVesselView += OnVesselView;
        hasInitialized = true;
    }

    private void OnSetHullpartActive(Hullpart hullpart, bool activeStatus)
    {
        bool isPartofTheOwningCompartment = data.assocationHierarchy.compartments.Exists(c => c.uid == hullpart.owningCompartmentUid);

        if(!isPartofTheOwningCompartment)
        {
            return;
        }

        var LHS = data.assocationHierarchy.compartments[0].hullparts.Select(h => h.hullpart).ToList();
        var RHS = ContextMenuManager.Instance.disabledHullpartsCache.Select(h=> h.name).ToList();

        gameObject.SetActive(LHS.Intersect(RHS).Count() != LHS.Count);
    }

    private void Start()
    {
        Initalize();
    }

    private void OnDestroy()
    {
        GameEvents.OnVesselView -= OnVesselView;
        GameEvents.OnHullpartIsolated -= OnHullpartIsolated;
        GameEvents.OnCompartmentIsolated -= OnCompatmentIsolated;
        GameEvents.OnSetHullPartActive -= OnSetHullpartActive;

        ContextMenuManager.Instance.SetInformationObject(false);
    }

    [Button]
    public void PingMarkup()
    {
        Color finalFillColor = Color.white;
        finalFillColor.a /= 2f;
        Color finalStrokeColor = Color.white;

        Color InitialColorFill = markupColor;
        InitialColorFill.a /= 4f;
        Color InitialStrokeColor = markupColor;

        foreach (var item in fillRenders)
        {
            item.material.color = finalFillColor;
        }

        foreach (var item in strokeRenderers)
        {
            item.material.color = finalStrokeColor;
        }

        foreach (var item in fillRenders)
        {
            item.material.DOColor(InitialColorFill, 1f).SetEase(Ease.InCirc);
        }

        foreach (var item in strokeRenderers)
        {
            item.material.DOColor(InitialStrokeColor, 1f).SetEase(Ease.InCirc);
        }
    }

    public void OnVesselView()
    {
        if (!ServiceRunner.GetService<MarkupManager>().areAnomaliesEnabled)
        {
            return;
        }

        this.gameObject.SetActive(true);
    }

    public void OnHullpartIsolated(Hullpart hullpart)
    {
        if(!ServiceRunner.GetService<MarkupManager>().areAnomaliesEnabled || data.assocationHierarchy.compartments.Count == 0)
        {
            return;
        }

        bool isPartofTheOwningCompartment = data.assocationHierarchy.compartments.Exists(c => c.uid == hullpart.owningCompartmentUid);
        if (isPartofTheOwningCompartment)
        {
            bool doesCoverThisHullpart = data.assocationHierarchy.compartments.Where(c => c.uid == hullpart.owningCompartmentUid).FirstOrDefault().hullparts.Exists(h => h.hullpart == hullpart.name);
            this.gameObject.SetActive(doesCoverThisHullpart);
        }
        else
        {
            this.gameObject.SetActive(false); 
        }
    }

    public void OnCompatmentIsolated(Compartment compartment)
    {
        if (!ServiceRunner.GetService<MarkupManager>().areAnomaliesEnabled || data.assocationHierarchy.compartments.Count == 0)
        {
            return;
        }

        this.gameObject.SetActive(data.assocationHierarchy.compartments.Exists(c => c.uid == compartment.uid));
    }

    public string GetJsonData()
    {
        CalculateArea();
        Markups markups = new Markups();
        markups.markups.Add(data);
        return JsonUtility.ToJson(markups, true);
    }

    [Button]
    public void BuildAssociations()
    {
        Bounds b = GetComponent<Collider>().bounds;

        Vector3 size = b.size;

        if (size.x <= 0)
        {
            size.x = 0.1f;
        }
        if (size.y <= 0)
        {
            size.y = 0.1f;
        }
        if (size.z <= 0)
        {
            size.z = 0.1f;
        }

        b.size = size;

        var results = Physics.OverlapBox(b.center, b.extents, transform.rotation);
        //var results = new List<Collider>();
        //foreach (var item in results_)
        //{
        //    Debug.Log("item : "+item.name);
        //    if (Physics.ComputePenetration( GetComponent<Collider>(), b.center, transform.rotation, item, item.bounds.center, Quaternion.identity, out Vector3 direction, out float distance))
        //    {

        //        Debug.Log("item after : " + item.name);
        //        results.Add(item);
                
        //    }
        //}

        VesselLookUpdata assocationHierarchy = new VesselLookUpdata();
        assocationHierarchy.compartments = new List<CompartmentLookUpdata>();

        if (ApplicationStateMachine.Instance.currentStateName == nameof(HullpartViewState))
        {
            string uid = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>().compartmentUid;
            string hullpartName = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>().hullpartName;

            List<string> subparts = new List<string>();

            foreach (var item in results)
            {
                if (item.gameObject == this.gameObject || item.enabled == false || 
                    item.gameObject.layer == LayerMask.NameToLayer("DocumentIcon") ||
                    item.gameObject.layer == LayerMask.NameToLayer("GaugePoint") ||
                    item.gameObject.layer == LayerMask.NameToLayer("Markup"))
                {
                    continue;
                }

                var subpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(item.gameObject);
                subparts.Add(subpart.name);
                Debug.Log(" - Subpart: " + subpart.name, subpart.subpartMeshRenderer.gameObject);
            }

            assocationHierarchy.compartments.Add(new CompartmentLookUpdata()
            {
                uid = uid,
                hullparts = new List<HullpartLookUpdata>()
                {
                    new HullpartLookUpdata()
                    {
                        hullpart = hullpartName,
                        subparts = subparts
                    }
                }
            });
        }
        else
        {
            foreach (var item in results)
            {
                if (item.gameObject == this.gameObject || item.enabled == false ||
                    item.gameObject.layer == LayerMask.NameToLayer("DocumentIcon") ||
                    item.gameObject.layer == LayerMask.NameToLayer("GaugePoint") ||
                    item.gameObject.layer == LayerMask.NameToLayer("Markup"))
                {
                    continue;
                }

                if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
                {
                    var compartment = GroupingManager.Instance.vesselObject.ProcessCompartmentSelection(item.GetComponent<Collider>().gameObject);
                    CompartmentLookUpdata comp = new CompartmentLookUpdata()
                    {
                        uid = compartment.uid,
                        hullparts = new List<HullpartLookUpdata>()
                    };
                    QueryForSubparts(b, compartment.uid, ref comp);
                    assocationHierarchy.compartments.Add(comp);
                }
                else if (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
                {
                    var hullpart = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(item.GetComponent<Collider>().gameObject);

                    List<string> subparts = new List<string>();
                    QueryForSubparts(b, hullpart.owningCompartmentUid, hullpart.name, ref subparts);

                    if (assocationHierarchy.compartments.Count == 0)
                    {
                        CompartmentLookUpdata comp = new CompartmentLookUpdata()
                        {
                            uid = hullpart.owningCompartmentUid,
                            hullparts = new List<HullpartLookUpdata>()
                            {
                                new HullpartLookUpdata()
                                {
                                    hullpart = hullpart.name,
                                    subparts = subparts
                                }
                            }
                        };

                        assocationHierarchy.compartments.Add(comp);
                    }
                    else
                    {
                        if(assocationHierarchy.compartments.Where(c => c.uid == hullpart.owningCompartmentUid).FirstOrDefault().hullparts.Exists(h => h.hullpart == hullpart.name))
                        {
                            var existingSubparts = assocationHierarchy.compartments.Where(c => c.uid == hullpart.owningCompartmentUid).FirstOrDefault().hullparts.Where(h => h.hullpart == hullpart.name).FirstOrDefault().subparts;
                            foreach (var subpartItem in subparts)
                            {
                                if (!existingSubparts.Contains(subpartItem))
                                {
                                    existingSubparts.Add(subpartItem);
                                }
                            }
                        }
                        else
                        {
                            assocationHierarchy.compartments.Where(c => c.uid == hullpart.owningCompartmentUid).FirstOrDefault().hullparts.Add(new HullpartLookUpdata()
                            {
                                hullpart = hullpart.name,
                                subparts = subparts
                            });
                        }
                    }
                }
            }
        }

        data.assocationHierarchy = new VesselLookUpdata() 
        {
            compartments = assocationHierarchy.compartments
        };
    }

    public void QueryForSubparts(Bounds b, string compartmentName, ref CompartmentLookUpdata compartmentAssocationHierarchy)
    {
        Compartment c = GroupingManager.Instance.vesselObject.GetCompartment(compartmentName);
        List<Hullpart> hullparts = GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, s => s);

        bool isHullpartLessVessel = false;

        List<Collider> disabledGameobjects = new List<Collider>();
        List<Collider> disabledColliders = new List<Collider>();
        hullparts.ForEach(p =>
        {
            if (ServiceRunner.GetService<MarkupManager>().data.subpartExpectation.AreExpectationsMet(p.hullpartMeshReference) && p.data.Count > 1)
            {
                isHullpartLessVessel = true;
                p.RunOverAllSubparts(s =>
                {
                    if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState))
                    {
                            disabledColliders.Add(s.GetCollider());
                            s.GetCollider().enabled = true;

                        if(!s.GetCollider().gameObject.activeSelf)
                        {
                            disabledGameobjects.Add(s.GetCollider());
                            s.GetCollider().gameObject.SetActive(true);
                        }
                    }
                });
            }
            else
            {
                isHullpartLessVessel = false;
                if (ApplicationStateMachine.Instance.currentStateName == nameof(VesselViewState) || !p.GetCollider().enabled || !p.GetCollider().gameObject.activeSelf)
                {
                    disabledColliders.Add(p.GetCollider());
                    p.GetCollider().enabled = true;

                    if (!p.GetCollider().gameObject.activeSelf)
                    {
                        disabledGameobjects.Add(p.GetCollider());
                        p.GetCollider().gameObject.SetActive(true);
                    }
                }
            }
        });

        Ray ray = CameraService.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        var hits = Physics.OverlapBox(b.center, b.extents);

        List<Collider> hitColliders = new List<Collider>();

        foreach (var item in hits)
        {
            if (item.gameObject == this.gameObject || GroupingManager.Instance.vesselObject.GetCompartments(c => c.compartmentMeshObjectReference).Contains(item.gameObject) ||
                    item.gameObject.layer == LayerMask.NameToLayer("DocumentIcon") ||
                    item.gameObject.layer == LayerMask.NameToLayer("GaugePoint") ||
                    item.gameObject.layer == LayerMask.NameToLayer("Markup"))
            {
                continue;
            }
            hitColliders.Add(item);
        }

        disabledColliders.ForEach(p => p.enabled = false);
        disabledGameobjects.ForEach(p => p.gameObject.SetActive(false));

        foreach (var item in hitColliders)
        {
            var hullpart = GroupingManager.Instance.vesselObject.ProcessHullpartSelection(item.gameObject);

            if(isHullpartLessVessel)
            {
                if (!compartmentAssocationHierarchy.hullparts.Exists(h => h.hullpart == hullpart.name))
                {
                    HullpartLookUpdata hp = new HullpartLookUpdata()
                    {
                        hullpart = hullpart.name,
                        subparts = new List<string>()
                    };
                    hp.subparts.Add(GroupingManager.Instance.vesselObject.ProcessSubpartSelection(item.gameObject).name);
                    compartmentAssocationHierarchy.hullparts.Add(hp);
                }
                else
                {
                    compartmentAssocationHierarchy.hullparts.Where(h => h.hullpart == hullpart.name).FirstOrDefault().subparts.Add(GroupingManager.Instance.vesselObject.ProcessSubpartSelection(item.gameObject).name);
                }
            }
            else
            {
                var subparts = new List<string>();
                QueryForSubparts(b, compartmentName, hullpart.name, ref subparts);
                compartmentAssocationHierarchy.hullparts.Add(new HullpartLookUpdata()
                {
                    hullpart = hullpart.name,
                    subparts = subparts
                });
            }
        }
    }

    public void QueryForSubparts(Bounds b, string compartmentName, string hullpartName, ref List<string> subparts)
    {
        List<Collider> plates = GroupingManager.Instance.vesselObject.GetSubparts(compartmentName, hullpartName, SubpartType.All, s => s.GetCollider());

        List<Collider> disabledColliders = new List<Collider>();
        plates.ForEach(p =>
        {
            if (!p.enabled)
            {
                disabledColliders.Add(p);
                p.enabled = true;
            }
        });

        Ray ray = CameraService.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        var hits = Physics.OverlapBox(b.center, b.extents);

        List<Collider> hitColliders = new List<Collider>();

        foreach (var item in hits)
        {
            if (item.gameObject == this.gameObject || 
                item.gameObject.layer == LayerMask.NameToLayer("DocumentIcon") ||
                item.gameObject.layer == LayerMask.NameToLayer("GaugePoint") || 
                item.gameObject.layer == LayerMask.NameToLayer("Markup") ||
                GroupingManager.Instance.vesselObject.GetHullparts(compartmentName, h => h.GetCollider().gameObject).Contains(item.gameObject))
            {
                continue;
            }
            hitColliders.Add(item);
        }

        disabledColliders.ForEach(p => p.enabled = false);

        foreach (var item in hitColliders)
        {
            var subpart = GroupingManager.Instance.vesselObject.ProcessSubpartSelection(item.gameObject);

            if(subpart != null && !subparts.Exists(s => s.Equals(subpart.name)))
            {
                subparts.Add(subpart.name);
            }
        }
    }

    public void Validate()
    {
        ParseDimensions();
        ValidateShape();
        ValidateColor();
        //FetchColliders(); // BEWARE THIS CAN BE EXPENSIVE ESPECIALLY WITH POLYGON MARKUPS
    }

    public virtual void ValidateColor()
    {
        if (ColorUtility.TryParseHtmlString(data.color, out Color pureColor))
        {
            markupColor = pureColor;
        }

        var renderers = GetComponentsInChildren<Renderer>();

        Color fillColor = markupColor;
        fillColor.a /= 4;
        foreach (var item in fillRenders)
        {
            if(Application.isPlaying)
            {
                item.material.color = fillColor;
            }
            else
            {
                item.sharedMaterial.color = fillColor;
            }
        }

        Color strokeColor = markupColor;
        foreach (var item in strokeRenderers)
        {
            if (Application.isPlaying)
            {
                item.material.color = strokeColor;
            }
            else
            {
                item.sharedMaterial.color = strokeColor;
            }
        }
    }

    private void OnMouseEnter()
    {
        if(MarkupCreator.Instance.enableMarking)
        {
            return;
        }

        ContextMenuManager.Instance.SetInformationObject(true);
        ContextMenuManager.Instance.SetInfromationText($"Anomaly : {(anomalyCode != null ? anomalyCode : data.AnomalyCode)}");

        Color finalFillColor = Color.white;
        finalFillColor.a /= 2f;
        Color finalStrokeColor = Color.white;

        foreach (var item in fillRenders)
        {
            item.material.DOColor(finalFillColor, 0.05f).SetEase(Ease.InCubic);
        }

        foreach (var item in strokeRenderers)
        {
            item.material.DOColor(finalStrokeColor, 0.05f).SetEase(Ease.InCubic);
        }
    }

    private void OnMouseDown()
    {
        if (MarkupCreator.Instance.enableMarking)
        {
            return;
        }

        CommunicationManager.Instance.HandleAnomalyClicked(data.AnomalyCode);
    }

    private void OnMouseExit()
    {
        if (MarkupCreator.Instance.enableMarking)
        {
            return;
        }

        Color InitialColorFill = markupColor;
        InitialColorFill.a /= 4f;
        Color InitialStrokeColor = markupColor;

        foreach (var item in fillRenders)
        {
            item.material.DOColor(InitialColorFill, 0.05f).SetEase(Ease.InCubic);
        }

        foreach (var item in strokeRenderers)
        {
            item.material.DOColor(InitialStrokeColor, 0.05f).SetEase(Ease.InCubic);
        }

        ContextMenuManager.Instance.SetInformationObject(false);
    }

    public virtual void ValidateShape() { }

    public virtual void UpdateData(MarkupData markupData)
    {
        data.markupType = markupData.markupType;
        data.location = markupData.location;
        data.rotation = markupData.rotation;
        data.dimensions = new List<float>();
        foreach (var item in markupData.dimensions)
        {
            data.dimensions.Add(item);
        }
        data.color = markupData.color;
        data.area = markupData.area;
        data.image = markupData.image;
        data.AnomalyCode= markupData.AnomalyCode;
        data.markUpName = markupData.markUpName;
        data.assocationHierarchy = markupData.assocationHierarchy;
        gameObject.name = $"{data.AnomalyCode}-{data.markupType}";
    }
}
