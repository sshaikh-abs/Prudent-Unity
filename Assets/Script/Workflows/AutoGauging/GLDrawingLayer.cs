using UnityEngine;
using UnityEngine.Rendering;

public class GLDrawingLayer : MonoBehaviour
{
    public bool drawingEnabled = false;
    public Material lineMaterial;
    public Camera cam;
    public float size = 0.5f;

    void Start()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if(drawingEnabled)
        {
            DrawTest();
        }
    }

    void DrawTest()
    {
        if (!lineMaterial)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        GL.PushMatrix();
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        GL.Color(Color.red);

        foreach (var item in PointProjector.Instance.raycastHits)
        {
            Vector3 upVector = Vector3.Cross(item.hit.normal, Vector3.right).normalized;

            float dotProd = Vector3.Dot(Vector3.right, item.hit.normal);
            if (Mathf.Abs(dotProd) >= 0.99999f)
            {
                upVector = Vector3.up;
            }

            DrawX(item.hit.point, size, Color.yellow, item.hit.normal, upVector);
        }

        GL.End();

        GL.PopMatrix();
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    public static void DrawX(Vector3 position, float size, Color color, Vector3 forward, Vector3 up)
    {
        Vector3 right = Vector3.Cross (forward, up).normalized;

        // Line 1: top-left to bottom-right
        GL.Vertex(position + (right * size * 0.5f) + (up * size * 0.5f));
        GL.Vertex(position - (right * size * 0.5f) - (up * size * 0.5f));

        // Line 2: top-right to bottom-left
        GL.Vertex(position + (right * size * 0.5f) - (up * size * 0.5f));
        GL.Vertex(position - (right * size * 0.5f) + (up * size * 0.5f));
    }
}