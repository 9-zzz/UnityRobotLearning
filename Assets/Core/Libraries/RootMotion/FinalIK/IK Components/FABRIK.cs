using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Forward and Backward Reaching %IK solver component.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/FABRIK")]
	public class FABRIK : IK {
		
		/// <summary>
		/// The %FABRIK solver.
		/// </summary>
		public IKSolverFABRIK solver = new IKSolverFABRIK();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}

