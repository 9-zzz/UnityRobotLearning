using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Analytic %IK algorithm based on the law of cosines
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/Trigonometric")]
	public class TrigonometricIK : IK {
		
		/// <summary>
		/// The Trigonometric %IK solver.
		/// </summary>
		public IKSolverTrigonometric solver = new IKSolverTrigonometric();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}
