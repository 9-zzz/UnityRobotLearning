using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// CCD (Cyclic Coordinate Descent) %IK solver component.
	/// </summary>
	[AddComponentMenu("Scripts/RootMotion/IK/CCD")]
	public class CCDIK : IK {
		
		/// <summary>
		/// The CCD %IK solver.
		/// </summary>
		public IKSolverCCD solver = new IKSolverCCD();
		
		public override IKSolver GetIKSolver() {
			return solver as IKSolver;
		}
	}
}
