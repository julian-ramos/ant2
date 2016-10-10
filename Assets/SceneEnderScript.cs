using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class SceneEnderScript : MonoBehaviour {


	static public int TIMEOUT = 1;
	static public int KILLED = 2;
	static public int FOUNDFOOD = 3;


	static private bool ended;

	static public bool Ended() {
		return ended;
	}

	// Use this for initialization
	void Start () {
		ended = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartTheScene() {

	}

	public void EndTheScene() {
		SceneManager.LoadScene ("titlescene");

	}

	public void CallSceneEndFunctions(int endReason) {
		if (ended) {
			return;
		}
		//print ("Scene ending: " + endReason.ToString ());
		GameObject player = GameObject.Find("antsprite");
		SaveTrailScript saver = player.GetComponent<SaveTrailScript> ();
		saver.SavePath (endReason);
		ended = true;
	}
}
