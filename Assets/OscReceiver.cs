using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OscReceiver : MonoBehaviour {


	[HideInInspector] public List<float> messages;
	[HideInInspector] public bool newMessageThisFrame;

	// Use this for initialization

	private long lastTimeStamp = 0;

	[SerializeField] private int numberOfInputs = 3;

	[SerializeField] private float defaultInputValue = 1f;

	public LoadAndSpawnTrails trailSpawner;

	void Start () {
		DontDestroyOnLoad (this.gameObject);
		if (!OSCHandler.Instance.Servers.ContainsKey ("AntGame")) {
			OSCHandler.Instance.Init ();
		}
	

		messages = new List<float> ();
		for (int i = 0; i < numberOfInputs; i++) {
			messages.Add (defaultInputValue);
		}
		if (Application.isEditor) {
			Application.runInBackground = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		OSCHandler.Instance.UpdateLogs();
	
		ServerLog serverLog;
		OSCHandler.Instance.Servers.TryGetValue ("AntGame", out serverLog);

		if (serverLog.server.LastReceivedPacket == null)
			return;
		if (serverLog.server.LastReceivedPacket.TimeStamp == lastTimeStamp)
			return;
		
		//new message received! do stuff!
		lastTimeStamp = serverLog.server.LastReceivedPacket.TimeStamp;
		messages.Clear ();
		UnityOSC.OSCPacket packet = serverLog.server.LastReceivedPacket;
		float x = (float)packet.Data [0];
		float y = (float)packet.Data [1];
		print (x.ToString() +", " +  y.ToString());
		if (packet.Address == "/arrow") {
			trailSpawner.setArrow (Mathf.Atan2 (y, x) * 180 / Mathf.PI);
		} else if (packet.Address == "/a") {
			trailSpawner.setEnemy (0, x, y);
		} else if (packet.Address == "/b") {
			trailSpawner.setEnemy (1, x, y);
		} else if (packet.Address == "/c") {
			trailSpawner.setEnemy (2, x, y);
		} else if (packet.Address == "/d") {
			trailSpawner.setEnemy (3, x, y);
		}
	}
}
