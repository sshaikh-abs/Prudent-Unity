using System;
using UnityEngine;
using UnityEngine.UI;

public class CrossSectionMenuHandler : MonoBehaviour
{
    public Toggle xPlaneToggle;
    public Toggle yPlaneToggle;
    public Toggle zPlaneToggle; 
    public Toggle xInvertToggle;
    public Toggle yInvertToggle;
    public Toggle zInvertToggle;

    public ShipBoundsComputer boundsComputer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        xPlaneToggle.onValueChanged.AddListener(OnXPlaneToggleChanged);
        yPlaneToggle.onValueChanged.AddListener(OnYPlaneToggleChanged);
        zPlaneToggle.onValueChanged.AddListener(OnZPlaneToggleChanged);
        xInvertToggle.onValueChanged.AddListener(OnXInvertToggleChanged);
        yInvertToggle.onValueChanged.AddListener(OnYInvertToggleChanged);
        zInvertToggle.onValueChanged.AddListener(OnZInvertToggleChanged);

        xPlaneToggle.isOn = boundsComputer.xPlaneEnabled;
        yPlaneToggle.isOn = boundsComputer.yPlaneEnabled;
        zPlaneToggle.isOn = boundsComputer.zPlaneEnabled;
        xInvertToggle.isOn = boundsComputer.xPlaneInverted;
        yInvertToggle.isOn = boundsComputer.yPlaneInverted;
        zInvertToggle.isOn = boundsComputer.zPlaneInverted;
    }

    private void OnZInvertToggleChanged(bool inputValue)
    {
        boundsComputer.xPlaneInverted = inputValue;
    }

    private void OnYInvertToggleChanged(bool inputValue)
    {
        boundsComputer.yPlaneInverted = inputValue;
    }

    private void OnXInvertToggleChanged(bool inputValue)
    {
        boundsComputer.zPlaneInverted = inputValue;
    }

    private void OnZPlaneToggleChanged(bool inputValue)
    {
        boundsComputer.xPlaneEnabled = inputValue;
    }

    private void OnYPlaneToggleChanged(bool inputValue)
    {
        boundsComputer.yPlaneEnabled = inputValue;
    }

    private void OnXPlaneToggleChanged(bool inputValue)
    {
        boundsComputer.zPlaneEnabled = inputValue;
    }
}