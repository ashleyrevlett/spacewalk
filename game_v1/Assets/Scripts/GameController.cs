using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	// public so we can check the status in our controllers
	public bool isPaused = false;
	public bool isGameOver = false;

	public GameObject pauseCanvas; // need to manually wire this up in IDE
	private int loseScene = 5;
	
	private HealthController healthController;

	// Use this for initialization
	void Start () {

		GameObject player = GameObject.FindWithTag ("Player");
		healthController = player.GetComponent<HealthController> ();
		Unpause (); // need to unpause in case we have left a paused scene behind

	}
	
	// Update is called once per frame
	void Update () {
	
		// pause/unpause
		if (Input.GetButtonDown ("Pause") && !pauseCanvas.activeInHierarchy) {
			Pause ();
		} else if (Input.GetButtonDown ("Pause") && pauseCanvas.activeInHierarchy) {
			Unpause ();
		}

		//lose conditions
		if (healthController.RemainingHitPoints <= 0) {
			GameLose ();
		}


	}

	
	public void Pause() {
		pauseCanvas.SetActive(true);
		Debug.Log("PAUSING");
		Time.timeScale = 0.0F;
		isPaused = true;
	}

	public void Unpause() {	
		pauseCanvas.SetActive(false);
		Debug.Log("UNPAUSING");
		Time.timeScale = 1.0F;
		isPaused = false;
	}

	public void GameLose() {
		if (!isGameOver) {
			Debug.Log ("GAME OVER!");
			isGameOver = true;
			Time.timeScale = 0.0F;
			Application.LoadLevelAdditive (loseScene);
		}
	}



}
