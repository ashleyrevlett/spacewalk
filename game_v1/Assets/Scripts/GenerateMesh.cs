using UnityEngine;
using System.Collections;

[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]


public class GenerateMesh : MonoBehaviour {

	public float m_Width;
	public float m_Length;
	public float m_Height;
	public float m_SegmentCount;

	void Start () {
			
		MeshBuilder meshBuilder = new MeshBuilder();
		
		for (int i = 0; i < m_SegmentCount; i++)
		{
			float z = m_Length * i;
			
			for (int j = 0; j < m_SegmentCount; j++)
			{
				float x = m_Width * j;
				
				Vector3 offset = new Vector3(x, Random.Range(0.0f, m_Height), z);
				
				BuildQuad(meshBuilder, offset);
			}
		}

	}

	void BuildQuad(MeshBuilder meshBuilder, Vector3 offset)
	{
		meshBuilder.Vertices.Add(new Vector3(0.0f, 0.0f, 0.0f) + offset);
		meshBuilder.UVs.Add(new Vector2(0.0f, 0.0f));
		meshBuilder.Normals.Add(Vector3.up);
		
		meshBuilder.Vertices.Add(new Vector3(0.0f, 0.0f, m_Length) + offset);
		meshBuilder.UVs.Add(new Vector2(0.0f, 1.0f));
		meshBuilder.Normals.Add(Vector3.up);
		
		meshBuilder.Vertices.Add(new Vector3(m_Width, 0.0f, m_Length) + offset);
		meshBuilder.UVs.Add(new Vector2(1.0f, 1.0f));
		meshBuilder.Normals.Add(Vector3.up);
		
		meshBuilder.Vertices.Add(new Vector3(m_Width, 0.0f, 0.0f) + offset);
		meshBuilder.UVs.Add(new Vector2(1.0f, 0.0f));
		meshBuilder.Normals.Add(Vector3.up);
		
		int baseIndex = meshBuilder.Vertices.Count - 4;
		
		meshBuilder.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
		meshBuilder.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
	}

	
	// Update is called once per frame
	void Update () {
		
	}


}
