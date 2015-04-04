using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public int startingLives = 3;
	public int remainingLives;

	public GameObject levelPrefab;

	public bool showIntro = false;

	// public so we can check the status in our controllers
	public bool isPaused = false;
	public bool isGameOver = false;
	public bool isPlaying = true;

	public GameObject pauseCanvas; // need to manually wire this up in IDE
	public GameObject loseLifeCanvas;
	public GameObject gameOverCanvas;
	public GameObject hudCanvas;
	//private int loseScene = 1; // scene # from build settings
	
	private HealthController healthController;
	private ScoreController scoreController;
	private LevelTimer levelTimer;

	public GameObject levelIntro; // need to manually wire this up in IDE
//	private bool isPlaying = false;

	private GameObject player;
	private Animator playerAnim; 
	private Vector3 playerSpawnPoint;
	private Quaternion playerSpawnRotation;
	private Vector3 cameraSpawnPoint;
	private Quaternion cameraSpawnRotation;
	private GameObject levelRoot;

	// Use this for initialization
	void Start () {

		
		GameObject levelTimerObject = GameObject.FindWithTag ("Level");
		levelTimer = levelTimerObject.GetComponent<LevelTimer> ();	

		remainingLives = startingLives;

		player = GameObject.FindWithTag ("Player");
		healthController = player.GetComponent<HealthController> ();
		scoreController = gameObject.GetComponent<ScoreController> ();
		playerAnim = player.GetComponent<Animator> ();

		playerSpawnPoint = player.transform.position;
		playerSpawnRotation = player.transform.rotation;
		cameraSpawnPoint = Camera.main.transform.position;
		cameraSpawnRotation = Camera.main.transform.rotation;

		levelRoot = GameObject.FindGameObjectWithTag ("Level");
		RestartLevel ();

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
//		if (healthController.remainingHitPoints < 0 && remainingLives >= 0) {		 // 
//			StartCoroutine ("LoseLife");
//		}
//
//		} else if (healthController.remainingHitPoints <= 0 && remainingLives < 0) {
//				
//			StartCoroutine("GameOver");
//
//		}



	}
		
	public void Pause() {
		pauseCanvas.SetActive(true);
		isPlaying = false;
		isPaused = true;
	}

	public void Unpause() {	
		pauseCanvas.SetActive(false);
		isPlaying = true;
		isPaused = false;
	}

	public void EndGame() {
		// public wrapper for gameover coroutine
		StartCoroutine ("GameOver");
	}
	
	IEnumerator GameOver() {
		Debug.Log ("GAME OVER!");	
		if (!isGameOver) {
			// finish death animation
			yield return new WaitForSeconds(3.8f);
			isGameOver = true;
			isPlaying = false;
			hudCanvas.SetActive(false);
			gameOverCanvas.SetActive(true);
		}
		yield return null;
		
	}
	
	public void EndLife() {
		// public wrapper for gameover coroutine
		StartCoroutine ("LoseLife");
	}

	IEnumerator LoseLife() {
		Debug.Log ("Losing life!!!");
		if (!isGameOver) {
			// finish death animation
			yield return new WaitForSeconds(2.5f);
			isGameOver = true;
			isPlaying = false;
			hudCanvas.SetActive(false);
			remainingLives -= 1;
			healthController.remainingHitPoints = healthController.startingHitPoints;
			loseLifeCanvas.SetActive(true);
			yield return null;
		}
		yield return null;
	}

	// TODO can't just restart, have to reset all npcs minerals etc... might as well reload level 
	// requires game controller to be outside level so it won't get destroyed
	// should this contain the health logic instead of the player character?

	public void RestartLevel() {

		hudCanvas.SetActive (true);

		Debug.Log ("Restarting level!");

		healthController.remainingHitPoints = healthController.startingHitPoints;
		scoreController.score = 0;
		levelTimer.ResetTimer ();

		player.transform.position = playerSpawnPoint;
		player.transform.rotation = playerSpawnRotation;
		Camera.main.transform.position = cameraSpawnPoint;
		Camera.main.transform.rotation = cameraSpawnRotation;

		hudCanvas.SetActive(true);
		loseLifeCanvas.SetActive(false);
		gameOverCanvas.SetActive(false);

		isGameOver = false;
		isPlaying = true;

		if (levelRoot) { // if level object is in scene, destroy and reload
			Destroy (levelRoot);
			levelRoot = Instantiate (levelPrefab) as GameObject;
		}
		Unpause (); // need to unpause in case we have left a paused scene behind
		
		// wait for player to hit start before upausing and playing
		if (showIntro) {
			isPlaying = false;
			levelIntro.SetActive(true);
		} else {
			StartLevel ();
		}

		// reset animations (after game over)
		playerAnim.SetBool ("Damaged", false);
		playerAnim.SetBool ("FallStart", false);
		playerAnim.SetBool ("JumpStart", false);
		playerAnim.SetBool ("JumpLoop", false);
		playerAnim.SetBool ("JumpEnd", false);
		playerAnim.SetBool ("DoubleJump", false);
		playerAnim.SetTrigger ("Alive");


	}


	public void StartLevel() {
		Debug.Log ("Starting level!");
		levelIntro.SetActive(false);
		hudCanvas.SetActive(true);
		isGameOver = false;
		isPaused = false;
		Unpause (); // resumes timescale = 1
	}


	public void QuitGame() {
		Application.Quit();
	}


	public void RestartGame() {
		remainingLives = startingLives;
		RestartLevel ();

	}


}
