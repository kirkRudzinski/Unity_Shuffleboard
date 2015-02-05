﻿using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public int speed;
	public float maxLimit;
	public int timeLimit;
	private static float forceOffTheGround = 0.89f;
	private static int MAXTURN = 8;
	private static int MAXROUND = 3;

	public GameObject prefabPlayer1;
	public GameObject prefabPlayer2;
	private GameObject player;
	private bool isPlayer1turn;
	private bool isPlayer2turn;

	private GameObject[] scoreObjects;
	private GameObject[] halfscoreObjects;
	private int scorePlayer1;
	private int scorePlayer2;
	private int turn;
	private int round;

	private float currentTime;

	private bool isDragFinish;
	private bool isLaunched;
	private bool isOutOfLimits;
	private bool isInstant;

	private Vector3 currentPoint;
	private Vector3 endPoint;

	// Use this for initialization
	void Start () {
		// Start with Player 1
		scoreObjects = GameObject.FindGameObjectsWithTag ("scores");
		halfscoreObjects = GameObject.FindGameObjectsWithTag ("halfscore");
		scorePlayer1 = 0;
		scorePlayer2 = 0;
		isLaunched = false;
		isDragFinish = false;
		isOutOfLimits = false;
		currentTime = 0;
		turn = 1;
		round = 1;
		isPlayer1turn = true;
		isPlayer2turn = false;
		isInstant = false;
	}

	// Update is called once per frame
	void Update () {
		// For each rounds
		if (round <= MAXROUND) {
			if (turn <= MAXTURN) {
				// If its not created, creat it.
				if (!isInstant){
					if (isPlayer1turn){
						player = Instantiate(prefabPlayer1, transform.position, transform.rotation) as GameObject;
						player.tag = "player1";
					} 
					else if (isPlayer2turn){
						player = Instantiate(prefabPlayer2, transform.position, transform.rotation) as GameObject;
						player.tag = "player2";
					}
					isInstant = true;
				}
				// Shuffle board
				shuffleboard ();
			}
			// After all the turns
			else if (turn == MAXTURN+1) {
				Debug.Log ("11");
				// Scores
				scores();
				//TODO Check? Remove all players objects
				DestryAllPlayers();

				// TODO Check? End of each turn
				round += 1;
				Rounds.round = round;
				turn = 0;
			}
		}
	}

	void DestryAllPlayers(){
		GameObject[] players1 = GameObject.FindGameObjectsWithTag ("player1");
		GameObject[] players2 = GameObject.FindGameObjectsWithTag ("player2");
		foreach (GameObject p1 in players1) {
			Destroy (p1);
		}
		foreach (GameObject p2 in players2) {
			Destroy (p2);
		}
	}

	void scores(){
		GameObject[] players1 = GameObject.FindGameObjectsWithTag ("player1");
		GameObject[] players2 = GameObject.FindGameObjectsWithTag ("player2");
		if (players1.Length == 0 || players2.Length == 0) return;

		bool isPlayer1Bigger = false;
		bool isPlayer2Bigger = false;

		// checking for Z Axis of Objects (should be higher than others)
		float maxP1Z = players1[0].transform.position.z;
		foreach (GameObject p1 in players1) {
			if (p1.transform.position.z > maxP1Z)
				maxP1Z = p1.transform.position.z;
		}
		float maxP2Z = players2[0].transform.position.z;
		foreach (GameObject p2 in players2) {
			if (p2.transform.position.z > maxP2Z)
				maxP2Z = p2.transform.position.z;
		}
		// TODO what if they are the same z
		if (maxP1Z > maxP2Z) {
			isPlayer1Bigger = true;
			isPlayer2Bigger = false;
		} 
		else {
			isPlayer1Bigger = false;
			isPlayer2Bigger = true;
		}

		// If player 1 is farther than Player 2
		if (isPlayer1Bigger) {
			foreach (GameObject p1 in players1) {
				// only those disk that is further is getting score
				if (p1.transform.position.z > maxP2Z) {
					// checking the bounds
					Bounds playerBound = p1.renderer.bounds;
					foreach (GameObject scoreObject in scoreObjects){
						Bounds scoreBound = scoreObject.collider.bounds;
						// if they intersects
						if (playerBound.Intersects(scoreBound)){
							// If the disk touches or crosses any of the lines, it scores the value of the lower scoring area.
							bool halfscoreTrigger = false;
							foreach (GameObject halfscoreObject in halfscoreObjects){
								Bounds halfscoreBound = halfscoreObject.collider.bounds;
								if (playerBound.Intersects(halfscoreBound)){
									halfscoreTrigger = true;
									break;
								}
							}
							if (halfscoreTrigger){
								// Sum the player1's score minus 1
								scorePlayer1 += (getScore(scoreObject) - 1);
								P1ScoreManager.score = scorePlayer1;
								//TODO break;
							}
							else {
								// Sum the player1's score
								scorePlayer2 += getScore(scoreObject);
								P2ScoreManager.score = scorePlayer2;
								//TODO break;
							}
						}
					}
				}
			}
		}
		else if (isPlayer2Bigger) {
			foreach (GameObject p2 in players2) {
				// only those disk that is further is getting score
				if (p2.transform.position.z > maxP1Z) {
					// checking the bounds
					Bounds playerBound = p2.collider.bounds;
					foreach (GameObject scoreObject in scoreObjects){
						Bounds scoreBound = scoreObject.collider.bounds;
						// if they intersects
						if (playerBound.Intersects(scoreBound)){
							// If the disk touches or crosses any of the lines, it scores the value of the lower scoring area.
							bool halfscoreTrigger = false;
							foreach (GameObject halfscoreObject in halfscoreObjects){
								Bounds halfscoreBound = halfscoreObject.collider.bounds;
								if (playerBound.Intersects(halfscoreBound)){
									halfscoreTrigger = true;
									break;
								}
							}
							if (halfscoreTrigger){
								// Sum the player1's score minus 1
								scorePlayer1 += (getScore(scoreObject) - 1);
								//TODO break;
							}
							else {
								// Sum the player1's score
								scorePlayer1 += getScore(scoreObject);
								//TODO break;
							}
						}
					}
				}
			}
		}
		// TODO Error Handling?
		else { }
	}
	
	void shuffleboard() {
		if (!isLaunched) {
			// 1) Going to Drag
			if (!isDragFinish){
				// Draging object
				StartCoroutine (drag());
				if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended) {
					isDragFinish = true;
				}
			} 
			// 2) Going to Launch
			else {
				// TODO CheckCurrLocation()
				// Launch the object When Dragging is finished 
				if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended) {
					if (setEndPosition()){
						Launch();
						isLaunched = true;
					}
				}
			}
		}
		// 3) its Launched
		else {
			// If its not Moving
			if (player.rigidbody.IsSleeping ()) {
				// TODO remove all object which is not in the score
				// Change player and other stuff
				DoThisWhenStopped();
			}
			currentTime += Time.deltaTime;
			// If its out or it reaches time limits
			if (isOutOfLimits || currentTime > timeLimit)
				DoThisWhenStopped();
			}
		}

	// Launch object
	void Launch(){
		float distance = Vector3.Distance (currentPoint, endPoint);
		float force = distance * distance;
		float dx = endPoint.x - currentPoint.x;
		float dz = endPoint.z - currentPoint.z;
		Vector3 movement = new Vector3 (dx, 0, dz);
		player.rigidbody.AddForce (movement * speed * force, ForceMode.Impulse);
	}

	// Drag the object
	IEnumerator drag(){
		while(Input.touchCount > 0) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

			if (!Physics.Raycast(ray, out hit)){ yield return null; }

			if(hit.point.z <= maxLimit){
				Vector3 temp = hit.point;
				temp.y = forceOffTheGround;
				player.transform.position = temp;
				player.rigidbody.velocity = Vector3.zero;
				yield return null;
			} else { yield return null; }
		}
	}

	bool setEndPosition(){
		if (Input.touchCount > 0) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
			if (!Physics.Raycast(ray, out hit)){ return false; }
			currentPoint = player.rigidbody.position;
			if( hit.point.z < currentPoint.z ){ return false; }
			endPoint = hit.point;
			return true;
		}
		return false;
	}

	// TODO
	bool CheckCurrLocation(){
		float epsilon = 0.8f;
		// Check for the location of Current
		if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
			
			if (!Physics.Raycast(ray, out hit)){ return false; }
			if(!(Mathf.Abs(hit.point.x - currentPoint.x) < epsilon )){ return false; }
			if(!(Mathf.Abs(hit.point.z - currentPoint.z) < epsilon )){ return false; }
			return true;
		}
		return false;
	}
	/* TODO Do we need it?!
	void OnCollisionEnter(Collision collision) {
		// if it hits Player2
		if (collision.gameObject.tag == "player2") {
			//score += 1;
			Debug.Log ("11");
		}
	}*/

	// change player
	void ChangePlayer(){
		// TODO
		if (isPlayer1turn) {
			isPlayer2turn = true;
			isPlayer1turn = false;
		} else if (isPlayer2turn) {
			isPlayer1turn = true;
			isPlayer2turn = false;
		}
	}

	void DoThisWhenStopped(){
		ChangePlayer();
		isLaunched = false;
		isDragFinish = false;
		turn += 1;
		isInstant = false;
		isOutOfLimits = false;
	}

	int getScore(GameObject score){
		if (score.name.Equals ("Score1"))
			return 1;
		if (score.name.Equals ("Score2"))
			return 2;
		if (score.name.Equals ("Score3"))
			return 3;
		if (score.name.Equals ("Score4"))
			return 4;
		return 0;
	}

	public void TriggerEntered(){
		isOutOfLimits = true;
	}
}