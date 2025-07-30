using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AutoGaugingMenu : MonoBehaviour
{
    public PointProjector PointProjector;

    private Vector3 offsetInput;
    private int iterationsInput;
    private float spacingInput;
    private bool autoGaugingInput;
    private SubpartType subpartTypeInput;

    public GameObject fullMenu;
    public Toggle autoGaugingToggle;
    public Toggle compartmentLevelToggle;
    public Toggle hullpartLevelToggle;
    public Toggle platesToggle;
    public Toggle bracketsToggle;
    public Toggle stiffenersToggle;

    public TMP_InputField offsetX;
    public TMP_InputField offsetY;
    public TMP_InputField offsetZ;

    public TMP_InputField Iterations;
    public TMP_InputField Spacing;

    public Button markButton;

    private void Awake()
    {
        offsetX.onValueChanged.AddListener(OnOffsetXChanged);
        offsetY.onValueChanged.AddListener(OnOffsetYChanged);
        offsetZ.onValueChanged.AddListener(OnOffsetZChanged);
        Iterations.onValueChanged.AddListener(OnIterationsChanged);
        Spacing.onValueChanged.AddListener(OnSpacingChanged);
        markButton.onClick.AddListener(OnMarkButtonClicked);
        autoGaugingToggle.onValueChanged.AddListener(OnAutoGaugingToggleChanged);
        platesToggle.onValueChanged.AddListener(OnPlatesToggleChanged);
        bracketsToggle.onValueChanged.AddListener(OnBracketsToggleChanged);
        stiffenersToggle.onValueChanged.AddListener(OnStiffenersToggleChanged);

        offsetInput = PointProjector.offset;
        iterationsInput = PointProjector.iterations;
        spacingInput = PointProjector.spacing;

        offsetX.text = offsetInput.x.ToString();
        offsetY.text = offsetInput.y.ToString();
        offsetZ.text = offsetInput.z.ToString();
        Iterations.text = iterationsInput.ToString();
        Spacing.text = spacingInput.ToString();
        autoGaugingInput = PointProjector.autoGauging;
        subpartTypeInput = SubpartType.All;
        hullpartLevelToggle.isOn = PointProjector.hullpartLevel;
        compartmentLevelToggle.isOn = PointProjector.compartmentLevel;
    }

    private void OnStiffenersToggleChanged(bool inputValue)
    {
        if (inputValue)
        {
            subpartTypeInput |= SubpartType.Stiffener;
        }
        else
        {
            subpartTypeInput &= ~SubpartType.Stiffener;
        }
    }

    private void OnBracketsToggleChanged(bool inputValue)
    {
        if (inputValue)
        {
            subpartTypeInput |= SubpartType.Bracket;
        }
        else
        {
            subpartTypeInput &= ~SubpartType.Bracket;
        }
    }

    private void OnPlatesToggleChanged(bool inputValue)
    {
        if (inputValue)
        {
            subpartTypeInput |= SubpartType.Plate;
        }
        else
        {
            subpartTypeInput &= ~SubpartType.Plate;
        }
    }

    private void OnAutoGaugingToggleChanged(bool inputValue)
    {
        autoGaugingInput = inputValue;
    }

    private void Update()
    {
        PointProjector.spacing = spacingInput;
        PointProjector.iterations = iterationsInput;
        PointProjector.offset = offsetInput;
        PointProjector.queryType = subpartTypeInput;
        PointProjector.autoGauging = autoGaugingInput;
        PointProjector.hullpartLevel = hullpartLevelToggle.isOn;
        PointProjector.compartmentLevel = compartmentLevelToggle.isOn;
        fullMenu.SetActive(autoGaugingInput);
    }

    private void OnMarkButtonClicked()
    {
        PointProjector.OnCaptureButtonClicked();
    }

    private void OnSpacingChanged(string inputValue)
    {
        if (!ValidatteFloatInString(inputValue))
        {
            Spacing.text = string.Empty;
            return;
        }
        else
        {
            if (float.TryParse(inputValue, out float result))
            {
                spacingInput = result;
            }
        }
    }

    private void OnIterationsChanged(string inputValue)
    {
        if (!ValidateIntInString(inputValue))
        {
            Iterations.text = string.Empty;
            return;
        }
        else
        {
            if (int.TryParse(inputValue, out int result))
            {
                iterationsInput = result;
            }
        }
    }

    private void OnOffsetZChanged(string inputValue)
    {
        if (!ValidatteFloatInString(inputValue))
        {
            offsetZ.text = string.Empty;
            return;
        }
        else
        {
            if (float.TryParse(inputValue, out float result))
            {
                offsetInput.z = result;
            }
        }
    }

    private void OnOffsetYChanged(string inputValue)
    {
        if (!ValidatteFloatInString(inputValue))
        {
            offsetY.text = string.Empty;
            return;
        }
        else
        {
            if (float.TryParse(inputValue, out float result))
            {
                offsetInput.y = result;
            }
        }
    }

    private void OnOffsetXChanged(string inputValue)
    {
        if (!ValidatteFloatInString(inputValue))
        {
            offsetX.text = string.Empty;
            return;
        }
        else
        {
            if (float.TryParse(inputValue, out float result))
            {
                offsetInput.x = result;
            }
        }
    }

    public bool ValidatteFloatInString(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            if (float.TryParse(input, out float result))
            {
                return true;
            }
        }

        return false;
    }
    public bool ValidateIntInString(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            if (int.TryParse(input, out int result))
            {
                return true;
            }
        }

        return false;
    }
}
