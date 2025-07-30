using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteResizeToTargetScale : MonoBehaviour
{
    public Transform targetObject; // Assign this in the Inspector

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Sliced;
    }

    void LateUpdate()
    {
        if (targetObject != null)
        {
            Vector3 scale = targetObject.localScale;
            sr.size = new Vector2(scale.x/1.5f, scale.y/1.5f);
        }
    }
}


