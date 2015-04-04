using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

	public float distanceAway = 8.0f;
	public float distanceUp = 3.0f;
	public float smooth = 12.0f;
	public float cameraVerticalOffset = 1.5f; // so we're not looking at the player's feet
	public float minDegreesToRotate = 12f;
	public float timeTillRotateReset = 4f;
	private float timeRemainingTillReset;


	private GameObject player;
	private Transform follow;
	private Vector3 cameraTargetPosition;	
	private CharacterController controller;

	public bool avoidObstacles = true;
	public float timeTillMove = 1f; // seconds for view to be obstructed until we move; avoid jitter
	private bool viewIsClear = true;
	private float rotateOffset = 0f;



	void Start () {
		// follow the player
		player = GameObject.FindWithTag ("Player");
		follow = player.transform;			
		controller = player.GetComponent<CharacterController> ();
		timeRemainingTillReset = timeTillRotateReset;
	}
	

	
	void Update() {

		if (!player) {
			player = GameObject.FindWithTag ("Player");
			controller = player.GetComponent<CharacterController> ();
			follow = controller.transform;
		}

		// update camera position to follow player
		Vector3 playerTarget = follow.position + (follow.up * distanceUp);
		cameraTargetPosition = playerTarget - (follow.forward * distanceAway);
		transform.position = Vector3.Lerp (transform.position, cameraTargetPosition, Time.deltaTime * smooth);

		// update rotation to look at player if diff is enough
		Vector3 camToPlayerDir = playerTarget - cameraTargetPosition;
		Quaternion newRot = new Quaternion ();
		newRot.SetLookRotation (camToPlayerDir);
		float angle = Quaternion.Angle(transform.rotation, newRot);
//		if (Mathf.Abs(angle) > minDegreesToRotate) {
			transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * smooth);
//		}		


		// TODO 
		// if !viewIsClear rotate around player until it is

	}


	void FixedUpdate() {

		// viewIsClear = isViewClear();		
	
	}


	
	private bool isViewClear() {

		// only need to see player's head, not feet
		Vector3 p1 = follow.position + controller.center + Vector3.up * -controller.height * 0.2F;
		Vector3 p2 = p1 + Vector3.up * controller.height * 0.2f;
		Vector3 playerToCameraDir = p2 - transform.position;

		RaycastHit hit;
		float dist = playerToCameraDir.magnitude;
		if (Physics.SphereCast(transform.position, (controller.radius * .7f), playerToCameraDir, out hit, 100)) {
			if (hit.transform.tag == "Terrain") {
				// Debug.Log ("View obstructed, distance to hit: " + hit.distance.ToString ("F1"));
				Debug.DrawRay (transform.position, (playerToCameraDir.normalized * hit.distance), Color.white, Time.deltaTime);
				dist = hit.distance;
			}
		}	

		if (dist < playerToCameraDir.magnitude) {
			return false;
		}
		return true;
		
	}
	

}
