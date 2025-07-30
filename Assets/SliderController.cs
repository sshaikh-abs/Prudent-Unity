using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;              // Assign in Inspector
    public Image panelImage;           // Assign in Inspector
    public Sprite lightSprite;         // Assign light sprite in Inspector
    public Sprite darkSprite;          // Assign drag sprite in Inspector

    private enum SliderState { Light, Dark }
    private SliderState currentState;

    void Start()
    {
        // Set initial state based on the slider's value
        currentState = slider.value == 1 ? SliderState.Light : SliderState.Dark;

        UpdateImage(currentState);

        // Add listener to slider value change
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Determine new state based on slider value
        SliderState newState = value == 0 ? SliderState.Light : SliderState.Dark;

        // Only update if the state has changed
        if (newState != currentState)
        {
            currentState = newState;
            UpdateImage(currentState);
        }
    }

    private void UpdateImage(SliderState state)
    {
        // Update the image based on the current state
        panelImage.sprite = (state == SliderState.Light) ? lightSprite : darkSprite;
    }
}
