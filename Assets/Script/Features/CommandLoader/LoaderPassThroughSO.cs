using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New Loader Passthrough", menuName = "ScriptableObjects/Load Command/Loader Passthrough", order = 1)]
public class LoaderPassThroughSO : ScriptableObject
{
    public List<BaseLoadCommandSO> blobLoadCommands;
    public List<BaseLoadCommandSO> localLoadCommands;

    public List<BaseLoadCommandSO> allCommands => blobLoadCommands.Concat(localLoadCommands).OrderBy(c => c.name).ToList();

    [HideInInspector] public string command;
    [HideInInspector] public bool jsonOutputFoldout;

    public ILoadCommand GetCommandSO()
    {
        return allCommands.FirstOrDefault(c => c.name == command).data;
    }

    public string GetJSON()
    {
        return JsonUtility.ToJson(GetCommandSO(), true);
    }

    public void ExportJSONAsFile()
    {
#if UNITY_EDITOR
        string json = GetJSON();
        string path = EditorUtility.SaveFilePanel("Save JSON", "", command + ".json", "json");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        System.IO.File.WriteAllText(path, json);
        AssetDatabase.Refresh();
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LoaderPassThroughSO))]
public class LoaderPassThroughSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Get the target object
        LoaderPassThroughSO myTarget = (LoaderPassThroughSO)target;

        // Get current index
        if(myTarget.allCommands == null || myTarget.allCommands.Count == 0)
        {
            return;
        }

        var options = myTarget.allCommands.Select(c => c.name).ToArray();

        if(options.Length == 0)
        {
            return;
        }

        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(options, myTarget.command));
        selectedIndex = EditorGUILayout.Popup("Selected Option", selectedIndex, options);
        myTarget.command = options[selectedIndex];

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(myTarget);
        }

        if (GUILayout.Button("Export JSON"))
        {
            myTarget.ExportJSONAsFile();
        }

        myTarget.jsonOutputFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(myTarget.jsonOutputFoldout, "JSON Output");
        if(myTarget.jsonOutputFoldout)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(myTarget.GetJSON());
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}
#endif