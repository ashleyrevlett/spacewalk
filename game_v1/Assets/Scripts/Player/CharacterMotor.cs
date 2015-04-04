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
	private Vector3 moveDirection = Vector3.zero; // global var used to track intended movement dir
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


	}
	
	
	void Update () {	

		if (!gameController.isPlaying) {
			// we are paused or a dialog is displaying
			animator.speed = 0f;
			StopAllCoroutines();
			return;
		} else {
			animator.speed = 1f;
		}

		// if force is being applied, don't let player control movement
		if (healthController.takingDamage) {
			forceAmount -= gravity * Time.deltaTime;
			forceAmount = Mathf.Clamp(forceAmount, 0f, terminalVelocity);
			moveDirection = forceDirection * forceAmount;
		} else {
					
			// get input from player if not being moved elsewhere
			playerInput = ProcessPlayerInput ();
			
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
			if (!isSliding) {
				moveDirection.y = verticalVelocity;
			}

		}


		// move
		controller.Move(moveDirection * Time.deltaTime);
		
		// update the animator's values
		UpdateAnimations (playerInput.x, playerInput.y, currentSpeed);

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
			speed = playerInput.y * runSpeed;
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

		// if there's something in front of us, don't try to move forward into it
//		if (distToForwardObstacle <= obstacleDistanceTolerance) {
//			// but do allow moving backwards away from it
//			moveVertical = Mathf.Min(0, moveVertical);
//		}

		// movement direction is determined by the direction we're facing
		Vector3 forward = transform.forward;
		Vector3 right = new Vector3 (forward.z, 0, -forward.x);
		Vector3 targetDirection = moveHorizontal * right + moveVertical * forward;

		// update rotate speed w/ accel if joystick pressed to turn
		if (Mathf.Abs (moveHorizontal) > .1f) {
			curRotSpeed = Mathf.Pow (curRotSpeed, 2f) * Time.deltaTime;
		} else {
			curRotSpeed = minRotSpeed;			
		}
		curRotSpeed = Mathf.Clamp(curRotSpeed, minRotSpeed, maxRotSpeed);

		moveDirection = Vector3.RotateTowards (moveDirection, targetDirection, 360 * Mathf.Deg2Rad * Time.deltaTime, curRotSpeed);
		moveDirection = moveDirection.normalized * speed;

		// look in direction of movement if player is pressing joystick
		if (moveHorizontal != 0 || moveVertical != 0) {
			Vector3 lookDirection = new Vector3(targetDirection.x, 0f, targetDirection.z);
			transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion.LookRotation(lookDirection)), Time.deltaTime * curRotSpeed);
		}

		return(moveDirection);
		
	}

	
	private void UpdateAnimations(float moveHorizontal, float moveVertical, float speed) {
		// update animation FSM with speed and turn info
		if (verticalVelocity <= 0f && isNearlyGrounded() ) { // not jumping or falling
			animator.SetFloat ("speed", speed / runSpeed); // normalize to 0-1
			animator.SetFloat ("angularVelocity", moveHorizontal);
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
		else if (isNearlyGrounded() && Input.GetButtonDown ("Jump"))
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
		Debug.Log ("Starting jump!");
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
		} else {
			// reached non-sloped ground, stop sliding
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
	
	void OnControllerColliderHit(ControllerColliderHit hit) {
		// detect collisions with sloped terrain
		
		float slope = Mathf.Acos(hit.normal.y) * Mathf.Rad2Deg; // slope of hit surface
		
		// object must be tagged Terrain, we shouldn't slide on npcs or anything not terrain
		// also we shouldn't slide down near vertical surfaces; gravity will handle that
		
		if (slope > controller.slopeLimit && hit.gameObject.tag == "Terrain" && !(slope >= 90 && slope <= 91)) {
			
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

	
	private bool isNearlyGrounded() {
		// can't trust character controller's isGrounded, so cast for nearest object below 
		// and use this distance instead to determine whether we're grounded		
		if (distToGround <= groundTolerance)
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
				Debug.DrawRay (p2, direction * hit.distance, Color.cyan, Time.deltaTime);
				dist = hit.distance;
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
	
	
	#endregion
	
}



	
