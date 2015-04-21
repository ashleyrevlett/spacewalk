﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HealthController : MonoBehaviour {

	public float startingHitPoints = 3f;
	public float remainingHitPoints;
	public GameObject damageParticlePrefab;
	public AudioClip damageSound;
	public AudioClip deathSound;
	
	public bool takingDamage; // for testing whether player should have control or not
	public float invulnerableTime = 1.05f; // seconds after damage player should be invulnerable and flashing
	public float blinkTime = .075f;
	private Component[] characterMeshes;
	private SkinnedMeshRenderer characterSkin;

	public bool isDead = false;

	private GameObject levelRoot; // make all instantiated objects children of this
	private GameController gameController;
	private GameObject particleObject;
	private ShakeObject cameraShake;
	private Animator animator; // player's animator, to set damaged animation state
	private CameraMovement camera;

	void Start () {	
		characterMeshes = gameObject.GetComponentsInChildren<MeshRenderer> ();
		characterSkin = gameObject.GetComponentInChildren<SkinnedMeshRenderer> ();
		remainingHitPoints = startingHitPoints;
		levelRoot = GameObject.FindGameObjectWithTag ("Level");
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();
		cameraShake = Camera.main.GetComponent<ShakeObject> ();
		animator = gameObject.GetComponent<Animator> ();
		camera = Camera.main.GetComponent<CameraMovement> ();
		takingDamage = false;
		isDead = false;
	}



	void Update() {

		// game has been restarted and we need a new reference to level root
		if (levelRoot == null) 
			levelRoot = GameObject.FindGameObjectWithTag ("Level");

		// on pause or game over, stop everything
		if (!gameController.isPlaying) {
			StopAllCoroutines();

			// in case the blink coroutine didn't finish, turn all meshes back to visible
			foreach (MeshRenderer characterRenderer in characterMeshes) {
				characterRenderer.enabled = true;
			}
			characterSkin.enabled = true;					
			takingDamage = false;

			// turn off the particles
			if (particleObject) {
				ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();
				particleSystem.enableEmission = false;
			}
		}

	}


	public void ApplyDamage(float points) {	

		if (!gameController.isPlaying || takingDamage)
			return;

		takingDamage = true;
		Debug.Log ("Applying damage, takingDamage: " + takingDamage);

		// DIE
		StartCoroutine(DoDamage (points));

	}

	public void Die() {
		if (!isDead) {
			isDead = true;
			animator.SetTrigger ("Dead");
			AudioSource.PlayClipAtPoint (damageSound, gameObject.transform.position);
			particleObject = (GameObject)Instantiate (damageParticlePrefab, gameObject.transform.position, gameObject.transform.rotation);	
			if (particleObject != null) {
				particleObject.transform.parent = levelRoot.transform;
				Destroy (particleObject, 3f); // destroy the particles in X sec
			}
			if ( gameController.remainingLives > 0)
				gameController.EndLife ();
			else 
				gameController.EndGame();
		}
	}


	public void FallDeath() {
		if (!isDead) {
			isDead = true;
			camera.isPaused = true;
			animator.SetTrigger ("Dead");
			if ( gameController.remainingLives > 0)
				gameController.EndLife ();
			else 
				gameController.EndGame();
		}
	}


	IEnumerator DoDamage(float points) {

		if (remainingHitPoints == 0) {
			Die ();
		// OR TAKE DAMAGE
		} else {
			// cameraShake.SendMessage("Shake", .05f);		
			animator.SetTrigger ("Damaged");
			remainingHitPoints -= points;
			AudioSource.PlayClipAtPoint (damageSound, gameObject.transform.position);
			particleObject = (GameObject)Instantiate (damageParticlePrefab, gameObject.transform.position, gameObject.transform.rotation);	
			if (particleObject != null) {
				particleObject.transform.parent = levelRoot.transform;
				Destroy (particleObject, 3f); // destroy the particles in X sec
			}

			// blink
			float duration = invulnerableTime;
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

			takingDamage = false;
		}
		
	}


}
