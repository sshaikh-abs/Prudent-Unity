using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingDialog : MonoBehaviour
{
    public RectTransform barTransform;
    public RectTransform embossTransform;
    public Image linesImage;
    public TextMeshProUGUI percentText;
    private float targetValue = 0f;
    [Range(0f, 1f)] public float value;

    public void Update()
    {
        if (value < targetValue)
        {
            value += Time.deltaTime * 0.5f;
            if (value > targetValue)
            {
                value = targetValue;
            }
        }
        ApplyValue();
    }

    private void OnValidate()
    {
        if(!Application.isPlaying)
        {
            targetValue = value;
            ApplyValue();
        }
    }

    public void UpdateValue(float targetValue)
    {
        this.targetValue = targetValue;
        //if(targetValue < value)
        //{
        //    value = targetValue;
        //    ApplyValue();
        //    return;
        //}
    }

    public void ApplyValue()
    {
        barTransform.sizeDelta = new Vector2((value * 320f) + 20f, barTransform.sizeDelta.y);
        barTransform.anchoredPosition = new Vector2((barTransform.sizeDelta.x / 2f) - 10f, barTransform.anchoredPosition.y);
        embossTransform.sizeDelta = new Vector2((value * 320f) + 20f, embossTransform.sizeDelta.y);
        embossTransform.anchoredPosition = new Vector2((embossTransform.sizeDelta.x / 2f) - 10f, embossTransform.anchoredPosition.y);

        //float x = Mathf.Lerp(-50f, 900f, value);
        //float x = Mathf.Lerp(0f, 0f, value);
        //float y = Mathf.Lerp(160f, -550f, value);
        //float y = Mathf.Lerp(0, 0, value);

        //linesTransform.anchoredPosition = new Vector2(x, y);
        //percentText.text = ((int)(value * 100f)).ToString() + "%";
    }
}
