using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;


public class SaveTrailScript : MonoBehaviour {

	public List<Vector3> savedPositions;
	private int frameCounter;
	public int framesBetweenSamples = 10;

	//public string FileNamePrefix;


	private int fileCount;

	public GameObject lineR;

	private StreamWriter writer;

	public GameObject saver;

	// Use this for initialization
	void Start () {
		savedPositions = new List<Vector3> ();
		frameCounter = 0;
	 	//writer = new StreamWriter(Application.dataPath + "/Resources/SavedPaths/" + FileNamePrefix + fileCount.ToString() + ".txt");

	}
	
	// Update is called once per frame
	void Update () {
		frameCounter++;
		if (frameCounter % framesBetweenSamples == 0) {
			frameCounter = 0;
			if (savedPositions.Count == 0 || transform.position != savedPositions [savedPositions.Count - 1]) {
				savedPositions.Add (transform.position);
				//writer.WriteLine (transform.position.x.ToString () + " " + transform.position.y.ToString ());
			}

		}
			
		LineRenderer[] renderers = lineR.GetComponents<LineRenderer> ();
		if (renderers.GetLength (0) > 0) {
			LineRenderer l = renderers [0];
			l.SetVertexCount (savedPositions.Count);
			Vector3[] positions = new Vector3[savedPositions.Count];
			savedPositions.CopyTo (positions);
			l.SetPositions (positions);
		}

	}


	public void SavePath(int endReason) {
		LoadAndSpawnTrails auto = saver.GetComponent<LoadAndSpawnTrails> ();
		string path = "" + endReason.ToString() + "\n";
		for (int i = 0; i < savedPositions.Count; i++) {
			path += savedPositions [i].x.ToString () + " " + savedPositions [i].y.ToString ();
			path += "\n";
		}
		path += transform.position.x.ToString() + " " + transform.position.y.ToString();
		auto.PostPath (path);

	}
		
}
