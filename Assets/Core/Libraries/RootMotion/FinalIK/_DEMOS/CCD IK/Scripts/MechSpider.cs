using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Mech spider Demo.
	/// </summary>
	public class MechSpider : MonoBehaviour {

		public LayerMask raycastLayers; // The ground layers
		public Transform body; // The body transform, the root of the legs
		public MechSpiderLeg[] legs; // All the legs of this spider
		public float legRotationWeight = 1f; // The weight of rotating the body to each leg
		public float bodyRotationSpeed = 30f; // The slerp speed of rotating the body to legs
		public float breatheSpeed = 2f; // Speed of the breathing cycle
		public float breatheMagnitude = 0.2f; // Magnitude of breathing

		private Vector3 lastPosition;
		private Vector3 defaultBodyLocalPosition;
		private float sine;

		void Update() {
			// Find the normal of the plane defined by leg positions
			Vector3 legsPlaneNormal = GetLegsPlaneNormal();

			// Rotating the body
			Quaternion bodyRotation = body.rotation;

			body.localRotation = Quaternion.identity;
			Quaternion fromTo = Quaternion.FromToRotation(transform.up, legsPlaneNormal);
			body.rotation = Quaternion.Slerp(bodyRotation, fromTo * body.rotation, Time.deltaTime * bodyRotationSpeed);

			// Update Breathing
			sine += Time.deltaTime * breatheSpeed;
			if (sine >= Mathf.PI * 2f) sine -= Mathf.PI * 2f;
			float br = Mathf.Sin(sine) * breatheMagnitude;

			// Apply breathing
			Vector3 breatheOffset = transform.up * br;
			body.transform.position = transform.position + breatheOffset;
		}

		// Calculate the normal of the plane defined by leg positions, so we know how to rotate the body
		private Vector3 GetLegsPlaneNormal() {
			Vector3 normal = transform.up;

			// Go through all the legs, rotate the normal by it's offset
			for (int i = 0; i < legs.Length; i++) {
				// Direction from the root to the leg
				Vector3 legDirection = legs[i].position - transform.position; 

				// Find the tangent to transform.up
				Vector3 legNormal = transform.up;
				Vector3 legTangent = legDirection;
				Vector3.OrthoNormalize(ref legNormal, ref legTangent);

				// Find the rotation offset from the tangent to the direction
				Quaternion fromTo = Quaternion.FromToRotation(legTangent, legDirection);
				fromTo = Quaternion.Lerp(Quaternion.identity, fromTo, 1f / Mathf.Lerp(legs.Length, 1f, legRotationWeight));

				// Rotate the normal
				normal = fromTo * normal;
			}
			
			return normal;
		}
	}
}
