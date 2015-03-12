using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {
	
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

	public float minVerticalRotation = 10f; // min degrees req'd to vertically rotate camera 

	private GameController gamecontroller;
	
	void Start () {
		// find the player to follow
		follow = GameObject.FindWithTag ("Player").transform;			

		GameObject gc = GameObject.FindWithTag ("GameController");
		gamecontroller = gc.GetComponent<GameController> ();


		//transform.position = new Vector3 (transform.position.x, follow.position.y + cameraVerticalOffset, transform.position.z); // initial camera position

	}
	

	
	void LateUpdate() {

		if (gamecontroller.isPaused)
			return;

		// update camera position to follow player
		cameraTargetPosition = follow.position + (follow.up * distanceUp) - (follow.forward * distanceAway);

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
