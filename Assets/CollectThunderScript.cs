using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CollectThunderScript : MonoBehaviour {

	private int thunderCount;

	public Text thunderText;
	public Image thunder1;
	public Image thunder2;

	public int ThunderNumber {
		get { return thunderCount; }
		set { thunderCount = value; }
	}

	void Start() {
		thunder1.enabled = false;
		thunder2.enabled = false;
		thunderCount = 0;
		thunderText.text = "";

	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag ("Thunder")) {
			other.gameObject.SetActive (false);
		}

		UpdateThunder ();

	}

	void UpdateThunder() {
		if (thunderCount == 1) {
			thunderCount += 1;
			thunder2.enabled = true;
		}
		if (thunderCount == 0) {
			thunderCount += 1;
			thunder1.enabled = true;
		}
	}


}
