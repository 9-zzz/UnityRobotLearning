using UnityEngine;
using System.Collections;
using RootMotion;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Picking up an object with both hands.
	/// </summary>
	public class PickUpBox : MonoBehaviour {

		// GUI for testing
		void OnGUI() {
			if (!holdingBox) {
				
				if (GUILayout.Button("Pick Up Box")) {
					interactionSystem.StartInteraction(FullBodyBipedEffector.LeftHand, box, false);
					interactionSystem.StartInteraction(FullBodyBipedEffector.RightHand, box, false);
				}
				
			} else {
				
				if (GUILayout.Button("Drop Box")) {
					interactionSystem.ResumeAll();
				}
			}
		}

		[SerializeField] InteractionSystem interactionSystem; // The InteractionSystem of the character
		[SerializeField] InteractionObject box; // The box to pick up
		[SerializeField] Transform pivot; // The pivot point of the hand targets
		[SerializeField] Transform boxHoldPoint; // The point where the box will lerp to when picked up
		[SerializeField] float boxLerpMaxSpeed = 7; // Maximum lerp speed of the box. Decrease this value to give the box more weight
		[SerializeField] float boxLerpAcceleration = 7f; // Acceleration of the lerping speed of the box. Decrease this value to give the box more weight

		private float currentBoxLerpSpeed;

		void Start() {
			// Listen to interaction events
			interactionSystem.OnInteractionStart += OnStart;
			interactionSystem.OnInteractionPause += OnPause;
			interactionSystem.OnInteractionResume += OnDrop;
		}

		// Called by the InteractionSystem when an interaction is paused (on trigger)
		private void OnPause(FullBodyBipedEffector effectorType, InteractionObject interactionObject) {
			if (effectorType != FullBodyBipedEffector.LeftHand) return;

			// Make the box inherit the character's movement
			box.transform.parent = interactionSystem.transform;

			// Make the box kinematic
			if (box.GetComponent<Rigidbody>() != null) box.GetComponent<Rigidbody>().isKinematic = true;

			// Set box lerping speed to 0 so we can smoothly accelerate picking up from that
			currentBoxLerpSpeed = 0f;
		}

		// Called by the InteractionSystem when an interaction starts
		private void OnStart(FullBodyBipedEffector effectorType, InteractionObject interactionObject) {
			if (effectorType != FullBodyBipedEffector.LeftHand) return;

			// Rotate the pivot of the hand targets by 90 degrees so we could grab the box from any direction

			// Get the flat direction towards the character
			Vector3 characterDirection = (interactionSystem.transform.position - pivot.position).normalized;
			characterDirection.y = 0f;

			// Convert the direction to local space of the box
			Vector3 characterDirectionLocal = box.transform.InverseTransformDirection(characterDirection);

			// QuaTools.GetAxis returns a 90 degree ortographic axis for any direction
			Vector3 axis = QuaTools.GetAxis(characterDirectionLocal);
			Vector3 upAxis = QuaTools.GetAxis(box.transform.InverseTransformDirection(interactionSystem.transform.up));

			// Rotate towards axis and upAxis
			pivot.localRotation = Quaternion.LookRotation(axis, upAxis);

			// Rotate the hold point so it matches the current rotation of the box
			boxHoldPoint.rotation = box.transform.rotation;
		}

		// Called by the InteractionSystem when an interaction is resumed from being paused
		private void OnDrop(FullBodyBipedEffector effectorType, InteractionObject interactionObject) {
			if (effectorType != FullBodyBipedEffector.LeftHand) return;

			// Make the box independent of the character
			box.transform.parent = null;

			// Turn on physics for the box
			if (box.GetComponent<Rigidbody>() != null) box.GetComponent<Rigidbody>().isKinematic = false;
		}

		void LateUpdate() {
			if (holdingBox) {
				// Box acceleration
				currentBoxLerpSpeed = Mathf.Clamp(currentBoxLerpSpeed + Time.deltaTime * boxLerpAcceleration, 0f, boxLerpMaxSpeed);

				// Box lerping
				box.transform.position = Vector3.Lerp(box.transform.position, boxHoldPoint.position, Time.deltaTime * currentBoxLerpSpeed);
				box.transform.rotation = Quaternion.Lerp(box.transform.rotation, boxHoldPoint.rotation, Time.deltaTime * currentBoxLerpSpeed);
			}
		}

		// Are we currently holding the box?
		private bool holdingBox {
			get {
				return interactionSystem.IsPaused(FullBodyBipedEffector.LeftHand);
			}
		}
	}
}
