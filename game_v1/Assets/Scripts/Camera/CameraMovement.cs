using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
{
	public float smooth = 1.5f;         // The relative speed at which the camera will catch up.
	public float rotateSmooth = 2.5f;
	public float minFollowDistance = 5f;
	public float maxFollowDistance = 10f;
	public float followHeight = 2f;
	public float lookDistance = 5f;
	public float angleDifferenceAllowed = 15f; // allowable difference in rotations between camera andn player before moving cam to update
	public float borderPercent = .2f; // percent of screen edge to keep targets within
	public float occlusionAllowedTime = .3f; // seconds occlusio is allowed before changing camera position
	private float occludedTime = 0f;
	private bool isOccluded = false;

	private Vector3 rotationOffset = Vector3.zero; // player pushes right joystic, rotates camera around
	public float joystickRotationForce = 3f;

	private float minScreenYPos;
	private float minScreenXPos;
	private float maxScreenYPos;
	private float maxScreenXPos;
	private float correctionIncrement;

	private Vector3 playerHeadPos; // A1
	private Vector3 playerFeetPos; // A2
	private Vector3 playerLookPos; // B
	
	private Transform player;
	private CharacterController controller;
	private CharacterMotor motor;
	private GameController gameController;
//	private int currentCameraIndex;
//	public float minSecondsBeforeCameraChange = 1f;
//	private float timeSinceCameraChange = 0f;
	public float journeyTime = 3.0F;
	private float startTime = 0f;

	public bool isPaused = false;
	private List<Vector3> positions;
	private bool holdCameraPosition;
	private float timePositionHeld = 0f;
	public float timeToHoldPositionAfterPlayerInput = 1f;

	void Start ()
	{
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag("Player").transform;
		motor = player.GetComponent<CharacterMotor> ();
		controller = player.GetComponent<CharacterController> ();
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag("GameController");
		gameController = gameControllerObject.GetComponent < GameController> ();

		// set initial camera position
		playerHeadPos = player.position + controller.center + (controller.height/2f * player.transform.up);

		transform.position = playerHeadPos + (-player.transform.forward * minFollowDistance) + (player.transform.up * followHeight);
		transform.LookAt (playerHeadPos);

		startTime = Time.time;

		minScreenXPos = (int)(Screen.width * borderPercent);
		maxScreenXPos = (int)(Screen.width - minScreenXPos);
		minScreenYPos = (int)(Screen.height * borderPercent);
		maxScreenYPos = (int)(Screen.height - minScreenYPos);
		correctionIncrement = (int)(Screen.width * borderPercent) / 2;

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

		int distDivisions = 3;
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
//
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

		// don't move camera but do rotate to track player
		if (isPaused) {
			playerHeadPos = player.position + controller.center + (controller.height/2f * player.transform.up);
			SmoothLookAt();
			return;
		}

		
		playerHeadPos = player.position + controller.center + (controller.height/2f * player.transform.up);
		playerFeetPos = playerHeadPos + (controller.height * -player.transform.up) + (controller.height/10f * player.transform.up);
		playerLookPos = playerHeadPos + player.transform.forward * lookDistance;
		Vector3 neutralPositionMin = (playerHeadPos + (-player.transform.forward * (minFollowDistance)));
		Vector3 neutralPositionMax = (playerHeadPos + (-player.transform.forward * (maxFollowDistance)));

		Vector2 playerInput = new Vector2 (Input.GetAxisRaw ("RightH"), Input.GetAxisRaw ("RightV"));
		if (playerInput != Vector2.zero) {
			holdCameraPosition = true;
			timePositionHeld = 0f;

			Debug.Log ("playerInput: " + playerInput.x.ToString("F1") + ", " + playerInput.y.ToString("F1"));

			float yRotationOffset = Mathf.Clamp(playerInput.x * joystickRotationForce, -179f, 179f);
			float xRotationOffset = Mathf.Clamp(playerInput.y * joystickRotationForce, -25, 25f);

			Vector3 newdir = player.rotation * new Vector3(xRotationOffset, yRotationOffset, 0f);
			float newdirDist = Vector3.Magnitude(newdir);


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
			Debug.Log("scaleFactor: " + scaleFactor.ToString("F1"));
			// camera shouldn't go lower than player's head
			//relPosition.y = Mathf.Max (relPosition.y, playerHeadPos.y);

			transform.position = Vector3.Lerp(transform.position, playerHeadPos + relPosition, smooth * Time.deltaTime);
			SmoothLookAt();

		}

		if (holdCameraPosition && timePositionHeld < timeToHoldPositionAfterPlayerInput) {		
			timePositionHeld += Time.deltaTime;
			SmoothLookAt();
		
		} else {
			holdCameraPosition = false;
			positions = CameraPositions ();
			UpdatePosition();

		}
//			Debug.DrawLine (transform.position, playerHeadPos, Color.white);
//			Debug.DrawLine (transform.position, playerFeetPos, Color.white);
//			Debug.DrawLine (transform.position, playerLookPos, Color.white);

	}


	void UpdatePosition() {
		
		bool validPositionFound = false;
		float degreesTurned = 0f;
		float distanceTried = 0f;
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
		
		// now make sure points remain within margin of camera's viewport
		//		int runs = 5;
		//		int i = 0;
		//		while (!InBoundsPosCheck () && i < runs) {
		//
		//			i += 1;
		//
		//			// find which point is out of bounds and move camera in that direction by an increment
		//			Vector3 dir = InBoundsCorrection();
		//			transform.position = Vector3.Lerp(transform.position, transform.position + dir, smooth * Time.deltaTime);
		//
		//		}
		
		// Make sure the camera is looking at the player.
		SmoothLookAt();
		
		//Debug.Log ("currentCameraIndex: " + currentCameraIndex);
		//Debug.Log ("timeSinceCameraChange: " + timeSinceCameraChange.ToString("F1"));

	}


	Vector3 InBoundsCorrection() {
		Vector3[] positionArray = new [] { playerHeadPos, playerLookPos }; // , playerFeetPos, playerLookPos
		Vector3 adjustment = Vector3.zero;
		for (int i = 0; i < positionArray.Length; i++) {
			Vector3 screenPos = Camera.main.WorldToScreenPoint(positionArray[i]);
			if (screenPos.x <= maxScreenXPos || screenPos.x >= minScreenXPos || 
			    screenPos.y <= maxScreenYPos || screenPos.y >= minScreenYPos) {
				Vector3 dirToObj = positionArray[i] - transform.position;
				adjustment += dirToObj.normalized * correctionIncrement;
			}
		}
		return adjustment;

	}


	bool InBoundsPosCheck () { 
		bool validPosition = true;
		Vector3[] positionArray = new [] { playerHeadPos, playerLookPos }; // , playerFeetPos, playerLookPos
		for (int i = 0; i < positionArray.Length; i++) {
			Vector3 screenPos = Camera.main.WorldToScreenPoint(positionArray[i]);
			if (screenPos.x <= maxScreenXPos && screenPos.x >= minScreenXPos &&
			    screenPos.y <= maxScreenYPos && screenPos.y >= minScreenYPos) {
				continue;
				//print("target " + i.ToString() + " is in bounds: (" + screenPos.x.ToString() + ", " + screenPos.y.ToString());
			} else {
				print ("out of bounds");
				//print("target " + i.ToString() + " is out of bounds: (" + screenPos.x.ToString() + ", " + screenPos.y.ToString());
				return false;
			}
		}
		return true;
	}


	bool ViewingPosCheck (Vector3 checkPos)
	{
		// check view of player's head and feet

		RaycastHit hit;
		Vector3[] positionArray = new [] { playerHeadPos, playerFeetPos }; // , playerLookPos
		Color[] colors = new[] { Color.blue, Color.red, Color.magenta };

		for (int i = 0; i < positionArray.Length; i++) {
			// reverse ray in case camera position is *inside* collider which is not detected by rayhit
			Vector3 direction = checkPos - positionArray[i];
			// If a raycast from the check position to the player hits something...
//			Debug.DrawRay (positionArray[i], direction, colors[i]);

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
//		Vector3 lookAtPoint = player.position + player.transform.forward * lookAheadDistance;
		//Vector3 relPlayerPosition = playerPos - transform.position;
		// Create a vector from the camera towards the player.
		Vector3 relPlayerPosition = playerHeadPos - transform.position;

//		Debug.DrawLine(player.position, lookAtPoint, Color.magenta);

		// Create a rotation based on the relative position of the player being the forward vector.
		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);
		
		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, rotateSmooth * Time.deltaTime);
	}



	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(playerHeadPos, .25f);
		Gizmos.DrawSphere(playerFeetPos, .25f);
//		Gizmos.DrawSphere(playerLookPos, .25f);

		if (positions != null) {
			
			float colorincrement = 1f/positions.Count;
			for (int i = 0; i < positions.Count; i++) {
				Color32 thisColor = new Color(0f, 0f, (colorincrement*i), 1);
				//Debug.DrawLine (playerHeadPos, positions[i], thisColor);
				Gizmos.color = thisColor;
				Gizmos.DrawSphere(positions[i], .1f);
			};

		}

	}

}

