using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    public bool defaultValue = false;
    public Sprite enabledSprite;
    public Sprite disabledSprite;

    private Toggle toggleComponent;
    public Image imageComponent;

    private void Awake()
    {
        toggleComponent = GetComponent<Toggle>();
        toggleComponent.onValueChanged.AddListener(OnToggled);
        toggleComponent.isOn = defaultValue;
    }

    private void OnToggled(bool toggleValue)
    {
        imageComponent.sprite = toggleValue ? enabledSprite : disabledSprite;
    }
}
