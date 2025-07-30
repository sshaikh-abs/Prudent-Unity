using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIThemeApplicator : MonoBehaviour
{
    private ThemeData currentThemeData;

    public string defaultTheme;
    public List<ThemeData> themeData;
    public Dictionary<string, ThemeData> themeLUT;
    public Dictionary<string, ThemeData> ThemeLUT
    {
        get
        {
            if(themeLUT == null)
            {
                themeLUT = new Dictionary<string, ThemeData>();
                Initialize();
            }
            return themeLUT;
        }
    }

    private void Awake()
    {
        ApplyDefaultTheme();
    }

    [ContextMenu(nameof(ApplyDefaultTheme))]
    private void ApplyDefaultTheme()
    {
        if (defaultTheme != null || defaultTheme != "")
        {
            ApplyTheme(defaultTheme);
        }
    }

    private void Initialize()
    {
        themeLUT = new Dictionary<string, ThemeData>();
        foreach (var theme in themeData)
        {
            themeLUT.Add(theme.Name, theme);
        }
    }

    public void ApplyTheme(string themeName, bool usePriority = false)
    {
        if (usePriority && currentThemeData.priority < themeLUT[themeName].priority)
        {
            Debug.LogWarning("trying to set a lower priorty theme, try not to use priority in this case");
            return;
        }

        currentThemeData= ThemeLUT[themeName];
        if (currentThemeData != null) 
        {
            foreach (var item in currentThemeData.imageToColorPairs)
            {
                item.colorableComponent.color = item.color;
                item.colorableComponent.gameObject.SetActive(item.visible);
            }
        }
    }
}

[System.Serializable]
public class ThemeData
{
    public string Name;
    public int priority;
    public List<ImageToColorPair> imageToColorPairs;
}

[System.Serializable]
public class ImageToColorPair
{
    public Color color;
    public MaskableGraphic colorableComponent;
    public bool visible = true;
}