using System.Collections.Generic;
using UnityEngine;

public class DeckLabel : MonoBehaviour
{
    public Renderer Sample;

    public Transform leftClaw;
    public Transform rightClaw;

    public Vector2 Bounds => new Vector2(bounds.y * 0.5f, bounds.x * 0.5f);
    public Vector2 bounds = new Vector2(0.5f, 0.5f);
    public FrameLabel frameLabel;
    public float size = 1f;
    public float bufferX = 0.1f;
    public float bufferY = 0.1f;

    public List<Transform> pinchesReference = new List<Transform>(); 
    public List<Transform> pinches = new List<Transform>();

    public void SetLabel(string label)
    {
        frameLabel.label = label;
    }

    public void UpdateLRS(Transform cam, string label)
    {
        frameLabel.UpdateLRS(cam, label);
        UpdatePosition();
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying && Sample != null)
        {
            transform.position = Sample.transform.position;

            if (Sample != null)
            {
                bounds.x = Sample.bounds.size.x / 2f;
                bounds.y = Sample.bounds.size.z / 2f;
            }
        }

        UpdatePosition();
    }

    public void UpdatePosition()
    {
        frameLabel.size = size;
        frameLabel.offset = Bounds.x + bufferX;
        frameLabel.UpdateLocation();
        UpdateClaws();
        UpdateClawPinchs();
    }

    public void UpdateClaws()
    {
        leftClaw.transform.localPosition = new Vector3(leftClaw.transform.localPosition.x, 
            leftClaw.transform.localPosition.y, -Bounds.x - bufferX);
        rightClaw.transform.localPosition = new Vector3(rightClaw.transform.localPosition.x, 
            rightClaw.transform.localPosition.y, Bounds.x + bufferX);

        leftClaw.transform.localScale = new Vector3((Bounds.y * 2f) + bufferY, leftClaw.transform.localScale.y, 
            leftClaw.transform.localScale.z);
        rightClaw.transform.localScale = new Vector3((Bounds.y * 2f) + bufferY, rightClaw.transform.localScale.y, 
            rightClaw.transform.localScale.z);
    }

    private void UpdateClawPinchs()
    {
        for (int i = 0; i < pinchesReference.Count; i++)
        {
            pinches[i].transform.LookAt(pinchesReference[i].parent);
            pinches[i].position = pinchesReference[i].position;
        }
    }
}
