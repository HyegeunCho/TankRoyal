using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoPilot : MonoBehaviour {

	public string targetTag;
	public int attackSpeed = 5;
	public int attackDamage = 1;
	public float attackRange = 8f;
	public float searchRange = 15f;

	// waypoint
	public Transform[] waypoints;
	public int currentWaypointIndex = 0;
	int previousWaypointIndex = -1;
	public float waypointOffset = 3f;

	float attackTimer;

	GameController gController;
	TargetHealth currentTarget;
	Collider _rangeCollider;
	NavMeshAgent nav;
	Transform _searchRangeTransform;

	Rigidbody _currentRigidbody;
	Transform _currentTransform;

	bool _isTargetInRange;

	// 자신의 사거리 안에 들어온 타겟 관리
	private List<GameObject> _myTargets;

	void Awake()
	{
		gController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();
		nav = GetComponent<NavMeshAgent> ();
		_currentTransform = GetComponent<Transform> ();
		_currentRigidbody = GetComponent<Rigidbody> ();
		_rangeCollider = GetComponent<SphereCollider> ();

		// 색적 범위 설정
		_searchRangeTransform = _currentTransform.FindChild ("Range").GetComponent<Transform>();
		_searchRangeTransform.localScale = new Vector3 (searchRange, 1f, searchRange);

		nav.stoppingDistance = attackRange;
		//(_rangeCollider as SphereCollider).radius = attackRange;
		_isTargetInRange = false;

		// waypoint
		currentWaypointIndex = 0;
		previousWaypointIndex = -1;

		_myTargets = new List<GameObject> ();
	}



	// 길 가다가 트리거링된 적을 바로 공격하도록 수정할 것
	TargetHealth FindTarget()
	{
		float minDistance = 999;
		int minIndex = -1;

		for (int i = 0; i < gController.targets.Length; i++) {
			// 지정된 타겟이 아니라면 공격하지 않음
			if (gController.targets [i].CompareTag (targetTag) == false)
				continue;

			TargetHealth targetHealth = gController.targets [i].GetComponent<TargetHealth> ();
			// 이미 타겟이 씬에서 제거되었거나, HP가 0 이하인 경우엔 넘어간다
			if (targetHealth == null || targetHealth.currentHealth <= 0)
				continue;
			float dist = Vector3.Distance (_currentTransform.position, (gController.targets [i] as GameObject).transform.position);
			if (minDistance >= dist) {
				minDistance = dist;
				minIndex = i;
			}
		}

		if (minIndex > -1) {
			return gController.targets [minIndex].GetComponent<TargetHealth> ();
		} else {
			return null;
		}
	}
		
	public void SetWaypoints(SpawnController.Waypoints inPoints)
	{
		int waypointsLength = (inPoints.point2 != null ? 2 : (inPoints.point1 != null ? 1 : 0));
		waypoints = new Transform[waypointsLength];

		int index = 0;
		if (inPoints.point1 != null)
			waypoints.SetValue (inPoints.point1, index++);
		if (inPoints.point2 != null)
			waypoints.SetValue (inPoints.point2, index++);
	}

	public void SetTargetTag(string inTargetTag)
	{
		targetTag = inTargetTag;
	}

	void Attack()
	{
		attackTimer = 0f;
		if (currentTarget == null || currentTarget.currentHealth <= 0) {
			return;
		}

		// 타겟으로 방향전환
		Vector3 turnDir = new Vector3 (currentTarget.transform.position.x, 0f, currentTarget.transform.position.z);
		Vector3 toTarget = (_currentTransform.position + turnDir) - _currentTransform.position;
		toTarget.y = 0f;
		Quaternion newRotation = Quaternion.LookRotation (toTarget);

		// 타겟에게 데미지 적용
		currentTarget.TakeDamage(attackDamage);
	}

	bool CheckCurrentWaypointArrived()
	{
		float distanceFromWaypoint = Vector3.Distance (_currentTransform.position, waypoints [currentWaypointIndex].transform.position);
		if (distanceFromWaypoint < waypointOffset) {
			return true;
		} else {
			return false;
		}
	}



	// Update is called once per frame
	void Update () {

		if (targetTag == null || targetTag == "")
			return;

		attackTimer += Time.deltaTime;

		// waypoint 따라 이동
		if (waypoints == null || waypoints.Length == 0)
			return;

		if (currentWaypointIndex > -1 && currentWaypointIndex < waypoints.Length) {
			if (CheckCurrentWaypointArrived ()) {
				currentWaypointIndex++;
			} else {
				if (currentWaypointIndex != previousWaypointIndex) {
					nav.SetDestination (waypoints[currentWaypointIndex].transform.position);
					previousWaypointIndex = currentWaypointIndex;
				}
			}
		} else {
			nav.enabled = false;
		}
			
		if (currentTarget == null) {
			currentTarget = _myTargets [0].GetComponent<TargetHealth>();
			_isTargetInRange = false;
		} else {
			if (currentTarget.currentHealth <= 0) {
				currentTarget = null;
				RemoveTargetCandidate (_myTargets [0]);
				_isTargetInRange = false;
				return;
			} else {
				nav.SetDestination (currentTarget.transform.position);

				float distanceToTarget = Vector3.Distance (_currentTransform.position, currentTarget.transform.position);
				if (distanceToTarget > attackRange)
					_isTargetInRange = false;
				else
					_isTargetInRange = true;

				if (_isTargetInRange) {
					if (attackTimer >= attackSpeed) {
						Attack ();
					}
				}
			}
		}





		return;


		TargetHealth newTarget = null;

		if (currentTarget == null || currentTarget.currentHealth <= 0) {
			newTarget = FindTarget ();
			if (newTarget == null) {
				nav.enabled = false;
				Debug.Log ("Game End");
			}
		}

		// 새로운 타겟을 찾았을 때, 해당 타겟이 현재 타겟과 다르다면 새로운 타겟을 현재 타겟으로 설정하고 초기화
		if (newTarget != null && newTarget != currentTarget) {
			currentTarget = newTarget;
			float distanceToTarget = Vector3.Distance (_currentTransform.position, currentTarget.transform.position);
			if (distanceToTarget > attackRange)
				_isTargetInRange = false;
			else
				_isTargetInRange = true;
		}

		if (currentTarget != null && currentTarget.currentHealth > 0) {
			if (_isTargetInRange) {
				if (attackTimer >= attackSpeed) {
					Attack ();
				} 
			} else {
				nav.SetDestination (currentTarget.transform.position);

				float distanceToTarget = Vector3.Distance (_currentTransform.position, currentTarget.transform.position);
				if (distanceToTarget <= attackRange)
					_isTargetInRange = true;
				else
					_isTargetInRange = false;
			}
		} else {
			_isTargetInRange = false;
		}
	}

	public void AddTargetCandidate (GameObject obj)
	{
		if (obj.CompareTag (targetTag)) {
			int objectIndex = _myTargets.IndexOf (obj);
			if (objectIndex < 0)
				_myTargets.Add (obj);
		}
	}

	public void RemoveTargetCandidate(GameObject obj)
	{
		int objectIndex = _myTargets.IndexOf (obj);
		if (objectIndex >= 0) {
			_myTargets.Remove (obj);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		AddTargetCandidate (other.gameObject);
	}

	void OnTirggerExit(Collider other)
	{
		RemoveTargetCandidate (other.gameObject);
	}
}
