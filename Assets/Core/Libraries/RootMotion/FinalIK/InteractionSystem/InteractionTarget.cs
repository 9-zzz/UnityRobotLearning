using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// The target of an effector in the InteractionSystem.
	/// </summary>
	public class InteractionTarget : MonoBehaviour {

		// Multiplies the value of a weight curve for this effector target.
		[System.Serializable]
		public class Multiplier {
			public InteractionObject.WeightCurve.Type curve;
			public float multiplier;
		}

		// The type of the FBBIK effector
		public FullBodyBipedEffector effectorType;
		// InteractionObject weight curve multipliers for this effector target
		public Multiplier[] multipliers;
		// The interaction speed multiplier for this effector
		public float interactionSpeedMlp = 1f;
		// The pivot to twist/swing this interaction target about
		public Transform pivot;
		// The axis of twisting the interaction target
		public Vector3 twistAxis = Vector3.up;
		// The weight of twisting the interaction target towards the effector bone in the start of the interaction
		public float twistWeight = 1f;
		// The weight of swinging the interaction target towards the effector bone in the start of the interaction
		public float swingWeight;
		// If true, will twist/swing around the pivot only once at the start of the interaction
		public bool rotateOnce = true;

		private Quaternion defaultLocalRotation;
		private Transform lastPivot;

		// Should a curve of the Type be ignored for this effector?
		public float GetValue(InteractionObject.WeightCurve.Type curveType) {
			for (int i = 0; i < multipliers.Length; i++) if (multipliers[i].curve == curveType) return multipliers[i].multiplier;
			return 1f;
		}

		// Reset the twist and swing rotation of the target
		public void ResetRotation() {
			if (pivot != null) pivot.localRotation = defaultLocalRotation;
		}

		// Rotate this target towards a position
		public void RotateTo(Vector3 position) {
			if (pivot == null) return;

			if (pivot != lastPivot) {
				defaultLocalRotation = pivot.localRotation;
				lastPivot = pivot;
			}

			// Rotate to the default local rotation
			pivot.localRotation = defaultLocalRotation;

			// Twisting around the twist axis
			if (twistWeight > 0f) {
				Vector3 targetTangent = transform.position - pivot.position;
				Vector3 n = pivot.rotation * twistAxis;
				Vector3 normal = n;
				Vector3.OrthoNormalize(ref normal, ref targetTangent);

				normal = n;
				Vector3 direction = position - pivot.position;
				Vector3.OrthoNormalize(ref normal, ref direction);

				Quaternion q = QuaTools.FromToAroundAxis(targetTangent, direction, n);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, q, twistWeight) * pivot.rotation;
			}

			// Swinging freely
			if (swingWeight > 0f) {
				Quaternion s = Quaternion.FromToRotation(transform.position - pivot.position, position - pivot.position);
				pivot.rotation = Quaternion.Lerp(Quaternion.identity, s, swingWeight) * pivot.rotation;
			}
		}
	}
}
