using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
	public float smooth = 1.5f;         // The relative speed at which the camera will catch up.
	public float rotateSmooth = 2.5f;
	public float followDistance = 6f;
	public float followHeight = 2f;
	public float lookAheadDistance = 2f;
	public float maxDistanceAbove = 8f;
	private Vector3 playerPos;
	
	private Transform player;           // Reference to the player's transform.
	private CharacterController controller;
	private Vector3 relCameraPos;       // The relative position of the camera from the player.
	private float relCameraPosMag;      // The distance of the camera from the player.
	private Vector3 newPos;             // The position the camera is trying to reach.
	private int currentCameraIndex;
	public float minSecondsBeforeCameraChange = 1f;
	private float timeSinceCameraChange = 0f;
	
	void Awake ()
	{
		// Setting up the reference.
		player = GameObject.FindGameObjectWithTag("Player").transform;
		controller = player.GetComponent<CharacterController> ();
		
		// Setting the relative position as the initial relative position of the camera in the scene.
		relCameraPos = transform.position - player.position;
		relCameraPosMag = relCameraPos.magnitude - 0.5f;

		currentCameraIndex = 0;

	}


	
	void FixedUpdate ()
	{

		//Vector3 playerPos = player.position + (player.transform.up * followHeight);
		playerPos = player.position + (controller.bounds.size.y/2f * player.transform.up);

		// The standard position of the camera is the relative position of the camera from the player.
		// times the rotation of the player
		Vector3 standardPos = playerPos + (player.transform.up * followHeight) + (-player.transform.forward * followDistance);
		
		// The abovePos is directly above the player at the same distance as the standard position.
		Vector3 abovePos = standardPos + Vector3.up * maxDistanceAbove;
		Vector3 leftPos = player.position + (player.transform.up * followHeight) + (-player.transform.right * followDistance);
		Vector3 rightPos = player.position + (player.transform.up * followHeight) + (player.transform.right * followDistance);

		// An array of 5 points to check if the camera can see the player.
		Vector3[] checkPoints = new Vector3[9];
		
		// The first is the standard position of the camera.
		checkPoints[0] = standardPos;
		
		// The next three are 25%, 50% and 75% of the distance between the standard position and abovePos.
		checkPoints[1] = Vector3.Lerp(standardPos, abovePos, 0.25f);
		checkPoints[2] = Vector3.Lerp(standardPos, abovePos, 0.5f);
		checkPoints[3] = Vector3.Lerp(standardPos, abovePos, 0.75f);
		
		// The last is the abovePos.
		checkPoints[4] = abovePos;

		// try from the left if above doesn't work
		checkPoints[5] = Vector3.Lerp(standardPos, leftPos, 0.5f);
		checkPoints [6] = leftPos;

		checkPoints[7] = Vector3.Lerp(standardPos, rightPos, 0.5f);
		checkPoints [8] = rightPos;


		Color[] colors = new Color[6];
		colors [0] = Color.red;
		colors [1] = Color.yellow;
		colors [2] = Color.green;
		colors [3] = Color.magenta;
		colors [4] = Color.cyan;
		colors [5] = Color.white;

		// Run through the check points...
		for(int i = 0; i < checkPoints.Length; i++)
		{
			//Debug.DrawLine (checkPoints[i], playerPos, colors[i]);
			// ... if the camera can see the player...
			if(ViewingPosCheck(checkPoints[i])) {
				// ... break from the loop.
				// determine whether this is a new camera position of not
//				Debug.Log("i: " + i);	
//				if (currentCameraIndex != i && timeSinceCameraChange >= minSecondsBeforeCameraChange) {
//					timeSinceCameraChange = 0f;
//					currentCameraIndex = i; // update current camera
//				} else {
//					timeSinceCameraChange += Time.deltaTime;
//				}
				Debug.DrawLine(checkPoints[i], playerPos, Color.gray);
				break;
			}
		}
		
		// Lerp the camera's position between it's current position and it's new position.
		transform.position = Vector3.Lerp(transform.position, newPos, smooth * Time.deltaTime);
		
		// Make sure the camera is looking at the player.
		SmoothLookAt();

		Debug.Log ("currentCameraIndex: " + currentCameraIndex);
		//Debug.Log ("timeSinceCameraChange: " + timeSinceCameraChange.ToString("F1"));

	}
	
	
	bool ViewingPosCheck (Vector3 checkPos)
	{
		RaycastHit hit;
		Vector3 direction = playerPos - checkPos;
		// If a raycast from the check position to the player hits something...
		Debug.DrawRay (checkPos, direction, Color.blue);

		if(Physics.Raycast(checkPos, direction, out hit, direction.magnitude)) {
//		if(Physics.SphereCast(checkPos, controller.radius, direction, out hit, relCameraPosMag))
			// ... if it is not the player...
			if(hit.transform != player) {
				// This position isn't appropriate.
//				Debug.Log("hit " + hit.collider.tag);
//				Debug.DrawRay(checkPos, transform.right * controller.radius/2f, Color.red);
//				Debug.DrawRay(checkPos, -transform.right * controller.radius/2f, Color.red);
				Debug.DrawLine(checkPos, hit.point, Color.yellow);
				return false;
			}
		}
		
		// If we haven't hit anything or we've hit the player, this is an appropriate position.
//		if (timeSinceCameraChange >= minSecondsBeforeCameraChange) { 
			newPos = checkPos;
//		}

		return true;
	}
	
	
	void SmoothLookAt ()
	{
		// Create a vector from the camera towards the player's head
//		Vector3 lookAtPoint = player.position + player.transform.forward * lookAheadDistance;
		//Vector3 relPlayerPosition = playerPos - transform.position;
		// Create a vector from the camera towards the player.
		Vector3 relPlayerPosition = player.position - transform.position;

//		Debug.DrawLine(player.position, lookAtPoint, Color.magenta);

		// Create a rotation based on the relative position of the player being the forward vector.
		Quaternion lookAtRotation = Quaternion.LookRotation(relPlayerPosition, Vector3.up);
		
		// Lerp the camera's rotation between it's current rotation and the rotation that looks at the player.
		transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, rotateSmooth * Time.deltaTime);
	}
}