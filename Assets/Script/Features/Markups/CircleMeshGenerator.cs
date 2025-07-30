#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class CircleOutlineMeshGenerator
{
    [MenuItem("Tools/Generate Mesh/Cube Outline")]
    public static void GenerateAndSaveOutlineCubeMesh()
    {
        float side = 1f;
        Mesh cubeMesh = GenerateCubeOutlineMesh(side);
        SaveMeshAsset(cubeMesh, $"CubeOutline_{side}m.asset");
    }

    private static Mesh GenerateCubeOutlineMesh(float side)
    {
        Mesh cubeMesh = new Mesh();
        cubeMesh.name = $"CubeOutline_{side}";

        Vector3 halfSide = Vector3.one * 0.5f;

        Vector3[] vertices = new Vector3[8]
        {
            new Vector3(-halfSide.x, -halfSide.y, -halfSide.z),
            new Vector3( halfSide.x, -halfSide.y, -halfSide.z),
            new Vector3( halfSide.x,  halfSide.y, -halfSide.z),
            new Vector3(-halfSide.x,  halfSide.y, -halfSide.z),
            new Vector3(-halfSide.x, -halfSide.y,  halfSide.z),
            new Vector3( halfSide.x, -halfSide.y,  halfSide.z),
            new Vector3( halfSide.x,  halfSide.y,  halfSide.z),
            new Vector3(-halfSide.x,  halfSide.y,  halfSide.z)
        };

        int[] indices = new int[24] // Line list
        {
            0, 1, // Bottom edge front
            1, 2, // Right edge front
            2, 3, // Top edge front
            3, 0, // Left edge front
            4, 5, // Bottom edge back
            5, 6, // Right edge back
            6, 7, // Top edge back
            7, 4, // Left edge back
            0, 4, // Bottom edges connecting front and back
            1, 5,
            2, 6,
            3, 7
        };

        cubeMesh.vertices = vertices;
        cubeMesh.SetIndices(indices, MeshTopology.Lines, 0);

        return cubeMesh;
    }

    [MenuItem("Tools/Generate Mesh/Square Outline")]
    public static void GenerateAndSaveOutlineSquareMesh()
    {
        float side = 1f;

        Mesh circleMesh = GenerateSquareOutlineMesh(side);
        SaveMeshAsset(circleMesh, $"SquareOutline_{side}m.asset");
    }

    private static Mesh GenerateSquareOutlineMesh(float side)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"SquareOutline_{side}";

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-side / 2, 0f, -side / 2),
            new Vector3(side / 2, 0f, -side / 2),
            new Vector3(side / 2, 0f, side / 2),
            new Vector3(-side / 2, 0f, side / 2)
        };

        int[] indices = new int[8] // Line list
        {
            0, 1, // Bottom edge
            1, 2, // Right edge
            2, 3, // Top edge
            3, 0  // Left edge
        };

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        return mesh;
    }

    [MenuItem("Tools/Generate Mesh/Cirlce Outline")]
    public static void GenerateAndSaveOutlineCircleMesh()
    {
        float radius = 0.5f; // Set desired radius
        int segments = 64; // Controls smoothness

        Mesh circleMesh = GenerateCircleOutlineMesh(radius, segments);
        SaveMeshAsset(circleMesh, $"CircleOutline_{radius}m_{segments}s.asset");
    }

    public static Mesh GenerateCircleOutlineMesh(float radius, int segments)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"CircleOutline_{radius}_{segments}";

        Vector3[] vertices = new Vector3[segments];
        int[] indices = new int[segments * 2]; // Line list

        float angleStep = 2 * Mathf.PI / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[i] = new Vector3(x, 0f, z);

            // Connect current point to the next (wrap around at the end)
            indices[i * 2] = i;
            indices[i * 2 + 1] = (i + 1) % segments;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        return mesh;
    }

    public static void SaveMeshAsset(Mesh mesh, string fileName)
    {
        string folderPath = "Assets/GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "GeneratedMeshes");

        string fullPath = $"{folderPath}/{fileName}";
        AssetDatabase.CreateAsset(mesh, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Outline circle mesh saved to: {fullPath}");
    }
}
#endif
