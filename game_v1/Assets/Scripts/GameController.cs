using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	
	public GameObject pauseCanvas; // need to manually wire this up in IDE

	// Use this for initialization
	void Start () {
		Unpause ();
	}
	
	// Update is called once per frame
	void Update () {
	
		// pause
		if (Input.GetButtonDown ("Pause") && !pauseCanvas.activeInHierarchy) {
			pauseCanvas.SetActive(true);
			Debug.Log("PAUSING");
			Time.timeScale = 0.0F;
		} else if (Input.GetButtonDown ("Pause") && pauseCanvas.activeInHierarchy) {
			Unpause ();
		}

	}

	public void Unpause() {	
		pauseCanvas.SetActive(false);
		Debug.Log("UNPAUSING");
		Time.timeScale = 1.0F;
	}
}
