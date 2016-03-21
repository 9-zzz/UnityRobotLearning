using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// %IK system for multiple branched %FABRIK chains.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/FABRIK Root")]
	public class FABRIKRoot : IK {
		
		/// <summary>
		/// The %FABRIKRoot solver.
		/// </summary>
		public IKSolverFABRIKRoot solver = new IKSolverFABRIKRoot();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}
