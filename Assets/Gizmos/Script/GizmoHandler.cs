using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class GizmoHandler : SingletonMono<GizmoHandler>, IPointerClickHandler
{
    [Header("Gizmo Setup")]
    public Camera gizmoCamera;
    public RenderTexture renderTexture;
    public RectTransform panelRect;
    public float timer = 1f;
    private bool gizmoEnable = true;

    [Header("Color Settings")]
    public Color selectionColor = Color.yellow;
    public Color hoverColor = Color.cyan;

    private GameObject currentlyHovered;
    private Material hoveredMaterial;
    private Color hoveredOriginalColor;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!gizmoEnable)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float normalizedX = (localPoint.x - panelRect.rect.xMin) / panelRect.rect.width;
            float normalizedY = (localPoint.y - panelRect.rect.yMin) / panelRect.rect.height;

            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
                return;

            Vector3 screenPoint = new Vector3(
                normalizedX * gizmoCamera.pixelWidth,
                normalizedY * gizmoCamera.pixelHeight,
                0f
            );

            Ray ray = gizmoCamera.ScreenPointToRay(screenPoint);
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                string clickedName = hit.collider.gameObject.name.ToUpperInvariant();
                //Debug.Log("Clicked on: " + clickedName);

                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    Color originalColor = mat.color;

                    mat.color = selectionColor;
                    StartCoroutine(RevertColorAfterDelay(mat, originalColor, timer));
                }

                // Switch case for various view controls
                switch (clickedName)
                {
                    case "TOP": CommunicationManager.Instance.ExcuteBottomBarButton_Extern("Top View"); break;
                    case "PORT": CommunicationManager.Instance.ExcuteBottomBarButton_Extern("Port View"); break;
                    case "STBD": CommunicationManager.Instance.ExcuteBottomBarButton_Extern("Starboard View"); break;
                    case "AFT": CommunicationManager.Instance.ExcuteBottomBarButton_Extern("Aft View"); break;
                    case "FWD": CommunicationManager.Instance.ExcuteBottomBarButton_Extern("Front View"); break;
                    case "BTM": CommunicationManager.Instance.ExcuteBottomBarButton_Extern("Bottom View"); break;

                    case "TSF": CameraService.Instance.PresetViewIsometricTSF(); break;
                    case "BSF": CameraService.Instance.PresetViewIsometricBSF(); break;
                    case "TFP": CameraService.Instance.PresetViewIsometricTFP(); break;
                    case "BFP": CameraService.Instance.PresetViewIsometricBFP(); break;
                    case "TPA": CameraService.Instance.PresetViewIsometricTPA(); break;
                    case "BPA": CameraService.Instance.PresetViewIsometricBPA(); break;
                    case "TAS": CameraService.Instance.PresetViewIsometricTAS(); break;
                    case "BAS": CameraService.Instance.PresetViewIsometricBAS(); break;

                    case "TS": CameraService.Instance.PresetViewIsometricTS(); break;
                    case "BS": CameraService.Instance.PresetViewIsometricBS(); break;
                    case "TP": CameraService.Instance.PresetViewIsometricTP(); break;
                    case "BP": CameraService.Instance.PresetViewIsometricBP(); break;
                    case "TF": CameraService.Instance.PresetViewIsometricTF(); break;
                    case "BF": CameraService.Instance.PresetViewIsometricBF(); break;
                    case "TA": CameraService.Instance.PresetViewIsometricTA(); break;
                    case "BA": CameraService.Instance.PresetViewIsometricBA(); break;
                    case "FP": CameraService.Instance.PresetViewIsometricFP(); break;
                    case "FS": CameraService.Instance.PresetViewIsometricFS(); break;
                    case "AP": CameraService.Instance.PresetViewIsometricAP(); break;
                    case "AS": CameraService.Instance.PresetViewIsometricAS(); break;

                    default:
                        //Debug.LogWarning("Unrecognized face: " + clickedName);
                        break;
                }
            }
        }
    }

    public override void Update()
    {
        if (!gizmoEnable || gizmoCamera == null || panelRect == null) return;
        HandleMouseHover();
    }

    private void HandleMouseHover()
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect, Input.mousePosition, null, out Vector2 localPoint))
        {
            float normalizedX = (localPoint.x - panelRect.rect.xMin) / panelRect.rect.width;
            float normalizedY = (localPoint.y - panelRect.rect.yMin) / panelRect.rect.height;

            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
            {
                ClearHover();
                return;
            }

            Vector3 screenPoint = new Vector3(
                normalizedX * gizmoCamera.pixelWidth,
                normalizedY * gizmoCamera.pixelHeight,
                0f
            );

            Ray ray = gizmoCamera.ScreenPointToRay(screenPoint);
            //Debug.DrawRay(ray.origin, ray.direction * 10f, Color.cyan, 0.2f);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj != currentlyHovered)
                {
                    ClearHover();

                    currentlyHovered = hitObj;
                    hoveredMaterial = hitObj.GetComponent<Renderer>()?.material;

                    if (hoveredMaterial != null)
                    {
                        hoveredOriginalColor = hoveredMaterial.color;
                        hoveredMaterial.color = hoverColor;
                    }
                }

                return;
            }
        }

        ClearHover(); // mouse moved away or nothing hit
    }

    private void ClearHover()
    {
        if (currentlyHovered != null && hoveredMaterial != null)
        {
            hoveredMaterial.color = hoveredOriginalColor;
        }

        currentlyHovered = null;
        hoveredMaterial = null;
    }

    private IEnumerator RevertColorAfterDelay(Material mat, Color originalColor, float delay)
    {
        gizmoEnable = false;
        yield return new WaitForSeconds(delay);
        mat.color = originalColor;
        gizmoEnable = true;
    }

    public void SetPanelProperties(float x, float y, float w, float h)
    {
        if (panelRect != null)
        {
            panelRect.anchoredPosition = new Vector2(x, y);
            panelRect.sizeDelta = new Vector2(w, h);
        }
    }
}
