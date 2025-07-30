using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;


namespace Xenon
{
    public class OutlinePassFinal : ScriptableRenderPass
    {
        private class PassData
        {
            internal TextureHandle FilterTextureHandle;
            internal TextureHandle OpaqueTextureHandle;
            internal bool UsePlatesAsReference;
            internal bool UseBrandColors;
            internal Material Material;
        }

        private static readonly int FilterTexture = Shader.PropertyToID("_FilterTexture");
        private static readonly int SeemlinesVisibility = Shader.PropertyToID("_SeemlinesVisibility");
        private static readonly int OutlineScale = Shader.PropertyToID("_OutlineScale");
        private static readonly int RobertsCrossMultiplier = Shader.PropertyToID("_RobertsCrossMultiplier");
        private static readonly int DepthThreshold = Shader.PropertyToID("_DepthThreshold");
        private static readonly int NormalThreshold = Shader.PropertyToID("_NormalThreshold");
        private static readonly int SteepAngleThreshold = Shader.PropertyToID("_SteepAngleThreshold");
        private static readonly int SteepAngleMultiplier = Shader.PropertyToID("_SteepAngleMultiplier");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        private static readonly int ViewMode = Shader.PropertyToID("_ViewMode");
        private static readonly int UseBrandColor = Shader.PropertyToID("_UseBrandColor");

        private readonly Material _blitMaterial;
        private readonly bool usePlatesAsReference = false;
        private readonly bool useBrandColors = false;

        public float actualOutlineScale;
        public static bool inScreenshotMode = false;

        public OutlinePassFinal(OutlineRenderFeature.Settings settings, OutlineRenderFeature.OutlineSettings outlineSettings)
        {
            renderPassEvent = settings.RenderPassEvent;
            _blitMaterial = settings.BlitMaterial;
            usePlatesAsReference = settings.usePlatesAsReference;
            useBrandColors = outlineSettings.useBrandColors;

            if (_blitMaterial == null)
                return;

            actualOutlineScale = outlineSettings.OutlineScale;
            _blitMaterial.SetFloat(RobertsCrossMultiplier, outlineSettings.RobertsCrossMultiplier);
            _blitMaterial.SetFloat(DepthThreshold, outlineSettings.DepthThreshold);
            _blitMaterial.SetFloat(NormalThreshold, outlineSettings.NormalThreshold);
            _blitMaterial.SetFloat(SteepAngleThreshold, outlineSettings.SteepAngleThreshold);
            _blitMaterial.SetFloat(SteepAngleMultiplier, outlineSettings.SteepAngleMultiplier);
            _blitMaterial.SetColor(OutlineColor, outlineSettings.OutlineColor);
        }

        private static void ExecutePass(PassData passData, RasterGraphContext context)
        {
            if (passData.Material != null)
            {
                float viewMode = 1f;

                if(ShaderUpdater.Instance != null)
                {
                    switch (ShaderUpdater.Instance.DisplayMode)
                    {
                        case DisplayMode.Transparency:
                            viewMode = 1f;
                            break;
                        case DisplayMode.Opaque:
                            viewMode = 0f;
                            break;
                        case DisplayMode.Wireframe:
                            viewMode = 2f;
                            break;
                        default:
                            break;
                    }
                }

                //if(ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentSelectionViewState))
                //{
                //    viewMode = 2f;
                //}

                passData.Material.SetTexture(FilterTexture, passData.FilterTextureHandle);
                passData.Material.SetFloat(ViewMode, viewMode);

                if (passData.UsePlatesAsReference && Application.isPlaying)
                {
                    passData.Material.SetFloat(UseBrandColor, passData.UseBrandColors && ApplicationStateMachine.Instance.currentStateName != nameof(SimpleVesselViewState) && ApplicationStateMachine.Instance.currentStateName != nameof(CompartmentSelectionViewState) ? 1f : 0f);
                    passData.Material.SetFloat(SeemlinesVisibility, CommunicationManager.Instance.platesActive ? 1f : 0f);
                    //Debug.Log("BrandColor Status : " + passData.Material.GetFloat(UseBrandColor));
                }
            }

            Blitter.BlitTexture(context.cmd, passData.FilterTextureHandle, new Vector4(1, 1, 0, 0), passData.Material, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var outlineData = frameData.Get<OutlineRenderFeature.OutlineData>();

            using var builder = renderGraph.AddRasterRenderPass<PassData>("OutlinePass_Final", out var passData, new ProfilingSampler("OutlinePass_Final"));

            passData.UseBrandColors = useBrandColors;
            passData.UsePlatesAsReference = usePlatesAsReference;

            if (!outlineData.FilterTextureHandle.IsValid())
                return;

            if (_blitMaterial == null)
                return;

            float targetOutlineScale = actualOutlineScale;

            if (ShaderUpdater.Instance != null)
            {
                switch (ShaderUpdater.Instance.DisplayMode)
                {
                    case DisplayMode.Transparency:
                    case DisplayMode.Opaque:
                        targetOutlineScale = Mathf.Max(1f, actualOutlineScale);
                        break;
                    case DisplayMode.Wireframe:
                    default:
                        break;
                }
            }

            if(inScreenshotMode)
            {
                targetOutlineScale = 1f;
            }

            _blitMaterial.SetFloat(OutlineScale, targetOutlineScale);

            passData.Material = _blitMaterial;
            passData.FilterTextureHandle = outlineData.FilterTextureHandle;

            builder.AllowPassCulling(false);
            builder.UseTexture(passData.FilterTextureHandle);
            builder.SetRenderAttachment(resourceData.cameraColor, index: 0);
            builder.SetRenderFunc<PassData>(ExecutePass);
        }
    }
}