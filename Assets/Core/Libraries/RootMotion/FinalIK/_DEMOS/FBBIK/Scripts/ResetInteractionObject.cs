using UnityEngine;
using System.Collections;

public class ResetInteractionObject : MonoBehaviour {

	public float resetDelay = 1f;

	private Vector3 defaultPosition;
	private Quaternion defaultRotation;

	void Start() {
		defaultPosition = transform.position;
		defaultRotation = transform.rotation;
	}

	void OnInteractionTrigger(Transform t) {
		StopAllCoroutines();
		StartCoroutine(ResetObject(Time.time + resetDelay));
	}

	private IEnumerator ResetObject(float resetTime) {
		while (Time.time < resetTime) yield return null;

		transform.parent = null;
		transform.position = defaultPosition;
		transform.rotation = defaultRotation;
	}
}
