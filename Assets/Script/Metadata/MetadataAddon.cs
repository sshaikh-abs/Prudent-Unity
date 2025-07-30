using GLTFast.Addons;
using GLTFast;
using GLTFast.Loading;
using UnityEngine;
using GLTFast.Custom;
using GltfImport = GLTFast.Newtonsoft.GltfImport;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using GLTFast.Logging;
using GLTFast.Materials;

public class MetadataAddon : ImportAddon<MetadataAddonInstance> { }
public class MetadataAddonInstance : ImportAddonInstance
{
    GltfImport m_GltfImport;

    public override void Dispose() { }

    public override void Inject(GltfImportBase gltfImport)
    {
        var newtonsoftGltfImport = gltfImport as GltfImport;
        if (newtonsoftGltfImport == null)
            return;

        m_GltfImport = newtonsoftGltfImport;
        newtonsoftGltfImport.AddImportAddonInstance(this);
    }

    public override void Inject(IInstantiator instantiator)
    {
        var goInstantiator = instantiator as GameObjectInstantiator;
        if (goInstantiator == null)
            return;
        var _ = new MyInstantiatorAddon(m_GltfImport, goInstantiator);
    }

    public override bool SupportsGltfExtension(string extensionName)
    {
        return false;
    }
}

//public class CustomGltfImport : GltfImport
//{
//    public string targetAddress;
//    public CustomGltfImport(
//          IDownloadProvider downloadProvider = null,
//          IDeferAgent deferAgent = null,
//          IMaterialGenerator materialGenerator = null,
//          ICodeLogger logger = null
//      ) : base(downloadProvider, deferAgent, materialGenerator, logger) { }
//}

//public class CustomGltfImport : GltfImport
//{
//    public int GetTotalNodeCount()
//    {
//        int totalNodeCount = 0;

//        foreach (var scene in this.GetSourceRoot().Scenes)
//        {
//            foreach (var item in scene.nodes)
//            {
//                //var node = item as GLTFast.Newtonsoft.Schema.Node;
//                //var extras = node?.extras;

//                //if (extras != null)
//                //{
//                //    totalNodeCount++;
//                //}
//                ItrNodes(item, ref totalNodeCount);
//            }
//        }

//        return totalNodeCount;
//    }

//    private void ItrNodes(uint nodeIndex, ref int count)
//    {
//        var node = this.Root.Nodes[(int) nodeIndex] as GLTFast.Newtonsoft.Schema.Node;
//        count++;

//        string nodeName = node.name;

//        if (node.children != null)
//        {
//            foreach (var child in node.children)
//            {
//                ItrNodes(child, ref count);
//            }
//        }
//    }
//}

public class MyInstantiatorAddon
{
    public static Action OnNodeCreatedCallback;
    public static bool isCompartmentModel = false;
    public static Material materialOverride;
    public static List<GameObject> meshesWithNoData = new List<GameObject>();

    GltfImport m_GltfImport;
    GameObjectInstantiator m_Instantiator;

    public MyInstantiatorAddon(GltfImport gltfImport, GameObjectInstantiator instantiator)
    {
        m_GltfImport = gltfImport;
        m_Instantiator = instantiator;
        m_Instantiator.NodeCreated += OnNodeCreated;
        m_Instantiator.MeshAdded += OnMeshAdded;
        m_Instantiator.EndSceneCompleted += () =>
        {
            m_Instantiator.NodeCreated -= OnNodeCreated;
            m_Instantiator.MeshAdded -= OnMeshAdded;
        };
    }

    private void OnMeshAdded(GameObject gameObject, uint nodeIndex, string meshName, MeshResult meshResult, uint[] joints, uint? rootJoint, float[] morphTargetWeights, int meshNumeration)
    {
        #if CREATE_COLLIDERS_INITIALLY
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshResult.mesh;
        meshCollider.enabled = false;
        #endif

        OutlineSelectionHandler outlineSelectionHandler = gameObject.AddComponent<OutlineSelectionHandler>();

        if (isCompartmentModel)
        {
            gameObject.layer = LayerMask.NameToLayer("Simple_Default");
            outlineSelectionHandler.Initialize();
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = materialOverride;
        ColorCodeHandler.Instance.SetColor(meshRenderer, out Color assigendColor);
        outlineSelectionHandler.Initialize();
        AssignVertexColor(meshResult, assigendColor);
    }

    void AssignVertexColor(MeshResult meshResult, Color color)
    {
        Mesh mesh = meshResult.mesh;
        int vertexCount = mesh.vertexCount;
        Color[] colors = new Color[vertexCount];
        for (int i = 0; i < vertexCount; i++)
            colors[i] = color;
        mesh.colors = colors;
    }

    void OnNodeCreated(uint nodeIndex, GameObject gameObject)
    {
        // De-serialize glTF JSON
        var gltf = m_GltfImport.GetSourceRoot();

        var node = gltf.Nodes[(int)nodeIndex] as GLTFast.Newtonsoft.Schema.Node;
        var extras = node?.extras;

        if (extras == null)
        {
#if ENABLE_GAMEOBJECTS_AS_THEYLOAD
            gameObject.SetActive(true);
#endif
            return;
        }

        var metaDataComponent = gameObject.AddComponent<MetadataComponent>();
        OnNodeCreatedCallback?.Invoke();
        Metadata metadata = new Metadata();
        foreach (var item in extras.GetFullData())
        {
            metadata.AddMetaDataEntry(item.Key, item.Value.ToString());
        }
        metaDataComponent.SetMetadata(metadata);

#if ENABLE_GAMEOBJECTS_AS_THEYLOAD
        bool hasClass = metaDataComponent.ContainsKey("TYPE");
        string classValue = null;
        if (hasClass)
        {
            classValue = metaDataComponent.GetValue("TYPE").ToLower();
        }
        bool hasFunction = metaDataComponent.ContainsKey("FUNCTION");
        bool hasSubType = metaDataComponent.ContainsKey("SUB_TYPE");

        bool condition = hasClass && classValue == "hull part" && hasSubType;
        bool condition2 = hasClass && classValue == "compartment" && hasFunction && !node.name.Contains("_Parent");

        if (!condition && !condition2)
        {
            gameObject.SetActive(true);
        }
        else
        {
            GltfLoader.disabled.Add(gameObject);
            gameObject.SetActive(false);
        }
#endif
    }
}

//public class ProgressDownloadProvider : IDownloadProvider
//{
//    public event Action<float> OnProgress;

//    public long totalContentLength { get; private set; } = -1L;
//    public long totalRead { get; private set; } = 0L;

//    public async Task<IDownload> Request(Uri url)
//    {
//        if (url == null)
//        {
//            Debug.LogError("URL is null.");
//            return null;
//        }

//        if (url.Scheme == Uri.UriSchemeHttp || url.Scheme == Uri.UriSchemeHttps)
//        {
//            return await DownloadFromWebAsync(url);
//        }
//        else if (url.Scheme == Uri.UriSchemeFile || url.Scheme == "")
//        {
//            return await LoadFromFileAsync(url.LocalPath);
//        }
//        else
//        {
//            Debug.LogError($"Unsupported URI scheme: {url.Scheme}");
//            return null;
//        }
//    }

//    public async Task<IDownload> RequestTexture(Uri url, bool nonReadable)
//    {
//        return await Request(url); // Reuse the same logic
//    }

//    private async Task<IDownload> DownloadFromWebAsync(Uri url)
//    {
//        using var httpClient = new HttpClient();

//        try
//        {
//            var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
//            if (!response.IsSuccessStatusCode)
//            {
//                Debug.LogError($"Failed to download {url}. Status: {response.StatusCode}");
//                return null;
//            }

//            var contentLength = response.Content.Headers.ContentLength ?? -1L;
//            if(response.Content.Headers.ContentLength != null)
//            {
//                totalContentLength += contentLength;
//            }
//            var stream = await response.Content.ReadAsStreamAsync();
//            var buffer = new byte[8192];
//            int bytesRead;
//            //long totalRead = 0;

//            using var memStream = new MemoryStream();
//            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
//            {
//                memStream.Write(buffer, 0, bytesRead);
//                totalRead += bytesRead;

//                if (contentLength > 0)
//                {
//                    //OnProgress?.Invoke((float)totalRead / contentLength);
//                    OnProgress?.Invoke((float)totalRead / totalContentLength);
//                }
//            }

//            return new CustomDownload(memStream.ToArray());
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"Exception downloading {url}: {e.Message}");
//            return null;
//        }
//    }

//    private async Task<IDownload> LoadFromFileAsync(string path)
//    {
//        try
//        {
//            if (!File.Exists(path))
//            {
//                Debug.LogError($"Local file not found: {path}");
//                return null;
//            }

//            var data = await File.ReadAllBytesAsync(path);
//            OnProgress?.Invoke(1f);
//            return new CustomDownload(data);
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"Error reading file at {path}: {e.Message}");
//            return null;
//        }
//    }

//    Task<ITextureDownload> IDownloadProvider.RequestTexture(Uri url, bool nonReadable)
//    {
//        throw new NotImplementedException();
//    }
//}

//public class CustomDownload : IDownload
//{
//    public byte[] Data { get; }
//    public string Text { get; }
//    public bool Success { get; }
//    public string Error { get; }
//    public bool? IsBinary { get; }

//    public CustomDownload(byte[] data)
//    {
//        Data = data;
//        Success = true;
//        Error = null;

//        // Determine if the data is binary by checking for the 'glTF' magic number
//        if (data.Length >= 4 && data[0] == 0x67 && data[1] == 0x6C && data[2] == 0x54 && data[3] == 0x46)
//        {
//            IsBinary = true;
//            Text = null;
//        }
//        else
//        {
//            IsBinary = false;
//            try
//            {
//                Text = Encoding.UTF8.GetString(data);
//            }
//            catch (Exception ex)
//            {
//                Text = null;
//                Success = false;
//                Error = $"Failed to decode text: {ex.Message}";
//            }
//        }
//    }

//    public CustomDownload(string error)
//    {
//        Data = null;
//        Text = null;
//        Success = false;
//        Error = error;
//        IsBinary = null;
//    }

//    public void Dispose()
//    {
//        // No unmanaged resources to release, but method is required by IDisposable
//    }
//}
