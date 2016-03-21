using UnityEngine;
using System.Collections;
using System;

	namespace RootMotion.FinalIK {

	/// <summary>
	/// Forward and Backward Reaching Inverse Kinematics solver.
	/// 
	/// This class is based on the "FABRIK: A fast, iterative solver for the inverse kinematics problem." paper by Aristidou, A., Lasenby, J.
	/// </summary>
	[System.Serializable]
	public class IKSolverFABRIK : IKSolverHeuristic {
		
		#region Main Interface
		
		/// <summary>
		/// Locks bone lengths to initial values. If false, bones can't be scaled or repositioned.
		/// </summary>
		public bool updateBoneLengths = false;
		
		/// <summary>
		/// Solving stage 1 of the %FABRIK algorithm.
		/// </summary>
		public void SolveForward(Vector3 position) {
			if (!initiated) {
				if (!Warning.logged) LogWarning("Trying to solve uninitiated FABRIK chain.");
				return;
			}
			
			OnPreSolve();
			
			ForwardReach(position);
		}
		
		/// <summary>
		/// Solving stage 2 of the %FABRIK algorithm.
		/// </summary>
		public void SolveBackward(Vector3 position) {
			if (!initiated) {
				if (!Warning.logged) LogWarning("Trying to solve uninitiated FABRIK chain.");
				return;
			}
			
			BackwardReach(position);
			
			OnPostSolve();
		}

		/// <summary>
		/// Interpolates the joint position to match the bone's length
		/// </summary>
		public static Vector3 SolveJoint(Vector3 pos1, Vector3 pos2, float length) {
			float d = length / (pos1 - pos2).magnitude;
			return (1 - d) * pos2 + d * pos1;
		}

		#endregion Main Interface

		private bool[] limitedBones = new bool[0];
		
		protected override void OnInitiate() {
			if (firstInitiation || !Application.isPlaying) IKPosition = bones[bones.Length - 1].transform.position;
			
			foreach (IKSolver.Bone bone in bones) bone.solverPosition = bone.transform.position;
			
			limitedBones = new bool[bones.Length];
			
			InitiateBones();
		}
		
		protected override void OnUpdate() {
			if (IKPositionWeight <= 0) return;
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			
			OnPreSolve();
			
			Vector3 singularityOffset = maxIterations > 1? GetSingularityOffset(): Vector3.zero;
			
			// Iterating the solver
			for (int i = 0; i < maxIterations; i++) {
				
				// Optimizations
				if (singularityOffset == Vector3.zero && i >= 1 && tolerance > 0 && positionOffset < tolerance * tolerance) break;
				lastLocalDirection = localDirection;
				
				Solve(IKPosition + (i == 0? singularityOffset: Vector3.zero));
			}
			
			OnPostSolve();
		}
		
		/*
		 * If true, the solver will work with 0 length bones
		 * */
		protected override bool boneLengthCanBeZero { get { return false; }} // Returning false here also ensures that the bone lengths will be calculated
		
		/*
		 * Check if bones have moved from last solved positions
		 * */
		private void OnPreSolve() {
			for (int i = 0; i < bones.Length; i++) {
				bones[i].solverPosition = bones[i].transform.position;
				
				if (updateBoneLengths) {
					chainLength = 0;
					
					if (i < bones.Length - 1) {
						bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
						chainLength += bones[i].length;
					}

					for (int b = 0; b < bones.Length; b++) bones[b].defaultLocalPosition = bones[b].transform.localPosition;
				} else {
					if (i > 0) bones[i].transform.localPosition = bones[i].defaultLocalPosition;
				}
			}
		}
		
		/*
		 * After solving the chain
		 * */
		private void OnPostSolve() {
			// Rotating bones to match the solver positions
			if (!useRotationLimits) MapToSolverPositions();
			
			lastLocalDirection = localDirection;
		}
		
		private void Solve(Vector3 targetPosition) {
			Vector3 firstPosition = bones[0].transform.position;
			
			// Forward reaching
			ForwardReach(targetPosition);
			
			// Backward reaching
			BackwardReach(firstPosition);
		}
		
		/*
		 * Stage 1 of FABRIK algorithm
		 * */
		private void ForwardReach(Vector3 position) {
			// Lerp last bone's solverPosition to position
			bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, position, IKPositionWeight);
			
			for (int i = 0; i < limitedBones.Length; i++) limitedBones[i] = false;
			
			for (int i = bones.Length - 2; i > -1; i--) {
				// Finding joint positions
				bones[i].solverPosition = SolveJoint(bones[i].solverPosition, bones[i + 1].solverPosition, bones[i].length);
				
				// Limiting bone rotation forward
				LimitForward(i + 1, i);
			}
			
			// Limiting the first bone's rotation
			LimitForward(0, 0);
		}
		
		/*
		 * Applying rotation limit to a bone in stage 1 in a more stable way
		 * */
		private void LimitForward(int limitBone, int rotateBone) {
			if (!useRotationLimits) return;
			if (bones[limitBone].rotationLimit == null) return;
			
			// Moving and rotating this bone and all its children to their solver positions
			for (int b = rotateBone; b < bones.Length; b++) {
				if (limitedBones[b]) break;
				
				bones[b].transform.position = bones[b].solverPosition;
				if (b < bones.Length - 1) bones[b].Swing(bones[b + 1].solverPosition);
			}
					
			// Storing last bone's position before applying the limit
			Vector3 lastPosition = bones[bones.Length - 1].transform.position;
			Vector3 toLastPosition = lastPosition - bones[rotateBone].transform.position;
					
			// Applying rotation limit
			if (bones[limitBone].rotationLimit.Apply()) {	
				// Rotating and positioning the hierarchy so that the last bone's position is maintained
				if (rotateBone < bones.Length - 2) {
					// Rotating to compensate for the limit
					Vector3 toLastPositionLimited = bones[bones.Length - 1].transform.position - bones[rotateBone].transform.position;
					Quaternion fromTo = Quaternion.FromToRotation(toLastPositionLimited, toLastPosition);
							
					bones[rotateBone].transform.rotation = fromTo * bones[rotateBone].transform.rotation;
							
					// Moving the bone so that last bone maintains it's initial position
					bones[rotateBone].transform.position += lastPosition - bones[bones.Length - 1].transform.position;
				}
			}
			
			// Move solver positions to bone positions
			for (int b = rotateBone; b < bones.Length; b++) bones[b].solverPosition = bones[b].transform.position;
			
			limitedBones[limitBone] = true;
		}
		
		/*
		 * Stage 2 of FABRIK algorithm
		 * */
		private void BackwardReach(Vector3 position) {
			if (useRotationLimits) BackwardReachLimited(position);
			else BackwardReachUnlimited(position);
		}
		
		/*
		 * Stage 2 of FABRIK algorithm without rotation limits
		 * */
		private void BackwardReachUnlimited(Vector3 position) {
			// Move first bone to position
			bones[0].solverPosition = position;
			
			// Finding joint positions
			for (int i = 1; i < bones.Length; i++) {
				bones[i].solverPosition = SolveJoint(bones[i].solverPosition, bones[i - 1].solverPosition, bones[i - 1].length);
			}
		}
		
		/*
		 * Stage 2 of FABRIK algorithm with limited rotations
		 * */
		private void BackwardReachLimited(Vector3 position) {
			// Move first bone to position
			bones[0].transform.position = position;
			
			// Applying rotation limits bone by bone
			for (int i = 0; i < bones.Length - 1; i++) {
				// Rotating bone to look at the solved joint position
				bones[i].Swing(SolveJoint(bones[i + 1].solverPosition, bones[i].transform.position, bones[i].length));
				
				// Applying rotation limit
				if (bones[i].rotationLimit != null) bones[i].rotationLimit.Apply();
				
				// Positioning the next bone to its default local position
				bones[i + 1].transform.localPosition = bones[i + 1].defaultLocalPosition;
			}
			
			// Matching solver positions to bone positions
			for (int i = 0; i < bones.Length; i++) bones[i].solverPosition = bones[i].transform.position;
		}
		
		/*
		 * Rotate bones to match the solver positions
		 * */
		private void MapToSolverPositions() {
			bones[0].transform.position = bones[0].solverPosition;
			
			for (int i = 0; i < bones.Length - 1; i++) {
				if (i > 0) bones[i].transform.localPosition = bones[i].defaultLocalPosition;
				
				bones[i].Swing(bones[i + 1].solverPosition);
			}
			
			if (bones.Length > 1) bones[bones.Length - 1].transform.localPosition = bones[bones.Length - 1].defaultLocalPosition;
		}
	}
}
