using UnityEngine;
using System.Collections;

public class HealthController : MonoBehaviour {

	public float StartingHitPoints = 100f;
	public float RemainingHitPoints;
	public GameObject damageParticlePrefab;
	public AudioClip damageSound;
	private GameObject player;

	// Use this for initialization
	void Start () {	
		RemainingHitPoints = StartingHitPoints;
		player = GameObject.FindGameObjectWithTag ("Player");
	}

	public void ApplyDamage(float points) {	
		RemainingHitPoints -= points;
		AudioSource.PlayClipAtPoint(damageSound, player.transform.position);
		GameObject particleObject = (GameObject) Instantiate(damageParticlePrefab, player.transform.position, player.transform.rotation);	
	}
	

}
