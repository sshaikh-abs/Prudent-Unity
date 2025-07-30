using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorCodeHandler : SingletonMono<ColorCodeHandler>
{
    public Color defaultColor;
    private bool isColorsChanged = false;

    public string JsonString = "";

    public List<ColorGroup> colorGroups = null ;
    public List<LayerGroup> layerGroups = null;

    private void Start()
    {
        updateColors(JsonString);
    }

    [ContextMenu(nameof(PrintJson))]
    public void PrintJson()
    {
        ColorGroupList colorGroupList = new ColorGroupList();
        colorGroupList.colorGroups = colorGroups;
        string jsonString = JsonUtility.ToJson(colorGroupList);
        Debug.Log(jsonString);
        JsonString = jsonString;
    }

    [ContextMenu("LoadFromInitString")]
   public void LoadFromInitString(string json)
   {
        LoadColorGroupsFromJson(json);
        foreach (var group in colorGroups)
        {
            group.SetColor(group.Color);
        }
   }

    public void OnValidate()
    {
        foreach (var group in colorGroups)
        {
            group.SetColor(group.pureColor);
        }
    }

    public void LoadColorGroupsFromJson(string JsonString)
    {
        colorGroups = new List<ColorGroup>();

        ColorGroupList colorGroupList = JsonUtility.FromJson<ColorGroupList>(JsonString);

        if (colorGroupList != null)
        {
            colorGroups = colorGroupList.colorGroups;
        }
        else
        {
            Debug.LogError("Failed to load color groups from JSON.");
        }
    }

    public void updateColors(string json)
    {
        LoadFromInitString(json);
        JsonString = json;
        ToggleChangeColors();
    }
    /// <summary>
    /// Depricated, changed this implimenation to as object loads instead of a post pass.
    /// </summary>
    public void ToggleChangeColors()
    {
        LoadColorGroupsFromJson(JsonString);

        isColorsChanged = !isColorsChanged;

        MeshRenderer[] allMeshRenderers = GroupingManager.Instance.vesselGameobject.GetComponentsInChildren<MeshRenderer>(true);

        // Filter out the ones with "outermostedge" in their name
        var filteredMeshRenderers = allMeshRenderers
            .Where(renderer => !renderer.name.Contains("OutermostEdges"))
            .ToArray();

        //if (!isColorsChanged)
        //{
        //    SetAllMeshesColor(Color.white);
        //    return;
        //}

        filteredMeshRenderers.ToList().ForEach(meshRenderer =>
        {
            SetColor(meshRenderer, out Color assigendColor);
        });        
    }

    public void SetColor(MeshRenderer meshRenderer, out Color assignedColor)
    {
        if (colorGroups == null)
        {
            LoadColorGroupsFromJson(JsonString);
        }

        Color? matchedColor = FindMatchingColor(meshRenderer.transform);
        LayerMask? matchedLayer = FindMatchingLayer(meshRenderer.transform);

        if (matchedLayer != null)
        {
            meshRenderer.gameObject.layer = matchedLayer.Value;
        }

        assignedColor = matchedColor ?? defaultColor;
        meshRenderer.material.color = assignedColor;
        ShaderUpdater.Instance.UpdateShadersBasedOnDisplatMode(new List<Renderer> { meshRenderer });
    }

    private LayerMask? FindMatchingLayer(Transform parent)
    {
        if (parent == null || parent.GetComponent<MetadataComponent>() == null)
        {
            return null;
        }

        MetadataComponent metaData = parent.GetComponent<MetadataComponent>();
        string checker = "";

        SubpartType type = SubpartType.Plate;

        using (TestFpsoSubpartTypeSolver testFpsoSubpartTypeSolver = new TestFpsoSubpartTypeSolver())
        {
            type = testFpsoSubpartTypeSolver.SolveForSubparType(metaData);
        }

        if (!metaData.ContainsKey("POSITION"))
        {
            return null;
        }

        switch (type)
        {
            case SubpartType.Plate:
                checker = metaData.GetValue("POSITION");
                break;
            case SubpartType.Bracket:
                checker = "Bracket";
                break;
            case SubpartType.Stiffener:
                checker = "Stiffener";
                break;
            default:
                break;
        }

        var matchingGroup = layerGroups.FirstOrDefault(group => checker.ToLower().Contains(group.PartType.ToLower()) || checker.ToLower().Equals(group.PartType.ToLower()));

        if (matchingGroup == null)
        {
            //Debug.LogWarning($"Couldn't find the color for {metaData.GetValue("PRT_NAME")}, could be an incorrect position entry", metaData);
            return LayerMask.NameToLayer("Default");
        }

        if (!string.IsNullOrEmpty(matchingGroup.PartType))
        {
            return LayerMask.NameToLayer(matchingGroup.layer);
        }

        return LayerMask.NameToLayer("Default");
    }

    private Color? FindMatchingColor(Transform parent)
    {
        if (parent == null || parent.GetComponent<MetadataComponent>() == null)
        {
            return null;
        }

        MetadataComponent metaData = parent.GetComponent<MetadataComponent>();
        string checker = "";

        SubpartType type = SubpartType.Plate;

        using (TestFpsoSubpartTypeSolver testFpsoSubpartTypeSolver = new TestFpsoSubpartTypeSolver())
        {
            type = testFpsoSubpartTypeSolver.SolveForSubparType(metaData);
        }

        if(!metaData.ContainsKey("POSITION"))
        {
            return null;
        }

        switch (type)
        {
            case SubpartType.Plate:
                checker = metaData.GetValue("POSITION");
                break;
            case SubpartType.Bracket:
                checker = "Bracket";
                break;
            case SubpartType.Stiffener:
                checker = "Stiffener";
                break;
            default:
                break;
        }

        var matchingGroup = colorGroups.FirstOrDefault(group => checker.ToLower().Contains(group.PartType.ToLower()) || checker.ToLower().Equals(group.PartType.ToLower()));

        if (matchingGroup == null)
        {
            //Debug.LogWarning($"Couldn't find the color for {metaData.GetValue("PRT_NAME")}, could be an incorrect position entry", metaData);
            return defaultColor;
        }

        if (!string.IsNullOrEmpty(matchingGroup.PartType))
        {
            if (ColorUtility.TryParseHtmlString(matchingGroup.Color, out Color color))
            {
                return color;
            }
            else {
                return defaultColor;
            }
        }

        return FindMatchingColor(parent.parent);
    }


    private void SetAllMeshesColor(Color color)
    {      

        MeshRenderer[] allMeshRenderers = GroupingManager.Instance.vesselGameobject.GetComponentsInChildren<MeshRenderer>();

        var filteredMeshRenderers = allMeshRenderers
           .Where(renderer => !renderer.name.Contains("OutermostEdges"))
           .ToArray();

        // Do something with the filtered MeshRenderers       
        filteredMeshRenderers.ToList().ForEach(meshRenderer => meshRenderer.material.color = color);

    }
}

[System.Serializable]
public class LayerGroup
{
    public string PartType;
    public string layer;
}

[System.Serializable]
public class ColorGroup
{
    public string PartType;
    public string Color; // Store color as a string to parse later
    public Color pureColor;

    public void SetColor(Color pureColor)
    {
        this.pureColor = pureColor;
        Color = ColorUtility.ToHtmlStringRGBA(pureColor);
    }

    public void SetColor(string colorCode)
    {
       
        if (ColorUtility.TryParseHtmlString(colorCode, out Color pureColor))
        {
            this.pureColor = pureColor;
        }
    }
}

[System.Serializable]
public class ColorGroupList
{
    public List<ColorGroup> colorGroups;
}