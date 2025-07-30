using EasyButtons;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class DataClass { }

public class ServiceRunner : SingletonMono<ServiceRunner>
{
    public CompartmentGroupingManager_DataClass compartmentGroupingManager;
    public CompartmentConditionStatusService_DataClass compartmentConditionStatusService;
    public LoadingScreenService_DataClass loadingScreenService_DataClass;
    public ComplexShipManager_DataClass complexShipManager_DataClass;
    public CorrosionStatusService_DataClass corrosionStatusService_DataClass;
    public UIService_DataClass UIService_DataClass;
    public EditorVesselLoaderService_DataClass EditorVesselLoaderService_DataClass;
    public FrameLabelFeature_DataClass FrameLabelFeature_DataClass;
    public MarkupManager_DataClass MarkupManager_DataClass;

    public static Dictionary<Type, IService> services = new Dictionary<Type, IService>()
    {
        { typeof(CompartmentConditionStatusService), new CompartmentConditionStatusService() },
        { typeof(LoadingScreenService), new LoadingScreenService() },
        { typeof(ComplexShipManager), new ComplexShipManager() },
        { typeof(CorrosionStatusService), new CorrosionStatusService() },
        { typeof(UIService), new UIService() },
        { typeof(EditorVesselLoaderService), new EditorVesselLoaderService() },
        { typeof(FrameLabelFeature), new FrameLabelFeature() },
        { typeof(MarkupManager), new MarkupManager() }
    };

    private void Awake()
    {
        GetService<CompartmentConditionStatusService>().Initialize(compartmentConditionStatusService);
        GetService<LoadingScreenService>().Initialize(loadingScreenService_DataClass);
        GetService<ComplexShipManager>().Initialize(complexShipManager_DataClass);
        GetService<CorrosionStatusService>().Initialize(corrosionStatusService_DataClass);
        GetService<UIService>().Initialize(UIService_DataClass);
        GetService<EditorVesselLoaderService>().Initialize(EditorVesselLoaderService_DataClass);
        GetService<FrameLabelFeature>().Initialize(FrameLabelFeature_DataClass);
        GetService<MarkupManager>().Initialize(MarkupManager_DataClass);
    }

    public override void Update()
    {
        services.Values.ToList().ForEach(s => s.Update());
    }

    public static T GetService<T>() where T : IService
    {
        return services[typeof(T)] as T;
    }

    public static void Kill<T>() where T : IService, new()
    {
        services[typeof(T)].Kill();
    }

    [ContextMenu(nameof(LoadCorrsionData))]
    public void LoadCorrsionData()
    {
        string jsonData = Resources.Load<TextAsset>("CorrosionData").text;
        ServiceRunner.GetService<CorrosionStatusService>().LoadGaugedPoints(jsonData);
    }
    [ContextMenu(nameof(ResetCorrosionData))]
    public void ResetCorrosionData()
    {       
        ServiceRunner.GetService<CorrosionStatusService>().ResetCorrosionData();
    }
    [ContextMenu(nameof(LoadFakeGaugePoints))]
    public void LoadFakeGaugePoints()
    {
        GetService<CorrosionStatusService>().LoadGaugedPoints(GetFakeData());
    }

    [Button]
    public void LoadGaugePointsFile()
    {
#if UNITY_EDITOR
        //string path = EditorUtility.SaveFilePanel("Save JSON", fileName, ".json", "json");
        string path = EditorUtility.OpenFilePanel("Gauge Plan", Application.dataPath, "json");
        string jsonData = System.IO.File.ReadAllText(path);
        CommunicationManager.Instance.SpawnGaugePoints_Extern(jsonData);
#endif

    }

    [ContextMenu(nameof(LoadCorrosionFile))]
    public void LoadCorrosionFile()
    {
        string jsonData = System.IO.File.ReadAllText(Application.dataPath + "/Resources/CorrosionData.json");
        ServiceRunner.GetService<CorrosionStatusService>().LoadGaugedPoints(jsonData);
    }

    [ContextMenu(nameof(GenerateCorrosionFile))]
    public void GenerateCorrosionFile()
    {
        ServiceRunner.GetService<CorrosionStatusService>().GenerateSampleData();
    }

    public float minThickness = 9.5f;
    public float maxThickness = 15f;

    public string GetFakeData()
    {
        int pointId = 0;

        GaugePointsData_External gaugePointsData_External = new GaugePointsData_External();

        foreach (var compartment in GroupingManager.Instance.vesselObject.GetCompartments(c => c))
        {
            GaugePointsData_Compartment_External gaugePointsData_Compartment_External = new GaugePointsData_Compartment_External()
            {
                assetName = compartment.name,
                assetUID = compartment.uid
            };

            foreach (var frame in compartment.data)
            {
                GaugePointsData_Frame_External gaugePointsData_Frame_External = new GaugePointsData_Frame_External()
                {
                    frameName = frame.name
                };

                foreach (var plate in frame.data)
                {
                    GaugePointsData_Plate_External gaugePointsData_Plate_External = new GaugePointsData_Plate_External()
                    {
                        plateName = plate.prt_name
                    };

                    if(!plate.subpartMeshRenderer)
                    {
                        continue;
                    }

                    for (int i = 0; i < UnityEngine.Random.Range(1, 5); i++)
                    {
                        GaugePointData gaugePointData = new GaugePointData()
                        {
                            id = pointId,
                            plate = plate.prt_name,
                            frame = frame.name,
                            comparment = compartment.name,
                            uId = compartment.uid,
                            location = plate.subpartMeshRenderer.bounds.center,
                            normal = plate.subpartObjectMeshReference.transform.forward,
                            originalThickness = 15f,
                            ruledThickness = 15f,
                            measuredThickness = UnityEngine.Random.Range(minThickness, maxThickness)// (new float[3] { 12f, 10f, 9.5f })[UnityEngine.Random.Range(0, 3)]
                        };
                        gaugePointsData_Plate_External.gaugingPoints.Add(new GaugePointData_External()
                        {
                            point_Id = gaugePointData.id.ToString(),
                            location = gaugePointData.location.GetStringyVector(),
                            normal = gaugePointData.normal.GetStringyVector(),
                            originalThickness = gaugePointData.originalThickness,
                            ruledThickness = gaugePointData.ruledThickness,
                            measuredThickness = gaugePointData.measuredThickness
                        });
                        pointId++;
                    }

                    gaugePointsData_Frame_External.plates.Add(gaugePointsData_Plate_External);

                    pointId++;
                }

                gaugePointsData_Compartment_External.frames.Add(gaugePointsData_Frame_External);
            }

            gaugePointsData_External.compartments.Add(gaugePointsData_Compartment_External);
        }

        Debug.Log(JsonUtility.ToJson(gaugePointsData_External));
         return JsonUtility.ToJson(gaugePointsData_External);
    }

    [ContextMenu(nameof(GenerateFakeData))]
    public void GenerateFakeData()
    {
        string jsonData = GetFakeData();
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/CorrosionData.json", jsonData);
    }
}
