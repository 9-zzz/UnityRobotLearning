using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Bend goal for FullBodyBipedIK.
	/// </summary>
	public class FBIKBendGoal: MonoBehaviour {
		
		public FullBodyBipedIK ik; // Refernce to the FBBIK component
		public FullBodyBipedChain chain; // Which limb is this bend goal for?
		
		public float weight; // Bend goal weight
		
		void Awake() {
			// Add to the OnPreBend delegate so get a call immediatelly before the solvers applies bend constraints
			ik.solver.OnPreBend += OnPreBend;
		}

		// Called by IKSolverFullBody
		private void OnPreBend() {
			// Find the direction from the solver position of the elbow/knee to the goal position
			ik.solver.GetBendConstraint(chain).direction = transform.position - ik.solver.GetChain(chain).nodes[0].solverPosition;

			// Set the weight of the bend constraint (if 0, will maintain animated bend direction)
			ik.solver.GetBendConstraint(chain).weight = weight;
		}
	}
}
	
	
