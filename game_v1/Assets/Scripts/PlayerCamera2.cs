using UnityEngine;
using System.Collections;

public class PlayerCamera2 : MonoBehaviour {
	
	[SerializeField]
	public float distanceAway = 3.0f;
	[SerializeField]
	public float distanceUp = 2.7f;
	[SerializeField]
	public float smooth = 6.0f;
	[SerializeField]
	public float cameraVerticalOffset = 1.5f;
	
	private Transform follow;
	private Vector3 cameraTargetPosition;
	
	public float minDistanceAway = 2.0f;
	public float maxDistanceAway = 15.0f;
	
	public float zoomSpeed = 1.5f;
	public float rotateDegrees = 30.0f;
	public float rotateSpeed = 10f;
	
	public float minVerticalMovement = 2f;
	public float minVerticalRotation = 10f; // min degrees req'd to vertically rotate camera 

	
	void Start () {
		// find the player to follow
		follow = GameObject.FindWithTag ("Player").transform;			
		transform.position = new Vector3 (transform.position.x, follow.position.y + cameraVerticalOffset, transform.position.z); // initial camera position

	}
	
	
	void Update() {
		
		//		cameraTargetPosition = follow.position + (follow.up * distanceUp) - (follow.forward * distanceAway);
		//		transform.position = Vector3.Lerp (transform.position, cameraTargetPosition, Time.deltaTime * smooth);
		
		// debug right joystick
//		float moveHorizontalR = Input.GetAxisRaw ("RightH");
//		float moveVerticalR = Input.GetAxisRaw ("RightV");
//		Debug.Log ("moveHorizontalR: " + moveHorizontalR + ", moveVerticalR: " + moveVerticalR);
//		
//		if (moveHorizontalR > 0) {	
//			desiredRotation = desiredRotation + (rotateDegrees * rotateSpeed * Time.deltaTime);
//			desiredRotation = Mathf.Clamp(desiredRotation, -359, 360);
//			//transform.RotateAround(follow.position, Vector3.up, rotateDegrees * rotateSpeed * Time.deltaTime);
//			//
//			//			Quaternion newRot = Quaternion.Euler (0, rotateDegrees, 0);
//			//			newRot = transform.rotation * newRot;
//			//			Debug.Log ("newRot: " + newRot);		
//			//			transform.rotation = Quaternion.Lerp (transform.rotation, newRot, Time.deltaTime * rotateSpeed);
//		} else if (moveHorizontalR < 0) {	
//			desiredRotation = desiredRotation - (rotateDegrees * rotateSpeed * Time.deltaTime);
//			desiredRotation = Mathf.Clamp(desiredRotation, -359, 360);
//			//transform.RotateAround(follow.position, Vector3.up, rotateDegrees * rotateSpeed * Time.deltaTime * -1);
//			//			Quaternion newRot = Quaternion.Euler (0, -rotateDegrees, 0);
//			//			newRot = transform.rotation * newRot;
//			//			Debug.Log ("newRot: " + newRot);		
//			//			transform.rotation = Quaternion.Lerp (transform.rotation, newRot, Time.deltaTime * rotateSpeed);
//		}
//		
//		if (moveVerticalR > 0) {
//			float newDist = desiredDistanceAway + (zoomSpeed * Time.deltaTime);
//			desiredDistanceAway = Mathf.Clamp(newDist, minDistanceAway, maxDistanceAway);
//		} 
//		else if (moveVerticalR < 0) {
//			float newDist = desiredDistanceAway - (zoomSpeed * Time.deltaTime);
//			desiredDistanceAway = Mathf.Clamp(newDist, minDistanceAway, maxDistanceAway);
//		}
//		
//		// reset rotation and zoom if joystick not moving
//		if (moveVerticalR == 0) {
//			desiredDistanceAway = distanceUp;
//		}
//		if (moveHorizontalR == 0) {
//			desiredRotation = 0f;
//		}
		//
		//		Vector3 lookAtLocation = follow.position;
		//		lookAtLocation.y += cameraVerticalOffset;
		//		transform.LookAt (lookAtLocation);
		
	}
	
	void LateUpdate() {

		// update camera position to follow player
		cameraTargetPosition = follow.position + (follow.up * distanceUp) - (follow.forward * distanceAway);

		// if vertical look movement is < min value, don't move vertically
//		if (Mathf.Abs (transform.position.y - cameraTargetPosition.y) < minVerticalMovement)
//				cameraTargetPosition.y = transform.position.y;
//
		transform.position = Vector3.Lerp (transform.position, cameraTargetPosition, Time.deltaTime * smooth);

		// if vertical rotation is < min value, don't rotate vertically
//		Vector3 lookAtLocation = follow.position;
//		lookAtLocation.y += cameraVerticalOffset;
//
		Vector3 lookAtLocation = new Vector3( follow.position.x, 
		                                     this.transform.position.y - cameraVerticalOffset, 
		                                      follow.position.z ) ;
		transform.LookAt( lookAtLocation );
//
//		float rotationDelta = Mathf.Abs (transform.rotation.eulerAngles.y) - Mathf.Abs (lookAtLocation.y);
//		float angle = Quaternion.Angle(transform.rotation, lookAtLocation);
//		Debug.Log ("rotationDelta: " + rotationDelta);
//		if (rotationDelta < minVerticalRotation)
//			lookAtLocation.y = 0;
//
//		transform.LookAt (lookAtLocation);
//				
	}


	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
	
}
