using UnityEngine;

public class FrameLabelRedirection : MonoBehaviour
{
    public FrameLabel parentReference;

    public void OnMouseDown()
    {
        parentReference.FrameIsolation();
    }
}