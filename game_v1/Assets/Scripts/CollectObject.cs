using UnityEngine;
using System.Collections;

public class CollectObject : MonoBehaviour {

	public GameObject particleEffectPrefab; // particle explosions
	public AudioClip collectSound; // sound fx

	// collect minerals
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Player") { 
			Debug.Log ("Contacted Mineral");
			// show particles
			GameObject particleObject = (GameObject) Instantiate(particleEffectPrefab, transform.position, transform.rotation);
			
			// play sound
			AudioSource.PlayClipAtPoint(collectSound, transform.position);
			
			// now destroy and increment score
			Destroy(gameObject, .2f);

		} 		
	}
	

}
