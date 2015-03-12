using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]


public class ProcTerrain : MonoBehaviour {

	public Texture2D NoiseTexture;

	public float HeightScale = 10f;
	public int Rows = 10;
	public int Columns = 10;
	public Vector2 QuadDimension = new Vector2 (1, 1);

	public int mountains = 3;
	public float mountainHeight = 1f;
	public float mountainRadius = 5f;
	public float mountainHeightScale = .1f;
	public float mountainVariance = .3f;

	private Vector3 Position = Vector3.zero;
	private List<Vector3> m_Vertices = new List<Vector3>();
	private List<int> m_Indices = new List<int>();
	private MeshFilter meshFilter;
	private MeshCollider myCollider;

	private float textureScale = 1f;
	
	public void Rebuild()
	{
		Clear ();

		textureScale = NoiseTexture.width / (Columns * QuadDimension.x);

		// get component refs
		meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null) {
			Debug.LogError("MeshFilter not found!");
			return;
		}
		myCollider = GetComponent<MeshCollider>();
		if (myCollider == null) {
			myCollider = gameObject.AddComponent<MeshCollider>();
		}
		myCollider.sharedMesh = meshFilter.sharedMesh;


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
			//Debug.Log(quadPosition);
			BuildQuad(quadPosition, quadWidth, quadHeight );
		}
		
	}
	
	void BuildQuad(Vector3 position, float w, float h) {
		
		float cx = position.x, cy = position.y, cz = position.z;
		
		m_Vertices.Add (new Vector3 (cx, cy, cz)); 		
		m_Vertices.Add (new Vector3(cx + w, cy, cz + h));		
		m_Vertices.Add (new Vector3(cx + w, cy, cz));		
		m_Vertices.Add (new Vector3(cx, cy, cz));
		m_Vertices.Add (new Vector3(cx, cy, cz + h));		
		m_Vertices.Add (new Vector3(cx + w, cy, cz + h));
		
		int baseIndex = m_Vertices.Count - 6;
		
		AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
		AddTriangle(baseIndex + 3, baseIndex + 4, baseIndex + 5);
		
	}
	
	
	public void CreateMesh()
	{

		// if there's already a meshfilter, clear it
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
		
		// add terrain detail
		for (int i = 0; i < mountains; i++) {
			float randomX = (int) Random.Range (1, Rows) * QuadDimension.x;
			float randomZ = (int) Random.Range (1, Columns)  * QuadDimension.y;
			float radiusRandom = mountainRadius + (Random.Range(0, mountainVariance * mountainRadius));
			float heightRandom = mountainHeight + (Random.Range(0, mountainVariance * mountainHeight));
			AddMountain (randomX, randomZ, radiusRandom, heightRandom);
		}

		AddNoise (mesh);

		mesh.vertices = m_Vertices.ToArray();
		mesh.triangles = m_Indices.ToArray();

		//have the mesh recalculate its bounding box (required for proper rendering):
		mesh.RecalculateBounds();
		mesh.RecalculateNormals ();
		mesh.Optimize();
		
		myCollider.sharedMesh = mesh; // update collider
		
		
	}
	
	public void AddTriangle(int index0, int index1, int index2)
	{
		m_Indices.Add(index0);
		m_Indices.Add(index1);
		m_Indices.Add(index2);
	}
	
	
	public void AddNoise(Mesh mesh) {
		for (int i = 0; i < m_Vertices.Count; i++) {

			float newY = m_Vertices[i].y + HeightFromTexture(m_Vertices[i].x, m_Vertices[i].z);
			newY = m_Vertices[i].y + newY;
//			m_Vertices[i] = new Vector3(m_Vertices[i].x, SampleHeight(m_Vertices[i].x, m_Vertices[i].z), m_Vertices[i].z);
			m_Vertices[i] = new Vector3(m_Vertices[i].x, newY, m_Vertices[i].z);
		}
		mesh.vertices =  m_Vertices.ToArray();
	}


	public float HeightFromTexture( float x, float z) {
		// return y
		Color pixelColor = NoiseTexture.GetPixel ((int)(x*textureScale), (int)(z*textureScale));
		return pixelColor.grayscale * HeightScale;
	}

	public float SampleHeight(float x, float z) {
		x = x * .01f;
		z = z * .01f;
		float newY = (Mathf.PerlinNoise (x, z) * 100f);
		//Debug.Log (x + ", " + newY + ", " + z);
		return newY;
	}

	
	public void AddMountain(float randomX, float randomZ, float radius, float height) {
		
		// get random center point
		// move to high point
		// get surrounding points
		// move to mid points
		//Debug.Log ("randomX: " + randomX + ", " + " randomZ: " + randomZ);

		// find matching vertices
//		foreach(Vector3 v in m_Vertices) {
//			Debug.Log(v);
//		}
		List<Vector3> matches = m_Vertices.FindAll(x => x.x == randomX && x.z == randomZ); 

		foreach (Vector3 vertex in matches) {
			int vertexIndex = m_Vertices.FindIndex(x => x.x == vertex.x && x.z == vertex.z);
			float newY = vertex.y + (height * mountainHeightScale);
			m_Vertices [vertexIndex] = new Vector3(m_Vertices [vertexIndex].x, newY, m_Vertices [vertexIndex].z);
			Debug.Log ("Mountain Peak: " + m_Vertices [vertexIndex].x + ", " + m_Vertices [vertexIndex].y + ", " + m_Vertices [vertexIndex].z + " INDEX: " + vertexIndex);
		}

		Vector2 centerPoint = new Vector2 (randomX, randomZ);
		for (int i = 0; i < m_Vertices.Count; i++) {
			Vector2 thisPoint = new Vector2(m_Vertices[i].x, m_Vertices[i].z);
			float dx = Mathf.Pow((thisPoint.x - centerPoint.x), 2);
			float dy = Mathf.Pow((thisPoint.y - centerPoint.y), 2);
			float newY = Mathf.Pow(radius, 2) - (dx + dy);
			newY = newY * mountainHeightScale;
			//newY =  + (mountainHeight * mountainHeightScale);
			//newY = newY *  mountainHeightScale  + (peakRandomOffset);

			if (newY > 0f) {	
				m_Vertices[i] = new Vector3(m_Vertices[i].x, newY, m_Vertices[i].z);
			}
		}

	}


	public void Clear() {
	
		meshFilter = GetComponent<MeshFilter>();
		if (meshFilter.sharedMesh) {
			meshFilter.sharedMesh.Clear ();
			meshFilter.sharedMesh = null;
		}
		myCollider = GetComponent<MeshCollider>();
		if (myCollider) {
			myCollider.sharedMesh = meshFilter.sharedMesh;
		}
		m_Vertices.Clear ();
		m_Indices.Clear ();

	}
	
}
