using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDController : MonoBehaviour {

	public float timeLimit = 30f;
	public int playerHitPoints = 10;

	public Text timerText; // hud text
	public Text scoreText;
	public Text hitPointsText;
	
	private float startTime; // timekeeping	
	private GameObject gameController;
	private ScoreController scoreController;
	private HealthController healthController;


	// Use this for initialization
	void Start () {
		startTime = Time.time; // start timer

		GameObject player = GameObject.FindWithTag ("Player");
		gameController = GameObject.FindWithTag ("GameController");
		scoreController = gameController.GetComponent<ScoreController> ();	
		healthController = player.GetComponent<HealthController> ();	

	}
	
	// Update is called once per frame
	void Update () {
		
		// update score
		scoreText.text = "Score: " + scoreController.score;
		
		// update timer
		float timeElapsed = Time.time - startTime;
		float timeRemaining = timeLimit - Mathf.Round(timeElapsed);
		timerText.text = timeRemaining.ToString();		
		hitPointsText.text = healthController.RemainingHitPoints.ToString();

	}


}



