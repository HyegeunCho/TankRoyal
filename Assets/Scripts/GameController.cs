using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject spawnObj;
	public Camera mainCamera;
	public GameObject[] targets;
	public Slider resourceSlider;
	public Text currentResourceText;
	public Text remainTargetText;
	public float resourceGenerateSpeed = 1f; // how many seconds nee for one resource
	public float currentResource = 0f;
	public float maxResource = 10f;
	public float mapSlidingSensitivity = 0.1f;
	public float dragOffset = 0.5f;

	public GameObject[] myBuildings;
	public bool isGameEnd = false;

	Ray placeRay;
	RaycastHit placeHit;
	int placableMask;
	float resourceTimer;
	int remainTargetCount;
	bool _isDragged;
	Vector3 mouseClickPosition;


	void Awake () {
		placableMask = LayerMask.GetMask ("Placable");
		currentResource = 0f;
		resourceSlider.value = currentResource;
		currentResourceText.text = currentResource.ToString ();
		remainTargetCount = targets.Length;
		remainTargetText.text = "Remain Targets: " + remainTargetCount.ToString ();

		_isDragged = false;

		mouseClickPosition = Vector3.zero;

		originCameraPosition = Camera.main.transform.position;
	}
		
	// Update is called once per frame
	void Update () {

		displayRemainTargets ();

		if (remainTargetCount > 0) resourceTimer += Time.deltaTime;
		if (resourceTimer >= resourceGenerateSpeed && currentResource < maxResource) {
			resourceTimer = 0;
			currentResource++;
			resourceSlider.value = currentResource;
			currentResourceText.text = currentResource.ToString();
		}
	}

	// For drag
	private Vector3 originCameraPosition;
	void LateUpdate()
	{
		if (Input.GetMouseButtonUp (0)) {
			if (!_isDragged)
				Place ();
			_isDragged = false;
			mouseClickPosition = Vector3.zero;
			originCameraPosition = Camera.main.transform.position;
		}

		if (Input.GetMouseButtonDown (0)) {
			mouseClickPosition = new Vector3(Input.mousePosition.x, 0f, Input.mousePosition.y);
		}

		if (Input.GetMouseButton (0)) {
			Vector3 inputMousePosition = new Vector3(Input.mousePosition.x, 0f, Input.mousePosition.y);

			if (mouseClickPosition != Vector3.zero) {
				//Vector3 newCameraPosition = new Vector3 (mainCamera.transform.position.x - (inputMousePosition.x - mouseClickPosition.x) * mapSlidingSensitivity, mainCamera.transform.position.y, mainCamera.transform.position.z - (inputMousePosition.z - mouseClickPosition.z) * mapSlidingSensitivity);
				Vector3 newCameraPosition = originCameraPosition - (inputMousePosition - mouseClickPosition) * mapSlidingSensitivity;
				newCameraPosition.y = originCameraPosition.y;
				// 현재 카메라가 이동해야할 거리
				float tmpDist = Vector3.Distance (newCameraPosition, mainCamera.transform.position);
				// 마우스로 드래그한 거리
				float mouseDragDistance = Vector3.Distance(Camera.main.ScreenToViewportPoint(inputMousePosition), Camera.main.ScreenToViewportPoint(mouseClickPosition));
				// 카메라가 이동한 거리
				float cameraMoveDistance = Vector3.Distance (originCameraPosition, newCameraPosition);

				//Debug.Log ("mouseDragDistance = " + mouseDragDistance.ToString () + " / cameraMoveDistance = " + cameraMoveDistance.ToString ());

				//if (tmpDist > dragOffset){// && mouseDragDistance < cameraMoveDistance) {
				if (mouseDragDistance > dragOffset) {
					mainCamera.transform.position = newCameraPosition;
					_isDragged = true;
				} 
			}
		}
	}

	void Place()
	{
		placeRay = mainCamera.ScreenPointToRay (Input.mousePosition);
		if (Physics.Raycast(placeRay, out placeHit, 100, placableMask)) {
			Debug.DrawRay(placeRay.origin, placeRay.direction * 100, Color.black);

			if (ReduceResources (spawnObj)) {
				Instantiate (spawnObj, placeHit.point, Quaternion.identity);
			}
		}
	}

	bool ReduceResources(GameObject inSpawnObj) {

		ResourceCost resourceCost = inSpawnObj.GetComponent<ResourceCost> ();
		if (resourceCost == null) {
			//Debug.Log ("UnSpawnable Instance!!");
			return false;	
		}
		if (resourceCost.resourceCost > currentResource) {
			//Debug.Log ("Not enough resources!!");
			return false;
		}

		currentResource -= resourceCost.resourceCost;
		resourceSlider.value = currentResource;
		currentResourceText.text = currentResource.ToString();
		return true;
	}

	void displayRemainTargets()
	{
		int activeTarget = 0;
		for (int i = 0; i < targets.Length; i++) {
			if (targets [i].activeSelf)
				activeTarget++;
		}
		remainTargetCount = activeTarget;
		remainTargetText.text = "Remain Targets: " + remainTargetCount.ToString ();
	}
}
