using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GaugePointInfoHandler : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public CanvasGroup canvasGroup;
    public Transform infoGraphic;
    public Transform gaugePointRepresentation;
    public Image gaugePointBackground;

    [Space]

    public float scaleOffset = 1.5f;
    public float fadeDuration = 0.1f;
    public float yOffset = 10;

    public Color defaultPointColor;
    public Color selectedPointColor;
    public Color hoveredPointColor;

    public Color idTextColor;
    public Color measuredThicknessTextColor;
    public Color originalThicknessTextColor;

    public string idTextColorRaw => "#" + ColorUtility.ToHtmlStringRGBA(idTextColor);
    public string measuredThicknessTextColorRaw => "#" + ColorUtility.ToHtmlStringRGBA(measuredThicknessTextColor);
    public string originalThicknessTextColorRaw => "#" + ColorUtility.ToHtmlStringRGBA(originalThicknessTextColor);

    public void ShowInfoGraphic(Vector3 targetPosition, string id, string measuredThickness, string originalThickness, Color clr)
    {

        string information = $"<color={idTextColorRaw}>{id}</color> - <color={measuredThicknessTextColorRaw}>{measuredThickness}</color>/<color={originalThicknessTextColorRaw}>{originalThickness}</color>";
        infoText.text = information;
        gameObject.SetActive(true);
        canvasGroup.DOFade(1f, fadeDuration).SetLink(gameObject);
        gaugePointRepresentation.position = targetPosition;
        infoGraphic.position = targetPosition;
        infoGraphic.DOMoveY(targetPosition.y - yOffset, 0f).SetLink(gameObject);
        infoGraphic.DOMoveY(targetPosition.y, fadeDuration).SetLink(gameObject);

        gaugePointRepresentation.DOScale(scaleOffset, fadeDuration).SetLink(gameObject);
        gaugePointBackground.DOColor(clr, fadeDuration).SetLink(gameObject);
    }

    public void HideInfoGraphic()
    {
        canvasGroup.GetComponent<CanvasGroup>().DOFade(0f, fadeDuration).SetLink(gameObject);
        infoGraphic.DOMoveY(infoGraphic.position.y - yOffset, fadeDuration).SetLink(gameObject);
        gaugePointRepresentation.DOScale(1f, fadeDuration).SetLink(gameObject);
        gaugePointBackground.DOColor(defaultPointColor, fadeDuration).SetLink(gameObject).OnComplete(() => 
        {
            gameObject.SetActive(false);
        });
    }
}
