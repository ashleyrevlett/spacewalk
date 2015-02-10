using UnityEngine;
using System.Collections;


public class ProcWater : ProcBase {

	//The width and length of each segment:
	public float m_Width = 1.0f;
	public float m_Length = 1.0f;
	
	//The maximum height of the mesh:
	public float m_Height = 1.0f;
	
	//The number of segments in each dimension (the plane will be m_SegmentCount * m_SegmentCount in area):
	public int m_SegmentCount = 10;

	public float m_WaveStrength = 5;

	//Build the mesh:
	public override Mesh BuildMesh()
	{
		//Create a new mesh builder:
		MeshBuilder meshBuilder = new MeshBuilder();
		
		//Loop through the rows:
		for (int i = 0; i <= m_SegmentCount; i++)
		{
			//incremented values for the Z position and V coordinate:
			float z = m_Length * i;
			float v = (1.0f / m_SegmentCount) * i;
			
			//Loop through the collumns:
			for (int j = 0; j <= m_SegmentCount; j++)
			{
				//incremented values for the X position and U coordinate:
				float x = m_Width * j;
				float u = (1.0f / m_SegmentCount) * j;
				
				//The position offset for this quad, with a random height between zero and m_MaxHeight:
				Vector3 offset = new Vector3(x, Random.Range(0.0f, m_Height), z);
				
				////Build individual quads:
				// BuildQuad(meshBuilder, offset);
				// BuildQuad(meshBuilder, offset, m_Width, m_Height);
				
				//build quads that share vertices:
				Vector2 uv = new Vector2(u, v);
				bool buildTriangles = i > 0 && j > 0;
				
				BuildQuadForGrid(meshBuilder, offset, uv, buildTriangles, m_SegmentCount + 1);

			}
		}
		
		//create the Unity mesh:
		Mesh mesh = meshBuilder.CreateMesh();
		
		//have the mesh calculate its own normals:
		mesh.RecalculateNormals();
		
		//return the new mesh:
		return mesh;
	}
	
	// Update is called once per frame
	void Update () {
//
//		Debug.Log ("Water updating");
//
//		Mesh mesh = GetComponent<MeshFilter>().mesh;
//		Vector3[] vertices = mesh.vertices;
//		int i = 0;
//		while (i < vertices.Length) {
//			vertices[i].y += Mathf.Sin(Time.deltaTime * m_WaveStrength);
//			i++;
//		}
//		mesh.vertices = vertices;
//		mesh.RecalculateBounds();
//




	}
}
