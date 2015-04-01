using UnityEngine;
using System.Collections;

public class CollectObject : MonoBehaviour {

	public GameObject particleEffectPrefab; // particle explosions
	public AudioClip collectSound; // sound fx
	public int objectPoints = 1;
	private GameObject gameController;
	private ScoreController scoreController;

	void Start() {	
		gameController = GameObject.FindWithTag ("GameController");
		scoreController = gameController.GetComponent<ScoreController> ();	
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") { 

			// show particles
			GameObject particleObject = (GameObject) Instantiate(particleEffectPrefab, transform.position, transform.rotation);
			
			// play sound
			AudioSource.PlayClipAtPoint(collectSound, transform.position);

			// increment score
			 scoreController.score += objectPoints;
			
			// destroy mineral and particle after collection
			Destroy(gameObject, .2f);
			Destroy(particleObject, 2f); // destroy in 2 sec

		} 		
	}
	

}
