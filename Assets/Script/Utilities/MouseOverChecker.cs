using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverChecker : MonoBehaviour
{
    private void Start()
    {
        ApplicationStateMachine.Instance.OnStateChanged += () =>
            OutlineManager.Instance.SetCurrentSelection(null);
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.Mouse1) && !IsMouseOverAUIElement())
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    if (!Physics.Raycast(ray))
        //    {
        //        OutlineManager.Instance.SetCurrentSelection(null);
        //    }
        //}
    }

    public static bool IsItemUnderTheCursor(params GameObject[] uiItems)
    {
        var uiElements = GetUIElementsUnderCursor();
        return uiElements.Select(ui => ui.gameObject).Intersect(uiItems).Count() > 0;
    }

    public static List<RaycastResult> GetUIElementsUnderCursor()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results;
    }

    public static bool IsMouseOverAUIElement()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
    }
}