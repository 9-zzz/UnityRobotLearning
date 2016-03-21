using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// FBBIK driving setup demo.
	/// </summary>
	public class FBIKDrivingRig : MonoBehaviour {

		/// <summary>
		/// Linking a FBBIK effector to a target
		/// </summary>
		[System.Serializable]
		public class EffectorTarget {

			public float positionWeight = 1f; // IK position weight
			public float rotationWeight = 1f; // IK rotation weight
			public FullBodyBipedEffector effector; // Effector type (this is just an enum)
			public Transform target; // The target

			// Update IK position, rotation and weight
			public void Update(IKSolverFullBodyBiped solver, float weight) {
				solver.GetEffector(effector).position = target.position;
				solver.GetEffector(effector).rotation = target.rotation;
				
				solver.GetEffector(effector).positionWeight = positionWeight * weight;
				solver.GetEffector(effector).rotationWeight = rotationWeight * weight;
			}
		}
		
		public FullBodyBipedIK ik; // Reference to the FBBIK component
		public float weight = 1f; // The master weight
		public float bendGoalWeight = 0.5f; // Weight of the bend goals
		public float handPoseWeight = 1f; // Weight of finger posing to the wheel

		public FBIKBendGoal[] bendGoals;
		public HandPoser[] handPosers;
		public EffectorTarget[] effectorTargets;
		
		void LateUpdate() {
			// Clamping weights
			weight = Mathf.Clamp(weight, 0f, 1f);
			bendGoalWeight = Mathf.Clamp(bendGoalWeight, 0f, 1f);
			handPoseWeight = Mathf.Clamp(handPoseWeight, 0f, 1f);

			// Update bend goal weights
			foreach (FBIKBendGoal bendGoal in bendGoals) {
				bendGoal.weight = weight * bendGoalWeight;
			}

			// Update hand poser weights
			foreach (HandPoser handPoser in handPosers) {
				handPoser.localRotationWeight = weight * handPoseWeight;
			}

			// Update effectors
			foreach (EffectorTarget effectorTarget in effectorTargets) effectorTarget.Update(ik.solver, weight);
		}
	}
}
