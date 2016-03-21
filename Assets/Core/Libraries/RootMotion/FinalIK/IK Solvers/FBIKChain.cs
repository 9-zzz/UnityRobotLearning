using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/// <summary>
	/// A chain of bones in IKSolverFullBody.
	/// </summary>
	[System.Serializable]
	public class FBIKChain {
		
		#region Main Interface
		
		/// <summary>
		/// Linear constraint between child chains of a FBIKChain.
		/// </summary>
		[System.Serializable]
		public class ChildConstraint {
			
			/// <summary>
			/// The first bone.
			/// </summary>
			public Transform bone1;
			/// <summary>
			/// The second bone.
			/// </summary>
			public Transform bone2;
			/// <summary>
			/// The push elasticity.
			/// </summary>
			public float pushElasticity = 0f;
			/// <summary>
			/// The pull elasticity.
			/// </summary>
			public float pullElasticity = 0f;

			/// <summary>
			/// Gets the nominal (animated) distance between the two bones.
			/// </summary>
			public float nominalDistance { get; private set; }
			/// <summary>
			/// The constraint is rigid if both push and pull elasticity are 0.
			/// </summary>
			public bool isRigid { get { return pushElasticity <= 0 && pullElasticity <= 0; }}

			[NonSerializedAttribute] private IKSolver.Node node1;
			[NonSerializedAttribute] private IKSolver.Node node2;
			[NonSerializedAttribute] private FBIKChain chain1;
			[NonSerializedAttribute] private FBIKChain chain2;
			
			/*
			 * Constructor
			 * */
			public ChildConstraint(Transform bone1, Transform bone2, float pushElasticity = 0f, float pullElasticity = 0f) {
				this.bone1 = bone1;
				this.bone2 = bone2;
				this.pushElasticity = pushElasticity;
				this.pullElasticity = pullElasticity;
			}
			
			/*
			 * Initiating the constraint
			 * */
			public void Initiate(IKSolverFullBody solver) {
				chain1 = solver.GetChain(bone1);
				chain2 = solver.GetChain(bone2);
				
				node1 = chain1.nodes[0];
				node2 = chain2.nodes[0];
				
				OnPreSolve();
			}

			/*
			 * Cross-fading pull weights
			 * */
			public float GetCrossFade() {
				float offset = chain1.pull - chain2.pull;
				return 0.5f + (offset * 0.5f);
			}

			
			/*
			 * Updating nominal distance because it might have changed in the animation
			 * */
			public void OnPreSolve() {
				nominalDistance = Vector3.Distance(node1.transform.position, node2.transform.position);
			}
			
			/*
			 * Solving the constraint
			 * */
			public void Solve(float pull) {
				if (pushElasticity >= 1 && pullElasticity >= 1) return;
				
				float distance = Vector3.Distance(node1.solverPosition, node2.solverPosition);
				
				float elasticity = distance > nominalDistance? pullElasticity: pushElasticity;
				
				float force = 1f - Mathf.Clamp(elasticity, 0f, 1f);
				
				force *= 1f - nominalDistance / distance;
				if (force == 0) return;
				
				Vector3 offset = (node2.solverPosition - node1.solverPosition) * force;
				
				node1.solverPosition += offset * pull;
				node2.solverPosition -= offset * (1f - pull);
			}
		}

		[System.Serializable]
		public enum ReachSmoothing {
			None,
			Exponential,
			Cubic
		}

		/// <summary>
		/// The pin weight. If closer to 1, the chain will be less influenced by child chains.
		/// </summary>
		public float pin;
		/// <summary>
		/// The weight of pulling the parent chain.
		/// </summary>
		public float pull = 1f;
		/// <summary>
		/// Only used in 3 segmented chains, pulls the first node closer to the third node.
		/// </summary>
		public float reach;
		/// <summary>
		/// Smoothing the effect of the reach with the expense of some accuracy.
		/// </summary>
		public ReachSmoothing reachSmoothing;
		/// <summary>
		/// The nodes in this chain.
		/// </summary>
		public IKSolver.Node[] nodes = new IKSolver.Node[0];
		/// <summary>
		/// The child chains.
		/// </summary>
		public int[] children = new int[0];
		/// <summary>
		/// The child constraints are used for example for fixing the distance between left upper arm and right upper arm
		/// </summary>
		public ChildConstraint[] childConstraints = new ChildConstraint[0];
		
		#endregion Main Interface
		
		private float rootLength;
		private bool initiated;
		private IKSolver.Point p;

		public FBIKChain() {}
		
		public FBIKChain (float pin, float pull, params Transform[] nodeTransforms) {
			this.pin = pin;
			this.pull = pull;
			
			SetNodes(nodeTransforms);
			
			children = new int[0];
		}
		
		/*
		 * Set nodes to the following bone transforms.
		 * */
		public void SetNodes(params Transform[] boneTransforms) {
			nodes = new IKSolver.Node[boneTransforms.Length];
			for (int i = 0; i < boneTransforms.Length; i++) {
				nodes[i] = new IKSolver.Node(boneTransforms[i]);
			}
		}

		/*
		 * Check if this chain is valid or not.
		 * */
		public bool IsValid(Warning.Logger logger = null) {
			if (nodes.Length == 0) {
				if (logger != null) logger("FBIK chain contains no nodes.");
				return false;
			}
			
			foreach (IKSolver.Node node in nodes) if (node.transform == null) {
				if (logger != null) logger("Node transform is null in FBIK chain.");
				return false;
			}
			
			return true;
		}
		
		/*
		 * Initiating the chain.
		 * */
		public void Initiate(IKSolver solver, FBIKChain[] chain) {
			initiated = false;
			
			foreach (IKSolver.Node node in nodes) {
				node.solverPosition = node.transform.position;
			}
			
			// Calculating bone lengths
			for (int i = 0; i < nodes.Length - 1; i++) {
				nodes[i].length = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position);
				if (nodes[i].length == 0) return;
			}
			
			for (int i = 0; i < children.Length; i++) {
				chain[children[i]].rootLength = (chain[children[i]].nodes[0].transform.position - nodes[nodes.Length - 1].transform.position).magnitude;
				if (chain[children[i]].rootLength == 0f) return;
			}
			
			// Initiating child constraints
			InitiateConstraints(solver);
			
			initiated = true;
		}

		/*
		 * Before updating the chain
		 * */
		public void ReadPose(FBIKChain[] chain) {
			if (!initiated) return;
			
			for (int i = 0; i < nodes.Length; i++) {
				nodes[i].solverPosition = nodes[i].transform.position + nodes[i].offset;
			}
			
			// Calculating bone lengths
			for (int i = 0; i < nodes.Length - 1; i++) {
				nodes[i].length = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position);
			}
			
			for (int i = 0; i < children.Length; i++) {
				chain[children[i]].rootLength = (chain[children[i]].nodes[0].transform.position - nodes[nodes.Length - 1].transform.position).magnitude;
			}
			
			// Pre-update child constraints
			PreSolveConstraints();
		}
		
		/*
		 * Initiating child constraints
		 * */
		public void InitiateConstraints(IKSolver solver) {
			foreach (ChildConstraint c in childConstraints) c.Initiate(solver as IKSolverFullBody);
		}
		
		/*
		 * Pre-update child constraints
		 * */
		private void PreSolveConstraints() {
			for (int i = 0; i < childConstraints.Length; i++) childConstraints[i].OnPreSolve();
		}

		#region Recursive Methods
		
		/*
		 * Reaching limbs
		 * */
		public void Reach(int iteration, FBIKChain[] chain) {
			if (!initiated) return;

			// Solve children first
			for (int i = 0; i < children.Length; i++) chain[children[i]].Reach(iteration, chain);

			if (nodes.Length != 3) return;

			float r = reach * Mathf.Clamp(nodes[2].effectorPositionWeight, 0f, 1f);

			if (r > 0) {

				float limbLength = nodes[0].length + nodes[1].length;

				Vector3 limbDirection = nodes[2].solverPosition - nodes[0].solverPosition;
				if (limbDirection == Vector3.zero) return;
				
				float currentLength = limbDirection.magnitude;

				//Reaching
				Vector3 straight = (limbDirection / currentLength) * limbLength;
				
				float delta = currentLength / limbLength;
				delta = Mathf.Clamp(delta, 1 - r, 1 + r);
				delta -= 1f;
				delta = Mathf.Clamp(delta + r, -1f, 1f);

				// Smoothing the effect of Reach with the expense of some accuracy
				switch (reachSmoothing) {
				case ReachSmoothing.Exponential:
					delta *= delta;
					break;
				case ReachSmoothing.Cubic:
					delta *= delta * delta;
					break;
				}
				
				Vector3 offset = straight * Mathf.Clamp(delta, 0f, currentLength);

				nodes[0].solverPosition += offset * (1f - nodes[0].effectorPositionWeight);
				nodes[2].solverPosition += offset;
			}
		}

		/*
		 * Applying trigonometric IK solver on the 3 segmented chains to relieve tension from the solver and increase accuracy.
		 * */
		public void SolveTrigonometric(FBIKChain[] chain) {
			if (!initiated) return;
			
			// Solve children first
			for (int i = 0; i < children.Length; i++) chain[children[i]].SolveTrigonometric(chain);
			
			if (nodes.Length != 3) return;
			
			float limbLength = nodes[0].length + nodes[1].length;

			// Trigonometry
			Vector3 limbDirection = nodes[2].solverPosition - nodes[0].solverPosition;
			if (limbDirection == Vector3.zero) return;

			float limbMag = limbDirection.magnitude;

			float maxMag = Mathf.Clamp(limbMag, 0f, limbLength * 0.999f);
			Vector3 direction = (limbDirection / limbMag) * maxMag;

			Vector3 bendDirection = GetBendDirection(direction, maxMag, nodes[0], nodes[1]);
			
			nodes[1].solverPosition = nodes[0].solverPosition + bendDirection;
		}
		
		/*
		 * Stage 1 of the FABRIK algorithm
		 * */
		public void Stage1(FBIKChain[] chain) {
			// Stage 1
			for (int i = 0; i < children.Length; i++) chain[children[i]].Stage1(chain);
			
			// If is the last chain in this hierarchy, solve immediatelly and return
			if (children.Length == 0) {
				ForwardReach(nodes[nodes.Length - 1].solverPosition);
				return;
			}
			
			// Finding the total pull force by all child chains
			float pullParentSum = 0f;
			for (int i = 0; i < children.Length; i++) pullParentSum += chain[children[i]].pull;
			Vector3 centroid = nodes[nodes.Length - 1].solverPosition;
			
			// Satisfying child constraints
			SolveChildConstraints();

			// Finding the centroid position of all child chains according to their individual pull weights
			for (int i = 0; i < children.Length; i++) {
				Vector3 childPosition = chain[children[i]].nodes[0].solverPosition;

				if (chain[children[i]].rootLength > 0) {
					childPosition = IKSolverFABRIK.SolveJoint(nodes[nodes.Length - 1].solverPosition, chain[children[i]].nodes[0].solverPosition, chain[children[i]].rootLength);
				}
					
				if (pullParentSum > 0) centroid += (childPosition - nodes[nodes.Length - 1].solverPosition) * (chain[children[i]].pull / Mathf.Clamp(pullParentSum, 1f, Mathf.Infinity));
			}
			
			// Forward reach to the centroid (unless pinned)
			ForwardReach(Vector3.Lerp(centroid, nodes[nodes.Length - 1].solverPosition, pin));
		}
		
		/*
		 * Stage 2 of the FABRIK algorithm.
		 * */
		public void Stage2(Vector3 position, int iterations, FBIKChain[] chain) {
			// Stage 2
			BackwardReach(position);

			// Iterating child constraints and child chains to make sure they are not conflicting
			for (int i = 0; i < Mathf.Min(iterations, 4); i++) SolveConstraintSystems(chain);

			// Stage 2 for the children
			for (int i = 0; i < children.Length; i++) chain[children[i]].Stage2(nodes[nodes.Length - 1].solverPosition, iterations, chain);
		}

		/*
		 * Iterating child constraints and child chains to make sure they are not conflicting
		 * */
		public void SolveConstraintSystems(FBIKChain[] chain) {
			if (childConstraints.Length == 0) return;
			
			// Satisfy child constraints
			SolveChildConstraints();
			
			float pullSum = nodes[nodes.Length - 1].effectorPositionWeight;
			
			for (int i = 0; i < children.Length; i++) pullSum += chain[children[i]].nodes[0].effectorPositionWeight * chain[children[i]].pull;
			
			for (int i = 0; i < children.Length; i++) {
				float crossFade = ((chain[children[i]].nodes[0].effectorPositionWeight * chain[children[i]].pull) / Mathf.Clamp(pullSum, 1f, Mathf.Infinity));
				
				SolveLinearConstraint(nodes[nodes.Length - 1], chain[children[i]].nodes[0], crossFade, chain[children[i]].rootLength);
			}
		}

		#endregion Recursive Methods
		
		/*
		 * Calculates the bend direction based on the law of cosines (from IKSolverTrigonometric). 
		 * */
		protected Vector3 GetBendDirection(Vector3 direction, float directionMagnitude, IKSolver.Node node1, IKSolver.Node node2) {
			float sqrMag1 = node1.length * node1.length;
			float sqrMag2 = node2.length * node2.length;
			
			float x = ((directionMagnitude * directionMagnitude) + sqrMag1 - sqrMag2) / 2f / directionMagnitude;
			float y = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - x * x, 0, Mathf.Infinity));

			return Quaternion.LookRotation(direction) * new Vector3(0f, y, x);
		}
		
	
		/*
		 * Satisfying child constraints
		 * */
		private void SolveChildConstraints() {
			for (int i = 0; i < childConstraints.Length; i++) {
				float crossFade = childConstraints[i].isRigid? childConstraints[i].GetCrossFade(): 0.5f;
				childConstraints[i].Solve(1f - crossFade);	
			}
		}

		/*
		 * Solve simple linear constraint
		 * */
		private static void SolveLinearConstraint(IKSolver.Node node1, IKSolver.Node node2, float crossFade, float distance) {
			float currentDistance = Vector3.Distance(node1.solverPosition, node2.solverPosition);
			
			float force = 1f - distance / currentDistance;
			if (force == 0) return;
			
			Vector3 offset = (node2.solverPosition - node1.solverPosition) * force;
			
			node1.solverPosition += offset * crossFade;
			node2.solverPosition -= offset * (1f - crossFade);
		}
		
		/*
		 * FABRIK Forward reach
		 * */
		public void ForwardReach(Vector3 position) {
			// Lerp last node's solverPosition to position
			nodes[nodes.Length - 1].solverPosition = position;
			
			for (int i = nodes.Length - 2; i > -1; i--) {
				// Finding joint positions
				nodes[i].solverPosition = IKSolverFABRIK.SolveJoint(nodes[i].solverPosition, nodes[i + 1].solverPosition, nodes[i].length);
			}
		}
		
		/*
		 * FABRIK Backward reach
		 * */
		private void BackwardReach(Vector3 position) {
			// Solve forst node only if it already hasn't been solved in SolveConstraintSystems
			if (rootLength > 0) position = IKSolverFABRIK.SolveJoint(nodes[0].solverPosition, position, rootLength);
			nodes[0].solverPosition = position;

			// Finding joint positions
			for (int i = 1; i < nodes.Length; i++) {
				nodes[i].solverPosition = IKSolverFABRIK.SolveJoint(nodes[i].solverPosition, nodes[i - 1].solverPosition, nodes[i - 1].length);
			}
		}
	}
}