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

	// collect minerals
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") { 
			Debug.Log ("Contacted Collectable Object");
			// show particles
			GameObject particleObject = (GameObject) Instantiate(particleEffectPrefab, transform.position, transform.rotation);
			
			// play sound
			AudioSource.PlayClipAtPoint(collectSound, transform.position);

			scoreController.score += objectPoints;
			
			// now destroy and increment score
			Destroy(gameObject, .2f);
			Destroy(particleObject, 2f); // destroy in 2 sec

		} 		
	}
	

}
