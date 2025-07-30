using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Xenon
{
    public class OutlineRenderFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;

            public LayerMask LayerMask = 0;

            public RenderingLayerMask RenderingLayerMask = 0;

            public Material OverrideMaterial;
            public Material OverrideMaterial_Wireframe;

            public Material BlitMaterial;

            public bool ClearDepth;
            public bool usePlatesAsReference;
        }

        [Serializable]
        public class OutlineSettings
        {
            public float OutlineScale = 1f;
            public float RobertsCrossMultiplier = 100;
            public float DepthThreshold = 10f;
            public float NormalThreshold = 0.4f;
            public float SteepAngleThreshold = 0.2f;
            public float SteepAngleMultiplier = 25f;
            public Color OutlineColor = Color.white;
            public bool useBrandColors = true;
            public bool obeyTransparency = false;
        }

        public class OutlineData : ContextItem
        {
            public TextureHandle FilterTextureHandle;

            public override void Reset()
            {
                FilterTextureHandle = TextureHandle.nullHandle;
            }
        }

        public Settings FeatureSettings;
        public OutlineSettings MaterialSettings;

        public static OutlineSettings globlaMaterialSettings;

        private OutlinePassFilter _outlinePassFilter;
        private OutlinePassFinal _outlinePassFinal;

        public override void Create()
        {
            if (MaterialSettings.useBrandColors)
            {
                globlaMaterialSettings = MaterialSettings;
            }
            _outlinePassFilter = new OutlinePassFilter(FeatureSettings);
            _outlinePassFinal = new OutlinePassFinal(FeatureSettings, MaterialSettings);
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(MaterialSettings.obeyTransparency)
            {
                if (ShaderUpdater.Instance != null)
                {
                    switch (ShaderUpdater.Instance.DisplayMode)
                    {
                        case DisplayMode.Wireframe:
                        case DisplayMode.Transparency:
                            _outlinePassFilter.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                            _outlinePassFinal.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                            break;
                        case DisplayMode.Opaque:
                            _outlinePassFilter.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                            _outlinePassFinal.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                            break;
                        default:
                            break;
                    }
                }
            }

            renderer.EnqueuePass(_outlinePassFilter);
            renderer.EnqueuePass(_outlinePassFinal);
        }
    }
}