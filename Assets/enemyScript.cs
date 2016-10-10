using UnityEngine;
using System.Collections;

public class enemyScript : MonoBehaviour {

	public GameObject player;

	public float sightRange;

	public float chaseSpeed;

	public float patrolSpeed;

	public Vector3 targetPos;

	private Vector3 startPos;

	private bool movingToTarget;


	public int framesBeforeSwitch;
	private int frameCounter;

	// Use this for initialization
	void Start () {
		startPos = transform.position;
		movingToTarget = true;
		frameCounter = 0;
	}
	
	// Update is called once per frame
	void Update () {


		float distance = Vector3.Distance (transform.position, player.transform.position);
		if (distance <= sightRange) {
			transform.position = Vector2.Lerp (transform.position, player.transform.position, chaseSpeed);
		} else {
			Vector3 moveToPosition;
			if (movingToTarget) {
				moveToPosition = targetPos;
			} else {
				moveToPosition = startPos;	
			}
			transform.position = Vector3.Lerp (transform.position, moveToPosition, patrolSpeed);
			frameCounter++;
			if (frameCounter >= framesBeforeSwitch) {
				frameCounter = 0;
				movingToTarget = !movingToTarget;
			}
		}

		
	
	}

	void OnTriggerEnter2D(Collider2D other) {
		print (name + " collided by " + other.name);
		if (other.name != "antsprite") return;
		GameObject oldTrailSpawner = GameObject.Find ("OldTrailSpawner");
		SceneEnderScript ender = oldTrailSpawner.GetComponent<SceneEnderScript> ();
		ender.CallSceneEndFunctions (SceneEnderScript.KILLED);
	}

}
