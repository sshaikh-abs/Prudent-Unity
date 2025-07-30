#if UNITY_EDITOR
#endif

using UnityEngine;

public class PartHighlighter : MonoBehaviour
{
#if UNITY_EDITOR

    public MeshRenderer selectedRender => GetSelectedRenderer();
    public Bounds Bounds => GetSelectedBounds();

    public Vector3 position;
    public Vector3 size;

    private void Update()
    {
        position = Bounds.center;
        size = Bounds.size;
        if(Mathf.Abs(Bounds.size.x) < 1e-05f)
        {
            size.x = 0f;
        }
        if (Mathf.Abs(Bounds.size.y) < 1e-05f)
        {
            size.y = 0f;
        }
        if (Mathf.Abs(Bounds.size.z) < 1e-05f)
        {
            size.z = 0f;
        }
    }

    public Bounds GetSelectedBounds()
    {
        if(selectedRender == null)
        {
            return new Bounds();
        }
        return selectedRender.bounds;
    }

    public MeshRenderer GetSelectedRenderer()
    {
        if(UnityEditor.Selection.gameObjects.Length == 0)
        {
            return null;
        }

        MeshRenderer selectedRender = UnityEditor.Selection.gameObjects[0].GetComponent<MeshRenderer>();
        if (selectedRender == null)
        {
            return null;
        }

        return selectedRender;
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(position, size);
    }
#endif
}
