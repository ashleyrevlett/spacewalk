using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {

	// running
	public float rotateSpeed = 1000f;
	public float rotateDegrees = 360;
	public float walkSpeed = 6.0f;
	public float runSpeed = 12.0f;
	public float acceleration = 1.2f;
	public float deceleration = 5f;
	public float accelStart = .2f;
	private float currentSpeed = 0f;
	private Vector3 moveDirection = Vector3.zero;

	// jumping
	public AudioClip jumpSound;
	public float jumpHeight = 100f;
	public float minJumpHeight = 20f;
	public float normalJumpHeight = 40f;
	public float highJumpHeight = 70f;
	public float jumpAcceleration = 1.2f;
	public float minJumpSpeed = 2f;
	public float maxJumpSpeed = 12f;
	private float currentJumpSpeed = 0f;

	private bool jumpInput = false;
	private bool canJump = true;

	private bool isJumping = false;
	private float currentJumpHeight = 0f;
	private float timeSinceJumpStart = 0f;

	// gravity
	public float gravity = 10.0f;
	public float gravityAcceleration = 1.2f;
	private bool isFalling = false;
	private float maxFallSpeed = 10f;
	private float currentFallDistance = 0f;

	// gameobject references	
	public Transform cam;
	private CharacterController controller;
	private Animator animator;
	private GameObject gameControllerObject;
	private GameController gameController;

	// new jump
	private Vector3 velocity = Vector3.zero;


	void Start () {		
		// cache references to everything 
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		cam = Camera.main.transform;
		gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();
	}


	void Update () {	

		// do nothing if paused or on gameover screen
//		if (gameController.isPaused || gameController.isGameOver )
//			return;

		Vector2 playerInput = ProcessPlayerInput ();

		// determine current speed based on joystick angle + current + accel
		if (playerInput.y != 0) {	
			if (currentSpeed == 0) {
				currentSpeed += (playerInput.y * acceleration * Time.deltaTime) + accelStart;
			} else if (playerInput.y == 1) {
				// only accelerate beyond normal speed if running!
				currentSpeed += (playerInput.y * acceleration * Time.deltaTime);
			}
		} else {
			// not wanting forward, slow down
			currentSpeed -= (deceleration * Time.deltaTime);
		}
		currentSpeed = Mathf.Clamp (currentSpeed, 0f, runSpeed);
//		Debug.Log (currentSpeed);
		
		UpdateAnimations (playerInput.x, playerInput.y, currentSpeed);

		moveDirection = LocalMovement (playerInput.x, playerInput.y);

		// look in direction
		if (playerInput != Vector2.zero)
		{
			Vector3 lookDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
			transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.LookRotation(lookDirection)), Time.deltaTime * 3);
		}

		// slide
		if (playerInput.y == 0) {
			moveDirection += (transform.forward * currentSpeed);		
		}

		// add gravity
		moveDirection.y -= gravity * Time.deltaTime;

		ProcessJump();		 
			

		// move at speed	
		controller.Move (moveDirection * Time.deltaTime);
	
		// check for release of jump button before jumping again
		if (Input.GetButtonUp ("Jump"))
			canJump = true;


	}


	private Vector2 ProcessPlayerInput() {
		
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		jumpInput = Input.GetButton ("Jump");
		
		return new Vector2(moveHorizontal, moveVertical);
		
	}


	private void UpdateAnimations(float moveHorizontal, float moveVertical, float speed) {
		
		if (!jumpInput)
			animator.SetFloat ("speed", speed/runSpeed); // normalize to 0-1
		animator.SetFloat ("angularVelocity", moveHorizontal);

	}

	private void ProcessJump() {	

		// hit ground
		if (!isJumping && (controller.collisionFlags & CollisionFlags.Below) != 0) {
//			isJumping = false;
//			animator.SetTrigger("isGrounded");
			animator.SetBool("Jump", false);
			//Debug.Log("Is Grounded");
			currentJumpSpeed = 0f;

		}

		// start jump
		if (controller.isGrounded && Input.GetButtonDown ("Jump") && canJump) {
			// starting a jump
			isJumping = true;
			currentJumpHeight = 0f;
			animator.SetBool("Jump", true);
			AudioSource.PlayClipAtPoint(jumpSound, transform.position);
			timeSinceJumpStart = 0f;
//			currentJumpSpeed = minJumpSpeed;
//			Debug.Log ("starting jump - currentJumpSpeed: " + currentJumpSpeed);
		} 


		// perform a jump
		if (isJumping) {

			timeSinceJumpStart += Time.deltaTime;

			if ((currentJumpHeight < minJumpHeight) || ((currentJumpHeight < normalJumpHeight) && Input.GetButton ("Jump"))) {
				// jumping under min height or 			
				// jumping under max height and still pressing button
//				float jumpDistance = jumpSpeed * Time.deltaTime;
//				float jumpAmount = (float)(-.05f * (timeSinceJumpStart*timeSinceJumpStart)) + (10f * timeSinceJumpStart) + (minJumpSpeed);
//				currentJumpSpeed += (jumpAcceleration * currentJumpSpeed);
//				float jumpAmount = currentJumpSpeed * Time.deltaTime;
				float jumpAmount = Mathf.Max(minJumpSpeed, (maxJumpSpeed - (timeSinceJumpStart * jumpAcceleration)));
				moveDirection.y += jumpAmount;
				currentJumpHeight += jumpAmount;
			} else if ((currentJumpHeight >= normalJumpHeight) || !Input.GetButton ("Jump")) {
				// ending a jump if we're over max jump height
				// or the button is no longer pressed and we're above min jump height
//				animator.SetTrigger("isFalling");
				isJumping = false;
				timeSinceJumpStart = 0;
				currentJumpHeight = 0;
			}


		} 

	}


	Vector3 LocalMovement( float moveHorizontal, float moveVertical) {
		//Debug.Log ("moveHorizontal: " + moveHorizontal + ", moveVertical: " + moveHorizontal);
		Vector3 forward = transform.forward;
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		Vector3 targetDirection = moveHorizontal * right + moveVertical * forward;
		moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateDegrees * Mathf.Deg2Rad * Time.deltaTime, rotateSpeed);
		moveDirection = moveDirection.normalized * currentSpeed;

		return(moveDirection);
	
	}

}
