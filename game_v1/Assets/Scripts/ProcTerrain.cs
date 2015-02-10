using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]


public class ProcTerrain : MonoBehaviour {

	public int Rows = 10;
	public int Columns = 10;
	public Vector3 Position = Vector3.zero;
	public Vector2 QuadDimension = new Vector2 (1, 1);
	
	private List<Vector3> m_Vertices = new List<Vector3>();
	private List<Vector3> m_Normals = new List<Vector3>();
	private List<int> m_Indices = new List<int>();


	public void Rebuild()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			Debug.LogError("MeshFilter not found!");
			return;
		}
		
		float w = QuadDimension.x, h = QuadDimension.y;

		for (int i=0; i < Rows; i++) {
			Vector3 newPosition = new Vector3(Position.x, Position.y, Position.z + (i*QuadDimension.y));
			BuildRow (newPosition, Columns, w, h);
		}

		CreateMesh ();

	}

	void BuildRow(Vector3 position, int columns, float quadWidth, float quadHeight) {
	
		for (int i=0; i < columns; i++) {
			Vector3 quadPosition = new Vector3(position.x + (quadWidth * i), position.y, position.z);
			Debug.Log(quadPosition);
			BuildQuad(quadPosition, quadWidth, quadHeight );
		}

	}

	void BuildQuad(Vector3 position, float w, float h) {

		float cx = position.x, cy = position.y, cz = position.z;

		m_Vertices.Add (new Vector3 (cx, cy, cz));
		m_Normals.Add (new Vector3 (0, 1, 0));
				
		m_Vertices.Add (new Vector3(cx + w, cy, cz + h));
		m_Normals.Add (new Vector3(0, 1, 0));
		
		m_Vertices.Add (new Vector3(cx + w, cy, cz));
		m_Normals.Add (new Vector3(0, 1, 0));
		
		m_Vertices.Add (new Vector3(cx, cy, cz));
		m_Normals.Add ( new Vector3(0, 1, 0));
		
		m_Vertices.Add (new Vector3(cx, cy, cz + h));
		m_Normals.Add ( new Vector3(0, 1, 0));
		
		m_Vertices.Add (new Vector3(cx + w, cy, cz + h));
		m_Normals.Add ( new Vector3(0, 1, 0));

		int baseIndex = m_Vertices.Count - 6;
		
		AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
		AddTriangle(baseIndex + 3, baseIndex + 4, baseIndex + 5);

	}


	public void CreateMesh()
	{
		// if there's already a meshfilter, clear it
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		Mesh mesh = meshFilter.sharedMesh;
		if (mesh == null)
		{
			meshFilter.mesh = new Mesh();
			mesh = meshFilter.sharedMesh;
		}
		mesh.Clear();    

		//add our vertex and triangle values to the new mesh:
		mesh.vertices = m_Vertices.ToArray();
		mesh.triangles = m_Indices.ToArray();
		
		//Normals are optional. Only use them if we have the correct amount:
		if (m_Normals.Count == m_Vertices.Count)
			mesh.normals = m_Normals.ToArray();

		//have the mesh recalculate its bounding box (required for proper rendering):
		mesh.RecalculateBounds();
		mesh.Optimize();


	}

	public void AddTriangle(int index0, int index1, int index2)
	{
		m_Indices.Add(index0);
		m_Indices.Add(index1);
		m_Indices.Add(index2);
	}


}
