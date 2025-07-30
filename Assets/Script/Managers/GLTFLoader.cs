using UnityEngine;
using System.Collections.Generic;
using GLTFast;
using System.Threading.Tasks;
using System.Linq;
using System;

#if OLD_IMPLIMENTATION
using GLTFast.Custom;
#else
using GLTFast.Addons;
using GltfImport = GLTFast.Newtonsoft.GltfImport;
#endif

public class GltfLoader : SingletonMono<GltfLoader>
{
    public bool bypassNetwork = false;
    public string offlineModelAddress = "";

    [SerializeField] private Material materialToApply;

    public static List<GameObject> disabled = new List<GameObject>();

    private void Start()
    {
        MyInstantiatorAddon.OnNodeCreatedCallback += OnNodeCreatedCallback;
    }

    public override void LateStart()
    {
#if UNITY_EDITOR
        RunEditorCommand();
#endif
    }

    void RunEditorCommand()
    {
        VesselLoadCommand command = (VesselLoadCommand)ServiceRunner.GetService<EditorVesselLoaderService>().data.PassThrough.GetCommandSO();
        if (command != null)
        {
            ExcuteLoadModels(command);
        }
        else
        {
            BulkLoadCommand bulkCommand = (BulkLoadCommand)ServiceRunner.GetService<EditorVesselLoaderService>().data.PassThrough.GetCommandSO();
            ExcuteBulkLoad(bulkCommand);
        }
    }

    private async Task<bool> DownloadModelAsync(GltfImport gltf, string link, Transform parentTransform, Action<int> OnDownloaded, Action OnLoaded)
    {
        await gltf.Load(link);

        while(gltf.GetSourceRoot() == null)
        {
            await Task.Yield();
        }

        OnDownloaded(gltf.GetSourceRoot().Nodes.Count);
        bool instantiated = await gltf.InstantiateMainSceneAsync(parentTransform);
        OnLoaded();
        return instantiated;
    }

    public void SetDownloadProgress(float p)
    {
        ServiceRunner.GetService<LoadingScreenService>().SetDownloadingBarProgress(p);
    }

    int currentFileCount = 0;

    private async Task<bool> GetDownloadTasks(ILoadCommand commandRef, List<string> incomingLinks, GameObject rootMost, bool _loadedCompexShip)
    {
        if (incomingLinks.Count == 0)
        {
            return true;
        }

        Material materialIntended = materialToApply;

        // List of tasks to download models concurrently
        List<Task<bool>> downloadTasks = new List<Task<bool>>();

        targetEntityCount = 0;
        currentEntityCount = 0;

#if !OLD_IMPLIMENTATION
        ImportAddonRegistry.RegisterImportAddon(new MetadataAddon());
#endif
        //var provider = new ProgressDownloadProvider();

        for (int i = 0; i < incomingLinks.Count; i++)
        {
#if OLD_IMPLIMENTATION
            var gltf = new CustomGltfImport(materialIntended, !_loadedCompexShip);
            downloadTasks.Add(DownloadModelAsync(gltf, incomingLinks[i], g.transform));
#else
            //provider.OnProgress += p => Debug.Log($"Loading Progress: {p * 100f:0.0}%");

            //provider.OnProgress += SetDownloadProgress;
            var gltf = new GltfImport(/*downloadProvider: provider*/);
            //var gltf = new GltfImport();

            MyInstantiatorAddon.isCompartmentModel = !_loadedCompexShip;
            MyInstantiatorAddon.materialOverride = materialIntended;
            downloadTasks.Add(DownloadModelAsync(gltf, incomingLinks[i], rootMost.transform, c => 
            {
                targetEntityCount += c;
            }, () => 
            {
                currentFileCount++;
                //ServiceRunner.GetService<LoadingScreenService>().SetLoadingMessage($"Loading Structures ({currentFileCount}/{incomingLinks})");

                int targetCount = 0;
                var vesselLoadCommand = (VesselLoadCommand)commandRef;

                if(vesselLoadCommand != null)
                {
                    targetCount = vesselLoadCommand.plates.Count + 2;
                }
                else
                {
                    targetCount = ((BulkLoadCommand)commandRef).chunkLinks.Count;
                }

                //ServiceRunner.GetService<LoadingScreenService>().SetLoadingMessage_Secondary($"{currentFileCount}/{targetCount}");
                ServiceRunner.GetService<LoadingScreenService>().SetLoadingMessage_Secondary($"{Mathf.RoundToInt((currentFileCount * 1f/targetCount) * 100f)}%");
                ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(currentFileCount * 1f / targetCount);
                //ServiceRunner.GetService<LoadingScreenService>().SetLoadingMessage_Secondary($"{currentFileCount}/{incomingLinks.Count}");
                //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(currentFileCount * 1f / incomingLinks.Count);
            }));
#endif
        }

        bool success = false;

        bool[] results = await Task.WhenAll(downloadTasks);

        for (int i = 0; i < results.Length; i++)
        {
            if (i == incomingLinks.Count - 1)
            {
                success = results[i];
            }
        }

        return success;
    }

    int currentEntityCount = 0;
    public int targetEntityCount = 0;
    private void OnNodeCreatedCallback()
    {
        if (targetEntityCount == 0)
        {
            return;
        }
        currentEntityCount++;
    }

    public void ExcuteLoadModel(string link)
    {
        ExcuteLoadModels(new VesselLoadCommand()
        {
            compartment = link,
        });
    }

    public void ExcuteBulkLoad(BulkLoadCommand command)
    {
        LoadModels_LoadTime(command, command.vesselId, command.chunkLinks, new List<string>(), new List<string>(), true, false);
        //ServiceRunner.GetService<UIService>().SetActive(false);
    }

    public void ExcuteLoadModels(VesselLoadCommand command)
    {
        List<string> initialLinks = new List<string>(), highPriorityLinks = new List<string>(), lowPriorityLinks = new List<string>();

        initialLinks = new List<string>() { command.compartment };
        if(!command.shell.IsNullOrEmpty())
        {
            highPriorityLinks = new List<string>() { command.shell };
        }

        if (command.plates != null)
        {
            lowPriorityLinks = command.plates.Select(p => p.link).ToList();
        }

        LoadModels_LoadTime(command, command.vesselId, initialLinks, highPriorityLinks, lowPriorityLinks, command.plates.Count > 1);
        GroupingManager.Instance.loadCommand = command;
        //ServiceRunner.GetService<UIService>().SetActive(false);
    }

    private async void LoadModels_LoadTime(ILoadCommand command, string vesselId, List<string> initialLinks, List<string> highPriorityLinks, List<string> lowPriorityLinks, bool _loadedCompexShip, bool enableForWorkflows = true)
    {
        Material materialIntended = materialToApply;
        //if (Application.isPlaying)
        //{
        //    ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(true, "Downloading Model");
        //}

        var settings = new ImportSettings
        {
            GenerateMipMaps = false,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = NameImportMethod.OriginalUnique
        };

        if(Application.isPlaying)
        {
            ApplicationStateMachine.Instance.GoToLoadingViewState(transform.name);
        }

        GameObject g = new GameObject("Root");
        g.transform.parent = transform;

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f);
        currentEntityCount = 0;
        targetEntityCount = 0;
        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f, true);
        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarActive(true, "Loading Compartments");
        bool success = await GetDownloadTasks(command, initialLinks, g, _loadedCompexShip);
        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f);
        currentEntityCount = 0;
        targetEntityCount = 0;
        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f, true);
        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarActive(true, "Loading Shell Model");
        bool success_high = await GetDownloadTasks(command, highPriorityLinks, g, _loadedCompexShip);
        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f);
        currentEntityCount = 0;
        targetEntityCount = 0;
        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f, true);
        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarActive(true, "Loading Structures");

        //if (Application.isPlaying)
        //{
        //    ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(true, "Getting structures ready");
        //}
        bool success_low = await GetDownloadTasks(command, lowPriorityLinks, g, _loadedCompexShip);

        stopwatch.Stop();
        Debug.Log("Time taken to load model: " + stopwatch.Elapsed);

        OnFinishedDownloading();

        if (success && success_high && success_low)
        {
            if (Application.isPlaying)
            {
                //ServiceRunner.GetService<LoadingScreenService>().SetLoadingMessage("Mapping and Initializing the Model");
                CommunicationManager.Instance.PreVesselLoad();

                if(enableForWorkflows)
                {
                    ServiceRunner.GetService<ComplexShipManager>().Initialize(transform.GetChild(0).gameObject, () =>
                    {
                        transform.GetChild(0).gameObject.SetActive(true);
                        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(false);
                        ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarActive(false);
                        //currentEntityCount = 0;
                        //targetEntityCount = 0;
                        CommunicationManager.Instance.PostVesselLoad(GroupingManager.Instance.vesselObject.GetJSON());
                        GameEvents.OnPostVesselLoaded?.Invoke(_loadedCompexShip);
                        ServiceRunner.GetService<FrameLabelFeature>().SpawnLabels();
                    }, !_loadedCompexShip, vesselId);
                }
                else
                {
                    transform.GetChild(0).gameObject.SetActive(true);
                    //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f);
                    ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(false);
                }

                foreach (var item in MyInstantiatorAddon.meshesWithNoData)
                {
                    item.SetActive(false);
                }
            }
        }
    }

    public async void LoadModelsRuntime(ILoadCommand command, string vesselId, List<string> incomingLinks, bool isloadingCompexShip, bool loadHidden = false, string message = "", Action OnComplete = null)
    {
        currentEntityCount = 0;
        targetEntityCount = 0;
        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f, true);
        //ServiceRunner.GetService<LoadingScreenService>().SetDownloadingBarProgress(0f);
        await LoadModels_Runtime(command, vesselId, incomingLinks, isloadingCompexShip, loadHidden, message, OnComplete);
    }

    private async Task LoadModels_Runtime(ILoadCommand command, string vesselId, List<string> incomingLinks, bool _loadedCompexShip, bool loadHidden = false, string message = "", Action OnComplete = null)
    {
        if(incomingLinks.Count == 0)
        {
            //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarProgress(0f);
            ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(false);
            return;
        }

        Material materialIntended = materialToApply;
        if(!message.IsNullOrEmpty())
        {
            //ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(true, $"{message}");
        }

        var settings = new ImportSettings
        {
            GenerateMipMaps = false,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = NameImportMethod.OriginalUnique
        };

        // List of tasks to download models concurrently
        List<Task<bool>> downloadTasks = new List<Task<bool>>();
        //ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarActive(true, "Loading Brackets and Stifferenrs");

#if !OLD_IMPLIMENTATION
        ImportAddonRegistry.RegisterImportAddon(new MetadataAddon());
#endif

        GameObject g = new GameObject("TempGameObject");
        g.SetActive(loadHidden);
        bool success = await GetDownloadTasks(command, incomingLinks, g, _loadedCompexShip);

        OnFinishedDownloading();

        if (success)
        {
            await RelationBuilder.Instance.BuildRelations(GroupingManager.Instance.vesselObject, g, vesselId, true);
            //ServiceRunner.GetService<LoadingScreenService>().SetLoadingScreenActive(false);
            ServiceRunner.GetService<LoadingScreenService>().SetLoadingBarActive(false);
            //targetEntityCount = 0;
            //currentEntityCount = 0;

            List<Transform> items = new List<Transform>();

            for (int i = 0; i < incomingLinks.Count; i++)
            {
                items.Add(g.transform.GetChild(i));
            }

            foreach (var item in items)
            {
                item.SetParent(transform.GetChild(0));
            }
            Destroy(g);
            OnComplete?.Invoke();
        }

        foreach (var item in MyInstantiatorAddon.meshesWithNoData)
        {
            item.SetActive(false);
        }
    }

    private void OnFinishedDownloading()
    {
        foreach (var item in disabled)
        {
            item?.gameObject.SetActive(true);
        }
        disabled.Clear();
    }
}

[System.Serializable]
public class CompartmentLinkEntity
{
    public string compartmentName;
    public string uid;
    public string link;
}

public interface ILoadCommand { }

[System.Serializable]
public class VesselLoadCommand : ILoadCommand
{
    public string compartment;
    public string shell;
    public string vesselId;
    public List<CompartmentLinkEntity> plates;
    public List<CompartmentLinkEntity> brackets;
    public List<CompartmentLinkEntity> stiffeners;

    public ILookup<string, string> platesGLBLookup => plates.ToLookup(p => p.uid, p => p.link);
    public ILookup<string, string> bracketsGLBLookup => brackets.ToLookup(b => b.uid, b => b.link);
    public ILookup<string, string> stiffenersGLBLookup => stiffeners.ToLookup(s => s.uid, s => s.link);
}

[System.Serializable]
public class BulkLoadCommand : ILoadCommand
{
    public string vesselId;
    public List<string> chunkLinks;
}