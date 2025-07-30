using TMPro;
using UnityEngine;

public class FrameLabel : MonoBehaviour
{
    public Transform leftLabelPivot;
    public Transform rightLabelPivot;
    public TextMeshPro leftLabelText;
    public TextMeshPro rightLabelText;
    public Transform lineTransform;
    public Transform lineTransform_Left;

    public string label;
    public float size;
    public float Size => size / 2f;
    public float offset = 0f;

    public bool forSS = false;

    private void Update()
    {
        if (forSS) { 
            return;
        }
        Vector3 targetPos = Camera.main.transform.position;
        Vector3 leftLabelTarget = targetPos;
        leftLabelTarget.y = leftLabelPivot.position.y;
        Vector3 rightLabelTarget = targetPos;
        rightLabelTarget.y = rightLabelPivot.position.y;
        leftLabelPivot.transform.rotation = Quaternion.LookRotation(-(leftLabelTarget - leftLabelPivot.position).normalized, Vector3.up);
        rightLabelPivot.transform.rotation = Quaternion.LookRotation(-(rightLabelTarget - rightLabelPivot.position).normalized, Vector3.up);
    }

    public void UpdateLRS(Transform cam, string label)
    {
        this.label = label;
        UpdateLocation();
        LookAt(cam);
    }

    public void FrameIsolation()
    {
        string ObjectName =  (gameObject.name != "FrameLabel3D")? gameObject.name : gameObject.transform.parent.name;

        if (ApplicationStateMachine.Instance.currentStateName == nameof(CompartmentViewState))
        {
            ApplicationStateMachine.Instance.ExcuteStateTransition(nameof(HullpartViewState), new System.Collections.Generic.List<string>() 
            {
                ApplicationStateMachine.Instance.GetCurrentState<CompartmentViewState>().GetTargettedCompartemnt(),
                ObjectName
            });
        }
    }

    private void LookAt(Transform cam)
    {
        leftLabelPivot.transform.rotation = cam.transform.rotation;
        rightLabelPivot.transform.rotation = cam.transform.rotation;
    }

    public void UpdateLocation()
    {
        if (leftLabelText != null)
        {
            leftLabelText.text = label;
        }
        if (rightLabelText != null)
        {
            rightLabelText.text = label;
        }
        if (lineTransform != null)
        {
            lineTransform.localScale = new Vector3(Size, lineTransform.localScale.y, lineTransform.localScale.z);
            lineTransform.localPosition = new Vector3((Size / 2) + offset, lineTransform.localPosition.y, lineTransform.localPosition.z);
            lineTransform_Left.localScale = new Vector3(Size, lineTransform_Left.localScale.y, lineTransform_Left.localScale.z);
            lineTransform_Left.localPosition = new Vector3(-((Size / 2) + offset), lineTransform_Left.localPosition.y, lineTransform_Left.localPosition.z);
        }

        if (forSS && Application.isPlaying)
        {
            lineTransform.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(Size * 3f, 1f);
            lineTransform_Left.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(Size * 3f, 1f);
        }

        if (leftLabelPivot != null)
        {
            leftLabelPivot.localPosition = new Vector3(-Size - offset, leftLabelPivot.localPosition.y, leftLabelPivot.localPosition.z);
        }

        if (rightLabelPivot != null)
        {
            rightLabelPivot.localPosition = new Vector3(Size + offset, rightLabelPivot.localPosition.y, rightLabelPivot.localPosition.z);
        }
    }

    public void OnValidate()
    {
        UpdateLocation();
    }
}