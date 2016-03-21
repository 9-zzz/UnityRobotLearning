using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Class for creating procedural FBBIK hit reactions.
	/// </summary>
	public class HitReaction: MonoBehaviour {
	
		/// <summary>
		/// Hit point definition
		/// </summary>
		[System.Serializable]
		public class HitPoint {

			/// <summary>
			/// Linking a FBBIK effector to this hit point
			/// </summary>
			[System.Serializable]
			public class EffectorLink {

				public FullBodyBipedEffector effector; // The effector type (this is just an enum)
				public float weight; // Weight of following the hit
				public float speed = 10f; // Hit speed
				public float spring = 10f; // Spring force
				public float multiplier = 1f; // Hit force multiplier
				public float gravity; // Gravity of the hit force

				private Vector3 offset;

				// Update and Apply the position offset to the effector
				public void Update(IKSolverFullBodyBiped solver, Vector3 offsetTarget, float weight) {
					// Lerping offset to the offset target
					offset = Vector3.Lerp(offset, offsetTarget * multiplier, Time.deltaTime * speed);

					// Apply gravity
					Vector3 g = (solver.GetRoot().up * gravity) * offset.magnitude;

					// Apply the offset to the effector
					solver.GetEffector(effector).positionOffset += (offset * weight * this.weight) + (g * weight);
				}
			}

			/// <summary>
			/// Linking an individual bone to this hit point
			/// </summary>
			[System.Serializable]
			public class BoneLink {

				public Transform transform; // The bone
				public Vector3 swingAxis = -Vector3.right; // the local swing axis of the bone (the axis towards the next bone)
				public float weight = 1f; // Weight of rotating the bone with the hit force
				public float speed = 10f; // Speed of rotating the bone
				public float spring = 10f; // Spring force
				public float multiplier = 2f; // Hit force multiplier

				private Vector3 offset;

				// Update and Apply the position offset to the effector
				public void Update(Vector3 offsetTarget, float weight) {
					// Lerping offset to the offset target
					offset = Vector3.Lerp(offset, offsetTarget * multiplier, Time.deltaTime * speed);

					// Calculating the bone rotation offset
					Vector3 axis = transform.rotation * swingAxis;
					Quaternion rotationOffset = Quaternion.FromToRotation(axis, axis + (offset * weight * this.weight));

					// Rotating the bone
					transform.rotation = rotationOffset * transform.rotation;
				}
			}

			public string name; // The hit point name
			public Transform transform; // Used for debug only
			public float spring = 10f; // The spring force of this hit point
			public float speed = 10f; // The speed of this hit point

			// The arrays containing all the EffectorLinks and BoneLinks
			public EffectorLink[] effectorLinks = new EffectorLink[0];
			public BoneLink[] boneLinks = new BoneLink[0];

			private Vector3 offset, velocity;

			/// <summary>
			/// Adds force to the hit point.
			/// </summary>
			public void AddForce(Vector3 force) {
				velocity += force;
			}

			// Update and apply this hit point
			public void Update(IKSolverFullBodyBiped solver, float weight) {
				// Update velocity
				velocity = Vector3.Lerp(velocity, -offset, Time.deltaTime * spring);

				// Update offset
				offset += velocity * Time.deltaTime * speed;

				// Update Effector Links
				foreach (EffectorLink e in effectorLinks) e.Update(solver, offset, weight);

				// Update Bone Links
				foreach (BoneLink b in boneLinks) b.Update(offset, weight);
			}
		}

		public FullBodyBipedIK ik; // Reference to the FBBIK component
		public float weight = 1f; // The master weight
		public HitPoint[] hitPoints; // The array of hit points

		void LateUpdate() {
			// Clamp the master weight
			weight = Mathf.Clamp(weight, 0f, weight);

			// Update all the hit points
			foreach (HitPoint hitPoint in hitPoints) hitPoint.Update(ik.solver, weight);
		}

		// For demo only, feel free to delete
		#region Debug

		public Transform debugWeapon;
		public Vector3 debugForce;
		private Transform hitT;

		void Update() {
			if (hitT != null) Debug.DrawLine(debugWeapon.position, hitT.position, Color.red);
		}

		public void OnGUI() {
			foreach (HitPoint hitPoint in hitPoints) {
				if (GUILayout.Button(hitPoint.name)) {
					hitT = hitPoint.transform;
					hitPoint.AddForce((hitT.position - debugWeapon.position).normalized);
				}
			}
		}

		#endregion Debug
	}
}
