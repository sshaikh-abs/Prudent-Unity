using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class RelationBuilder : SingletonMono<RelationBuilder>
{
    private Dictionary<string, CompartmentGroup> compartmentGroups = new Dictionary<string, CompartmentGroup>();
    public List<Compartment> compartments = new List<Compartment>();

    private Dictionary<string, Subpart> subpartLookupByPrtName = new Dictionary<string, Subpart>();
    [NonSerialized] private List<SubpartHelper> subpartHelpers = new List<SubpartHelper>();
    [NonSerialized] private List<HullpartHelper> hullPartHelpers = new List<HullpartHelper>();
    [NonSerialized] public Dictionary<string, Hullpart> hullpartRegistry = new Dictionary<string, Hullpart>();

    public Expectation subpartExpectation;
    public Expectation rootPlateExpectation;
    public Expectation compartmentExpectation;

    public Material transparentMaterial;
    public Material opaqueMat;

    public List<Renderer> renderersCollection = new List<Renderer>();

    private PetrobrasHullpartSolver petrobrasHullpartSolver = new PetrobrasHullpartSolver();
    private PetrobrasRootSolver petrobrasCompartmentRootSolver = new PetrobrasRootSolver();

    private TestFpsoHullpartSolver testFpsoHullpartSolver = new TestFpsoHullpartSolver();
    private TestFpsoCompartmentRootSolver testFpsoCompartmentRootSolver = new TestFpsoCompartmentRootSolver();

    public async Task BuildRelations(Vessel vesselObject, GameObject specificFile, string vesselId, bool isCompartmentModel)
    {
        IHullpartNameSolver hullpartNameSolver = (vesselId.Equals("0")) ? testFpsoHullpartSolver : petrobrasHullpartSolver;
        ICompartmentRootSolver compartmentRootSolver = (vesselId.Equals("0")) ? testFpsoCompartmentRootSolver : petrobrasCompartmentRootSolver;

        GameObject root = null;

        if(specificFile != null)
        {
            root = specificFile;
        }
        else
        {
            root = gameObject;
        }

        var metadatas = root.GetComponentsInChildren<MetadataComponent>();

        Compartment activeCompartmentRelation = null;
        Hullpart activeHullpartRelation = null;

        int entityCount = 0;

        foreach (var metadata in metadatas)
        {
            bool failedFully = true;

            if (compartmentExpectation.AreExpectationsMet(metadata.gameObject))
            {
                failedFully = false;
                activeHullpartRelation = null;
                var compRelation = compartments.Where(c => c.uid.Equals(metadata.GetValue("ASSET_UID"))).FirstOrDefault();

                if (compRelation != null)
                {
                    activeCompartmentRelation = compRelation;
                   // Debug.Log($"Compartment name : {compRelation.name}", compRelation.compartmentMetaData);
                }
                else
                {
                    Compartment compartmentRelation = new Compartment(metadata.GetValue("ASSET_UID"), metadata.GetValue("NAME"), metadata, isCompartmentModel);
                    compartmentRelation.compartmentMeshObjectReference.GetComponent<MeshRenderer>().sharedMaterial = isCompartmentModel ? opaqueMat : transparentMaterial;
                    renderersCollection.Add(compartmentRelation.compartmentMeshObjectReference.GetComponent<MeshRenderer>());

                    string function = metadata.GetValue("FUNCTION").ToLower();
                    compartmentRelation.function = function;
                    if (compartmentGroups.ContainsKey(function))
                    {
                        compartmentGroups[function].data.Add(compartmentRelation);
                    }
                    else
                    {
                        CompartmentGroup compartmentGroup = new CompartmentGroup()
                        {
                            name = function,
                            data = new List<Compartment>()
                        };
                        compartmentGroup.data.Add(compartmentRelation);
                        compartmentGroups.Add(function, compartmentGroup);
                    }

                    compartments.Add(compartmentRelation);
                    activeCompartmentRelation = compartmentRelation;
                }

                GameObject compartmentRoot = compartmentRootSolver.SolveForRootGameobject(metadata);
                if (!activeCompartmentRelation.compartmentGameobject.Contains(compartmentRoot))
                {
                    activeCompartmentRelation.compartmentGameobject.Add(compartmentRoot);
                }

                continue;
            }

            if (subpartExpectation.AreExpectationsMet(metadata.gameObject))
            {
                failedFully = false;
                activeHullpartRelation = GetOrCreateHullpart(activeHullpartRelation, activeCompartmentRelation, metadata, hullpartNameSolver);

                string displayName = "";
                string prtName = "";

                if(metadata.ContainsKey("DISPLAY_NAME"))
                {
                    displayName = metadata.GetValue("DISPLAY_NAME");
                }
                else
                {
                    displayName = metadata.GetValue("NAME");
                }

                prtName = metadata.GetValue("PRT_NAME");

                Subpart subpart = new Subpart(metadata, activeHullpartRelation, activeCompartmentRelation)
                {
                    //name = Regex.Replace(metadata.GetValue("NAME"), @"_1$", "")
                    name = displayName,
                    prt_name = prtName,
                    associatedCompartments = new List<string> { activeCompartmentRelation.uid }
                };

                SetupSubpart(activeHullpartRelation, subpart);
                subpartHelpers.Add(new SubpartHelper(metadata.gameObject, subpart));

                continue;
            }

            if(rootPlateExpectation.AreExpectationsMet(metadata.gameObject) && metadata.gameObject.GetComponent<MeshRenderer>())
            {
                Vessel.isHullpartLessVessel = false;
                failedFully = false;
                activeHullpartRelation = GetOrCreateHullpart(activeHullpartRelation, activeCompartmentRelation, metadata, hullpartNameSolver);
                activeHullpartRelation.SetupHullpartMesh(metadata.gameObject, transparentMaterial);
                hullPartHelpers.Add(new HullpartHelper(activeHullpartRelation.hullpartMeshReference, activeHullpartRelation));
            }

            if(failedFully)
            {
                if(metadata.transform.childCount == 0)
                {
                    MyInstantiatorAddon.meshesWithNoData.Add(metadata.gameObject);
                }
            }

            entityCount++;

            if(entityCount > 50000)
            {
                await Task.Yield();
            }
        }

        FinalizeVessel(vesselObject);
    }

    public void FinalizeVessel(Vessel vesselObject)
    {
        vesselObject.InjectHelpers(compartmentGroups, subpartHelpers, hullPartHelpers, renderersCollection);
    }

    public void SetupSubpart(Hullpart hullpart, Subpart subpart)
    {
        hullpart.RegisterSubpart(subpart);
        SetupSubpartType(subpart);
        if (subpart.type == SubpartType.Plate)
        {
            hullpart.UpdateBounds(subpart);
            hullpartRegistry[hullpart.name].RegisterGlobal(subpart);
        }

        if (ContextMenuManager.Instance.disabledHullpartsCache.Contains(hullpart))
        {
            subpart.SetActive(false);
        }

        var viewState = ApplicationStateMachine.Instance.GetCurrentState<HullpartViewState>();

        if (viewState != null)
        {
            if (viewState.hullpartName != hullpart.name)
            {
                subpart.SetActive(false);
            }
            else
            {
                subpart.GetCollider().enabled = false;
            }
        }
        else
        {
            var viewStateComp = ApplicationStateMachine.Instance.GetCurrentState<CompartmentViewState>();
            if (viewStateComp != null)
            {
                if (viewStateComp.GetTargettedCompartemnt() != subpart.owningCompartment.uid)
                {
                    subpart.SetActive(false);
                }
                //else
                //{
                //    subpart.GetCollider().enabled = true;
                //}
            }
        }

        renderersCollection.Add(subpart.subpartMeshRenderer);

        string buildThickness = "";

        if (subpart.metadata.ContainsKey("BUILD_THICKNESS"))
        {
            buildThickness = subpart.metadata.GetValue("BUILD_THICKNESS");

            if(float.TryParse(buildThickness, out float buildThicknessValue))
            {
                hullpart.originalThicknessSum += float.Parse(buildThickness) / 1000f;
            }
            else
            {
                Debug.LogWarning($"Failed to parse build thickness: {buildThickness}, failing safe and setting the value to 15f");
                //hullpart.originalThicknessSum += 15f / 1000f;
            }
        }
        else if (subpart.metadata.ContainsKey("THICKNESS"))
        {
            buildThickness = subpart.metadata.GetValue("THICKNESS");
            hullpart.originalThicknessSum += float.Parse(buildThickness);
        }
        else if (subpart.metadata.ContainsKey("WEB_THICKNESS"))
        {
            buildThickness = subpart.metadata.GetValue("WEB_THICKNESS");
            hullpart.originalThicknessSum += float.Parse(buildThickness);
        }
        //else
        //{
        //    hullpart.originalThicknessSum += 15f;
        //}

        subpart.buildThickness = buildThickness;

        if(subpartLookupByPrtName.TryGetValue(subpart.prt_name, out Subpart match))
        {
            match.connectedSubparts.Add(subpart);
            subpart.connectedSubparts.Add(match);

            var compartmentsList = match.associatedCompartments;
            compartmentsList.AddRange(subpart.associatedCompartments);
            compartmentsList = compartmentsList.Distinct().ToList();
            subpart.associatedCompartments = compartmentsList;
            match.associatedCompartments = compartmentsList;
        }
        else
        {
            subpartLookupByPrtName.Add(subpart.prt_name, subpart);
        }
    }

    public void SetupSubpartType(Subpart subpart)
    {
        using (TestFpsoSubpartTypeSolver testFpsoSubpartTypeSolver = new TestFpsoSubpartTypeSolver())
        {
            subpart.SetType(testFpsoSubpartTypeSolver.SolveForSubparType(subpart.metadata));
            bool active = false;
            switch (subpart.type)
            {
                case SubpartType.Plate:
                    subpart.owningHullpart.hullpartType = subpart.metadata.GetValue("POSITION");
                    active = CommunicationManager.Instance.platesActive;
                    break;
                case SubpartType.Bracket:
                    active = CommunicationManager.Instance.bracketsActive;
                    break;
                case SubpartType.Stiffener:
                    active = CommunicationManager.Instance.stiffenersActive;
                    break;
                default:
                    break;
            }
            subpart.SetVisibility(active);
        }

        subpart.SetVisibility(CommunicationManager.Instance.platesActive);
    }

    public Hullpart GetOrCreateHullpart(Hullpart hullpart, Compartment compartment, MetadataComponent metadata, IHullpartNameSolver hullpartSolver)
    {
        string hullpartName = "";

        hullpartName = hullpartSolver.SolverForHullpartName(metadata);

        if (hullpart != null && hullpart.name == hullpartName)
        {
            return hullpart;
        }
        else
        {
            if (compartment.hullpartLookup.ContainsKey(hullpartName))
            {
                return compartment.hullpartLookup[hullpartName];
            }
            else
            {
                Hullpart hullpartRelation = new Hullpart(hullpartName, compartment)
                {
                    data = new List<Subpart>(),
                    hullpartMeshReference = metadata.gameObject,
                    associatedCompartments = new List<string>() { compartment.uid },
                    hullpartType = metadata.GetValue("POSITION")
                };

                if(!hullpartRegistry.ContainsKey(hullpartName))
                {
                    hullpartRegistry.Add(hullpartName, hullpartRelation);
                }

                compartment.RegisterHullpart(hullpartRelation);

                foreach (var testedCompartment in compartmentGroups.SelectMany(c => c.Value.data))
                {
                    if(testedCompartment == compartment)
                    {
                        continue;
                    }

                    if (testedCompartment.hullpartLookup.ContainsKey(hullpartName))
                    {
                        var testHullpart = testedCompartment.hullpartLookup[hullpartName];
                        testHullpart.associatedCompartments.Add(compartment.uid);
                        hullpartRelation.associatedCompartments.Add(testedCompartment.uid);
                    }

                }

                return hullpartRelation;
            }
        }
    }
}