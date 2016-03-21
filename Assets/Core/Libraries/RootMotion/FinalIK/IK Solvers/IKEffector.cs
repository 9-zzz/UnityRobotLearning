using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	///  Effector for manipulating node based %IK solvers.
	/// </summary>
	[System.Serializable]
	public class IKEffector {
		
		#region Main Interface

		/// <summary>
		/// Gets the main node.
		/// </summary>
		public IKSolver.Node GetNode() {
			return node;
		}
		
		/// <summary>
		/// The node transform used by this effector.
		/// </summary>
		public Transform bone;
		/// <summary>
		/// The position weight.
		/// </summary>
		public float positionWeight;
		/// <summary>
		/// The rotation weight.
		/// </summary>
		public float rotationWeight;
		/// <summary>
		/// The effector position in world space.
		/// </summary>
		public Vector3 position = Vector3.zero;
		/// <summary>
		/// The effector rotation relative to default rotation in world space.
		/// </summary>
		public Quaternion rotation = Quaternion.identity;
		/// <summary>
		/// The position offset in world space. positionOffset will be reset to Vector3.zero each frame after the solver is complete.
		/// </summary>
		public Vector3 positionOffset;
		/// <summary>
		/// Is this the last effector of a node chain?
		/// </summary>
		public bool isEndEffector { get { return planeBone1 != null; }}
		/// <summary>
		/// If false, child nodes will be ignored by this effector (if it has any).
		/// </summary>
		public bool effectChildNodes = true;
		/// <summary>
		/// Keeps the node position relative to the triangle defined by the plane bones (applies only to end-effectors).
		/// </summary>
		public float maintainRelativePositionWeight;

		/// <summary>
		/// Pins the effector to the animated position of it's bone.
		/// </summary>
		public void PinToBone(float positionWeight, float rotationWeight) {
			position = bone.position;
			this.positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);

			rotation = bone.rotation;
			this.rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
		}

		#endregion Main Interface

		public Transform[] childBones = new Transform[0]; // The optional list of other bones that positionOffset and position of this effector will be applied to.
		public Transform planeBone1; // The first bone defining the parent plane.
		public Transform planeBone2; // The second bone defining the parent plane.
		public Transform planeBone3; // The third bone defining the parent plane.
		public Quaternion planeRotationOffset = Quaternion.identity; // Used by Bend Constraints

		private IKSolver.Node node = new IKSolver.Node(), planeNode1 = new IKSolver.Node(), planeNode2 = new IKSolver.Node(), planeNode3 = new IKSolver.Node();
		private IKSolver.Node[] childNodes = new IKSolver.Node[0];
		private IKSolver solver;
		private float posW, rotW;
		private Vector3[] localPositions = new Vector3[0];
		private bool usePlaneNodes;
		private Quaternion animatedPlaneRotation = Quaternion.identity;
		
		public IKEffector() {}
		
		public IKEffector (Transform bone, Transform[] childBones) {
			this.bone = bone;
			this.childBones = childBones;
		}
		
		/*
		 * Determines whether this IKEffector is valid or not.
		 * */
		public bool IsValid(IKSolver solver, Warning.Logger logger) {
			if (bone == null) {
				if (logger != null) logger("IK Effector bone is null.");
				return false;
			}
			
			if (solver.GetPoint(bone) == null) {
				if (logger != null) logger("IK Effector is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.");
				return false;
			}

			foreach (Transform b in childBones) {
				if (b == null) {
					if (logger != null) logger("IK Effector contains a null reference.");
					return false;
				}
			}
			
			foreach (Transform b in childBones) {
				if (solver.GetPoint(b) == null) {
					if (logger != null) logger("IK Effector is referencing to a bone '" + b.name + "' that does not excist in the Node Chain.");
					return false;
				}
			}

			if (planeBone1 != null && solver.GetPoint(planeBone1) == null) {
				if (logger != null) logger("IK Effector is referencing to a bone '" + planeBone1.name + "' that does not excist in the Node Chain.");
				return false;
			}

			if (planeBone2 != null && solver.GetPoint(planeBone2) == null) {
				if (logger != null) logger("IK Effector is referencing to a bone '" + planeBone2.name + "' that does not excist in the Node Chain.");
				return false;
			}

			if (planeBone3 != null && solver.GetPoint(planeBone3) == null) {
				if (logger != null) logger("IK Effector is referencing to a bone '" + planeBone3.name + "' that does not excist in the Node Chain.");
				return false;
			}

			return true;
		}
		
		/*
		 * Initiate the effector, set default values
		 * */
		public void Initiate(IKSolver solver) {
			this.solver = solver;
			position = bone.position;
			rotation = bone.rotation;

			// Getting the node
			node = solver.GetPoint(bone) as IKSolver.Node;

			// Child nodes
			if (childNodes.Length != childBones.Length) childNodes = new IKSolver.Node[childBones.Length];

			for (int i = 0; i < childBones.Length; i++) {
				childNodes[i] = solver.GetPoint(childBones[i]) as IKSolver.Node;
			}

			if (localPositions.Length != childBones.Length) localPositions = new Vector3[childBones.Length];

			// Plane nodes
			usePlaneNodes = false;

			if (planeBone1 != null) {
				planeNode1 = solver.GetPoint(planeBone1) as IKSolver.Node;

				if (planeBone2 != null) {
					planeNode2 = solver.GetPoint(planeBone2) as IKSolver.Node;

					if (planeBone3 != null) {
						planeNode3 = solver.GetPoint(planeBone3) as IKSolver.Node;
						usePlaneNodes = true;
					}
				}
			}
		}
		
		/*
		 * Clear node offset
		 * */
		public void ResetOffset() {
			node.offset = Vector3.zero;
			for (int i = 0; i < childNodes.Length; i++) childNodes[i].offset = Vector3.zero;
		}
		
		/*
		 * Presolving, applying offset
		 * */
		public void OnPreSolve() {
			positionWeight = Mathf.Clamp(positionWeight, 0f, 1f);
			rotationWeight = Mathf.Clamp(rotationWeight, 0f, 1f);
			maintainRelativePositionWeight = Mathf.Clamp(maintainRelativePositionWeight, 0f, 1f);

			// Calculating weights
			posW = positionWeight * solver.GetIKPositionWeight() * solver.GetIKPositionWeight();
			rotW = rotationWeight * solver.GetIKPositionWeight();

			node.effectorPositionWeight = posW;
			node.effectorRotationWeight = rotW;

			node.solverRotation = rotation;

			if (float.IsInfinity(positionOffset.x) ||
				float.IsInfinity(positionOffset.y) ||
				float.IsInfinity(positionOffset.z)
			    ) Debug.LogError("Invalid IKEffector.positionOffset (contains Infinity)! Please make sure not to set IKEffector.positionOffset to infinite values.", bone);

			if (float.IsNaN(positionOffset.x) ||
			    float.IsNaN(positionOffset.y) ||
			    float.IsNaN(positionOffset.z)
			    ) Debug.LogError("Invalid IKEffector.positionOffset (contains NaN)! Please make sure not to set IKEffector.positionOffset to NaN values.", bone);

			if (positionOffset.sqrMagnitude > 10000000000f) Debug.LogError("Additive effector positionOffset detected in Full Body IK (extremely large value). Make sure you are not circularily adding to effector positionOffset each frame.", bone);

			node.offset += positionOffset;

			if (effectChildNodes) {
				for (int i = 0; i < childNodes.Length; i++) {
				localPositions[i] = childNodes[i].transform.position - node.transform.position;
					childNodes[i].offset += positionOffset;
				}
			}

			// Relative to Plane
			if (usePlaneNodes && maintainRelativePositionWeight > 0f) {
				animatedPlaneRotation = Quaternion.LookRotation(planeNode2.transform.position - planeNode1.transform.position, planeNode3.transform.position - planeNode1.transform.position);;
			}
		}

		/*
		 * Called after writing the pose
		 * */
		public void OnPostWrite() {
			positionOffset = Vector3.zero;
		}

		/*
		* Rotation of plane nodes in the solver
		* */
		private Quaternion planeRotation {
			get {
				Vector3 viewingVector = planeNode2.solverPosition - planeNode1.solverPosition;
				Vector3 upVector = planeNode3.solverPosition - planeNode1.solverPosition;

				return Quaternion.LookRotation(viewingVector, upVector);
			}
		}

		/*
		 * Manipulating node solverPosition
		 * */
		public void Update() {
			if (float.IsInfinity(position.x) ||
			    float.IsInfinity(position.y) ||
			    float.IsInfinity(position.z)
			    ) Debug.LogError("Invalid IKEffector.position (contains Infinity)!");

			node.solverPosition = Vector3.Lerp(GetPosition(out planeRotationOffset), position, posW);

			// Child nodes
			if (!effectChildNodes) return;
			
			for (int i = 0; i < childNodes.Length; i++) {
				childNodes[i].solverPosition = Vector3.Lerp(childNodes[i].solverPosition, node.solverPosition + localPositions[i], posW);
			}
		}

		/*
		 * Gets the starting position of the iteration
		 * */
		private Vector3 GetPosition(out Quaternion planeRotationOffset) {
			planeRotationOffset = Quaternion.identity;
			if (!isEndEffector) return node.solverPosition; // non end-effectors are always free

			Vector3 animated = node.transform.position + node.offset;

			if (maintainRelativePositionWeight <= 0f) return animated;

			// Maintain relative position
			Vector3 p = node.transform.position;
			Vector3 dir = p - planeNode1.transform.position;
				
			planeRotationOffset = planeRotation * Quaternion.Inverse(animatedPlaneRotation);

			p = planeNode1.solverPosition + planeRotationOffset * dir;
			planeRotationOffset = Quaternion.Lerp(planeRotationOffset, Quaternion.identity, posW);
			planeRotationOffset = Quaternion.Lerp(planeRotationOffset, Quaternion.identity, 1f - maintainRelativePositionWeight);

			return Vector3.Lerp(animated, p + node.offset, maintainRelativePositionWeight);
		}
	}
}