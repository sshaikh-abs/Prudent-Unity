using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GroupingManager : SingletonMono<GroupingManager>
{
    public Material transparentMaterial;
    public Material opaqueMat;
    public GameObject vesselGameobject;
    public Vessel vesselObject;
    public bool isCompartmentModel = false;

    public VesselLoadCommand loadCommand;

    [ContextMenu(nameof(BuildVesselObject))]
    public async Task BuildVesselObject(bool isCompartmentModel, Action OnBuildVesselComplete, string vesselId)
    {
        this.isCompartmentModel = isCompartmentModel;
        vesselObject.SetupMaterials(isCompartmentModel ? opaqueMat : transparentMaterial, opaqueMat);

        await RelationBuilder.Instance.BuildRelations(vesselObject, vesselGameobject, vesselId, isCompartmentModel);
       // Debug.Log("Building Finished");
        OnBuildVesselComplete?.Invoke();
    }

    public string compartmentId;

    [ContextMenu(nameof(ForceIsolate))]
    public void ForceIsolate()
    {
        CommunicationManager.Instance.Isolate_Extern(compartmentId);
    }

    [ContextMenu(nameof(CSVVessel))]
    public void CSVVessel()
    {
        if (vesselObject != null)
        {
            List<MetadataComponent> metadatas = new List<MetadataComponent>();
            //vesselObject.GetCompartment(compartmentId).RunOverAllSubparts((subpart) =>
            //{
            //    //if (!subpart.metadata.TryGetValue("NAME", out string PlatefullName))
            //    //{
            //        metadatas.Add(subpart.metadata);
            //        //PlatefullName = $"{subpart.owningCompartment}/{subpart.owningHullpart.name}/{subpart.prt_name}".GetHashCode().ToString();
            //    //}
            //});

            //MetadataCsvExporter.ExportToCSV(metadatas);
            var compartment = vesselObject.GetCompartment(compartmentId);
            MetadataCsvExporter.ExportToCSV(compartment.data.SelectMany(h => h.data).Select(s => s.metadata).ToList(), compartment.name, "C:/Projects/Unity/ABS-Vessels/Data/PETROBRAS");
        }
    }
}

public class SubpartHelper
{
    public GameObject mesh;
    public Subpart link;

    public SubpartHelper(GameObject mesh, Subpart link)
    {
        this.mesh = mesh;
        this.link = link;
    }
}

public class HullpartHelper
{
    public GameObject mesh;
    public Hullpart link;

    public HullpartHelper(GameObject mesh, Hullpart link)
    {
        this.mesh = mesh;
        this.link = link;
    }
}
