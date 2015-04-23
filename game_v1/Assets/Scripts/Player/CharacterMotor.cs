/* NOTES:
 * 
 * Terrain must be tagged "Terrain" in order to slide down it
 * Falling Platforms must be tagged "FallingPlatform" to be triggered by the player
 * Moving Platforms must be tagged "Platform" to affect the player's position
 * Enemies must be tagged "Enemy" to hurt player and be killed
 * Natural hazards (water, spikes) must be tagged "Hazard"
 * 
 * */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CharacterMotor : MonoBehaviour {
	
	#region Parameters

	// basic physics controls
	public float gravity = 10.0f; // makes character fall
	public float terminalVelocity = 12f; // fastest possible fall/slide speed
	public float groundTolerance = .1f; // distance controller can be from ground and still be considered "grounded"

	// locomotion speeds
	public float maxRotSpeed = 300f;
	public float minRotSpeed = 2f;
	public float rotAccel = 2f;
	private float curRotSpeed = 0f;
	

	//public float rotateSpeed = 2.6f; // how fast to rotate the player when turning
	public float runSpeed = 10.0f; // max speed while running
	private float currentSpeed = 0f;
	private float previousSpeed = 0f; // we base our new speed off the previous speed
	public Vector3 moveDirection = Vector3.zero; // global var used to track intended movement dir
	private Vector2 playerInput = Vector2.zero; // horiz and vert joystick input
	private float distToForwardObstacle = 100f;
	public float obstacleDistanceTolerance = .2f;

	// accel
	public bool useAcceleration = true;
	public float acceleration = 4.5f; // used for walk/run speed change
	public float deceleration = 25f; // used to slow walk/run when no user input
	public float accelStart = 4.0f; // provide better responsiveness by giving accel a headstart

	// jumping
	public bool useJump = true;
	public float jumpVelocity = 10f; // strength of upward jump force
	public bool useVariableHeightJump = true;
	public float cutJumpSpeed = 4f; // jump velocity to use when jump is cut short
	public float endFallAnimationTime = 1.0f; // length in sec of landing animation
	public float inAirControl = .5f; // reduces amount of horizontal control in air
	public float verticalVelocity { get; private set; } // jump/fall velocity	
	private float distToGround = 0f; // used instead of isGrounded
	private float distToCeiling = 1000f; // dist to obstacle above; set to 1000 if no obstacle 
	private bool triggerJump = false;

	// double jumping
	public bool useDoubleJump = true;
	public float doubleJumpVelocity = 16f;
	public float timeDoubleJumpAvailable = .5f; // seconds after landing 1st jump in which we can double-jump
	private float timeDoubleJumpRemaining = 0f;
	private bool isDoubleJumping = false;

	// slide control
	public bool useSlide = true;
	private float slideSpeed = 0f; // slide velocity
	public float slideFriction = 2f; // slow the slide down by friction amount
	private bool isSliding = false;
	private Vector3 slideDirection = Vector3.zero; // direction to slide in
		
	// particles and sounds fx
	public GameObject dustParticlePrefab; // dust particle prefab for sudden start/stop
	public AudioClip footstepSound; 
	public AudioClip jumpSound;
	public AudioClip slideSound;
	public AudioClip landingSound;
	private AudioSource soundEffectsSource;

	// various gameobject references	
	private CharacterController controller; // used to move the player
	private Animator animator;
	private GameObject levelRoot;
	private GameController gameController;
	private HealthController healthController;

	// enable powerups!
	public bool canUseJetpack = true;

	// allow force to be applied from outside, like enemy hit
	private float forceAmount = 0f;
	private Vector3 forceDirection = Vector3.zero;
	private bool forceApplied = false;

	// detect when we're on top of moving platforms and offset our motion by theirs
	private GameObject objectBelowPlayer;
	private bool isMovingWithPlatform;
	private Vector3 platformMoveDirection = Vector3.zero;
	private PlatformMove platformMove;

	// pole grabbing
	private bool isOnPole = false;
	private GameObject activePole = null;

	// dieing while falling out of screen
	public bool fallingDeath;

	#endregion


	void Start () {		

		// cache object references
		controller = GetComponent<CharacterController> ();
		animator = GetComponent<Animator> ();
		levelRoot = GameObject.FindGameObjectWithTag ("Level");
		GameObject gameControllerObject = GameObject.FindGameObjectWithTag ("GameController");
		gameController = gameControllerObject.GetComponent<GameController> ();

		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		healthController = player.GetComponent<HealthController> ();

		// create new sound source for sfx
		soundEffectsSource = gameObject.AddComponent<AudioSource>();
		soundEffectsSource.loop = false;
		soundEffectsSource.Stop ();

		fallingDeath = false;
		isMovingWithPlatform = false;

	}
//
//	void Reset() {
//		healthController = player.GetComponent<HealthController> ();
//	}
//	
	void Update () {	

		if (!gameController.isPlaying) {
			// we are paused or a dialog is displaying
			animator.speed = 0f;
			StopAllCoroutines();
			return;
		} else if (gameController.isLevelEnd || fallingDeath) {
			return;
		} else {
			animator.speed = 1f;
		}

		
		// get input from player if not being moved elsewhere
		playerInput = ProcessPlayerInput ();

		//		if (forceAmount > 0f) {
//			forceAmount -= gravity * Time.deltaTime;
//			forceAmount = Mathf.Clamp(forceAmount, 0f, terminalVelocity);
//		}




		// if damage is being taken, don't let player control movement
		if (healthController.takingDamage) {
			moveDirection = forceDirection * forceAmount;
		} else if (isOnPole && activePole != null) {

			//float yVal = activePole.GetComponent<BoxCollider>().bounds.size.y / 2f;
			//transform.position = new Vector3(activePole.transform.position.x, yVal, activePole.transform.position.z);
//			Vector3 relPos = activePole.transform - transform.position;
//			Vector3 newPos = activePole.transform.position + activePole.GetComponent<BoxCollider>().center + controller.bounds.size;
//			transform.position = activePole.transform.position + activePole.GetComponent<BoxCollider>().center + controller.bounds.size;

			//float moveHorizontal = Input.GetAxisRaw ("Horizontal");


			// have to center our position around it in case we hit it from an angle or edge
			// first rotate to look in direction of pole

			// if we're on a pole, we have to look toward the pole's center
			// and be correct distance from it
	


			CapsuleCollider poleCollider = activePole.GetComponent<CapsuleCollider>();
			Vector3 poleCenter = activePole.transform.position + poleCollider.center;		
			// don't allow to move into flag; subtract flag height
			Vector3 poleTopPosition = poleCenter + (Vector3.up * poleCollider.height/2f) + (-Vector3.up * 1f);


			Vector3 playerHeadPosition = transform.position + controller.center + (Vector3.up * (controller.height/2f));
			playerHeadPosition += -(Vector3.up * (controller.height/2f));
			Vector3 playerFeetPosition = playerHeadPosition + (-Vector3.up * (controller.height));

			Vector3 playerCenter = transform.position + controller.center;
			Vector3 poleCenterHrz = new Vector3(poleCenter.x, playerCenter.y, poleCenter.z);
			float dist = Vector3.Distance(poleCenterHrz, playerCenter);
			float correctDist = controller.radius + poleCollider.radius;
			float diff = correctDist - dist;
			float diffAllowed = 0.01f;
			if (diff > diffAllowed) {
				Vector3 relDir = playerCenter - poleCenterHrz; 
				transform.position = Vector3.Lerp(transform.position, transform.position + relDir.normalized * diff, Time.deltaTime);
			}

			moveDirection = Vector3.zero;
			float moveHorizontal = Input.GetAxisRaw ("Horizontal");
			float moveVertical = Input.GetAxisRaw ("Vertical");
			
			// left/right = rotation around pole
			if (moveHorizontal != 0f) {
				
				float joystickRotationForce = 120f;
				float yRot = Mathf.Clamp(moveHorizontal * joystickRotationForce, -179f, 179f);
				Vector3 newRot = transform.rotation * new Vector3(0f, yRot, 0f);
				Vector3 nearPoleCenter = new Vector3(poleCenter.x, transform.position.y, poleCenter.z);
				Vector3 newPos = RotatePointAroundPivot(transform.position, nearPoleCenter, newRot);
				Debug.Log("newPos: " + newPos);
				//				Vector3 newdir = transform.rotation * new Vector3(0f, yRotationOffset, 0f);
				//				Vector3 pos = RotatePointAroundPivot(transform.position, poleCenter, newdir);
				Vector3 relPosition = newPos - transform.position;
				moveDirection = relPosition;
				
			}
			
			if (moveVertical != 0f) {
				if (playerHeadPosition.y >= poleTopPosition.y) {
					moveVertical = Mathf.Min (0f, moveVertical);
				} else if (playerFeetPosition.y <= activePole.transform.position.y) {
					if (moveVertical < 0) {
						StartJump();
						isOnPole = false;
						activePole = null;
					}
					moveVertical = Mathf.Max (0f, moveVertical);
					
				}
				
				moveDirection = moveDirection + new Vector3(0f, moveVertical * runSpeed / 4f, 0f);
				
				
			}
			
			if (Input.GetButtonDown ("Jump")) {
				StartJump();
				isOnPole = false;
				activePole = null;
			}

		
//			// first we have to push the player back the distance to exit collisions
//			Vector3 closestPointOnPole = activePole.GetComponent<CapsuleCollider>().ClosestPointOnBounds(transform.position + controller.center);
//			Vector3 relDirFromCenter = closestPointOnPole - transform.position + controller.center;
//			Vector3 closestPointOnPlayer = transform.position + controller.center + (relDirFromCenter.normalized * controller.radius);
//			float distToPole = Vector3.Distance(closestPointOnPlayer, closestPointOnPole);
//			//Debug.Log ("distToPole: " + distToPole);
//
//			Debug.DrawRay (closestPointOnPlayer, relDirFromCenter.normalized * distToPole, Color.yellow, Time.deltaTime);
//			Debug.DrawLine(closestPointOnPlayer, closestPointOnPole, Color.blue);


			// then we rotate toward pole
//			Vector3 newDir = Vector3.RotateTowards(transform.forward, poleCenter, 5f * Time.deltaTime, 0.0F);
//			transform.rotation = Quaternion.LookRotation(newDir);

		} else {
			
			// figure current speed based on accel, past speed and player input
			currentSpeed = CalculatePlayerSpeed (playerInput, currentSpeed);
			
			// convert player input to worldspace direction, rotate player 
			moveDirection = LocalMovement (playerInput.x, playerInput.y, currentSpeed);
			
			// listen for jump input and handle jump animations, sfx
			if (useJump)
				ProcessJump ();
			
			// dust clouds during sudden accel
			AddDustClouds ();
			
			// slide on slopes
			if (useSlide)
				ProcessSlide ();
			
			// add gravity if not on ground
			if (!isNearlyGrounded()) {
				verticalVelocity -= gravity * Time.deltaTime;
				verticalVelocity = Mathf.Max(-terminalVelocity, verticalVelocity);
			} 
			
			// apply gravity & jump force if not sliding
			// if we slide then fall off the surface
			if (!isSliding || !isNearlyGrounded() ) {
				moveDirection.y = verticalVelocity;
			}

		} // end taking damage

		// in all cases position may be affected by moving platform
		//Debug.Log ("distToGround: " + distToGround.ToString("F1"));
		if (objectBelowPlayer != null && !isMovingWithPlatform) {
			StartCoroutine(MoveWithPlatform());
		}

		// move
		//Debug.Log ("moveDirection: " + moveDirection);
		if (moveDirection != Vector3.zero)
			controller.Move(moveDirection * Time.deltaTime);

		// if we're on a pole, we need to look toward its center
		if (isOnPole) {
			Vector3 lookDir = new Vector3(activePole.transform.position.x, transform.position.y, activePole.transform.position.z);
			transform.LookAt(lookDir);
		}
		// update the animator's values
		UpdateAnimations (playerInput.x, playerInput.y, currentSpeed);

	}

	IEnumerator MoveWithPlatform() {

		isMovingWithPlatform = true;

		platformMove = objectBelowPlayer.GetComponent<PlatformMove>();

		while (objectBelowPlayer != null) {

			float moveDistance = .1f;
			float distanceMoved = 0f;
			while (distanceMoved < moveDistance) {
				distanceMoved += Time.deltaTime * platformMove.moveSpeed;
				Vector3 newPosition = transform.position + (platformMove.currentDirection * Time.deltaTime * platformMove.moveSpeed);
				transform.position = newPosition;
				yield return null;
			}
		}

		isMovingWithPlatform = false;

		yield return null;

	}
	
	void FixedUpdate() {

		// update distance to ground
		distToGround = ProbeDirection (Vector3.down);

		// update distance to obstacle above
		distToCeiling = ProbeDirection (Vector3.up);

		distToForwardObstacle = ProbeDirection (transform.forward);
		
	}


	#region Movement
	
	/* 
	 * MOVEMENT & SPEED
	 */ 

	private Vector2 ProcessPlayerInput() {		
		float moveHorizontal = Input.GetAxisRaw ("Horizontal");
		float moveVertical = Input.GetAxisRaw ("Vertical");
		return new Vector2(moveHorizontal, moveVertical);	
	}

	private float CalculatePlayerSpeed(Vector2 playerInput, float prevSpeed) {		

		// with accel: current speed based on joystick angle + previous speed + accel
		previousSpeed = prevSpeed; // store as member var
		float speed = 0f;
		if (useAcceleration) {
			if (playerInput.y != 0) { // player is pressing joystick up/down	
				if (prevSpeed == 0) { // currently standing still
					speed = (playerInput.y * acceleration * Time.deltaTime) + accelStart; // accelStart is a head start for accel; more responsive feel
				} else {
					// player is already moving, and joystick is fully depressed, so continue to accelerate
					speed = prevSpeed + (playerInput.y * acceleration * Time.deltaTime);
				}
			} else {
				// not moving joystick forward, so slow to a stop
				speed = prevSpeed - (deceleration * Time.deltaTime);
			}
		} else {			
			// no accel: 
			speed = (Mathf.Abs(playerInput.y) + Mathf.Abs(playerInput.x)) * runSpeed;
		}

		speed = Mathf.Clamp (speed, 0f, runSpeed); // don't exceed running speed
		
		return (speed);
		
	}


	private Vector3 LocalMovement( float moveHorizontal, float moveVertical, float speed) {

		// TODO if there is something in front of us, we are jumping up, and we are moving forward,
		// move the player up only 

		// reduce control level if in air
		if (!isNearlyGrounded()) { // falling or jumping
			moveHorizontal *= inAirControl;
		}

		// TODO: if there's something in front of us, don't try to move forward into it
//		if (distToForwardObstacle <= obstacleDistanceTolerance) {
//			// but do allow moving backwards away from it
//			moveVertical = Mathf.Min(0, moveVertical);
//		}

		// movement direction is determined by the direction we're facing
//		Vector3 forward = transform.forward;
//		Vector3 right = new Vector3 (forward.z, 0, -forward.x);
//		Vector3 targetDirection = moveHorizontal * right + moveVertical * forward;

//		// TODO: update rotate speed w/ accel if joystick pressed to turn
//		if (Mathf.Abs (moveHorizontal) <= .01f)
//			curRotSpeed = minRotSpeed;
//		else 
//			curRotSpeed += rotAccel * Time.deltaTime;
//
//		curRotSpeed = Mathf.Clamp(curRotSpeed, minRotSpeed, maxRotSpeed);
//		curRotSpeed = minRotSpeed; 
//
//		float step = curRotSpeed * Time.deltaTime;
//		moveDirection = Vector3.RotateTowards (moveDirection, targetDirection, step, 0.0F);
//		moveDirection = moveDirection.normalized * speed;
		
		if (moveVertical == 0f && moveHorizontal == 0f) {
		
			// don't move
			moveDirection = Vector3.zero; 
		
		} else if (moveHorizontal != 0 || moveVertical != 0) {

			// look in direction of movement if player is pressing joystick
		
			// Create a new vector of the horizontal and vertical inputs.
			Vector3 targetDirection = new Vector3(moveHorizontal, 0f, moveVertical);

			// if we're trying to only rotate, don't base it off camera, which may not be behind us
			if  (moveVertical == 0f) {
				targetDirection = gameObject.transform.TransformDirection(targetDirection);
			} else {
				targetDirection = Camera.main.transform.TransformDirection(targetDirection);
			}

			// Create a rotation based on this new vector assuming that up is the global y axis.
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

			// do not rotate around x or z axis
			targetRotation = Quaternion.Euler( new Vector3(0f, targetRotation.eulerAngles.y, 0f) );

			// Create a rotation that is an increment closer to the target rotation from the player's rotation.
			curRotSpeed = (Mathf.Abs (moveHorizontal) * minRotSpeed + Mathf.Abs(moveVertical) * minRotSpeed);
			curRotSpeed = Mathf.Clamp(curRotSpeed, minRotSpeed, maxRotSpeed);
			Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, curRotSpeed * Time.deltaTime);
			
			// Change the players rotation to this new rotation.
			transform.rotation = newRotation;

			// don't want to change animations, so don't set speed if only turning
			if (moveVertical != 0f) {
				moveDirection = targetDirection.normalized * speed;
			}

		}



		return(moveDirection);
		
	}

	
	private void UpdateAnimations(float moveHorizontal, float moveVertical, float speed) {
		// update animation FSM with speed and turn info
		if (verticalVelocity <= 0f && isNearlyGrounded() && moveVertical != 0f ) { // not jumping or falling
			animator.SetFloat ("speed", speed / runSpeed); // normalize to 0-1
			animator.SetBool("JumpLoop", false); // in case we're stuck in animator loop
			animator.SetBool("JumpStart", false);
		}
		if (moveVertical == 0f) 
			animator.SetFloat ("speed", 0f); // normalize to 0-1

		animator.SetFloat ("angularVelocity", moveHorizontal);

		// pole positions
		if (isOnPole && (moveHorizontal == 0f && moveVertical == 0f)) {
			animator.SetBool("Hanging", true);
			animator.SetBool("Climbing", false);
		} else if (isOnPole && (moveHorizontal != 0f || moveVertical != 0f)) {
			animator.SetBool("Hanging", true);
			animator.SetBool("Climbing", true);
		} else {
			animator.SetBool("Hanging", false);
			animator.SetBool("Climbing", false);
		}

	}

	#endregion
	#region Jumping


	/*
	 * JUMPING
	 */ 
	
	
	private void ProcessJump() {	


//		if (distToGround > 0f && !onGround && verticalSpeed <= 0f) {
//			StartFall (); // we ran off edge, but didn't jump
//		}

		// double jump timer
		if (useDoubleJump && timeDoubleJumpRemaining > 0f) 
			timeDoubleJumpRemaining -= Time.deltaTime;

		// on ground
		if (useDoubleJump && isNearlyGrounded() && Input.GetButtonDown ("Jump") && timeDoubleJumpRemaining > 0f)
			StartDoubleJump ();
		else if ((isNearlyGrounded() && Input.GetButtonDown ("Jump")) || triggerJump )
			StartJump ();

		// made it 1/2way through jump, stopped pressing button
		// stop jumping
		if (useVariableHeightJump) {
			if (verticalVelocity > 0f && verticalVelocity <= jumpVelocity * .75 && !Input.GetButton ("Jump")) {
				Debug.Log("Cutting jump short at: " + verticalVelocity);
				verticalVelocity = cutJumpSpeed;
			}
		}

		// jumping up
		if (verticalVelocity > 0f) {		
			// stop jumping up if we hit a ceiling
			if (distToCeiling <= .01f) {
				Debug.Log("Hit ceiling");
				JumpLoop();
			}

		// falling down
		} else if (verticalVelocity < 0f) {

			// enter jump loop animation until we get close to the ground
			if( animator.GetCurrentAnimatorStateInfo(0).IsName("JumpStart") ) {
				JumpLoop();
			}

			// estimate time to reach ground based on current yVel
			float timeToReachGround = Mathf.Abs(distToGround / verticalVelocity); // meters / meters per sec
			// if we've hit the ground and are still in jump animation, end the jump animation
			if( animator.GetCurrentAnimatorStateInfo(0).IsName("JumpLoop") && 
			    (timeToReachGround <= endFallAnimationTime || isNearlyGrounded() ) )
				EndFall();
			if( animator.GetCurrentAnimatorStateInfo(0).IsName("DoubleJumpUp") && 
			    (timeToReachGround <= endFallAnimationTime || isNearlyGrounded() ) )
				EndFall();
		}

		// ending fall, reset animations
		if (isNearlyGrounded() && !animator.GetCurrentAnimatorStateInfo(0).IsName ("Locomotion")) {
			Land();
		}
	
	}

	public void StartDoubleJump() {
		timeDoubleJumpRemaining = 0f;
		// player pushed jump button; also used as message to react to jumping on npcs
		isDoubleJumping = true;
		verticalVelocity = doubleJumpVelocity;	
		animator.SetBool("Jump", false);
		animator.SetBool("Fall", false);
		animator.SetBool("BigJump", true);
		soundEffectsSource.Stop(); // no footsteps sfx in air		
		AudioSource.PlayClipAtPoint(jumpSound, transform.position);
		Debug.Log ("Starting double jump!");
	}
	
	public void StartJump() {
		// player pushed jump button; also used as message to react to jumping on npcs
		verticalVelocity = jumpVelocity;	
		animator.SetBool("JumpStart", true);
		soundEffectsSource.Stop(); // no footsteps sfx in air		
		AudioSource.PlayClipAtPoint(jumpSound, transform.position);
				
		// add platform velocity to ours
		if (isMovingWithPlatform) {
			Vector3 platformDir = platformMove.currentDirection * Time.deltaTime * platformMove.moveSpeed;
			if (platformDir.y > 0)
				verticalVelocity += platformMove.currentDirection.y * platformMove.moveSpeed;	
		}

		Debug.Log ("Starting jump!");
		triggerJump = false;
	}
	
	public void JumpLoop() {
		// player walked off ledge	
		verticalVelocity = -.1f; // reset vspeed, gravity will cause fall; don't use 0 so we won't divide by 0 later
		animator.SetBool("JumpLoop", true);
		soundEffectsSource.Stop(); // no footsteps sfx in air		
		Debug.Log ("Starting jump loop");
	}
	
	public void EndFall() {
		// end jump or fall; same animation	
		if (animator.GetBool ("JumpStart") || animator.GetBool ("JumpLoop") || animator.GetBool ("DoubleJump")) {
			Debug.Log ("Ending jump loop");
			animator.SetBool ("JumpEnd", true);
			StartCoroutine ("PlayDelayedAudio");
		}

	}

	public void Land() {

		animator.SetBool ("JumpStart", false);
		animator.SetBool ("JumpLoop", false);
		animator.SetBool ("JumpEnd", false);
		animator.SetBool ("FallStart", false);

		// reset double jump timer if this was a regular jump
		if (useDoubleJump) {
			if (isDoubleJumping) {
				isDoubleJumping = false;
				timeDoubleJumpRemaining = 0f;
			} else {
				timeDoubleJumpRemaining = timeDoubleJumpAvailable;
			}
		}

	}


	private void ProcessSlide() {	
		// slideDirection and isSliding are detected during OnControllerColliderHit
		// ProcessSlide runs every update to move player in slideDirection when sliding
		if (slideDirection != Vector3.zero && isSliding) {
			moveDirection += slideDirection * slideSpeed;
			if (soundEffectsSource.clip != slideSound || !soundEffectsSource.isPlaying) {
				soundEffectsSource.Stop ();
				soundEffectsSource.clip = slideSound;
				soundEffectsSource.loop = true;
				soundEffectsSource.Play ();	
			}
		}

		// if we're no longer on the ground, or if the surface below us is not sloped
		// we are no longer sliding
		if (!isNearlyGrounded() || !isSliding) {
			isSliding = false;
			if (soundEffectsSource.clip == slideSound && soundEffectsSource.isPlaying) {
				soundEffectsSource.Stop ();
				soundEffectsSource.loop = false;
			}
		}

		
	}


	#endregion
	#region Physics
	
	/* 
	 * Physics, pre-emptive collision detection and Collisions
	 */

	void OnTriggerEnter(Collider other) {
		Debug.Log ("Trigger entered - " + other.gameObject.tag);
		if (other.gameObject.tag == "Platform") {
			objectBelowPlayer = other.gameObject; // remember what's below us
		} else if (other.gameObject.tag == "Pole") {
			// grab the pole
			activePole = other.gameObject;
			isOnPole = true;

		}
	}

	void OnTriggerExit(Collider other) {
//		Debug.Log ("Trigger exited - " + other.gameObject.tag);
		if (other.gameObject.tag == "Platform") {
			objectBelowPlayer = null; // nothing
		} 
	}



	void OnControllerColliderHit(ControllerColliderHit hit) {

		// detect collisions with sloped terrain
		
		float slope = Mathf.Acos(hit.normal.y) * Mathf.Rad2Deg; // slope of hit surface
		
		// object must be tagged Terrain, we shouldn't slide on npcs or anything not terrain
		// also we shouldn't slide down near vertical surfaces; gravity will handle that
		
		if (slope > controller.slopeLimit && hit.gameObject.tag != "Enemy" && !(slope >= 90 && slope <= 91)) {
			
			if (!isSliding) {
				isSliding = true;
				slideSpeed = 0f;
				verticalVelocity = -.1f; // need to reset this so jump can detect we're on ground
				//StartFall(); // animations
			}
			slideSpeed += slope/gravity/slideFriction * Time.deltaTime;
			slideSpeed = Mathf.Min(terminalVelocity, slideSpeed);
			Vector3 nonUnitAnswer = Vector3.down - Vector3.Dot (Vector3.down, hit.normal) * hit.normal; // could be 0
			slideDirection = Vector3.Normalize (nonUnitAnswer);
		} else {
			// not on slope any longer
			isSliding = false;
		}

		
	}



	
	public bool isNearlyGrounded() {
		// can't trust character controller's isGrounded, so cast for nearest object below 
		// and use this distance instead to determine whether we're grounded		
		if (isMovingWithPlatform && distToGround * 3f <= groundTolerance) // need to be more forgiving when on platforms
			return true;		
		else if (distToGround <= groundTolerance)
			return true;		
		return false;
	}


	private float ProbeDirection( Vector3 direction) {

		Vector3 p1 = transform.position + controller.center + Vector3.up * -controller.height * 0.2F;
		Vector3 p2 = p1 + Vector3.up * controller.height * 0.2f;
		RaycastHit hit;
		float dist = 100f;
		if (Physics.CapsuleCast(p1, p2, controller.radius, direction, out hit, 10)) {
			if (hit.transform.tag != "Player") {
				//Debug.Log ("Hit direction: " + direction + ", " + hit.transform.tag + " at point " + hit.point.ToString ("F1") + ", distance " + hit.distance.ToString ("F1"));

				dist = hit.distance;
//				if (direction == Vector3.down) {
//					Debug.Log("Hit " +  hit.transform.gameObject.tag);
//					Debug.DrawRay (p2, direction * hit.distance, Color.cyan, Time.deltaTime);
//				}
			}
		}

		// Debug.DrawRay (p1, direction * (dist + 1f), Color.yellow, Time.deltaTime);
		// Debug.DrawRay (p2, direction * (dist + 1f), Color.yellow, Time.deltaTime);

		return (dist);
		
	}

	
	#endregion
	#region FX
	
	/* 
	 * VFX & SFX
	 */
	
	private IEnumerator PlayDelayedAudio() {
		yield return new WaitForSeconds(.3f); // approx time until landing anim appears to hit ground
		AudioSource.PlayClipAtPoint(landingSound, transform.position);
	}
	
	void PlayFootstep() {
		// play a single footstep sound; called by animation events
		soundEffectsSource.Stop ();
		soundEffectsSource.clip = footstepSound;
		soundEffectsSource.loop = false;
		soundEffectsSource.Play();
	}
	
	
	void AddDustClouds() {		
		// add dust particles if sudden acceleration
		if (Mathf.Abs(currentSpeed - previousSpeed) >= runSpeed / 2 && controller.isGrounded) {

			GameObject particleObject = (GameObject) Instantiate(dustParticlePrefab, transform.position, transform.rotation);

			if (!levelRoot)
				levelRoot = GameObject.FindGameObjectWithTag ("Level");
			particleObject.transform.parent = levelRoot.transform;
			Destroy(particleObject, 2f); // destroy in 2 sec
		}
		
	}

	public void ApplyForce(Vector3 direction, float amount) {
		// used to push player away from enemy after hit; accessed via other scripts
		forceAmount = amount;
		forceDirection = direction;			
	}

	public void TriggerJump() {
		triggerJump = true;
	}
	
	
	#endregion
	
	
	Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
		Vector3 dir = point - pivot; // get point direction relative to pivot
		dir = Quaternion.Euler(angles) * dir; // rotate it
		point = dir + pivot; // calculate rotated point
		return point; // return it
	}
	

}



	
