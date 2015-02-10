using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (Animator))]

[System.Serializable]
public class Boundary
{
	public float xMin, xMax, zMin, zMax;
}

public class PlayerController2 : MonoBehaviour {
	
	public enum State {
		Idle,
		Walk,
		Run,
		Jump,
		Die,
	}
	
	
	public AudioClip collectSound; // sound fx
	public AudioClip jumpSound;
	public AudioClip damageSound;
	
	public GameObject particleEffectPrefab; // particle explosions
	public GameObject damageParticlePrefab;

	public Boundary boundary; // walkable perimeter
	public float rotateSpeed = 2.5f;
	public float walkSpeed = 1.5f;
	public float runSpeed = 2.5f;
	public float jumpForce = 10f;
	public float jumpRunBonus = 1.4f;
	public float jumpForwardInertia = 2f;
	public float smooth = 2.0f;

	public int score = 0; // scorekeeping, used by HUD
	public int hitPoints = 10; // health, used by HUD

	private State state = State.Idle;
	private GameObject gameCamera; // reference to MainCamera tagged object
	private Rigidbody playerRigidBody;
	private Animator animator;
	private float moveVertical;
	private float moveHorizontal;
	private bool isGrounded = true;
	private Vector3 moveDirection; // current velocity

	private float velocity = 0f; // used by animator
	private float jumpVelocity = 0f; // used by animator


	void Start () {
		// cache component references
		animator = GetComponent<Animator> ();
		playerRigidBody = GetComponent<Rigidbody> ();
		gameCamera = GameObject.FindWithTag("MainCamera");	
	}
	

	void Update () {
		moveDirection = LocalMovement(); // get user input
		MovePlayer (moveDirection); // move player, listen for jump
	}

	void FixedUpdate() {
		// using rigid body so run in fixedupdate
		// update animations		
		jumpVelocity = playerRigidBody.velocity.y;

		switch (state) {
		case State.Idle:
			velocity = 0f;
			break;
		case State.Walk:
			velocity = .2f;
			break;
		case State.Run:
			Debug.Log ("run!");
			velocity = 1f;
			break;
		case State.Jump:
			Debug.Log ("jump!");
			//velocity = 0f;
			break;		
		}

		animator.SetFloat ("velocity", velocity);
		animator.SetFloat ("jumpVelocity", jumpVelocity);

		// if in idle state but not in idle animation, trigger
		Debug.Log (animator.GetCurrentAnimatorStateInfo (0).IsName ("Idle"));
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && state == State.Idle) {
			animator.SetTrigger("isIdle");
			playerRigidBody.angularVelocity = Vector3.zero;
		}
		
		
	}
		
	Vector3 LocalMovement() {
		/* return player movement amount relative to camera position */

		Vector3 local = Vector3.zero;

		// only control movement when not in air
		if (isGrounded) {

			// get the joystick input 
			float moveHorizontal = Input.GetAxisRaw ("Horizontal");
			float moveVertical = Input.GetAxisRaw ("Vertical");

			// calculate local movement vector based on forward/right relative to camera and player				
			Vector3 camPosition = gameCamera.transform.position;
			Vector3 playerPosition = transform.position;
			Vector3 forwardDirection = playerPosition - camPosition;		
			forwardDirection.y = 0; // naturally looks downward; so we can see it change to no vertical change
			Debug.DrawRay (playerPosition, forwardDirection, Color.blue);			

			Vector3 right = Vector3.Cross (Vector3.up, forwardDirection); // find "right" direction

			if (moveHorizontal != 0) {
					local += right * moveHorizontal;
			}

			if (moveVertical != 0) {
					local += forwardDirection * moveVertical;
			}
		}

		return local;
		
	}

	
	void MovePlayer( Vector3 moveDirection ) {
		/* move player by amount input in Update, update state */
		
		if (moveDirection != Vector3.zero) {
			
			// run or walk
			if (Input.GetButton("Run") && isGrounded ) {
				//playerRigidBody.velocity = moveDirection * runSpeed;
				transform.position = Vector3.Lerp (transform.position, transform.position + (moveDirection * runSpeed), Time.deltaTime * smooth);
				state = State.Run;				
			} else {
				//playerRigidBody.velocity = movement * walkSpeed;
				transform.position = Vector3.Lerp (transform.position, transform.position + (moveDirection * walkSpeed), Time.deltaTime * smooth);
				state = State.Walk;
			}
			
			// rotate character to match forward movement direction
			Quaternion rot = new Quaternion();
			rot.SetLookRotation(moveDirection);
			transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotateSpeed);		
			
		} else {
			// idle state, remove velocity from rotation of player
			playerRigidBody.angularVelocity = Vector3.zero;
			state = State.Idle;		
		}
		
		// jump if button pressed		
		if (Input.GetButton("Jump") && isGrounded ) {
			Jump (moveDirection);
		}
		
	}


	void Jump (Vector3 moveDirection) {
		// jump goes in direction of movement without allowing user to adjust direction in air
		Debug.Log ("Jumping!");
		// jump height depends on state
		if (state == State.Run) {
			moveDirection = (Vector3.up * jumpForce * jumpRunBonus) + (moveDirection * runSpeed * jumpForwardInertia);
		} else if (state == State.Walk) {		
			moveDirection = (Vector3.up * jumpForce) + (moveDirection * walkSpeed * jumpForwardInertia);
		} else {
			moveDirection = (Vector3.up * jumpForce);
		}
		playerRigidBody.AddForce (moveDirection, ForceMode.Impulse);

		// update isGrounded and state
		state = State.Jump;
		isGrounded = false;

		// play jump animation and sound fx
		animator.SetTrigger ("isJumping");
		AudioSource.PlayClipAtPoint(jumpSound, transform.position);
		
	}


	void OnCollisionEnter(Collision collision) {
		/* check for when player collides with terrain (= grounded), update state to idle */

		if (collision.gameObject.tag == "Terrain") {
			isGrounded = true;

			// if jumping then collided with ground, stop jumping
			if (state == State.Jump) {
				animator.SetTrigger ("isIdle");
				state = State.Idle;
				playerRigidBody.angularVelocity = Vector3.zero;
			}

		}	
				
		if (collision.gameObject.tag == "Hazard") {
			AudioSource.PlayClipAtPoint(damageSound, transform.position);
			hitPoints -= 1;
			GameObject particleObject = (GameObject) Instantiate(damageParticlePrefab, transform.position, transform.rotation);
		}

	}


	
	// collect minerals
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Mineral") { 
			Debug.Log ("Contacted Mineral");
			// show particles
			GameObject particleObject = (GameObject) Instantiate(particleEffectPrefab, transform.position, transform.rotation);
			
			// play sound
			AudioSource.PlayClipAtPoint(collectSound, transform.position);
			
			// now destroy and increment score
			Destroy(other.gameObject);
			score += 1;
		} 		
	}


	void DrawDebugInfo() {
		
		// draw forward facing ray
		Debug.DrawRay(transform.position, transform.forward, Color.green);			
		
		// draw downward facing ray
		Debug.DrawRay(transform.position, transform.up * -1f, Color.green);			
		

	}


}
