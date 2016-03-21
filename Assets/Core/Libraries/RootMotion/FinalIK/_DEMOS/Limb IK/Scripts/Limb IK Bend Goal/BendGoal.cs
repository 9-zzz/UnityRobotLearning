using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Bend goal for LimbIK. Attach this to a GameObject that you want the limb to bend towards.
	/// </summary>
	public class BendGoal : MonoBehaviour {

		public LimbIK limbIK; // reference to the LimbIK component

		void LateUpdate () {
			if (limbIK == null) return;

			// Set LimbIK bend goal position to myself
			limbIK.solver.SetBendGoalPosition(transform.position);
		}
	}
}
