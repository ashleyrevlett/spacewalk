using UnityEngine;
using System.Collections;

public class TriggerPlatformFall : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		//Debug.Log("ControllerColliderHit");
		if (hit.gameObject.tag == "FallingPlatform") {
			//Debug.Log("Triggering platform");
			PlatformController platform = hit.gameObject.GetComponent<PlatformController>();
			platform.SendMessage("Trigger");
		}		
	}

}
