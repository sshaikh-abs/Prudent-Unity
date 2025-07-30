using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class CommunicationTester : EditorWindow
{
    [MenuItem("Tools/Run Communication Tester")]
    public static void ShowWindow()
    {
        GetWindow<CommunicationTester>("Communication Tester");
    }

    string input = "";

    void OnGUI()
    {
        if(!Application.isPlaying || ApplicationStateMachine.Instance.currentStateName == nameof(LoadingViewState))
        {
            EditorGUILayout.LabelField("Please load the vessel first");
            return;
        }

        input = EditorGUILayout.TextField("Input", input);

        GUILayout.BeginVertical("box"); // Start the box
        EditorGUILayout.LabelField("Gauge Points");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(nameof(CommunicationManager.AutoGaugeCompartment_Extern)))
        {
            string path = EditorUtility.OpenFilePanel("Condition Data", Application.dataPath, "json");
            string jsonData = System.IO.File.ReadAllText(path);
            CommunicationManager.Instance.AutoGaugeCompartment_Extern(jsonData);
        }
        if (GUILayout.Button(nameof(CommunicationManager.SpawnGaugePoints_Extern)))
        {
            string path = EditorUtility.OpenFilePanel("Gauge Plan", Application.dataPath, "json");
            string jsonData = System.IO.File.ReadAllText(path);
            CommunicationManager.Instance.SpawnGaugePoints_Extern(jsonData);
        }
        GUI.enabled = !input.IsNullOrEmpty();
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.HandleGaugePointMThicknessUpdate_Extern), "input : false/true")))
        {
            CommunicationManager.Instance.HandleGaugePointMThicknessUpdate_Extern(input);
        }
        GUI.enabled = true;
        if (GUILayout.Button(nameof(CommunicationManager.ClearGaugePoints_Extern)))
        {
            CommunicationManager.Instance.ClearGaugePoints_Extern();
        }
        if (GUILayout.Button(nameof(CommunicationManager.HandleCaptureIamges_Extern)))
        {
            //string path = EditorUtility.OpenFilePanel("Gauge Plan", Application.dataPath, "json");
            //string jsonData = System.IO.File.ReadAllText(path);
            CommunicationManager.Instance.HandleCaptureIamges_Extern(/*jsonData*/);
        }
        if (GUILayout.Button(nameof(CommunicationManager.ShowConditionStatus_Extern)))
        {
            string path = EditorUtility.OpenFilePanel("Condition Data", Application.dataPath, "json");
            string jsonData = System.IO.File.ReadAllText(path);
            CommunicationManager.Instance.ShowConditionStatus_Extern(jsonData);
        }
        GUI.enabled = !input.IsNullOrEmpty();
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.SetGaugePointLabelsActive), "input : false/true")))
        {
            CommunicationManager.Instance.SetGaugePointLabelsActive(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.SetGaugePointViewActive), "input : false/true")))
        {
            CommunicationManager.Instance.SetGaugePointViewActive(input);
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box"); // Start the box
        EditorGUILayout.LabelField("Corrosion");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(nameof(CommunicationManager.ShowPlateCorrosion_Extern)))
        {
            string path = EditorUtility.OpenFilePanel("Plate Corrosion Data", Application.dataPath, "json");
            string jsonData = System.IO.File.ReadAllText(path);
            CommunicationManager.Instance.ShowPlateCorrosion_Extern(jsonData);
        }
        if (GUILayout.Button(nameof(CommunicationManager.HideCorrosion_Extern)))
        {
            CommunicationManager.Instance.HideCorrosion_Extern();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box"); // Start the box
        EditorGUILayout.LabelField("Frame Labels");
        GUILayout.BeginHorizontal();
        GUI.enabled = !input.IsNullOrEmpty();
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.HandleFrameLables_Extern), "input : 0/1")))
        {
            CommunicationManager.Instance.HandleFrameLables_Extern(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.HandleFrameLablesDensity_Extern), "input : float")))
        {
            CommunicationManager.Instance.HandleFrameLablesDensity_Extern(input);
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box"); // Start the box
        EditorGUILayout.LabelField("Parts");
        GUILayout.BeginHorizontal();
        GUI.enabled = !input.IsNullOrEmpty();
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.Clicked_Extern), "input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>")))
        {
            CommunicationManager.Instance.Clicked_Extern(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.Show_Extern), "input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>")))
        {
            CommunicationManager.Instance.Show_Extern(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.Hide_Extern), "input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>")))
        {
            CommunicationManager.Instance.Hide_Extern(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.GetMetadataTable_Extern), "input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>")))
        {
            CommunicationManager.Instance.GetMetadataTable_Extern(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.GetDisplayMetadataTable_Extern), "input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>")))
        {
            CommunicationManager.Instance.GetDisplayMetadataTable_Extern(input);
        }
        if (GUILayout.Button(new GUIContent(nameof(CommunicationManager.SetGaugePointViewActive_Extern), "input : <Compartment Name>/<Frame Name>/<Plate Name(optional)>")))
        {
            CommunicationManager.Instance.SetGaugePointViewActive_Extern(input);
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box"); // Start the box
        EditorGUILayout.LabelField("State Treversal");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(nameof(CommunicationManager.EnterCompartmentSelectionMode_Extern)))
        {
            CommunicationManager.Instance.EnterCompartmentSelectionMode_Extern();
        }
        if (GUILayout.Button(nameof(CommunicationManager.ExitCompartmentSelectionMode_Extern)))
        {
            CommunicationManager.Instance.ExitCompartmentSelectionMode_Extern();
        }
        if (GUILayout.Button(nameof(CommunicationManager.ToggleConditionManagerFlow_Extern)))
        {
            CommunicationManager.Instance.ToggleConditionManagerFlow_Extern();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();


        GUILayout.BeginVertical("box"); // Start the box
        EditorGUILayout.LabelField("State Treversal");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(nameof(CommunicationManager.BeginMarkupDrawing_Extern)))
        {
            CommunicationManager.Instance.BeginMarkupDrawing_Extern();
        }
        if (GUILayout.Button(nameof(CommunicationManager.EndMarkupDrawing_Extern)))
        {
            CommunicationManager.Instance.EndMarkupDrawing_Extern();
        }
        GUI.enabled = !input.IsNullOrEmpty();
        if (GUILayout.Button(nameof(CommunicationManager.ToggleMarkup_Extern)))
        {
            CommunicationManager.Instance.ToggleMarkup_Extern(input);
        }
        if (GUILayout.Button(nameof(CommunicationManager.SetMarkupShapeType_Extern)))
        {
            CommunicationManager.Instance.SetMarkupShapeType_Extern(input);
        }
        GUI.enabled = true;
        if (GUILayout.Button(nameof(CommunicationManager.LoadMarkups_Extern)))
        {
            string path = EditorUtility.OpenFilePanel("Markups Data", Application.dataPath, "json");
            string jsonData = System.IO.File.ReadAllText(path);
            CommunicationManager.Instance.LoadMarkups_Extern(jsonData);
        }
        if (GUILayout.Button(nameof(CommunicationManager.DeleteAllMarkups)))
        {
            CommunicationManager.Instance.DeleteAllMarkups();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
}
#endif // UNITY_EDITOR
