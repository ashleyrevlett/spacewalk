using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HealthController : MonoBehaviour {

	public float startingHitPoints = 3f;
	public float remainingHitPoints;
	public GameObject damageParticlePrefab;
	public AudioClip damageSound;

	// for blink damage
	private bool isVulnerable = true;
	private Component[] characterMeshes;
	private SkinnedMeshRenderer characterSkin;

	private GameObject levelRoot; // make all instantiated objects children of this
	private GameController gameController;
	private GameObject particleObject;
	

	void Start () {	
		characterMeshes = gameObject.GetComponentsInChildren<MeshRenderer> ();
		characterSkin = gameObject.GetComponentInChildren<SkinnedMeshRenderer> ();
		remainingHitPoints = startingHitPoints;
		levelRoot = GameObject.FindGameObjectWithTag ("Level");
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();
	}

	void Update() {
		if (levelRoot == null) 
			levelRoot = GameObject.FindGameObjectWithTag ("Level");

		if (!gameController.isPlaying) {
			StopAllCoroutines();

			// in case the blink coroutine didn't finish, turn all meshes back to visible
			foreach (MeshRenderer characterRenderer in characterMeshes) {
				characterRenderer.enabled = true;
			}
			characterSkin.enabled = true;			
			isVulnerable = true;

			// turn off the particles
			if (particleObject) {
				ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
				particleSystem.enableEmission = false;
			}

		}
	}

	public void ApplyDamage(float points) {	


		if (!isVulnerable || !gameController.isPlaying)
			return;

		isVulnerable = false;
		remainingHitPoints -= points;
		AudioSource.PlayClipAtPoint (damageSound, gameObject.transform.position);
		particleObject = (GameObject)Instantiate (damageParticlePrefab, gameObject.transform.position, gameObject.transform.rotation);	
		if (particleObject != null) {
			particleObject.transform.parent = levelRoot.transform;
			Destroy (particleObject, 3f); // destroy the particles in X sec
		}
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
