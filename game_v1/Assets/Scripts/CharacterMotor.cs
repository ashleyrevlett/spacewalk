using UnityEngine;
using System.Collections;

public class CharacterMotor : MonoBehaviour {


	public float accel = 4f;
	public float terminalVelocity = 180f;

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

	// moving forward but colliding with obstacle
	private bool pushing = false;
	private bool sliding = false;
	private Vector3 slideDirection = Vector3.zero;
//	private Vector3 groundNormal = Vector3.zero;

	// jumping
	public AudioClip jumpSound;
	public float normalJumpHeight = 160f;
	public float minJumpSpeed = .2f;


	private bool jumpInput = false;
	private bool canJump = true;
	private bool isJumping = false;
	private float currentJumpHeight = 0f;
	private float timeSinceJumpStart = 0f;
	private bool isFalling = false;

	// double jumping
//	public float doubleJumpHeight = 200f;
//	public float doubleJumpSpeed = 24f;
//	public float timeToDoubleJump = 0.1f;
//	public float doubleJumpAcceleration = 1.2f;
//	private bool canDoubleJump = false;
//	private float doubleJumpTimer = 0f;
//	private bool isDoubleJumping = false;
	

	// gravity
	public float gravity = 10.0f;
//	public float gravityAcceleration = 1.2f;
	private float timeSinceFallStart = 0f;
	private float timeSinceSlideStart = 0f;
	private float groundToControllerDist = 0f;

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

		ProcessJump ();

		//ProcessCollisions ();  // check for slopes and jump landings
		ProcessSlide ();

		// add gravity
		if (!isJumping && !controller.isGrounded) {
			timeSinceFallStart += Time.deltaTime;
		} else {
			timeSinceFallStart = 0f;
		}
		//Debug.Log("timeSinceFallStart: " + timeSinceFallStart.ToString("F1"));
		if (!isJumping) {
			float moveDelta = Mathf.Min(terminalVelocity, (2 * Mathf.Sqrt(accel) * Mathf.Sqrt(timeSinceFallStart)));
			//moveDirection.y += Time.deltaTime * (-1 * (terminalVelocity - (-4 * accel - Mathf.Pow (timeSinceFallStart, 2))));
			moveDirection.y -= moveDelta;
		}

		// move at speed	
		controller.Move (moveDirection * Time.deltaTime);

		//		if (pushing) {
//			// can jump but not walk if pushing
//			controller.Move (new Vector3(0, moveDirection.y * Time.deltaTime, 0));
//		} else {
//			controller.Move (moveDirection * Time.deltaTime);
//		};
			
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
//
//		if (pushing) {
//				animator.SetBool ("Push", true);
//		} else {
//			animator.SetBool("Push", false);
//		}
//
//		if (sliding) {
//			animator.SetBool("Fall", true);
//		} 

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

		Debug.Log ("shortestDist: " + shortestDist);

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
			if (!sliding) {
				timeSinceSlideStart = 0f;
				sliding = true;
			}

			//Debug.DrawRay(transform.position, newDir, Color.yellow, Time.deltaTime);
//			moveDirection += newDir * runSpeed;

		}
		Debug.Log ("Dist to ctr: " + groundToControllerDist.ToString("F1"));
		//Debug.Log ("Dist to ctr: " + distCtr.ToString("F1") + ", Angle A-C: " + angleAC.ToString("F1") + ", Angle B-D: " + angleBD.ToString("F1"));
		//		Debug.Log ("Height diff between A-C: " + Mathf.Abs((distA - distC)).ToString("F1") + ", B-D: " + Mathf.Abs((distB - distD)).ToString("F1"));

		
		if (sliding) {
			timeSinceSlideStart += Time.deltaTime;
			moveDirection += slideDirection * Mathf.Min(terminalVelocity, (2 * Mathf.Sqrt(accel) * Mathf.Sqrt(timeSinceSlideStart)));
		}

		// check for end of slide
		if (sliding && groundToControllerDist <= 1.4f) {
			sliding = false;
			timeSinceSlideStart = 0f;
		}


	}

	private void ProcessCollisions() {


		// set state to pushing if moving against wall
		RaycastHit hit;
//		Vector3 position = new Vector3 (transform.position.x, transform.position.y + controller.height / 2, transform.position.z);
//		if (Physics.Raycast(position, transform.forward, out hit)) {
//			//Debug.Log ("forward hit: " + hit.distance.ToString("F1") + ", gameobject: " + hit.collider.gameObject.tag);
//			Debug.DrawRay(position, transform.forward * 2, Color.green, Time.deltaTime, false);				
//			if (hit.distance <= 2f && currentSpeed > 0f && hit.collider.gameObject.tag == "Terrain") {
//				pushing = true;
//			} else {
//				pushing = false;
//			}
//		}	

		// standing && slope > slopelimit -> slide downward
		Vector3 p1 = transform.position + controller.center + Vector3.up * -controller.height * 0.5F; // bottom of controller
		Vector3 p2 = p1 + Vector3.up * controller.height; // top of controller

		RaycastHit[] hits;
		hits = Physics.CapsuleCastAll(p1, p2, controller.radius, -transform.up, 10);
		int i = 0;
		while (i < hits.Length) {
			RaycastHit hitb = hits[i];
			Vector3 dir = hitb.point - p1;
			float angle = Vector3.Angle(transform.up, hitb.normal);
			Debug.DrawRay(p1, dir.normalized * hitb.distance, Color.yellow, Time.deltaTime, false);	
			Debug.Log ("downward hit: " + hitb.distance.ToString("F1") + ", gameobject: " + hitb.collider.gameObject.tag + ", angle: " + angle);
			i++;
		}
		if (hits.Length == 0) {
			Debug.DrawRay(p1, -transform.up, Color.cyan, Time.deltaTime);		
		}


	}

		
	private Vector3 LocalMovement( float moveHorizontal, float moveVertical, float speed) {
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


	public void StartJump() {
		isJumping = true;
		isFalling = false;
		timeSinceJumpStart = 0f;
		currentJumpHeight = 0f;
		animator.SetBool("Jump", true);
		AudioSource.PlayClipAtPoint(jumpSound, transform.position);
	}

	public void StartFall() {
		isJumping = false;
		isFalling = true;
		animator.SetBool("Jump", false);
		timeSinceFallStart = 0f;
	}
	


	private void ProcessJump() {	
		
		// check for release of jump button before jumping again
//		if (Input.GetButtonUp ("Jump"))
//			canJump = true;

		// start jump or double jump		
		if (controller.isGrounded && Input.GetButtonDown ("Jump") ) {		// && canJump
			// starting a jump
			StartJump();
		} 
				
		// continue jump
		if (isJumping) {			

			timeSinceJumpStart += Time.deltaTime; // speed is based on timer

			// calculate jump height (double, single, button pressed or not)
			float jumpAmount = 0f;
			if (currentJumpHeight < normalJumpHeight && ((controller.collisionFlags & CollisionFlags.Above) == 0)) {
				// not at top of jump yet and can keep moving up
				jumpAmount = Mathf.Max (minJumpSpeed, (terminalVelocity - Mathf.Min(terminalVelocity, (2 * Mathf.Sqrt(accel) * Mathf.Sqrt(timeSinceJumpStart)))));
				jumpAmount *=  Time.deltaTime;
				moveDirection.y += jumpAmount;
				currentJumpHeight += jumpAmount;
			} else {
				// jump at apex, start falling
				StartFall();
			}

		} 

		// test if we've hit the ground or if we're falling but didn't jump up first
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit)) {
			//Debug.Log("Stop fall, distance to ground: " + hit.distance.ToString("F1"));	
			if (hit.distance <= .1f) {
				// hitting ground
				animator.SetBool("Fall", false);
				isFalling = false;
			}
//			else {
//				// falling down
//				if (!isJumping && !isFalling) 
//					StartFall();
//			}
		}

		
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {

		// capsule collider can hit on the front angle, not on the bottom, so check for ground beneath
		// with a raycast


//		Debug.Log ("OnControllerColliderHit");
//
//		// on collision w/ terrain, check for slope and slide
//		if (hit.gameObject.tag != "Terrain")
//			return;
//
//		Vector3 dir = hit.point - controller.transform.position;
//		float angle = Vector3.Angle(transform.up, hit.normal);
//		Debug.DrawRay(controller.transform.position, dir, Color.magenta, Time.deltaTime, false);	


//		RaycastHit hit;
//		if (Physics.Raycast(transform.position, -Vector3.up, out hit)) {
//			if (hit.distance >= 1f) {
//
//
//			}
//		}



	}

}
