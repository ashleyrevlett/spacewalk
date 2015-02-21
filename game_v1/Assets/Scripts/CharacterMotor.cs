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
	public float minJumpHeight = 120f;
	public float normalJumpHeight = 160f;
	public float minJumpSpeed = 4f;
	public float maxJumpSpeed = 8f;
	public float jumpAcceleration = 5f;
	private bool jumpInput = false;
	private bool canJump = true;
	private bool isJumping = false;
	private float currentJumpHeight = 0f;
	private float timeSinceJumpStart = 0f;

	// double jumping
	public float doubleJumpHeight = 200f;
	public float doubleJumpSpeed = 24f;
	public float timeToDoubleJump = 0.1f;
	public float doubleJumpAcceleration = 1.2f;
	private bool canDoubleJump = false;
	private float doubleJumpTimer = 0f;
	private bool isDoubleJumping = false;
	

	// gravity
	public float gravity = 10.0f;
	public float gravityAcceleration = 1.2f;

	// gameobject references	
	public Transform cam;
	private CharacterController controller;
	private Animator animator;
	private GameObject gameControllerObject;
	private GameController gameController;



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
		if (gameController.isPaused || gameController.isGameOver )
			return;

		// get input from gamepad
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
		// Debug.Log (currentSpeed);

		// convert player input to worldspace direction, rotate player transform
		moveDirection = LocalMovement (playerInput.x, playerInput.y, currentSpeed);

		// handle jump input and movement
		//ProcessJump();		 
		//StartCoroutine ("ProcessJumpInput");			


		ProcessJump ();


		// add gravity
		moveDirection.y -= gravity * Time.deltaTime;

		// move at speed	
		controller.Move (moveDirection * Time.deltaTime);
			
		// update the animator's values
		UpdateAnimations (playerInput.x, playerInput.y, currentSpeed);


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

		
	Vector3 LocalMovement( float moveHorizontal, float moveVertical, float speed) {
		//Debug.Log ("moveHorizontal: " + moveHorizontal + ", moveVertical: " + moveHorizontal);
		Vector3 forward = transform.forward;
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		Vector3 targetDirection = moveHorizontal * right + moveVertical * forward;
		moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateDegrees * Mathf.Deg2Rad * Time.deltaTime, rotateSpeed);
		moveDirection = moveDirection.normalized * speed;

		// if player doesn't push joystick forward, add forward inertia to slide to a stop
		if (moveVertical == 0) {
			moveDirection += (transform.forward * speed);				
		}
		
		// look in direction of movement if player is pressing joystick
		if (moveHorizontal != 0 || moveVertical != 0)
		{
			Vector3 lookDirection = new Vector3(targetDirection.x, 0f, targetDirection.z);
			transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.LookRotation(lookDirection)), Time.deltaTime * rotateSpeed);
			//Debug.Log("lookDirection: " + lookDirection);
		}

		return(moveDirection);
		
	}


//	private IEnumerator ProcessJumpInput() {	
//	
//		if (jumpInput && canJump) {
//			animator.SetBool("Jump", true);
//			isJumping = true;
//			timeSinceJumpStart = 0;
//			yield return 0;
//			animator.SetBool("Jump", false);
//			canJump = false;
//		} else if (!jumpInput) {
//			yield return 0;
//			canJump = true;
//		}
//			
//	}
//
//
//	private void ProcessJump() {	
//		
//		// continue jump
//		
//		timeSinceJumpStart += Time.deltaTime; // speed is based on timer
//		
//		// calculate jump height (double, single, button pressed or not)
//		float jumpAmount = 0f;
//		if ((currentJumpHeight < minJumpHeight) || 
//		    ((currentJumpHeight < normalJumpHeight) && Input.GetButton ("Jump"))) {
//			// jumping under min height or 			
//			// jumping under normal height and still pressing button
//			jumpAmount = Mathf.Max(minJumpSpeed, (maxJumpSpeed - (timeSinceJumpStart * jumpAcceleration)));
//		}
//		
//		// move to new jump height
//		moveDirection.y += jumpAmount;
//		currentJumpHeight += jumpAmount;
//		
//		// end jump conditions
//		if (currentJumpHeight >= normalJumpHeight) {
//			isJumping = false;			
//		} else if ((controller.collisionFlags & CollisionFlags.Above) != 0) {
//			// we never made it to the max height but we can't go any higher
//			Debug.Log ("Collision above!");
//			isJumping = false;
//		}
//		
//		// test if jump is complete
//		//		RaycastHit hit;
//		//		if (Physics.Raycast(transform.position, -Vector3.up, out hit)) {
//		//			float distanceToGround = hit.distance;
//		//			if (distanceToGround <= .04f) {
//		//				isJumping = false;
//		//			}
//		//		}
//		
//	} 
//
	


	private void ProcessJump() {	
		
		// check for release of jump button before jumping again
		if (Input.GetButtonUp ("Jump"))
			canJump = true;

		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit)) {
			float distanceToGround = hit.distance;
			if (distanceToGround <= .04f) {
				animator.SetBool("Fall", false);
				animator.SetBool("Jump", false);
				isDoubleJumping = false;
			}
		}

//		if ((controller.collisionFlags & CollisionFlags.Below) != 0) {
//			animator.SetBool("Fall", false);
//			isDoubleJumping = false;
//		}

		// double jump timer bookkeeping
		if (canDoubleJump) 
			doubleJumpTimer += Time.deltaTime;		
		if (canDoubleJump && doubleJumpTimer > timeToDoubleJump) {
			canDoubleJump = false;
			doubleJumpTimer = 0f;
		}
		
		// start jump or double jump		
		if (controller.isGrounded && Input.GetButtonDown ("Jump") && canJump) {
			// canJump means we've released the button before pressing it again
			// starting a jump
			isJumping = true;
			timeSinceJumpStart = 0f;
			currentJumpHeight = 0f;
			animator.SetBool("Jump", true);
			AudioSource.PlayClipAtPoint(jumpSound, transform.position);

			if (canDoubleJump) {
				Debug.Log("Double jumping!");
				isDoubleJumping = true;
			}

		} 
				
		// continue jump
		if (isJumping) {			

			timeSinceJumpStart += Time.deltaTime; // speed is based on timer

			// calculate jump height (double, single, button pressed or not)
			float jumpAmount = 0f;
			if (isDoubleJumping) {			
				jumpAmount = Mathf.Max(minJumpSpeed, (doubleJumpSpeed - (timeSinceJumpStart * doubleJumpAcceleration)));			
			} else if ((currentJumpHeight < minJumpHeight) || 
			           ((currentJumpHeight < normalJumpHeight) && Input.GetButton ("Jump"))) {
				// jumping under min height or 			
				// jumping under normal height and still pressing button
				jumpAmount = Mathf.Max(minJumpSpeed, (maxJumpSpeed - (timeSinceJumpStart * jumpAcceleration)));
			}

			// move to new jump height
			moveDirection.y += jumpAmount;
			currentJumpHeight += jumpAmount;

			// end jump conditions
			// double jump at max height
			if (isDoubleJumping && (currentJumpHeight >= doubleJumpHeight)) {
				isDoubleJumping = false;
				isJumping = false;
				animator.SetBool("Jump", false);
				animator.SetBool("Fall", true);
			} else if (!isDoubleJumping && (currentJumpHeight >= normalJumpHeight)) {
				// normal jump over max jump height
				isJumping = false;
				canDoubleJump = true; // can double jump if we've just landed; TODO check if has already double jumped
				animator.SetBool("Jump", false);
				animator.SetBool("Fall", true);		
			} else if ((controller.collisionFlags & CollisionFlags.Above) != 0) {
				// we never made it to the max height but we can't go any higher
				Debug.Log ("Collision above!");
				isJumping = false;
				canDoubleJump = false; // can double jump if we've just landed; TODO check if has already double jumped
				animator.SetBool("Jump", false);
				animator.SetBool("Fall", true);		
			}
			
		} 
		
	}
//
//	private void ProcessJump() {	
//		Debug.Log ("canDoubleJump: " + canDoubleJump);
//		Debug.Log ("isDoubleJumping: " + isDoubleJumping);
//		Debug.Log ("isJumping: " + isJumping);
//
//		// hit ground
//		if (isJumping && (controller.collisionFlags & CollisionFlags.Below) != 0) {
//
//			isJumping = false;
//			timeSinceJumpStart = 0;
//			currentJumpHeight = 0;
//
//			animator.SetBool("Jump", false);
//			//Debug.Log("Is Grounded");
//			currentJumpSpeed = 0f;
//			canDoubleJump = true;
//			isDoubleJumping = false;
//		}
//
//		// double jump bookkeeping
//		if (canDoubleJump) 
//			doubleJumpTimer += Time.deltaTime;
//
//		if (canDoubleJump && doubleJumpTimer > timeToDoubleJump) {
//			canDoubleJump = false;
//			doubleJumpTimer = 0f;
//		}
//
//		// start jump or double jump
//
//		if (controller.isGrounded && Input.GetButtonDown ("Jump") && canDoubleJump) {
//			isDoubleJumping = true;
//		}
//		if (controller.isGrounded && Input.GetButtonDown ("Jump") && canJump) {
//			// starting a jump
//			isJumping = true;
//			currentJumpHeight = 0f;
//			animator.SetBool("Jump", true);
//			AudioSource.PlayClipAtPoint(jumpSound, transform.position);
//			timeSinceJumpStart = 0f;
////			currentJumpSpeed = minJumpSpeed;
////			Debug.Log ("starting jump - currentJumpSpeed: " + currentJumpSpeed);
//		} 
//
//
//		// perform a jump
//		if (isJumping) {
//
//			timeSinceJumpStart += Time.deltaTime;
//
//			if ((currentJumpHeight < minJumpHeight) || ((currentJumpHeight < normalJumpHeight) && Input.GetButton ("Jump"))) {
//				// jumping under min height or 			
//				// jumping under max height and still pressing button
////				float jumpDistance = jumpSpeed * Time.deltaTime;
////				float jumpAmount = (float)(-.05f * (timeSinceJumpStart*timeSinceJumpStart)) + (10f * timeSinceJumpStart) + (minJumpSpeed);
////				currentJumpSpeed += (jumpAcceleration * currentJumpSpeed);
////				float jumpAmount = currentJumpSpeed * Time.deltaTime;
//				float jumpAmount = Mathf.Max(minJumpSpeed, (maxJumpSpeed - (timeSinceJumpStart * jumpAcceleration)));
//				if (isDoubleJumping) {
//					jumpAmount = Mathf.Max(minJumpSpeed, (doubleJumpSpeed - (timeSinceJumpStart * jumpAcceleration)));
//				}
//				moveDirection.y += jumpAmount;
//				currentJumpHeight += jumpAmount;
//			}
//
//			if (isDoubleJumping && (currentJumpHeight >= doubleJumpHeight)) {
//				// ending a jump if we're over max jump height
//				// or the button is no longer pressed and we're above min jump height
////				animator.SetTrigger("isFalling");
//				isJumping = false;
//				isDoubleJumping = false;
//				canDoubleJump = false;
//			}
//
//
//		} 
//
//	}



}
