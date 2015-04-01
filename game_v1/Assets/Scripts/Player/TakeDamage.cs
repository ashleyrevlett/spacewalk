using UnityEngine;
using System.Collections;

public class TakeDamage : MonoBehaviour {

	public float hazardDamagePoints = 1f;
	private HealthController healthController;
	private ShakeObject cameraShake;
	private CharacterController controller;
	
	void Start () {
		healthController = gameObject.GetComponent<HealthController> ();	
		cameraShake = Camera.main.GetComponent<ShakeObject> ();
		controller = gameObject.GetComponent<CharacterController> ();
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		if (hit.gameObject.tag == "Hazard") {
			healthController.SendMessage("ApplyDamage", hazardDamagePoints);
			cameraShake.SendMessage("Shake", .05f);
		}

		if (hit.gameObject.tag == "Enemy" || hit.gameObject.tag == "Boss") {
			// hit an enemy not from above, take damage
			if (((controller.collisionFlags & CollisionFlags.Sides) != 0) || ((controller.collisionFlags & CollisionFlags.Above) != 0)) {
				NPCController npccontroller = hit.gameObject.GetComponent<NPCController>();
				float damagePoints = npccontroller.points;
				healthController.SendMessage("ApplyDamage", damagePoints);
				cameraShake.SendMessage("Shake", .05f);
			}

		}
	}

}
