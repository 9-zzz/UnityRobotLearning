using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %IK component for IKSolverLimb.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/Limb")]
	public class LimbIK : IK {
		
		/// <summary>
		/// The Limb %IK solver.
		/// </summary>
		public IKSolverLimb solver = new IKSolverLimb();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}
