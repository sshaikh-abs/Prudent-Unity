using UnityEngine;

public class UIPanelAdjuster : MonoBehaviour
{
    private RectTransform uiPanel; // Your UI Panel RectTransform
    public float threshold = 200f;
    public float spacingBottom = 1.25f;
    public float spacingTop = 0.5f;
    private void Start()
    {
        uiPanel = this.GetComponent<RectTransform>();

    }
    void LateUpdate()
    {
      
        Vector3 worldPosition = uiPanel.localPosition;
        Vector2 newPivot = uiPanel.pivot;      
    
        newPivot.x = worldPosition.x < 0 ? 0 : 1;
        newPivot.y = worldPosition.y < 0 ? 0 : 1;

        //if (worldPosition.y < -(Screen.height / 2) + threshold)
        //{
        //    newPivot.y = -spacingBottom;
        //}
        //else
        //{
        //    newPivot.y = spacingTop;
        //}

        // Apply the new pivot if it has changed
        if (uiPanel.pivot != newPivot)
        {
            uiPanel.pivot = newPivot;
        }
    }
}
