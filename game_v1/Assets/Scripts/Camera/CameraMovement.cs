using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{
	public float smooth = 1.5f; // relative speed at which the camera will catch up
	public float rotateSmooth = 2.5f;
	public float minFollowDistance = 5f; // default distance
	public float maxFollowDistance = 10f; // distance we can be before camera will move
	public float followHeight = 2f; // height above player to follow at
	public float angleDifferenceAllowed = 15f; // allowable difference in rotations between camera andn player before moving cam to update
	public float joystickRotationForce = 3f; // camera rotation speed when joystick is pressed
	public float timeToHoldPositionAfterPlayerInput = 1f; // after releasing joystick camera pauses for thid long
	public bool isPaused = false; // disables camera movement, used when we fall off screen

	private Transform player;
	private Vector3 playerHeadPos; 
	private Vector3 playerFeetPos; 
	private CharacterController controller;
	private GameController gameController;
	
	private List<Vector3> positions; // possible camera positions
	private bool holdCameraPosition; // whether to hold the camera in the same position for limited time
	private float timePositionHeld = 0f; // how long it's been held in place

	public float occlusionAllowedTime = .3f; // seconds occlusion is allowed before changing camera position
	private float occludedTime = 0f;
	private bool isOccluded = false;

	void Start ()
	{
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag("Player").transform;

		// if there's no player in scene, we are just testing the level
		if (player == null)
			return;

		// store refs
		controller = player.GetComponent<CharacterController> ();
		gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent < GameController> ();

		// set initial camera position
		playerHeadPos = player.position + controller.center + (controller.height/2f * player.transform.up);
		transform.position = playerHeadPos + (-player.transform.forward * minFollowDistance) + (player.transform.up * followHeight);
		transform.LookAt (playerHeadPos);

		// not currently paused or holding position
		holdCameraPosition = false;
		isPaused = false;
	}



	List<Vector3> CameraPositions() {	
		// return list of possible camera positions, sorted by dist from neutral pos
		// all positions are relative directions from the player's head position

		List<Vector3> cameraPositions = new List<Vector3>();
		Vector3 neutralPositionMin = (playerHeadPos + (-player.transform.forward * (minFollowDistance)));

		int divisions = 12;
		int increment = (int)360f / divisions;

		int verticalDivisions = 3;
		int verticalIncrement = (int)60f / verticalDivisions;

		int distDivisions = 2;
		int distIncrement = (int)(maxFollowDistance - minFollowDistance) / distDivisions;

		for (int k = 0; k < distDivisions; k++) {

			Vector3 neutralPosition = (playerHeadPos + (-player.transform.forward * (minFollowDistance + (distIncrement * k))));

			for (int j = 0; j < 60; j += verticalIncrement) {
				for (int i = -180; i < 180f; i += increment) {

					Vector3 newdir = player.rotation * new Vector3(j, i, 0f);

					Vector3 pos = RotatePointAroundPivot(neutralPosition, playerHeadPos, newdir);
					cameraPositions.Add (pos);
				}
			}
		}

		cameraPositions.Sort ((a, b) => Vector3.Distance(a, neutralPositionMin).CompareTo(Vector3.Distance(b, neutralPositionMin)));		
		cameraPositions.Reverse ();

//		float colorincrement = 1f/cameraPositions.Count;
//		for (int i = 0; i < cameraPositions.Count; i++) {
//			Color32 thisColor = new Color(0f, colorincrement*i, 0f, 1);
//			Debug.DrawLine (playerHeadPos, cameraPositions[i], thisColor);
//		};

		return cameraPositions;

	}


	Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}


	void Update ()
	{

		// dont change cam position once level ends
		if (gameController.isLevelEnd)
			return;

		// don't move camera but do rotate to track player if paused
		if (isPaused) {
			playerHeadPos = player.position + controller.center + (controller.height/2f * player.transform.up);
			SmoothLookAt();
			return;
		}
				
		playerHeadPos = player.position + controller.center + (controller.height/2f * player.transform.up);
		playerFeetPos = playerHeadPos + (controller.height * -player.transform.up) + (controller.height/10f * player.transform.up);

		// player moves right joystick, adjusts camera
		Vector2 playerInput = new Vector2 (Input.GetAxisRaw ("RightH"), Input.GetAxisRaw ("RightV"));
		if (playerInput != Vector2.zero) {

			holdCameraPosition = true;
			timePositionHeld = 0f;
			float yRotationOffset = Mathf.Clamp(playerInput.x * joystickRotationForce, -179f, 179f);
			float xRotationOffset = Mathf.Clamp(playerInput.y * joystickRotationForce, -25, 25f);
			Vector3 newdir = player.rotation * new Vector3(xRotationOffset, yRotationOffset, 0f);
			Vector3 pos = RotatePointAroundPivot(transform.position, playerHeadPos, newdir);

			// push position back to min distance
			Vector3 relPosition = pos - playerHeadPos;
			float dist = Vector3.Distance(playerHeadPos, pos);
			float scaleFactor = 1f;
			if (dist < minFollowDistance) {
				scaleFactor = minFollowDistance / dist;
			} else if (dist > maxFollowDistance) {
				scaleFactor = maxFollowDistance / dist;
			}
			relPosition.Scale( new Vector3(scaleFactor, scaleFactor, scaleFactor));

			// camera shouldn't go lower than player's head
			relPosition.y = Mathf.Max (relPosition.y, playerHeadPos.y);

			transform.position = Vector3.Lerp(transform.position, playerHeadPos + relPosition, smooth * Time.deltaTime);
			SmoothLookAt();

		}

		if (holdCameraPosition && timePositionHeld < timeToHoldPositionAfterPlayerInput) {		
			// hold position, look at player
			timePositionHeld += Time.deltaTime;
			SmoothLookAt();		
		} else {
			// normal update - test position for occlusion and choose new pos if nec.
			holdCameraPosition = false;
			positions = CameraPositions ();
			UpdatePosition();

		}

	}


	void UpdatePosition() {

		Vector3 newPos = transform.position;
		
		// if player has moved outside allowed distance, move back within range
		Vector3 relPosition = playerHeadPos - transform.position;
		float dist = Vector3.Magnitude (relPosition);
		if (dist > maxFollowDistance) {
			newPos = playerHeadPos + (-player.transform.forward * maxFollowDistance) + (player.transform.up * followHeight);
		} else if (dist < minFollowDistance) {
			newPos = playerHeadPos + (-player.transform.forward * minFollowDistance) + (player.transform.up * followHeight);
		}
		
		// if player has rotated and we're no longer behind them, move and rotate
		// find difference in rotation around y axis
		float playerYRot = player.transform.eulerAngles.y;
		float cameraYRot = transform.eulerAngles.y;
		if (Mathf.Abs (playerYRot - cameraYRot) > angleDifferenceAllowed) {
			newPos = playerHeadPos + (-player.transform.forward * minFollowDistance) + (player.transform.up * followHeight);
		}
		
		// check new pos and rotate if nec
		// sort positions by distance from current camera position
		if (!ViewingPosCheck(newPos)) {

			//
//			if (occludedTime < occlusionAllowedTime) {
//				occludedTime += Time.deltaTime;
//			} else {
//				occludedTime = 0f;

				positions.Sort ((a, b) => Vector3.Distance(a, transform.position).CompareTo(Vector3.Distance(b, transform.position)));		
				positions.Reverse ();
				
				Stack<Vector3> positionStack = new Stack<Vector3>(positions);			
				while (positionStack.Count > 0) {
					newPos = positionStack.Pop();
					if (ViewingPosCheck(newPos)) // keep the 1st valid pos we find
						break;
				}
				// if we didn't find a good position, just stay where we are
				if (positionStack.Count == 0)
					newPos = transform.position;
//			}
		
		}

		// Lerp the camera's position between it's current position and it's new position.
		transform.position = Vector3.Lerp(transform.position, newPos, smooth * Time.deltaTime);

		// Make sure the camera is looking at the player.
		SmoothLookAt();

	}



	bool ViewingPosCheck (Vector3 checkPos)
	{
		// check view of player's head and feet

		RaycastHit hit;
		Vector3[] positionArray = new [] { playerHeadPos, playerFeetPos }; // , playerLookPos

		for (int i = 0; i < positionArray.Length; i++) {
			// reverse ray in case camera position is *inside* collider which is not detected by rayhit
			Vector3 direction = checkPos - positionArray[i];
			// If a raycast from the check position to the player hits something...
			// Debug.DrawRay (positionArray[i], direction, colors[i]);
			if(Physics.Raycast(positionArray[i], direction, out hit, direction.magnitude)) {	
				// camera won't be hit because it has no collider
				// if anything is hit the view is blocked
				if (hit.transform.gameObject.tag != "Player") {
					Debug.DrawLine(positionArray[i], hit.point, Color.yellow);
					return false;			
				}
			}
		}
		return true;
	}
	
	
	void SmoothLookAt ()
	{
		// Create a vector from the camera towards the player's head
		Vector3 relPlayerPosition = playerHeadPos - transform.position;

		// Create a rotation based on the relative position of the player being the forward vector.
		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);
		
		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, rotateSmooth * Time.deltaTime);
	}



	void OnDrawGizmos() {
		// debug draw for possible camera positions
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(playerHeadPos, .25f);
		Gizmos.DrawSphere(playerFeetPos, .25f);

		if (positions != null) {
			
			float colorincrement = 1f/positions.Count;
			for (int i = 0; i < positions.Count; i++) {
				Color32 thisColor = new Color(0f, 0f, (colorincrement*i), 1);
				Gizmos.color = thisColor;
				Gizmos.DrawSphere(positions[i], .1f);
			};

		}

	}

}

