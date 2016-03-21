using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Leg of the Mech spider. Controls stepping and positioning the IK target
	/// </summary>
	public class MechSpiderLeg : MonoBehaviour {
		
		public MechSpider mechSpider; // Reference to the target
		public MechSpiderLeg unSync; // One of the other legs that we dont want to be completely in sync with, that is stepping at the same time
		public Vector3 offset; // Offset from the default position
		public float raycastHeight = 1.5f, minDelay = 0.2f, maxOffset = 1.0f, stepSpeed = 5.0f, footHeight = 0.15f, velocityPrediction = 0.2f, maintainWorldUp = 0.5f; // Parameters for stepping
		public AnimationCurve yOffset;

		public ParticleSystem sand; // FX for sand

		private IK ik;
		private float stepProgress = 1f, lastStepTime;
		private Vector3 defaultDirection;
		private RaycastHit hit = new RaycastHit();

		// Is the leg stepping?
		public bool isStepping {
			get {
				return stepProgress < 1f;
			}
		}

		// Gets and sets the IK position for this leg
		public Vector3 position {
			get {
				return ik.GetIKSolver().GetIKPosition();
			}
			set {
				ik.GetIKSolver().SetIKPosition(value);
			}
		}
		
		void Start() {
			// Find the ik component
			ik = GetComponent<IK>();

			// Store the default rest position of the leg
			defaultDirection = mechSpider.transform.TransformDirection(position + offset - mechSpider.transform.position);
		}

		// Find the relaxed grounded positon of the leg relative to the body in world space
		private bool GetStepTarget(out Vector3 stepTarget) {
			// place hit.point to the default position relative to the body
			stepTarget = mechSpider.transform.position + mechSpider.transform.TransformDirection(defaultDirection);
			stepTarget += (hit.point - position) * velocityPrediction;
			Vector3 up = mechSpider.transform.up;

			// Rotate the spider local up vector to world up by maintainWorldUp
			Quaternion fromTo = Quaternion.FromToRotation(up, Vector3.up);
			fromTo = Quaternion.Lerp(Quaternion.identity, fromTo, maintainWorldUp);
			up = fromTo * up;

			// Raycast to ground the relaxed position
			if (!Physics.Raycast(stepTarget + up * raycastHeight, -up, out hit, raycastHeight * 2f, mechSpider.raycastLayers)) return false;

			stepTarget = hit.point + up * footHeight;
			return true;
		}
		
		void Update () {
			// if already stepping, do nothing
			if (isStepping) return;

			// Minimum delay before stepping again
			if (Time.time < lastStepTime + minDelay) return;

			// If the unSync leg is stepping, do nothing
			if (unSync != null) {
				if (unSync.isStepping) return;
			}

			// Find the ideal relaxed position for the leg relative to the body
			Vector3 idealPosition = Vector3.zero;
			if (!GetStepTarget(out idealPosition)) return;

			// If distance to that ideal position is less than the threshold, do nothing
			if (Vector3.Distance(position, idealPosition) < maxOffset * UnityEngine.Random.Range(0.9f, 1.2f)) return;

			// Need to step closer to the ideal position
			StopAllCoroutines();
			StartCoroutine(Step(position, idealPosition));
		}

		// Stepping co-routine
		private IEnumerator Step(Vector3 stepStartPosition, Vector3 targetPosition) {
			stepProgress = 0f;

			// Moving the IK position
			while (stepProgress < 1) {
				stepProgress += Time.deltaTime * stepSpeed;
				
				position = Vector3.Lerp(stepStartPosition, targetPosition, stepProgress);
				position += mechSpider.transform.up * yOffset.Evaluate(stepProgress);
				
				yield return null;
			}

			// Emit sand
			if (sand != null) {
				sand.transform.position = position - mechSpider.transform.up * footHeight;
				sand.Emit(20);
			}
			
			lastStepTime = Time.time;
		}
	}
}
