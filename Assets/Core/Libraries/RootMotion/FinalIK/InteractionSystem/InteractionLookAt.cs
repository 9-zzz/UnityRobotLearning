using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Controls LookAtIK for the InteractionSystem
	/// </summary>
	[RequireComponent(typeof(LookAtIK))]
	[RequireComponent(typeof(InteractionSystem))]
	public class InteractionLookAt: MonoBehaviour {

		public float lerpSpeed = 10f; // Interpolation speed of the LookAtIK target
		public float weightSpeed = 2f; // Interpolation speed of the LookAtIK weight

		// References to the components
		private InteractionSystem interactionSystem;
		private LookAtIK lookAt;
		private Transform lookAtTarget;

		private float stopLookTime; // Time to start fading out the LookAtIK
		private float weight; // Current weight

		void Awake() {
			// Find the components
			interactionSystem = GetComponent<InteractionSystem>();
			lookAt = GetComponent<LookAtIK>();

			// Add to the interaction system delegates
			interactionSystem.OnInteractionStart += OnInteractionStart;
		}

		// Called by the InteractionSystem on start of an interaction
		private void OnInteractionStart(FullBodyBipedEffector effector, InteractionObject interactionObject) {
			lookAtTarget = interactionObject.lookAtTarget;
			stopLookTime = Time.time + (interactionObject.length * 0.5f);
		}

		void Update() {
			if (lookAtTarget == null) return;

			// Interpolate the weight
			float add = Time.time < stopLookTime? weightSpeed: -weightSpeed;
			weight = Mathf.Clamp(weight + add * Time.deltaTime, 0f, 1f);

			// Set LookAtIK weight
			lookAt.solver.IKPositionWeight = Interp.Float(weight, InterpolationMode.InOutQuintic);

			// Set LookAtIK position
			lookAt.solver.IKPosition = Vector3.Lerp(lookAt.solver.IKPosition, lookAtTarget.position, lerpSpeed * Time.deltaTime);

			// Release the LookAtIK for other tasks once we're weighed out
			if (weight <= 0f) lookAtTarget = null;
		}
	}
}