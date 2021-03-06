﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class MoveTheAnt : MonoBehaviour {

	private Vector3 mousePos;
	public float moveSpeed = 0.1f;
	public Animator anim;
	public Rigidbody2D body;

	//me
	private int thunderCount;
	private int count = 0;
	private bool isSpeedup = false;
	private bool isShowClockText = false;
	private float timeLeftForSpeedUp;
	private float timeLeftForClockText;

	public Text thunderText;
	public Text clockText;
	public Image thunder1;
	public Image thunder2;

	public int ThunderNumber {
		get { return thunderCount; }
		set { thunderCount = value; }
	}

	// Use this for initialization
	void Start () {
		//print ("start!");
		thunder1.enabled = false;
		thunder2.enabled = false;
		thunderCount = 0;
		thunderText.text = "";
		clockText.text = "";

		anim = GetComponent<Animator> ();
		body = GetComponent<Rigidbody2D> ();
			
	}
	
	// Update is called once per frame
	void Update () {
		if (SceneEnderScript.Ended ()) {
			return;
		}
		//leftclick
		if (Input.GetMouseButton (0)) {
			mousePos = Input.mousePosition;
			mousePos = Camera.main.ScreenToWorldPoint (mousePos);
			transform.position = Vector2.Lerp (transform.position, mousePos, moveSpeed);

			Vector3 vectorToTarget = mousePos - transform.position;
			float angle = Mathf.Atan2 (vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
			Quaternion q = Quaternion.AngleAxis (angle, Vector3.forward);
			transform.rotation = q;
			anim.enabled = true;


			anim.speed = (vectorToTarget.magnitude/44f-0.8f)*8f;
//			Debug.Log (anim.speed);
//			Debug.Log (body.velocity.y);

		} else {
			anim.enabled = false;
		}
			

	
		// press key
		if (Input.GetKeyDown ("space")) {
			if (isSpeedup == true || thunderCount < 1)
				return;
			print ("space key" + count);
			ReduceThunder ();
			moveSpeed = moveSpeed * 2;
			isSpeedup = true;
			timeLeftForSpeedUp = 5.0f;
			thunderText.text = "Speed Up !!!";
		}

		if (isSpeedup == true) {
			timeLeftForSpeedUp -= Time.deltaTime;
			if(timeLeftForSpeedUp < 0)
			{
				moveSpeed = moveSpeed / 2;
				isSpeedup = false;
				timeLeftForSpeedUp = 5.0f;
				print ("down");
				thunderText.text = "";
			}
		}

		// check clock text
		if (isShowClockText == true) {
			timeLeftForClockText -= Time.deltaTime;
			if (timeLeftForClockText < 0) {
				clockText.text = "";
				isShowClockText = false;
			}
		}
	
	}


	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.CompareTag ("Thunder")) {
			other.gameObject.SetActive (false);
			UpdateThunder ();
		}

		if (other.gameObject.CompareTag ("Clock")) {
			other.gameObject.SetActive (false);
			GameTimerScript.timer -= 5;
			if (isShowClockText == false) {
				clockText.text = "Time + 5";
				isShowClockText = true;
				timeLeftForClockText = 2;
			}

		}



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

	void ReduceThunder() {
		if (thunderCount == 1) {
			thunderCount -= 1;
			thunder1.enabled = false;
		}
		else if (thunderCount == 2) {
			thunderCount -= 1;
			thunder2.enabled = false;
		}		
	}
}
