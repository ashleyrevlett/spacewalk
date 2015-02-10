using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {

	public AudioClip jumpSound;

	public float rotateSpeed = 1000f;
	public float rotateDegrees = 360;
	public float speed = 6.0F;
	public float jumpSpeed = 12.0F;
	public float jumpForwardInertia = 2f;
	public float gravity = 10.0F;
	public float gravityAcceleration = 1.2F;
	
	public float runScaleFactor = 1.2f;
	private Vector3 moveDirection = Vector3.zero;

	
	public Transform cam;
	private CharacterController controller;
	
	private bool isJumping = false;
	private float normalJumpHeight = 2.2f;
	private float minJumpHeight = 1f;
	private float currentJumpHeight = 0f;
	
	private bool isFalling = false;
	private float maxFallSpeed = 10f;
	private float currentFallDistance = 0f;
	
	private Animator animator;

	void Start () {		
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		cam = Camera.main.transform;
	}


	void Update () {	

		// calculate move relative to forward-facing direction
		Vector3 forward = transform.forward;
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		Debug.Log ("moveHorizontal: " + moveHorizontal + ", moveVertical: " + moveHorizontal);
		Vector3 targetDirection = moveHorizontal * right + moveVertical * forward;
		moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateDegrees * Mathf.Deg2Rad * Time.deltaTime, rotateSpeed);

		animator.SetFloat ("speed", moveVertical);
		animator.SetFloat ("angularVelocity", moveHorizontal);

		// normalize and time step movement
		moveDirection = moveDirection.normalized * Time.deltaTime * speed;


		// jumping
		if (controller.isGrounded & Input.GetButton ("Jump")) {
			// starting a jump
			isJumping = true;
			currentJumpHeight = 0f;
			animator.SetTrigger("isJumping");
			AudioSource.PlayClipAtPoint(jumpSound, transform.position);

		}
		if (isJumping) {
			if ((currentJumpHeight < minJumpHeight) || ((currentJumpHeight < normalJumpHeight) && Input.GetButton ("Jump"))) {
				// under min height			
				// jumping, under max height and still pressing button
				// continuing a jump
				float jumpDistance = jumpSpeed * Time.deltaTime;
				moveDirection.y += jumpDistance;
				currentJumpHeight += jumpDistance;
			} else if ((currentJumpHeight >= normalJumpHeight) || !Input.GetButton ("Jump")) {
				// ending a jump if we're over max jump height
				// or the button is no longer pressed and we're above min jump height
				isJumping = false;
				isFalling = true;
				animator.SetTrigger("isFalling");
			}
		}

		// hit ground
		Debug.Log (controller.collisionFlags);
		if (controller.collisionFlags == CollisionFlags.Below) {
			currentFallDistance = 0f;
			isFalling = false;
			animator.SetFloat("velocity", moveVertical);	
		}

		// not jumping, not grounded => falling, apply gravity and accel
		if (!isJumping & !controller.isGrounded) {
			float distanceToFall = (currentFallDistance * gravityAcceleration * Time.deltaTime) + (gravity * Time.deltaTime);
			currentFallDistance += distanceToFall;
			distanceToFall = Mathf.Clamp(distanceToFall, 0.1f, maxFallSpeed);
			moveDirection.y -= distanceToFall;
		} 

		// move at speed	
		controller.Move (moveDirection);

		// look in direction
		if (targetDirection != Vector3.zero)
		{
			moveDirection.y = 0f; // don't fall forward
			transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.LookRotation(moveDirection)), Time.deltaTime * 3);
		}

	}



	
//	Vector3 LocalMovement() {
//		/* return player movement amount relative to camera position */
//		
//		Vector3 local = Vector3.zero;
//		
//		// get the joystick input 
//		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
//		float moveVertical = Input.GetAxisRaw ("Vertical");
//		
//		// calculate local movement vector based on forward/right relative to camera and player				
//
//		Vector3 camPosition = cam.transform.position;
//		Vector3 playerPosition = transform.position;
//		Vector3 forwardDirection = playerPosition - camPosition;		
//		forwardDirection.y = 0; // naturally looks downward; so we can see it change to no vertical change
//		//Debug.DrawRay (playerPosition, forwardDirection, Color.blue);			
//		
//		Vector3 right = Vector3.Cross (Vector3.up, forwardDirection); // find "right" direction
//		
//		if (moveHorizontal != 0) {
//			local += right * moveHorizontal;
//		}
//		
//		if (moveVertical != 0) {
//			local += forwardDirection * moveVertical;
//		}
//
//		
//		return local;
//		
//	}


	

}
