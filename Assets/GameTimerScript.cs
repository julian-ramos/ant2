using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameTimerScript : MonoBehaviour {

	private float timer;

	public float timerLength;

	private bool ended;

	public Text timerDisplay;


	// Use this for initialization
	void Start () {
	
		timer = 0.0f; 
		ended = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (ended) {
			return;
		}
		if (timerDisplay == null) {
			return;
		}
		timer += Time.deltaTime;
		timerDisplay.text = Mathf.Max (timerLength - timer, 0.0f).ToString();
		if (timer >= timerLength) {
			ended = true;
			SceneEnderScript ender = GetComponentInParent<SceneEnderScript> ();
			ender.CallSceneEndFunctions (SceneEnderScript.TIMEOUT);
		}
		
	}
}
