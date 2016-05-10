using UnityEngine;
using System.Collections;

public class TankAutoPilot : MonoBehaviour {

	public int fireSpeed = 5;
	public int fireDamage = 1;
	public Rigidbody m_Shell;
	public Transform m_FireTransform;
	public float fireForce = 1f;
	public float fireRange = 8f;
	float fireTimer;

	GameController gController;
	TargetHealth currentTarget;
	Collider _rangeCollider;
	NavMeshAgent nav;
	Rigidbody _tankRigidbody;

	Transform currentTransform;

	bool _isTargetInRange;

	// Use this for initialization
	void Awake () {
		gController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController>();
		nav = GetComponent<NavMeshAgent> ();
		currentTransform = GetComponent<Transform> ();
		_rangeCollider = GetComponent<SphereCollider> ();
		_tankRigidbody = GetComponent<Rigidbody> ();

		nav.stoppingDistance = fireRange;
		(_rangeCollider as SphereCollider).radius = fireRange;
		_isTargetInRange = false;
	}

	TargetHealth FindTarget()
	{
		float minDistance = 999;
		int minIndex = -1;
		for (int i = 0; i < gController.targets.Length; i++) {

			if (gController.targets [i].CompareTag ("MyBuilding"))
				continue;
			TargetHealth targetHealth = gController.targets [i].GetComponent<TargetHealth> ();
			if (targetHealth == null || targetHealth.currentHealth <= 0)
				continue;
			float dist = Vector3.Distance (currentTransform.position, (gController.targets[i] as GameObject).transform.position);
			if (minDistance >= dist) {
				minDistance = dist;
				minIndex = i;
			}
		}

		if (minIndex > -1) {
			return gController.targets [minIndex].GetComponent<TargetHealth>();
			Debug.Log ("[TankAutoPilot] (FindTarget) Target Name : " + currentTarget.gameObject.name);
		} else {
			return null;
		}
	}

	void Shoot()
	{
		fireTimer = 0f;
		if (currentTarget == null || currentTarget.currentHealth <= 0) {
			return;
		}

		Vector3 turnDir = new Vector3(currentTarget.transform.position.x , 0f , currentTarget.transform.position.z);
		Vector3 playerToMouse = (transform.position + turnDir) - transform.position;
		playerToMouse.y = 0f;
		Quaternion newRotatation = Quaternion.LookRotation (playerToMouse);
		//_tankRigidbody.MoveRotation(newRotatation);

		Rigidbody shellInstance =
			Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
		shellInstance.velocity = fireForce * m_FireTransform.forward; 
		currentTarget.TakeDamage (fireDamage);
		//Debug.Log ("[TankAutoPilot] (Shoot) target CurrentHealth = " + currentTarget.currentHealth);
	}

	// Update is called once per frame
	void Update () {
		fireTimer += Time.deltaTime;

		TargetHealth newTarget = null;

		if (currentTarget == null || currentTarget.currentHealth <= 0) {
			newTarget = FindTarget ();
			if (newTarget == null) {
				nav.enabled = false;
				//Debug.Log ("Game End");
			}
		}

		if (newTarget != null && newTarget != currentTarget) {
			currentTarget = newTarget;
			float distanceToTarget = Vector3.Distance (currentTransform.position, currentTarget.transform.position);
			if (_isTargetInRange && distanceToTarget > fireRange)
				_isTargetInRange = false;
		}

		if (currentTarget != null && currentTarget.currentHealth > 0 && _isTargetInRange == false) {
			float distanceToTarget = Vector3.Distance (currentTransform.position, currentTarget.transform.position);
			if (distanceToTarget <= fireRange)
				_isTargetInRange = true;
		}

		if (_isTargetInRange) {
			if (fireTimer >= fireSpeed)
				Shoot ();
			//Debug.Log ("[TankAutoPilot] (Update) Target in Range!!");
		} else {
			if (currentTarget != null && currentTarget.currentHealth > 0) {
				nav.SetDestination (currentTarget.transform.position);
			} else {
				_isTargetInRange = false;
				//Debug.Log ("[TankAutoPilot] (Update) No currentTarget!!");
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		//Debug.Log ("[TankAutoPilot] (OnTriggerEnter) collider name = " + other.gameObject.name);
		if (currentTarget == null)
			return;
		if (other.gameObject.name == currentTarget.gameObject.name) {
			_isTargetInRange = true;
		} 
	}

	public bool isTargetInRange()
	{
		return _isTargetInRange;
	}
}
