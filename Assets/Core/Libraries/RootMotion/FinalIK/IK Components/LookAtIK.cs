using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Rotates a hierarchy of bones to face a target
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/LookAt")]
	public class LookAtIK : IK {
		
		/// <summary>
		/// The LookAt %IK solver.
		/// </summary>
		public IKSolverLookAt solver = new IKSolverLookAt();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}
