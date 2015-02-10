using UnityEngine;
using System.Collections;

public class TakeDamage : MonoBehaviour {

	public float hazardDamagePoints = 1f;
	private GameObject gameController;
	private HealthController healthController;
	
	void Start () {
		gameController = GameObject.FindWithTag ("GameController");
		healthController = gameController.GetComponent<HealthController> ();	
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.gameObject.tag == "Hazard") {
			healthController.SendMessage("ApplyDamage", hazardDamagePoints);
		}
		
	}

}
