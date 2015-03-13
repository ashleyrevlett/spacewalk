using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {
	
	public float gravity = 10.0f;
	public float acceleration = 4.5f;
	public float deceleration = 25f;
	public float terminalVelocity = 12f;

	// locomotion speeds
	public float rotateSpeed = 2.6f; // how fast to rotate the player
	public float walkSpeed = 6.0f; // max speed while walking
	public float runSpeed = 10.0f; // max speed while running
	public float accelStart = 4.0f; // provide better responsiveness by giving accel a headstart
	private float currentSpeed = 0f; // remember speed in previous frame for calculating new speed
	private Vector3 moveDirection = Vector3.zero; // global var used to track intended movement dir
	
	// jumping
	public AudioClip jumpSound;
	public float normalJumpHeight = 80f;
	public float minJumpSpeed = .2f; // decreasing this increases the "hang time" of a jump	
	private bool isJumping = false;
	private float currentJumpHeight = 0f; // track amount we've jumped so far for accel
	private float timeSinceJumpStart = 0f;
	private bool isFalling = false; // jump is over and we're now falling state	
	private float timeSinceFallStart = 0f; // for acceleration tracking

	// slide on slopes
	private bool isSliding = false; // whether we're in the sliding state
	private Vector3 slideDirection = Vector3.zero; // direction to slide in
	private float timeSinceSlideStart = 0f; // for acceleration tracking
	private float groundToControllerDist = 0f; // how far the center of the controller bottom is from ground
	private float shortestDistToController = 0f;

	// various gameobject references	
	public Transform cam;
	private CharacterController controller; // used to move the player
	private Animator animator;
	private GameController gameController; // used for general game info (paused, dead, etc)


	void Start () {		
		// cache references to everything 
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		cam = Camera.main.transform;
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();
	}


	void Update () {	

		// do nothing if paused or on gameover screen
		if (gameController.isPaused || gameController.isGameOver )
			return;

		// get input from player
		Vector2 playerInput = ProcessPlayerInput ();

		// figure current speed based on accel and past speed and player input
		currentSpeed = CalculatePlayerSpeed (playerInput, currentSpeed);

		// convert player input to worldspace direction, rotate player 
		moveDirection = LocalMovement (playerInput.x, playerInput.y, currentSpeed);

		ProcessSlide ();

		// handle jump start, continue, and end
		ProcessJump ();

		// apply gravity and accel if nec. / ie, fall
		if (isFalling) {
			timeSinceFallStart += Time.deltaTime;
			// fall starts slow (small) and gets faster (larger)
			float moveDelta = Mathf.Max (minJumpSpeed, Mathf.Min(terminalVelocity, Mathf.Pow((acceleration * timeSinceFallStart + 1f * gravity), 2)));		
			Vector3 newPos = new Vector3(moveDirection.x, moveDirection.y - moveDelta, moveDirection.z);
			moveDirection = Vector3.Lerp(moveDirection, newPos, moveDelta / terminalVelocity);
		}

		// finally actually move	
		controller.Move (moveDirection * Time.deltaTime);
					
		// update the animator's values
		UpdateAnimations (playerInput.x, playerInput.y, currentSpeed);


	}



	private Vector2 ProcessPlayerInput() {		
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		// Debug.Log ("moveInput: " + moveHorizontal + ", " + moveVertical);
		return new Vector2(moveHorizontal, moveVertical);	
	}


	
	private float CalculatePlayerSpeed(Vector2 playerInput, float prevSpeed) {
		
		// determine current speed based on joystick angle + previous speed + accel
		float speed = 0f;
		if (playerInput.y != 0) {	
			if (prevSpeed == 0) {
				// start moving from standing still
				speed = (playerInput.y * acceleration * Time.deltaTime) + accelStart; // accelStart is like a head start for acceleration
			} else {
				// player is already moving, and joystick is fully depressed 
				// so continue to accelerate
				speed = prevSpeed + (playerInput.y * acceleration * Time.deltaTime);
			}
		} else {
			// not moving joystick forward, so slow to a stop
			speed = prevSpeed - (deceleration * Time.deltaTime);
		}
		speed = Mathf.Clamp (speed, 0f, runSpeed); // don't exceed running speed
		
		return (speed);
		
	}


	private void UpdateAnimations(float moveHorizontal, float moveVertical, float speed) {

		animator.SetFloat ("speed", speed/runSpeed); // normalize to 0-1
		animator.SetFloat ("angularVelocity", moveHorizontal);

		if (isSliding) {
			animator.SetBool("Fall", true);
		} 

	}


		
	private Vector3 LocalMovement( float moveHorizontal, float moveVertical, float speed) {
		Vector3 forward = transform.forward;
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		Vector3 targetDirection = moveHorizontal * right + moveVertical * forward;
		moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, 360 * Mathf.Deg2Rad * Time.deltaTime, rotateSpeed);
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
		}

		return(moveDirection);
		
	}


	private void ProcessJump() {	

		// start jump or double jump		
		if (controller.isGrounded && Input.GetButtonDown ("Jump") ) {	
			StartJump();
		} 
				
		// continue jump
		if (isJumping) {			

			timeSinceJumpStart += Time.deltaTime; // speed is based on timer
		
			// calculate jump height, must not be at top of jump yet and can keep moving up
			if (currentJumpHeight < normalJumpHeight && ((controller.collisionFlags & CollisionFlags.Above) == 0)) {
				// jumpamount is large (fast) at start and small (slow) toward apex
				float jumpAmount = terminalVelocity - Mathf.Min(terminalVelocity, Mathf.Pow(acceleration * timeSinceJumpStart + 1f * gravity, 2f)); // multiple gravity by 2 to overcome it
				jumpAmount = Mathf.Max (minJumpSpeed, jumpAmount);
				Vector3 newPos = new Vector3(moveDirection.x, moveDirection.y + jumpAmount, moveDirection.z);
				moveDirection = Vector3.Lerp(moveDirection, newPos, currentJumpHeight / normalJumpHeight);
				currentJumpHeight += jumpAmount;
			} else {
				// jump at apex, start falling
				StartFall();
			}

		} 

		// check for falling too 		
		// test if we've hit the ground or if we're falling but didn't jump up first
		Debug.Log ("controller.isGrounded" + controller.isGrounded);
		Debug.Log("Stop fall, distance to ground: " + shortestDistToController.ToString("F1"));	
		if (shortestDistToController <= .95f && isFalling) {
			Debug.Log("Stop fall, distance to ground: " + shortestDistToController.ToString("F1"));	
			// hitting ground
			EndFall();
		} else if (shortestDistToController > .95f && !isJumping && !isFalling)  {
			// ground's not underneath us and we're not known to be jumping,
			Debug.Log("Starting fall from edge");
			// so we must have fallen off a ledge
			StartFall();
		}

				
	}
	
	public void StartJump() {
		isJumping = true;
		isFalling = false;
		isSliding = false;
		timeSinceJumpStart = 0f;
		timeSinceFallStart = 0f;
		currentJumpHeight = 0f;
		animator.SetBool("Jump", true);
		AudioSource.PlayClipAtPoint(jumpSound, transform.position);
	}
	
	public void StartFall() {
		Debug.Log ("Starting fall");
		isJumping = false;
		isFalling = true;
		isSliding = false;
		animator.SetBool("Jump", false);
		animator.SetBool("Fall", true);	
		timeSinceFallStart = 0f;
	}

	public void EndFall() {
		isJumping = false;
		isSliding = false;
		isFalling = false;
		animator.SetBool("Jump", false);
		animator.SetBool("Fall", false);	
		timeSinceFallStart = 0f;
	}
		
		
	private void ProcessSlide() {
		// 4 rays, one for each edge of the controller capsule's bounding box
		// need to offset from center then align to beginning of bottom sphere in capsule
		Vector3 pA = transform.position + controller.center + transform.forward * controller.radius + transform.up * -controller.radius * .5f; 
		Vector3 pB = transform.position + controller.center + transform.right * controller.radius + transform.up * -controller.radius * .5f;
		Vector3 pC = transform.position + controller.center + transform.forward * -controller.radius + transform.up * -controller.radius * .5f;
		Vector3 pD = transform.position + controller.center + transform.right * -controller.radius + transform.up * -controller.radius * .5f;
		Vector3 pCtr = transform.position + controller.center; 
		
		float shortestDist = 100f;
		Vector3 pAHitPoint = Vector3.zero;
		Vector3 pBHitPoint = Vector3.zero;
		Vector3 pCHitPoint = Vector3.zero;
		Vector3 pDHitPoint = Vector3.zero;
		Vector3 lowestPoint = Vector3.zero;
		RaycastHit hit;
		
		if (Physics.Raycast(pA, -transform.up, out hit)) {		
			Debug.DrawRay(pA, -transform.up * hit.distance, Color.green, Time.deltaTime, false);				
			if (hit.distance < shortestDist) shortestDist = hit.distance;
			pAHitPoint = hit.point;
			if (lowestPoint == Vector3.zero || pAHitPoint.y < lowestPoint.y)
				lowestPoint = pAHitPoint;
		}	
		if (Physics.Raycast(pB, -transform.up, out hit)) {		
			Debug.DrawRay(pB, -transform.up * hit.distance, Color.blue, Time.deltaTime, false);				
			pBHitPoint = hit.point;
			if (hit.distance < shortestDist) shortestDist = hit.distance;
			if (lowestPoint == Vector3.zero || pBHitPoint.y < lowestPoint.y)
				lowestPoint = pBHitPoint;
		}	
		if (Physics.Raycast(pC, -transform.up, out hit)) {		
			Debug.DrawRay(pC, -transform.up * hit.distance, Color.green, Time.deltaTime, false);						
			pCHitPoint = hit.point;
			if (hit.distance < shortestDist) shortestDist = hit.distance;
			if (lowestPoint == Vector3.zero || pCHitPoint.y < lowestPoint.y)
				lowestPoint = pCHitPoint;
		}	
		if (Physics.Raycast(pD, -transform.up, out hit)) {		
			Debug.DrawRay(pD, -transform.up * hit.distance, Color.blue, Time.deltaTime, false);				
			pDHitPoint = hit.point;
			if (hit.distance < shortestDist) shortestDist = hit.distance;
			if (lowestPoint == Vector3.zero || pDHitPoint.y < lowestPoint.y)
				lowestPoint = pDHitPoint;
		}
		if (Physics.Raycast(pCtr, -transform.up, out hit)) {		
			Debug.DrawRay(pCtr, -transform.up * hit.distance, Color.magenta, Time.deltaTime, false);				
			groundToControllerDist = hit.distance;
		}
		
		//Debug.Log ("shortestDist: " + shortestDist);
		shortestDistToController = shortestDist;

		// not on ground
		if (shortestDist >= .9f)
			return;
		
		Vector3 AC = pAHitPoint - pCHitPoint;
		Vector3 BD = pBHitPoint - pDHitPoint;
		float angleAC = Vector3.Angle (AC, transform.forward);
		float angleBD = Vector3.Angle (BD, transform.right);
		if (angleAC > controller.slopeLimit || angleBD > controller.slopeLimit ) {
			Debug.Log("Slide!");
			Debug.Log("Lowest Point: " + lowestPoint);
			
			Vector3 newDir = lowestPoint - transform.position;
			slideDirection = newDir;
			
			// set sliding bool here and reset timer first time a steep slope is stepped on
			// process accel and timer in main update loop
			if (!isSliding) {
				timeSinceSlideStart = 0f;
				isSliding = true;
			}		
		}
		
		if (isSliding) {
			timeSinceSlideStart += Time.deltaTime;
			moveDirection += slideDirection * Mathf.Min(terminalVelocity, (acceleration * timeSinceSlideStart * runSpeed));
		}
		
		// check for end of slide
		if (isSliding && groundToControllerDist <= 1.4f) { // 1.4 is based on astroman model
			isSliding = false;
			timeSinceSlideStart = 0f;
		}

		
	}

	
}
