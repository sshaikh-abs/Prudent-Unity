#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LoadCommandWizard : EditorWindow
{
    private string assetName = "NewMyScriptableData";
    private string savePath = "Assets";
    private VesselLoadCommandSO tempObject;
    public string compartmentData;

    [MenuItem("Tools/ScriptableObject Creator")]
    public static void ShowWindow()
    {
        GetWindow<LoadCommandWizard>("Load Command Wizard");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create MyScriptableData", EditorStyles.boldLabel);

        assetName = EditorGUILayout.TextField("Asset Name", assetName);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        if(GUILayout.Button("Select Models Source"))
        {
            string path = EditorUtility.SaveFolderPanel("Select Models Folder", "Assets", "");
        }

        //string[] glbFiles = Directory.GetFiles(folderPath, "*.glb", SearchOption.TopDirectoryOnly)
        //                     .Select(Path.GetFileName) // Keep only file names with extension
        //                     .ToArray();

        if (GUILayout.Button("Create ScriptableObject"))
        {
            CreateAndSaveScriptableObject();
        }

        // Optional preview of temp object
        if (tempObject != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            Editor.CreateEditor(tempObject).OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }

    private void CreateAndSaveScriptableObject()
    {
        // Create instance
        var asset = CreateInstance<VesselLoadCommandSO>();

        // Ensure unique path
        string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{assetName}.asset");

        // Save asset
        AssetDatabase.CreateAsset(asset, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Keep a preview reference
        tempObject = asset;

        // Ping it in Project window
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        Debug.Log($"Created ScriptableObject at {fullPath}");
    }

    public CompartmentsList ExtractCompartments(string compartmentListJSON)
    {
        CompartmentsList compartmentsList = new CompartmentsList();
        compartmentData = compartmentListJSON;
        // Deserialize the JSON string into the CompartmentsList object
        compartmentsList = JsonUtility.FromJson<CompartmentsList>(compartmentData);
        // Log the compartments for debugging
        foreach (var compartment in compartmentsList.compartments)
        {
            Debug.Log($"Compartment Name: {compartment.compartmentName}, ID: {compartment.compartmentID}");
        }
        return compartmentsList;
    }
}


[System.Serializable]
public class CompartmentsList
{
    public List<CompartmentUIDPair> compartments = new List<CompartmentUIDPair>();
    public void AddCompartment(string name, string id)
    {
        compartments.Add(new CompartmentUIDPair(name, id));
    }
}

[System.Serializable]
public class CompartmentUIDPair
{
    public string compartmentName;
    public string compartmentID;
    public CompartmentUIDPair(string name, string id)
    {
        compartmentName = name;
        compartmentID = id;
    }
}
#endif