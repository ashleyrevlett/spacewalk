using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ProcTerrain))]


public class ProcTerrainEditor : Editor {
	
	// Override the Inspector GUI for the target object (i.e. Box) of this custom editor
	public override void OnInspectorGUI()
	{
		ProcTerrain obj = (ProcTerrain)target;
		
		if (obj == null)
			return;
		
		// Show the default Inspector GUI first
		DrawDefaultInspector();


//		// Add our custom GUI to the default Inspector GUI here
//		obj.Rows = EditorGUILayout.IntField("Rows", obj.Rows);
//		obj.Columns = EditorGUILayout.IntField("Columns", obj.Columns);
//		obj.HeightScale = EditorGUILayout.FloatField("Height Scale", obj.HeightScale);
//
//		obj.NoiseTexture = (Texture2D) EditorGUILayout.ObjectField("Image", obj.NoiseTexture, typeof (Texture2D), false); 

		EditorGUILayout.BeginHorizontal();

		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button("Rebuild"))
		{
			obj.Rebuild();
		}
		if (GUILayout.Button("Clear"))
		{
			obj.Clear();
		}

		EditorGUILayout.EndHorizontal();
	}

}
