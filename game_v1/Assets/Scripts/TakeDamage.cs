using UnityEngine;
using System.Collections;

public class TakeDamage : MonoBehaviour {

	public float hazardDamagePoints = 1f;
	private GameObject gameController;
	private HealthController healthController;
	private ShakeObject cameraShake;
	
	void Start () {
		gameController = GameObject.FindWithTag ("GameController");
		healthController = gameController.GetComponent<HealthController> ();	
		cameraShake = Camera.main.GetComponent<ShakeObject> ();

	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.gameObject.tag == "Hazard") {
			healthController.SendMessage("ApplyDamage", hazardDamagePoints);
			cameraShake.SendMessage("Shake", .05f);
		}
		
	}

}
