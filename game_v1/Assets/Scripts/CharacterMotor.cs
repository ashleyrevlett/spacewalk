using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {

	// running
	public float rotateSpeed = 1000f;
	public float rotateDegrees = 360;
	public float speed = 6.0F;
	public float runScaleFactor = 1.2f;
	private Vector3 moveDirection = Vector3.zero;

	// jumping
	public AudioClip jumpSound;
	public float jumpHeight = 100f;
	public float minJumpHeight = 20f;
	public float normalJumpHeight = 40f;
	public float highJumpHeight = 70f;
	private bool jumpInput = false;
	private bool canJump = true;

	private bool isJumping = false;
	private float currentJumpHeight = 0f;
	private float timeSinceJumpStart = 0f;

	// gravity
	public float gravity = 10.0F;
	public float gravityAcceleration = 1.2F;
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

		moveDirection = LocalMovement (playerInput.x, playerInput.y);

		// look in direction
		if (playerInput != Vector2.zero)
		{
			Vector3 lookDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
			transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.LookRotation(lookDirection)), Time.deltaTime * 3);
		}

		// add gravity
		moveDirection.y -= gravity * Time.deltaTime;

		ProcessJump();		 

		// move at speed	
		controller.Move (moveDirection * Time.deltaTime);
	

	}


	private Vector2 ProcessPlayerInput() {
		
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		jumpInput = Input.GetButton ("Jump");

		if (!jumpInput)
			animator.SetFloat ("speed", moveVertical);
		animator.SetFloat ("angularVelocity", moveHorizontal);
		
		return new Vector2(moveHorizontal, moveVertical);
		
	}


	private void ProcessJump() {	
		// IEnumerator
//		if (jumpInput && canJump) { 
//			//trigger jump
//			animator.SetBool("Jump", true);
//			// prevent more jumps
//			yield return null;
//			animator.SetBool("Jump", false);
//			canJump = false;
//		} else if (!jumpInput) {
//			yield return null;
//			canJump = true;
//		}

		// hit ground
		if ((controller.collisionFlags & CollisionFlags.Below) != 0) {
			isJumping = false;
			animator.SetTrigger("isGrounded");
			//Debug.Log("Is Grounded");
		}

		// start jump
		if (controller.isGrounded & Input.GetButton ("Jump")) {
			// starting a jump
			isJumping = true;
			currentJumpHeight = 0f;
			animator.SetTrigger("isJumping");
			AudioSource.PlayClipAtPoint(jumpSound, transform.position);
			timeSinceJumpStart = 0f;
		} 


		// perform a jump
		if (isJumping) {

			if ((currentJumpHeight < minJumpHeight) || ((currentJumpHeight < normalJumpHeight) && Input.GetButton ("Jump"))) {
				// jumping under min height or 			
				// jumping under max height and still pressing button
				//float jumpDistance = jumpSpeed * Time.deltaTime;
//				float jumpAmount = (float)(-.05f * (timeSinceJumpStart*timeSinceJumpStart)) + (10f * timeSinceJumpStart);
				float jumpAmount = jumpHeight * Time.deltaTime;
				moveDirection.y += jumpAmount;
				currentJumpHeight += jumpAmount;
			} else if ((currentJumpHeight >= normalJumpHeight) || !Input.GetButton ("Jump")) {
				// ending a jump if we're over max jump height
				// or the button is no longer pressed and we're above min jump height
				animator.SetTrigger("isFalling");
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
		moveDirection = moveDirection.normalized * speed;

		return(moveDirection);
	
	}

}
