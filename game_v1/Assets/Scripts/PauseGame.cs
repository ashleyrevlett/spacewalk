using UnityEngine;
using System.Collections;

public class PauseGame : MonoBehaviour {

	public void Start()
	{
		Debug.Log ("Pausing!");
		Time.timeScale = 0.0F;
	}

	
	public void Unpause()
	{
		Debug.Log ("Unpausing!");
		Time.timeScale = 1.0F;
		foreach (Transform childTransform in gameObject.transform) 
			Destroy(childTransform.gameObject);
		Destroy (gameObject);

	}
	

	
}
