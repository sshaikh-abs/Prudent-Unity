using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseCameraState
{
    protected Bounds m_focusedObjectsCombinedBounds;

    public virtual void Enter()
    {
        // Calculated distance based on part size
        m_focusedObjectsCombinedBounds = CalculateBounds(CameraService.Instance.itemsToFocus);
        CameraService.Instance.FocusedPosition = m_focusedObjectsCombinedBounds.center;
    }

    private Bounds CalculateBounds(List<GameObject> gameobjects)
    {
        // Create a new zeroed Bounds object
        Bounds combinedBounds = new Bounds();
        combinedBounds.size = Vector3.zero;
        List<Renderer> renderers = new List<Renderer>();

        foreach (GameObject go in gameobjects)
        {
            if(go.GetComponent<Renderer>() != null)
            {
                renderers.Add(go.GetComponent<Renderer>());
            }
            else
            {
                renderers.AddRange(go.GetComponentsInChildren<Renderer>().ToList());
            }
        }

        foreach (Renderer rendererChild in renderers)
        {
            // If the bounds is zero, use the first child's bounds to start us off
            if (combinedBounds.size == Vector3.zero)
            {
                combinedBounds = rendererChild.bounds;
            }

            // Add the bounds of each child to our combined bounds
            try
            {
                combinedBounds.Encapsulate(rendererChild.bounds);
            }
            catch
            {
                Debug.LogError("Something went wrong");
            }
        }

        return combinedBounds;
    }

    public virtual void Execute() { }

    public virtual void Exit() { }
}
