using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(NoiseGenerator))]

public class NoiseGeneratorEditor : Editor {

	// Override the Inspector GUI for the target object (i.e. Box) of this custom editor
	public override void OnInspectorGUI()
	{
		NoiseGenerator obj = (NoiseGenerator)target;
		
		if (obj == null)
			return;
		
		// Show the default Inspector GUI first
		DrawDefaultInspector();
		

//		obj.seed = EditorGUILayout.FloatField("Seed", obj.seed);
//		obj.pixWidth = EditorGUILayout.IntField("pixWidth", obj.pixWidth);
//		obj.pixHeight = EditorGUILayout.IntField("pixHeight", obj.pixHeight);
//		obj.scale = EditorGUILayout.FloatField("Scale", obj.scale);

		// Add our custom GUI to the default Inspector GUI here
		EditorGUILayout.BeginHorizontal();

		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button("Generate Noise"))
		{
			obj.Generate();
		}
		if (GUILayout.Button("Save Texture"))
		{
			obj.SaveTexture();
		}
		if (GUILayout.Button("Clear"))
		{
			obj.Clear();
		}
		EditorGUILayout.EndHorizontal();
	}

}
