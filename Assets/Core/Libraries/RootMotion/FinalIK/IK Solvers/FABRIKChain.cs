using UnityEngine;
using System.Collections;

	namespace RootMotion.FinalIK {
		
	/// <summary>
	/// Branch of FABRIK components in the FABRIKRoot hierarchy.
	/// </summary>
	[System.Serializable]
	public class FABRIKChain {
		
		#region Main Interface
		
		/// <summary>
		/// The FABRIK component.
		/// </summary>
		public FABRIK ik;
		/// <summary>
		/// Parent pull weight.
		/// </summary>
		public float pull = 1f;
		/// <summary>
		/// Resistance to being pulled by child chains.
		/// </summary>
		public float pin = 1f;
		/// <summary>
		/// The child chain indexes.
		/// </summary>
		public int[] children = new int[0];
		
		/// <summary>
		/// Checks whether this FABRIKChain is valid.
		/// </summary>
		public bool IsValid(Warning.Logger logger) {
			if (ik == null) {
				if (logger != null) logger("IK unassigned in FABRIKChain.");
				return false;
			}
			
			if (!ik.solver.IsValid(true)) return false;

			return true;
		}

		#endregion Main Interface
		
		private Vector3 position;
		
		/*
		 * Initiate the chain
		 * */
		public void Initiate() {
			ik.Disable();
			
			position = ik.solver.bones[ik.solver.bones.Length - 1].transform.position;
		}
	
		/*
		 * Solving stage 1 of the FABRIK algorithm from end effectors towards the root.
		 * */
		public void Stage1(FABRIKChain[] chain) {
			// Solving children first
			for (int i = 0; i < children.Length; i++) chain[children[i]].Stage1(chain);
			
			// The last chains
			if (children.Length == 0) {
				ik.solver.SolveForward(ik.solver.GetIKPosition());
				return;
			}
			
			// Finding the centroid of child root solver positions
			position = ik.solver.GetIKPosition();
			Vector3 centroid = position;
			
			float pullSum = 0f;
			for (int i = 0; i < children.Length; i++) pullSum += chain[children[i]].pull;
			
			for (int i = 0; i < children.Length; i++) {
				if (chain[children[i]].children.Length == 0) chain[children[i]].ik.solver.SolveForward(chain[children[i]].ik.solver.GetIKPosition());
				
				if (pullSum > 0) centroid += (chain[children[i]].ik.solver.bones[0].solverPosition - position) * (chain[children[i]].pull / Mathf.Clamp(pullSum, 1f, pullSum));
			}
			
			// Solve this chain forward
			ik.solver.SolveForward(Vector3.Lerp(centroid, position, pin));
		}
		
		/*
		 * Solving stage 2 of the FABRIK algoright from the root to the end effectors.
		 * */
		public void Stage2(Vector3 rootPosition, FABRIKChain[] chain) {
			// Solve this chain backwards
			ik.solver.SolveBackward(rootPosition);
			
			// Solve child chains
			for (int i = 0; i < children.Length; i++) {
				chain[children[i]].Stage2(ik.solver.bones[ik.solver.bones.Length - 1].transform.position, chain);
			}
		}
	}
}
