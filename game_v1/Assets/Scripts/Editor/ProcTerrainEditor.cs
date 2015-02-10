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
		
		// Add our custom GUI to the default Inspector GUI here
		EditorGUILayout.BeginHorizontal();
		// Rebuild mesh when user click the Rebuild button
		if (GUILayout.Button("Rebuild"))
		{
			obj.Rebuild();
		}
		EditorGUILayout.EndHorizontal();
	}

}
