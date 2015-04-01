using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HealthController : MonoBehaviour {

	public float startingHitPoints = 3f;
	public int startingLives = 3;
	public float remainingHitPoints;
	public int remainingLives;
	public GameObject damageParticlePrefab;
	public AudioClip damageSound;

	// for blink damage
	private bool isVulnerable = true;
	private Component[] characterMeshes;
	private SkinnedMeshRenderer characterSkin;

	

	void Start () {	
		remainingHitPoints = startingHitPoints;
		characterMeshes = gameObject.GetComponentsInChildren<MeshRenderer> ();
		characterSkin = gameObject.GetComponentInChildren<SkinnedMeshRenderer> ();
		remainingLives = startingLives;
	}


	public void ApplyDamage(float points) {	
		if (!isVulnerable)
			return;

		isVulnerable = false;
		remainingHitPoints -= points;
		AudioSource.PlayClipAtPoint (damageSound, gameObject.transform.position);
		GameObject particleObject = (GameObject)Instantiate (damageParticlePrefab, gameObject.transform.position, gameObject.transform.rotation);	
		Destroy (particleObject, 3f); // destroy the particles in X sec
		StartCoroutine(DoBlinks(0.2f, 0.08f));
	}


	IEnumerator DoBlinks(float duration, float blinkTime) {
		while (duration > 0f) {
			duration -= Time.deltaTime;
			
			//toggle renderer
			foreach (MeshRenderer characterRenderer in characterMeshes) {
				characterRenderer.enabled = !characterRenderer.enabled;
			}
			characterSkin.enabled = !characterSkin.enabled;
					
			//wait for a bit
			yield return new WaitForSeconds(blinkTime);
		}
		
		//make sure renderer is enabled when we exit
		foreach (MeshRenderer characterRenderer in characterMeshes) {
			characterRenderer.enabled = true;
		}
		characterSkin.enabled = true;

		isVulnerable = true;
	
	}

}
