using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnController : MonoBehaviour {

	[System.Serializable]
	public class Waypoints
	{
		public Transform point1;
		public Transform point2;
	}
		
	public Waypoints[] minionWays;

	public string targetTag;
	public GameObject[] spawnMinions;
	public int[] spawnCountAtOnce;

	public float spawnTime = 5f;

	GameController gController;

	void Awake()
	{
		gController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController>();
	}

	// Use this for initialization
	void Start () {
		InvokeRepeating ("Spawn", spawnTime, spawnTime);
	}

	void Spawn()
	{
//		if (gController.isGameEnd) {
//			return;  // exit function
//		}

		// 각 웨이포인트에 따라 미니언 생성
		for (int k = 0; k < minionWays.Length; k++) {
			for (int i = 0; i < spawnMinions.Length; i++) {
				for (int j = 0; j < spawnCountAtOnce [i]; j++) {
					GameObject spawnedObject = (GameObject)Instantiate (spawnMinions [i], transform.position, transform.rotation);
					AutoPilot spawnedPilot = spawnedObject.GetComponent<AutoPilot> ();
					if (spawnedPilot) {
						spawnedPilot.SetWaypoints (minionWays[k]);
						spawnedPilot.SetTargetTag (targetTag);
					}
				}
			}
		}


//		for (int i = 0; i < spawnMinions.Length; i++) {
//			for (int j = 0; j < spawnCountAtOnce [i]; j++) {
//				for (int k = 0; k < minionWays.Length; k++) {
//					GameObject spawnedObject = (GameObject)Instantiate (spawnMinions [i], transform.position, transform.rotation);
//					AutoPilot spawnedPilot = spawnedObject.GetComponent<AutoPilot> ();
//					if (spawnedPilot) {
//						spawnedPilot.SetWaypoints (minionWays[k]);
//						spawnedPilot.SetTargetTag (targetTag);
//					}
//				}
//			}
//		}
	}
}
