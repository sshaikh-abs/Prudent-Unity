using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DisplayMode
{
    Transparency,
    Opaque,
    Wireframe
}

public class ShaderUpdater : SingletonMono<ShaderUpdater>
{
    public UnityEngine.Material material;
    public UnityEngine.Material material_alpha;
    public UnityEngine.Material material_Lit;
    private Renderer[] allRenderers;

    public SeamlinesConfig seamlinesConfig; 

    public DisplayMode DisplayMode 
    {
        get
        {
            return displayMode;
        }
        set
        {
            displayMode = value;

            switch (displayMode)
            {
                default:
                case DisplayMode.Opaque:
                case DisplayMode.Transparency:
                    UpdateRenderQue();
                    seamlinesConfig.ToggleForWireframe(false);
                    break;
                case DisplayMode.Wireframe:
                    UpdateRenderQueueTransparency();
                    seamlinesConfig.ToggleForWireframe(ApplicationStateMachine.Instance.currentStateName != nameof(SimpleVesselViewState));
                    break;
            }
        }
    }
    private DisplayMode displayMode = DisplayMode.Opaque;

    public UnityEngine.Material materialComparer => (GroupingManager.Instance.isCompartmentModel || ApplicationStateMachine.Instance.currentStateName == nameof(SimpleVesselViewState)) ? material_Lit : material;

    public void CollectAllRenderers()
    {
        allRenderers = GroupingManager.Instance.vesselGameobject.GetComponentsInChildren<Renderer>(true);
    }

    //public void UpdateRenderQue()
    //{
    //    Debug.Log("Alpha queue");
    //    //Renderer[] allRenderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
    //    //material.renderQueue = 2100; // Set render queue value

    //    foreach (Renderer rend in allRenderers)
    //    {
    //        // Check if the renderer's material is the one you want to modify
    //        //rend.material.renderQueue = 2100;
    //        //rend.material.SetFloat("_SurfaceType", 1);
    //        Color copiedColor = rend.material.GetColor("_Color");
    //        rend.material.CopyPropertiesFromMaterial(materialComparer);
    //        rend.material.SetColor("_Color", copiedColor);
    //    }
    //}
    //public void UpdateRenderQueueTransparency()
    //{
    //    Debug.Log("Transparency queue");
    //    //Renderer[] allRenderers = GameObject.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
    //    //material.renderQueue = 3000; // Set render queue value

    //    foreach (Renderer rend in allRenderers)
    //    {
    //        if (rend.sharedMaterial.shader == materialComparer.shader)
    //        {
    //            //rend.material.renderQueue = 3000;
    //            //rend.material.SetFloat("_SurfaceType", 0);

    //            Color copiedColor = rend.material.GetColor("_Color");
    //            rend.material.CopyPropertiesFromMaterial(materialComparer_Alpha);
    //            rend.material.SetColor("_Color", copiedColor);
    //        }
    //    }
    //}

    private void UpdateRenderQue()
    {
        //return;
        UpdateShadersBasedOnDisplatMode(GroupingManager.Instance.vesselObject.renderersCollection/* allRenderers.ToList()*/);
    }

    private void UpdateRenderQueueTransparency()
    {
        //return;
        UpdateShadersBasedOnDisplatMode(GroupingManager.Instance.vesselObject.renderersCollection/* allRenderers.ToList()*/);
    }


    public void UpdateShadersBasedOnDisplatMode(List<Renderer> renderers)
    {
        foreach (Renderer rend in renderers)
        {
            try
            {
                if (rend.sharedMaterial.shader == (displayMode == DisplayMode.Opaque) ? material_alpha.shader : materialComparer.shader)
                {
                    Color copiedColor = rend.material.GetColor("_Color");
                    rend.material.CopyPropertiesFromMaterial((displayMode == DisplayMode.Opaque) ? materialComparer : material_alpha);
                    rend.material.SetColor("_Color", copiedColor);
                }
            }
            catch
            {
                Debug.Log("Something Went Wrong");
            }
        }
    }
}