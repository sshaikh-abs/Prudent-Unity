using System.Collections.Generic;
using UnityEngine;

public class MeshExtruder : SingletonMono<MeshExtruder>
{
	public Dictionary<MeshFilter, Material> originalMaterialCache = new Dictionary<MeshFilter, Material>();
	public Dictionary<MeshFilter, Mesh> originalMeshCache = new Dictionary<MeshFilter, Mesh>();
	public Dictionary<MeshFilter, Mesh> extrudedMeshCache = new Dictionary<MeshFilter, Mesh>();
	public Dictionary<MeshFilter, string> originalLayer = new Dictionary<MeshFilter, string>();

    public void Revert()
	{
        foreach (var sourceMeshFilter in extrudedMeshCache)
        {
            sourceMeshFilter.Key.gameObject.layer =  LayerMask.NameToLayer(originalLayer[sourceMeshFilter.Key]);
            sourceMeshFilter.Key.mesh = originalMeshCache[sourceMeshFilter.Key];
            sourceMeshFilter.Key.GetComponent<Renderer>().material = originalMaterialCache[sourceMeshFilter.Key];
            Destroy(sourceMeshFilter.Value);
            originalMeshCache.Remove(sourceMeshFilter.Key);
            originalMaterialCache.Remove(sourceMeshFilter.Key);
			originalLayer.Remove(sourceMeshFilter.Key);
        }

		extrudedMeshCache.Clear();
    }

    public void Extrude(List<MeshFilter> sourceMeshFilters, float thickness, Material replacementMat)
    {
        foreach (var sourceMeshFilter in sourceMeshFilters)
        {
			originalLayer.Add(sourceMeshFilter, LayerMask.LayerToName(sourceMeshFilter.gameObject.layer));
            sourceMeshFilter.gameObject.layer = LayerMask.NameToLayer("Water");
            originalMeshCache.Add(sourceMeshFilter, sourceMeshFilter.mesh);
            originalMaterialCache.Add(sourceMeshFilter, sourceMeshFilter.GetComponent<Renderer>().material);
            sourceMeshFilter.GetComponent<Renderer>().material = replacementMat;
            Mesh extrudedMesh = ExtrudeMesh(sourceMeshFilter.mesh, thickness);
            extrudedMeshCache.Add(sourceMeshFilter, extrudedMesh);
            sourceMeshFilter.mesh = extrudedMesh;
        }
    }

	public void ExtrudePlain(MeshFilter sourceMeshFilter, float thickness)
	{
		Mesh extrudedMesh = ExtrudeMesh(sourceMeshFilter.mesh, thickness);
		sourceMeshFilter.mesh = extrudedMesh;
	}

    public Mesh ExtrudeFace(Mesh originalMesh, float factor)
    {
        Vector3[] originalVertices = originalMesh.vertices;
        Vector3[] normals = originalMesh.normals;
        int[] originalTriangles = originalMesh.triangles;

        int vertexCount = originalVertices.Length;
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        // Step 1: Duplicate original vertices and extrude them along vertex normals
        for (int i = 0; i < vertexCount; i++)
        {
            newVertices.Add(originalVertices[i] - normals[i] * (factor/2f)); // Original vertex
            newVertices.Add(originalVertices[i] + normals[i] * (factor/2f)); // Extruded vertex
        }

        // Step 2: Connect original and extruded vertices to form side faces
        for (int i = 0; i < originalTriangles.Length; i += 3)
        {
            int v0 = originalTriangles[i] * 2;
            int v1 = originalTriangles[i + 1] * 2;
            int v2 = originalTriangles[i + 2] * 2;

            int v0_extruded = v0 + 1;
            int v1_extruded = v1 + 1;
            int v2_extruded = v2 + 1;

            // Original face (same as input)
            newTriangles.Add(v0);
            newTriangles.Add(v1);
            newTriangles.Add(v2);

            // Extruded face (flipped winding order)
            newTriangles.Add(v2_extruded);
            newTriangles.Add(v1_extruded);
            newTriangles.Add(v0_extruded);
        }

        // Step 3: Assign new vertices and triangles to mesh
        Mesh extrudedMesh = new Mesh();
        extrudedMesh.vertices = newVertices.ToArray();
        extrudedMesh.triangles = newTriangles.ToArray();
        extrudedMesh.RecalculateNormals();
        extrudedMesh.RecalculateBounds();
        extrudedMesh.RecalculateTangents();
        return extrudedMesh;
    }

    public Mesh ExtrudeMesh(Mesh originalMesh, float factor)
	{
		Vector3[] originalVertices = originalMesh.vertices;
		Vector3[] normals = originalMesh.normals;
		int[] originalTriangles = originalMesh.triangles;

		int vertexCount = originalVertices.Length;
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();

		// Step 1: Duplicate original vertices and extrude them along vertex normals
		for (int i = 0; i < vertexCount; i++)
		{
			newVertices.Add(originalVertices[i]); // Original vertex
			newVertices.Add(originalVertices[i] + normals[i] * factor); // Extruded vertex
		}

		// Step 2: Connect original and extruded vertices to form side faces
		for (int i = 0; i < originalTriangles.Length; i += 3)
		{
			int v0 = originalTriangles[i] * 2;
			int v1 = originalTriangles[i + 1] * 2;
			int v2 = originalTriangles[i + 2] * 2;

			int v0_extruded = v0 + 1;
			int v1_extruded = v1 + 1;
			int v2_extruded = v2 + 1;

			// Original face (same as input)
			newTriangles.Add(v0);
			newTriangles.Add(v1);
			newTriangles.Add(v2);

			// Extruded face (flipped winding order)
			newTriangles.Add(v2_extruded);
			newTriangles.Add(v1_extruded);
			newTriangles.Add(v0_extruded);

			// Side faces
			CreateQuad(newTriangles, v0, v1, v1_extruded, v0_extruded);
			CreateQuad(newTriangles, v1, v2, v2_extruded, v1_extruded);
			CreateQuad(newTriangles, v2, v0, v0_extruded, v2_extruded);
		}

		// Step 3: Assign new vertices and triangles to mesh
		Mesh extrudedMesh = new Mesh();
		extrudedMesh.vertices = newVertices.ToArray();
		extrudedMesh.triangles = newTriangles.ToArray();
		extrudedMesh.RecalculateNormals();
		extrudedMesh.RecalculateBounds();
		extrudedMesh.RecalculateTangents();
		return extrudedMesh;
	}

	private void CreateQuad(List<int> triangles, int v0, int v1, int v2, int v3)
	{
		triangles.Add(v0);
		triangles.Add(v1);
		triangles.Add(v2);

		triangles.Add(v2);
		triangles.Add(v3);
		triangles.Add(v0);
	}
}