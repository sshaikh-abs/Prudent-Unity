using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Xenon;

#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "New Seamlines Config", menuName = "ScriptableObjects/Configs/Seamlines Config", order = 1)]
public class SeamlinesConfig : ScriptableObject
{
    public Material OverrideMaterial;
    public Material OverrideMaterial_Wireframe;
    public RenderingLayerMask RenderingLayerMask = 0;

    public float OutlineScale = 1f;
    public float RobertsCrossMultiplier = 100;
    public float DepthThreshold = 10f;
    public float NormalThreshold = 0.4f;
    public float SteepAngleThreshold = 0.2f;
    public float SteepAngleMultiplier = 25f;

    public List<OutlinesEntry> outlines;
    public OutlinesEntry defaultOutlines;

    public void ToggleForWireframe(bool active)
    {
        defaultOutlines.renderFeature.SetActive(!active);

        foreach (var item in outlines)
        {
            item.renderFeature.SetActive(active);
        }
    }

    public void InjectData()
    {
        foreach (var item in outlines)
        {
            item.renderFeature.MaterialSettings.OutlineScale = OutlineScale;
            item.renderFeature.MaterialSettings.RobertsCrossMultiplier = RobertsCrossMultiplier;
            item.renderFeature.MaterialSettings.DepthThreshold = DepthThreshold;
            item.renderFeature.MaterialSettings.NormalThreshold = NormalThreshold;
            item.renderFeature.MaterialSettings.SteepAngleThreshold = SteepAngleThreshold;
            item.renderFeature.MaterialSettings.SteepAngleMultiplier = SteepAngleMultiplier;
            item.renderFeature.MaterialSettings.OutlineColor = item.Color;
            item.renderFeature.MaterialSettings.useBrandColors = true;

            item.renderFeature.FeatureSettings.RenderPassEvent = item.RenderPassEvent;
            item.renderFeature.FeatureSettings.LayerMask = item.LayerMask;
            item.renderFeature.FeatureSettings.RenderingLayerMask = RenderingLayerMask;

            item.renderFeature.FeatureSettings.OverrideMaterial = OverrideMaterial;
            item.renderFeature.FeatureSettings.OverrideMaterial_Wireframe = OverrideMaterial_Wireframe;
            item.renderFeature.FeatureSettings.BlitMaterial = item.BlitMaterial;

            item.renderFeature.FeatureSettings.ClearDepth = true;
            item.renderFeature.FeatureSettings.usePlatesAsReference = item.usePlatesAsReference;

#if UNITY_EDITOR
            EditorUtility.SetDirty(item.renderFeature);
#endif
        }

        if(defaultOutlines == null)
        {
            return;
        }

        defaultOutlines.renderFeature.MaterialSettings.OutlineScale = OutlineScale;
        defaultOutlines.renderFeature.MaterialSettings.RobertsCrossMultiplier = RobertsCrossMultiplier;
        defaultOutlines.renderFeature.MaterialSettings.DepthThreshold = DepthThreshold;
        defaultOutlines.renderFeature.MaterialSettings.NormalThreshold = NormalThreshold;
        defaultOutlines.renderFeature.MaterialSettings.SteepAngleThreshold = SteepAngleThreshold;
        defaultOutlines.renderFeature.MaterialSettings.SteepAngleMultiplier = SteepAngleMultiplier;
        defaultOutlines.renderFeature.MaterialSettings.OutlineColor = defaultOutlines.Color;
        defaultOutlines.renderFeature.MaterialSettings.useBrandColors = true;

        defaultOutlines.renderFeature.FeatureSettings.RenderPassEvent = defaultOutlines.RenderPassEvent;
        defaultOutlines.renderFeature.FeatureSettings.LayerMask = defaultOutlines.LayerMask;
        defaultOutlines.renderFeature.FeatureSettings.RenderingLayerMask = RenderingLayerMask;

        defaultOutlines.renderFeature.FeatureSettings.OverrideMaterial = OverrideMaterial;
        defaultOutlines.renderFeature.FeatureSettings.OverrideMaterial_Wireframe = OverrideMaterial_Wireframe;
        defaultOutlines.renderFeature.FeatureSettings.BlitMaterial = defaultOutlines.BlitMaterial;

        defaultOutlines.renderFeature.FeatureSettings.ClearDepth = true;
        defaultOutlines.renderFeature.FeatureSettings.usePlatesAsReference = defaultOutlines.usePlatesAsReference;

#if UNITY_EDITOR
        EditorUtility.SetDirty(defaultOutlines.renderFeature);
#endif
    }
}

[System.Serializable]
public class OutlinesEntry
{
    public OutlineRenderFeature renderFeature;
    public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    public LayerMask LayerMask = 0;
    public bool usePlatesAsReference;
    public Color Color;
    public Material BlitMaterial;
}

#if UNITY_EDITOR
[CustomEditor(typeof(SeamlinesConfig))]
public class SeamlinesConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Get the target object
        SeamlinesConfig myTarget = (SeamlinesConfig)target;

        if (GUILayout.Button("Inject to RP"))
        {
            myTarget.InjectData();
        }
    }
}
#endif