using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuButton : MonoBehaviour
{
    public TextMeshProUGUI buttonLabel;
    public string hoveredTheme;
    public string clickedTheme;
    public string idleTheme;

    private bool IsEnabled;

    private UIThemeApplicator _themeApplicator;
    public UIThemeApplicator ThemeApplicator
    {
        get
        {
            if(_themeApplicator == null )
            {
                _themeApplicator = GetComponent<UIThemeApplicator>();
            }
            return _themeApplicator;
        }
    }
    
    private Action OnButtonClick;

    public void AddListener(Action OnClick)
    {
        OnButtonClick = OnClick;
    }

    public void UpdateButton(string label, bool isEnabled)
    {
        buttonLabel.text = label;
        IsEnabled = isEnabled;
        ThemeApplicator.ApplyTheme(IsEnabled ? "Default" : "Disable");
    }

    public void Initialized(string label, bool isEnabled, Action OnClick = null)
    {
        buttonLabel.text = label;
        if(OnClick != null)
        {
            AddListener(OnClick);
        }
        GetComponent<Image>().raycastTarget = isEnabled;
        GetComponent<Button>().interactable = isEnabled;
        IsEnabled = isEnabled;
        ThemeApplicator.ApplyTheme(IsEnabled ? "Default" : "Disable");
    }

    public void OnPointerEnter()
    {
        if (IsEnabled)
        {
            ThemeApplicator.ApplyTheme(hoveredTheme);
        }
    }

    public void OnPointerExit()
    {
        if(IsEnabled)
        {
            ThemeApplicator.ApplyTheme(idleTheme);
        }
    }

    public void OnPointerDown()
    {
        if (IsEnabled)
        {
            OnButtonClick?.Invoke();
        }
    }
}
