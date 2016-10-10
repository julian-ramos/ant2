using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Amazon.CognitoIdentity;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Text;
using UnityEngine.SceneManagement;

public class LoadAndSpawnTrails : MonoBehaviour {

	static private CognitoAWSCredentials credentials;

	public GameObject blueTrailSpawnerModel;
	public GameObject redTrailSpawnerModel;
	public GameObject yellowTrailSpawnerModel;
	public GameObject enemyModel;
	public GameObject foodModel;

	private List<GameObject> enemies = new List<GameObject> ();
	private List<GameObject> foods = new List<GameObject> ();


	private int fileCount;
	// Use this for initialization

	public string identityPoolID;
	public string region;

	public string S3BucketName;

	private bool sentTheRequests;

	private int requestsSent;

	private int requestsFinished;

	static private List<String> receivedPaths;
	static private String receivedFood;
	static private String receivedEnemies;
	static private String receivedArrow;
	static private List<String> filesToLoad;

	private float timeCount = 0;
	private float foodTimeCount = 0;

	private List<GameObject> lineRenderers = new List<GameObject>();


	private RegionEndpoint _Region
	{
		get { return RegionEndpoint.GetBySystemName(region); }
	}

	private CognitoAWSCredentials _credentials;

	private CognitoAWSCredentials Credentials
	{
		get
		{
			if (_credentials == null)
				_credentials = new CognitoAWSCredentials(identityPoolID, _Region);
			return _credentials;
		}
	}


	private AmazonS3Client _s3Client;

	private AmazonS3Client S3Client {
		get {
			if (_s3Client == null)
				_s3Client = new AmazonS3Client (Credentials, _Region);
			return _s3Client;
		}
	}

	private void GetObjectAsString(string fileName) {
		print(string.Format("fetching {0} from bucket {1}", fileName, S3BucketName));
		S3Client.GetObjectAsync(S3BucketName, fileName, (responseObj) =>
			{
				string data = null;
				var response = responseObj.Response;
				if (response.ResponseStream != null)
				{
					using (StreamReader reader = new StreamReader(response.ResponseStream))
					{
						data = reader.ReadToEnd();
					}

					if (fileName.StartsWith("path")) {
						print ("got path");
						receivedPaths.Add(data);
					} else if (fileName.StartsWith("enemies")) {
						print ("got enemies");

						receivedEnemies = data;
					} else if (fileName.StartsWith("arrow")) {
						print ("got arrow");

						receivedArrow = data;
					} else if (fileName.StartsWith("food")) {
						print ("got food");

						receivedFood = data;
					}
					requestsFinished += 1;
				}
			});
	}

	public void PostPath(string path) {
		byte[] byteArray = Encoding.UTF8.GetBytes(path);
		MemoryStream stream = new MemoryStream(byteArray);


		string fileName = "path" + (filesToLoad.Count).ToString () + ".txt";
		var request = new PostObjectRequest()
		{
			Bucket = S3BucketName,
			Key = fileName,
			InputStream = stream,
			CannedACL = S3CannedACL.Private
		};

		//print("\nMaking HTTP post call");

		S3Client.PostObjectAsync(request, (responseObj) =>
			{
				if (responseObj.Exception == null)
				{
					//print(string.Format("\nobject {0} posted to bucket {1}", responseObj.Request.Key, responseObj.Request.Bucket));
					SceneEnderScript ender = GetComponentInParent<SceneEnderScript>();
					ender.EndTheScene();
				}
				else
				{
					//print("\nException while posting the result object");
					//ResultText.text += string.Format("\n receieved error {0}", responseObj.Response.HttpStatusCode.ToString());
					//print("\n" +responseObj.Exception.Message.ToString());

				}
			});
	}


	public void PostFile(string data, string filenamePrefix) {
		byte[] byteArray = Encoding.UTF8.GetBytes(data);
		MemoryStream stream = new MemoryStream(byteArray);


		string fileName = filenamePrefix + ".txt";
		var request = new PostObjectRequest()
		{
			Bucket = S3BucketName,
			Key = fileName,
			InputStream = stream,
			CannedACL = S3CannedACL.Private
		};

		//print("\nMaking HTTP post call");

		S3Client.PostObjectAsync(request, (responseObj) =>
			{
				if (responseObj.Exception == null)
				{
					//print(string.Format("\nobject {0} posted to bucket {1}", responseObj.Request.Key, responseObj.Request.Bucket));
				}
				else
				{
					//print("\nException while posting the result object");
					//ResultText.text += string.Format("\n receieved error {0}", responseObj.Response.HttpStatusCode.ToString());
					//print("\n" +responseObj.Exception.Message.ToString());

				}
			});
	}

	public void clearData() {
		receivedPaths.Clear ();
		filesToLoad.Clear ();
	}

	public void loadPaths() {
		print ("Fetching all the Objects from " + S3BucketName);

		var request = new ListObjectsRequest()
		{
			BucketName = S3BucketName
		};


		S3Client.ListObjectsAsync(request, (responseObject) =>
			{
				if (responseObject.Exception == null)
				{
					//print ("Got Response \nPrinting now \n");
					responseObject.Response.S3Objects.ForEach((o) =>
						{
							print (string.Format("{0}\n", o.Key));
							filesToLoad.Add(o.Key);
						});
					CallAfterGetFiles();
				}
				else
				{
					//print("Got Exception \n");
					//print (responseObject.Exception.Message.ToString());
				}
			});
	}

	public void loadNewPaths() {
		print ("Fetching all the NEW Objects from " + S3BucketName);

		var request = new ListObjectsRequest()
		{
			BucketName = S3BucketName
		};


		S3Client.ListObjectsAsync(request, (responseObject) =>
			{
				if (responseObject.Exception == null)
				{
					//print ("Got Response \nPrinting now \n");
					responseObject.Response.S3Objects.ForEach((o) =>
						{
							print (string.Format("{0}\n", o.Key));
							if (!filesToLoad.Contains(o.Key)) {
								GetObjectAsString (o.Key);
							}
							filesToLoad.Add(o.Key);
						});
				}
				else
				{
					//print("Got Exception \n");
					//print (responseObject.Exception.Message.ToString());
				}
			});
	}


	void Start () {
		UnityInitializer.AttachToGameObject(this.gameObject);

		if (receivedPaths == null) {
			receivedPaths = new List<String> ();
		}
		if (filesToLoad == null) {
			filesToLoad = new List<string> ();
		}

		if (SceneManager.GetActiveScene ().name == "Ant Scene") {
			makeLineRenderers ();
			makeArrow ();
			makeEnemies ();
			makeFood ();
		}
		if (SceneManager.GetActiveScene ().name == "titlescene") {
			clearData ();
		}
		if (SceneManager.GetActiveScene ().name == "Map Scene") {
			clearData ();
			loadPaths ();
		}

		sentTheRequests = false;
			

		//the below is for loading local files.
		/*object[] loaded = Resources.LoadAll ("SavedPaths/");
		fileCount = loaded.Length;

		for (int i = 0; i < fileCount; i++) {
			GameObject trailRenderer =  Instantiate (TrailSpawnerModel);
			LineRenderer l = trailRenderer.GetComponent<LineRenderer> ();
			TextAsset text = loaded [i] as TextAsset;
			string[] splitText = text.text.Split (new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			l.SetVertexCount (splitText.Length);
			Vector3[] positions = new Vector3[splitText.Length];
			for (int j = 0; j < splitText.Length; j++) {
				string[] splitLine = splitText [j].Split (new String[] { " " }, StringSplitOptions.None);
				float x = float.Parse (splitLine [0]);
				float y = float.Parse (splitLine [1]);
				positions [j] = new Vector3 (x, y, 1);
			}
			l.SetPositions (positions);
		}*/

	}

	void CallAfterGetFiles() {
		requestsSent = filesToLoad.Count;
		requestsFinished = 0;
		sentTheRequests = true;
		foreach (string filename in filesToLoad) {
			GetObjectAsString (filename);
		}
	}

	public void makeLineRenderers() {
		for (int i = 0; i < receivedPaths.Count; i++) {
			makeLineRenderer (i);
		}
	}

	void makeArrow() {
		float rotation = float.Parse (receivedArrow);
		GameObject.Find ("arrow").transform.rotation = Quaternion.AngleAxis (rotation, Vector3.forward); 
	}

	public void setArrow(float rotation) {
		GameObject.Find ("arrow").transform.rotation = Quaternion.AngleAxis (rotation, Vector3.forward); 
		PostFile (rotation.ToString (), "arrow");
	}

	void makeEnemies() {
		string[] splitText = receivedEnemies.Split (new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

		Vector3 position = new Vector3 (0, 0, 1);
		for (int i = 0; i < splitText.Length; i++) {
			GameObject enemy = Instantiate (enemyModel);
			string[] splitLine = splitText [i].Split (new String[] { " " }, StringSplitOptions.None);
			position.x = float.Parse (splitLine [0]);
			position.y = float.Parse (splitLine [1]);
			enemy.transform.position = position;
			enemies.Add (enemy);
		}
	}

	public void setEnemy(int index, float x, float y) {
		GameObject enemy = enemies [index];
		x = Mathf.Max (-30f, x);
		x = Mathf.Min (30f, x);
		y = Mathf.Max (-30f, y);
		y = Mathf.Min (30f, y);
		print (x.ToString () + " " + y.ToString ());
		float newX = enemy.transform.position.x + x;
		float newY = enemy.transform.position.y + y;
		if (newX < -150)
			newX = -150;
		if (newX > 150)
			newX = 150;
		if (newY < -150)
			newY = -150;
		if (newY > 150)
			newY = 150;
		enemy.transform.position = new Vector3 (newX, newY, 1);
		string enemyPositions = "";
		for (int i = 0; i < enemies.Count; i++) {
			enemyPositions += enemies [i].transform.position.x.ToString () + " " + enemies [i].transform.position.y.ToString ();
			if (i != enemies.Count - 1) {
				enemyPositions += "\n";
			}
		}
		PostFile (enemyPositions, "enemies");
	}

	void makeFood() {
		string[] splitText = receivedFood.Split (new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

		Vector3 position = new Vector3 (0, 0, 1);
		for (int i = 0; i < splitText.Length; i++) {
			GameObject food = Instantiate (foodModel);
			string[] splitLine = splitText [i].Split (new String[] { " " }, StringSplitOptions.None);
			position.x = float.Parse (splitLine [0]);
			position.y = float.Parse (splitLine [1]);
			food.transform.position = position;
			foods.Add (food);
			int eaten = 2;	
			int foodSize = 5 - eaten;
			if (foodSize == 0) {

			}
			food.transform.localScale = new Vector3 (foodSize, foodSize, 1);
		}

	}

	void setFood(int index, float x, float y, int eaten) {
		GameObject food = foods [index];
		food.transform.position = new Vector3 (x, y, 2);
	}

	void makeLineRenderer(int index) {
		string[] splitText = receivedPaths [index].Split (new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
		Vector3[] positions = new Vector3[splitText.Length];
		int endReason = int.Parse(splitText[0]);
		GameObject trailRenderer;
		GameObject text = GameObject.Find ("Text");
		if (endReason == SceneEnderScript.TIMEOUT) {
			trailRenderer = Instantiate (blueTrailSpawnerModel);
			if (SceneManager.GetActiveScene ().name == "Map Scene") {
				text.GetComponent<scoreboardscript> ().antsStarved += 1;
			}
		} else if (endReason == SceneEnderScript.KILLED) {
			trailRenderer = Instantiate (redTrailSpawnerModel);
			if (SceneManager.GetActiveScene ().name == "Map Scene") {
				text.GetComponent<scoreboardscript> ().antsKilled += 1;
			}
		} else if (endReason == SceneEnderScript.FOUNDFOOD) {
			trailRenderer = Instantiate (yellowTrailSpawnerModel);
			if (SceneManager.GetActiveScene ().name == "Map Scene") {
				text.GetComponent<scoreboardscript> ().foodFound += 1;
			}
		} else {
			trailRenderer = Instantiate (blueTrailSpawnerModel);
		}
		LineRenderer l = trailRenderer.GetComponent<LineRenderer> ();
		l.SetVertexCount (splitText.Length);

		//index at 1 cause the first line is the death reason.
		for (int j = 1; j < splitText.Length; j++) {
			string[] splitLine = splitText [j].Split (new String[] { " " }, StringSplitOptions.None);
			float x = float.Parse (splitLine [0]);
			float y = float.Parse (splitLine [1]);
			positions [j] = new Vector3 (x, y, 1);
		}
		l.SetPositions (positions);
		lineRenderers.Add (trailRenderer);
		if (lineRenderers.Count > 10) {
			GameObject r = lineRenderers [0];
			lineRenderers.RemoveAt (0);
			GameObject.Destroy (r);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (sentTheRequests && requestsFinished == requestsSent && SceneManager.GetActiveScene().name == "titlescene") {
			requestsSent = 0;
			requestsFinished = 0;
			sentTheRequests = false;
			SceneManager.LoadScene ("Ant Scene");
		}
		if (sentTheRequests && requestsFinished == requestsSent && SceneManager.GetActiveScene ().name == "Map Scene") {
			requestsSent = 0;
			requestsFinished = 0;
			sentTheRequests = false;
			makeLineRenderers ();
			makeArrow ();
			makeEnemies ();
			makeFood ();
		}

		if (SceneManager.GetActiveScene ().name == "Map Scene" ) {
			timeCount += UnityEngine.Time.deltaTime;
			foodTimeCount += UnityEngine.Time.deltaTime;
			if (timeCount >= 10f) {
				timeCount -= 10f;
				loadNewPaths ();
			}
			/*if (foodTimeCount >= 30f) {
				foodTimeCount -= 30f;
				for (int i = 0; i < foods.Count; i++) {
					int x = UnityEngine.Random.Range (0, 2);
					if (x == 0) {
						x -= 1;
					}
					x *= UnityEngine.Random.Range (50, 145);
					int y = UnityEngine.Random.Range (0, 2);
					if (y == 0)
						y -= 1;
					y *= UnityEngine.Random.Range (50, 145);
					setFood (i, x, y, 0);
				}
				string foodPositions = "";
				for (int i = 0; i < foods.Count; i++) {
					foodPositions += foods [i].transform.position.x.ToString () + " " + foods [i].transform.position.y.ToString () + " 2";
					if (i != foods.Count - 1) {
						foodPositions += "\n";
					}
				}
				PostFile (foodPositions, "food");

			}*/
		}
	}
}
