using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour {
	
	public bool fallWhenWalkedOn = false;
	public float whenToFall = 2f;
	public float fallSpeed = 1f;
	public float fallAcceleration = .2f;
	public Color triggeredColor;

	public bool linearMove = false;
	public Vector3 moveDirection = Vector3.up;
	public float moveTime = 3f;
	public float moveSpeed = 1f;
	
	private bool triggered = false;
	private bool falling = false;
	private float timeElapsed = 0f; // time waiting after trigger for action
	private float timeFalling = 0f; // keep track of how long it's been falling for accel calc
	
	private bool movingForward = true;
	private float moveTimeElapsed = 0f; // change direction timer
	
	
	void Update () {
		
		if (fallWhenWalkedOn) {
			
			if (triggered) {
				timeElapsed += Time.deltaTime;
			}
			
			if (timeElapsed > whenToFall && !falling) {
				falling = true;			
			}
			
			if (falling)
				Fall();
			
			// end fall, reset everything, remove objects
		}
		
		if (linearMove) {
			
			// move in direction at speed		
			gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, 
			                                              gameObject.transform.position + (moveDirection * Time.deltaTime * moveSpeed),
			                                              Time.deltaTime);	
			
			// time to turn around?
			if (moveTimeElapsed >= moveTime) {
				moveDirection = new Vector3(-moveDirection.x, -moveDirection.y, -moveDirection.z);
				moveTimeElapsed = 0f;
			}
			
			// add time to since the last direction change
			moveTimeElapsed += Time.deltaTime;
			
			
		}
		
		
	}
	
	public void Trigger() {
		if (!triggered) {
			triggered = true;
			timeElapsed = 0f; // reset these
			timeFalling = 0f;
			
			// change material color
			gameObject.renderer.material.color = triggeredColor;
		}
		
	}
	
	private void Fall() {
		// move downward
		float newY = gameObject.transform.position.y - (timeElapsed * fallAcceleration) - (fallSpeed * Time.deltaTime);
		gameObject.transform.position = Vector3.Lerp (gameObject.transform.position, 
		                                              new Vector3 (gameObject.transform.position.x, newY, gameObject.transform.position.z),
		                                              Time.deltaTime);

		// fade color to white
		float change = (fallSpeed * Time.deltaTime);
		Color newColor = new Color (gameObject.renderer.material.color.r + change, gameObject.renderer.material.color.g + change, gameObject.renderer.material.color.b + change);
		gameObject.renderer.material.color = newColor;

	}
	
	
	void OnCollisionEnter(Collision collision) {
		Debug.Log ("Collision with platform!");
		foreach (ContactPoint contact in collision.contacts) {
			Debug.Log ("contacted by: " + contact.otherCollider.tag);
			
			if (contact.otherCollider.tag == "Player") {
				Trigger ();
			}
		}
	}
	
	
	
	
}
