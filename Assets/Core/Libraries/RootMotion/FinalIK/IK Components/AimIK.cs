using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Aim %IK solver component.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/Aim")]
	public class AimIK : IK {
		
		/// <summary>
		/// The Aim %IK solver.
		/// </summary>
		public IKSolverAim solver = new IKSolverAim();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}

