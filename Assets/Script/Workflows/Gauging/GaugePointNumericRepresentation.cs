using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GaugePointNumericRepresentation : MonoBehaviour
{
    public Camera _camera;
    
    public Camera _sccamera;

    private Camera _maincam;

    public Transform container;
    public GameObject numberTemplate;
    public Vector2 offset;

    private Dictionary<Transform, GameObject> numbers = new Dictionary<Transform, GameObject>();

    public void Clear()
    {
        foreach (var item in numbers)
        {
            DestroyImmediate(item.Value, false);
        }

        numbers.Clear();
    }

    private void Start()
    {
        _maincam = _camera;
    }

    private void Update()
    {

        foreach (var keyPair in numbers)
        {

            Vector2 screenPoint = _maincam.WorldToScreenPoint(keyPair.Key.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(container as RectTransform, screenPoint, null, out Vector2 localPoint);
            (keyPair.Value.transform as RectTransform).anchoredPosition = localPoint + offset;
        }
    }

    public void AddNumber(Transform targetTransform, string text)
    {
        GameObject n = Instantiate(numberTemplate, container);
        numbers.Add(targetTransform, n);
        n.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void RemoveNumber(Transform targetTransform)
    {
        Destroy(numbers[targetTransform]);
        numbers.Remove(targetTransform);
    }

    public void SetNumericRepresentationActive(Transform transform, bool value)
    {
        numbers[transform].SetActive(value);
    }

    public void ChangeSCCamera()
    {
        _maincam = _sccamera;

        Debug.Log(" Main Cam =  SC Camera ");
    }

    public void ChangeCamera()
    {
        _maincam = _camera;
        Debug.Log(" Main Cam = Camera ");
    }

}
