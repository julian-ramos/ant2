using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class scoreboardscript : MonoBehaviour {

	public int foodFound = 0;
	public int antsStarved = 0;
	public int antsKilled = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		Text t = GetComponentInParent<Text> ();
		t.text = "ants starved: " + antsStarved.ToString () + " ants killed: " + antsKilled.ToString () + " food found: " + foodFound.ToString ();
	}
}
