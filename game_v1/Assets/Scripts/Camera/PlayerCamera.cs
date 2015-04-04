using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {
	
	public float distanceAway = 8.0f;
	public float distanceUp = 3.0f;
	
	public float minSpeed = 12f;
	public float maxSpeed = 12f;
	public float moveAccel = 2f;
	private float startMoveTime = 0f;
	private float curMoveSpeed = 0f;
	
	public float minDegreesToRotate = 12f;
//	public float timeTillRotateReset = 4f;
//	private float timeRemainingTillReset;

	public float maxRotSpeed = 300f;
	public float minRotSpeed = 2f;
	public float rotAccel = 2f;
	private float curRotSpeed = 0f;


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
	}
	

	
	void Update() {

		if (!player) {
			player = GameObject.FindWithTag ("Player");
			controller = player.GetComponent<CharacterController> ();
		}

		// update camera position to follow player

		Vector3 playerTarget = controller.transform.position + controller.center + Vector3.up * distanceUp;
		cameraTargetPosition = playerTarget - (controller.transform.forward * distanceAway);

		float distance = Vector3.Distance (cameraTargetPosition, transform.position);
		if (distance > .1f) {

			if (startMoveTime <= 0f) {
				startMoveTime = Time.time;
			}

			curMoveSpeed += Mathf.Pow (minSpeed * moveAccel, 2f) * Time.deltaTime;
			curMoveSpeed = Mathf.Clamp(curMoveSpeed, minSpeed, maxSpeed);
			float distCovered = (Time.time - startMoveTime) * curMoveSpeed;
			float fracDistance = distCovered / distance;
			transform.position = Vector3.Lerp(transform.position, cameraTargetPosition, fracDistance);

		} else {
			startMoveTime = 0f;
		}


		// update rotation to look at player if diff is enough
		Vector3 camToPlayerDir = (playerTarget - transform.position);
		///Quaternion lookRotation = Quaternion.LookRotation(player.transform.forward);
		Quaternion lookRotation = Quaternion.LookRotation(camToPlayerDir);
		float angle = Quaternion.Angle(transform.rotation, lookRotation);

		//curRotSpeed += minRotSpeed;
		curRotSpeed = Mathf.Pow (curRotSpeed, 2f) * Time.deltaTime;
		curRotSpeed = Mathf.Clamp(curRotSpeed, minRotSpeed, maxRotSpeed);
		float timeToComplete = angle / curRotSpeed;
		float donePercentage = Mathf.Min(1F, Time.deltaTime / timeToComplete);
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, donePercentage);

		// reset speed if not turning anymore
		if (angle < minDegreesToRotate) 
			curRotSpeed = minRotSpeed;



		// TODO if !viewIsClear rotate around player until it is

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
