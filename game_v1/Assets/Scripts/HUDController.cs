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
	private PlayerController2 player;


	// Use this for initialization
	void Start () {
		startTime = Time.time; // start timer
		GameObject playerObject = GameObject.FindGameObjectWithTag ("Player");
		player = playerObject.GetComponent<PlayerController2> ();
	}
	
	// Update is called once per frame
	void Update () {
		
		// update score
		scoreText.text = "Score: " + player.score;
		
		// update timer
		float timeElapsed = Time.time - startTime;
		float timeRemaining = timeLimit - Mathf.Round(timeElapsed);
		timerText.text = timeRemaining.ToString();		
		hitPointsText.text = player.hitPoints.ToString();

	}


}



