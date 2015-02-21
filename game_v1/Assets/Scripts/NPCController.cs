using UnityEngine;
using System.Collections;

public class NPCController : MonoBehaviour {

	// walking back and forth
	public bool walkAround = true;
	public float walkSpeed = 1f;
	public float rotateSpeed = 20f;
	public float turnDegrees = 180f;
	public float secondsTillTurn = 3f;	
	private float timeUntilTurn; // countdown clock till turning
	private bool isTurning = false;
	private float anglesTurnedSoFar = 0f;
		
	// attacking
	public bool attackTarget = true;
	public float attackRange = 2f;
	private GameObject player;
	private Vector3 ourPosition;
	private Animator animator;

	// chasing
	public bool chaseTarget = true;
	public float chaseTime = 5f; // sec to chase until give up


	// Use this for initialization
	void Start () {
		animator = gameObject.GetComponent<Animator> ();
		ourPosition = gameObject.transform.position;
		player = GameObject.FindGameObjectWithTag ("Player");
		timeUntilTurn = secondsTillTurn;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (attackTarget) {

			Vector3 playerPosition = player.transform.position;
			float distance = Vector3.Distance (ourPosition, playerPosition);

		
			if (distance <= attackRange) {
				animator.SetTrigger("attack");
				Debug.Log("attacking! ");
			}

		}

		if (walkAround) {
					
			timeUntilTurn -= Time.deltaTime;
			
			if (timeUntilTurn <= 0 && !isTurning) {
				//			Debug.Log ("Starting Turn");
				isTurning = true;
			}
			
			if (isTurning && anglesTurnedSoFar < turnDegrees) {
				//			Debug.Log ("Continuing Turn");
				transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);		
				anglesTurnedSoFar += rotateSpeed * Time.deltaTime;
			} else if (isTurning && anglesTurnedSoFar >= turnDegrees) {
				//			Debug.Log ("Ending Turn");
				isTurning = false;
				timeUntilTurn = secondsTillTurn;
				anglesTurnedSoFar = 0f;
			}
			
			// don't move when turning
			if (!isTurning) {
				transform.position = Vector3.Lerp (transform.position, transform.position + transform.forward * walkSpeed, Time.deltaTime);
			}

		}// end walkaround

	}


	// turn when we run into a wall
	void OnCollisionEnter(Collision collision) {
		foreach (ContactPoint contact in collision.contacts) {
			//	Debug.Log ("contact: " + contact);
			if (contact.normal == Vector3.forward) {
				Debug.DrawRay(contact.point, contact.normal, Color.white);
				timeUntilTurn = 0f;
				anglesTurnedSoFar = 0f;
			}
		}
	}


}
