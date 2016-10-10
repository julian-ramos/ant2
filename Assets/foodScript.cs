using UnityEngine;
using System.Collections;

public class foodScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other) {
		//print (name + " collided by " + other.name);
		if (other.name != "antsprite") return;
		GameObject oldTrailSpawner = GameObject.Find ("OldTrailSpawner");
		SceneEnderScript ender = oldTrailSpawner.GetComponent<SceneEnderScript> ();
		ender.CallSceneEndFunctions (SceneEnderScript.FOUNDFOOD);
	}

}
