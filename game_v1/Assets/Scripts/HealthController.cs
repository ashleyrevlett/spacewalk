using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HealthController : MonoBehaviour {

	public float StartingHitPoints = 100f;
	public float RemainingHitPoints;
	public GameObject damageParticlePrefab;
	public AudioClip damageSound;
	private GameObject player;	

	// for blink damage
	private bool isVulnerable = true;
	private Component[] playerMeshes;
	private SkinnedMeshRenderer playerSkin;

	

	void Start () {	
		RemainingHitPoints = StartingHitPoints;
		player = GameObject.FindGameObjectWithTag ("Player");
		playerMeshes = player.GetComponentsInChildren<MeshRenderer> ();
		playerSkin = player.GetComponentInChildren<SkinnedMeshRenderer> ();
	}


	public void ApplyDamage(float points) {	
		if (!isVulnerable)
			return;

		isVulnerable = false;
		RemainingHitPoints -= points;
		AudioSource.PlayClipAtPoint (damageSound, player.transform.position);
		GameObject particleObject = (GameObject)Instantiate (damageParticlePrefab, player.transform.position, player.transform.rotation);	
		Destroy (particleObject, 3f); // destroy the particles in X sec
		StartCoroutine(DoBlinks(0.2f, 0.08f));
	}


	IEnumerator DoBlinks(float duration, float blinkTime) {
		while (duration > 0f) {
			duration -= Time.deltaTime;
			
			//toggle renderer
			foreach (MeshRenderer playerRenderer in playerMeshes) {
				playerRenderer.enabled = !playerRenderer.enabled;
			}
			playerSkin.enabled = !playerSkin.enabled;
					
			//wait for a bit
			yield return new WaitForSeconds(blinkTime);
		}
		
		//make sure renderer is enabled when we exit
		foreach (MeshRenderer playerRenderer in playerMeshes) {
			playerRenderer.enabled = true;
		}
		playerSkin.enabled = true;

		isVulnerable = true;
	
	}

}
