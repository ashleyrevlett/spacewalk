using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HealthController : MonoBehaviour {

	public float StartingHitPoints = 100f;
	public float RemainingHitPoints;
	public GameObject damageParticlePrefab;
	public AudioClip damageSound;

	// for blink damage
	private bool isVulnerable = true;
	private Component[] characterMeshes;
	private SkinnedMeshRenderer characterSkin;

	

	void Start () {	
		RemainingHitPoints = StartingHitPoints;
		characterMeshes = gameObject.GetComponentsInChildren<MeshRenderer> ();
		characterSkin = gameObject.GetComponentInChildren<SkinnedMeshRenderer> ();
	}


	public void ApplyDamage(float points) {	
		if (!isVulnerable)
			return;

		isVulnerable = false;
		RemainingHitPoints -= points;
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
