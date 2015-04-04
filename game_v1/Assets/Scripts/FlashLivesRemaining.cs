using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlashLivesRemaining : MonoBehaviour {

	private Text livesText;
	private GameController gameController;

	// Use this for initialization
	void Start () {
//		Debug.Log ("TXT Starting");
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();
		livesText = gameObject.GetComponent<Text> ();
		int lives = gameController.remainingLives + 1;
		livesText.text = "x " + lives.ToString();
//		Debug.Log ("Flashing lives");		
		StartCoroutine(UpdateLives());
	}

	void Update() {
		//Debug.Log ("TXT Updating");
		if (!gameController) {
			GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
			gameController = gameControllerObject.GetComponent<GameController> ();
		}
	}


	void OnEnable() {
//		Debug.Log ("TXT Enabling");
		if (gameController) {
			int lives = gameController.remainingLives + 1;
			livesText.text = "x " + lives.ToString();
			StartCoroutine(UpdateLives());
		}
	}

	void OnDisable () {
//		Debug.Log ("TXT Disabling");
	}

	private IEnumerator UpdateLives() {
		yield return new WaitForSeconds(.5f);
		int lives = gameController.remainingLives;
		livesText.text = "x " + lives.ToString();
		yield return null;
	}


}
