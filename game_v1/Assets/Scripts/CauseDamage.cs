﻿using UnityEngine;
using System.Collections;

public class CauseDamage : MonoBehaviour {
	
	public float damagePoints = 1f;
	private GameObject gameController;
	private HealthController healthController;

	// Use this for initialization
	void Start () {
		gameController = GameObject.FindWithTag ("GameController");
		healthController = gameController.GetComponent<HealthController> ();
	}
		
	/* 
	 * Detecting collisions: 
	 * If the object hits the player, it's detected here.
	 * If the player hits the object, it's detected by the player's TakeDamage script.
	*/

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Player") {
			healthController.SendMessage("ApplyDamage", damagePoints);
		}

	}
	
}
