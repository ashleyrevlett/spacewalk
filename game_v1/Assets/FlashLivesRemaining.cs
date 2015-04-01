using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlashLivesRemaining : MonoBehaviour {


	private Text livesText;
	private HealthController healthController;


	// Use this for initialization
	void Start () {

		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		healthController = player.GetComponent<HealthController> ();

		livesText = gameObject.GetComponent<Text> ();

	}

	void Awake() {
		int lives = healthController.remainingLives + 1;
		livesText.text = "x " + lives.ToString();
		// now flash and update to real lives remaining
		StartCoroutine ("UpdateLives");

	}

	private IEnumerator UpdateLives() {

		yield return new WaitForSeconds(1f);
		livesText.text = "x " + healthController.remainingLives.ToString();



	}

	// Update is called once per frame
	void Update () {

		// 
	
	}
}
