using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Maps a 3-segmented bone hierarchy to a node chain of an %IK Solver
	/// </summary>
	[System.Serializable]
	public class IKMappingLimb: IKMapping {
		
		#region Main Interface

		/// <summary>
		/// Limb Bone Map type
		/// </summary>
		[System.Serializable]
		public enum BoneMapType {
			Parent,
			Bone1,
			Bone2,
			Bone3
		}

		/// <summary>
		/// The optional parent bone (clavicle).
		/// </summary>
		public Transform parentBone;
		/// <summary>
		/// The first bone (upper arm or thigh).
		/// </summary>
		public Transform bone1;
		/// <summary>
		/// The second bone (forearm or calf).
		/// </summary>
		public Transform bone2;
		/// <summary>
		/// The third bone (hand or foot).
		/// </summary>
		public Transform bone3;
		/// <summary>
		/// The weight of maintaining the third bone's rotation as it was in the animation
		/// </summary>
		public float maintainRotationWeight;
		/// <summary>
		/// The slerp weight of rotating the limb to it's IK pose. This can be useful if you want to disable the effect of IK for the limb or move the hand to the target in a sperical trajectory instead of linear.
		/// </summary>
		public float weight = 1f; // Added in 0.2
		
		/// <summary>
		/// Determines whether this IKMappingLimb is valid
		/// </summary>
		public override bool IsValid(IKSolver solver, Warning.Logger logger = null) {
			if (!base.IsValid(solver, logger)) return false;
			
			if (!BoneIsValid(bone1, solver, logger)) return false;
			if (!BoneIsValid(bone2, solver, logger)) return false;
			if (!BoneIsValid(bone3, solver, logger)) return false;
			
			return true;
		}

		/// <summary>
		/// Gets the bone map of the specified bone.
		/// </summary>
		public BoneMap GetBoneMap(BoneMapType boneMap) {
			switch(boneMap) {
			case BoneMapType.Parent:
				if (parentBone == null) Warning.Log("This limb does not have a parent (shoulder) bone", bone1);
				return boneMapParent;
			case BoneMapType.Bone1: return boneMap1;
			case BoneMapType.Bone2: return boneMap2;
			default: return boneMap3;
			}
		}

		/// <summary>
		/// Makes the limb mapped to the specific local directions of the bones. Added in 0.3
		/// </summary>
		public void SetLimbOrientation(Vector3 upper, Vector3 lower) {
			boneMap1.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone1.rotation) * Quaternion.LookRotation(bone2.position - bone1.position, bone1.rotation * -upper));
			boneMap2.defaultLocalTargetRotation = Quaternion.Inverse(Quaternion.Inverse(bone2.rotation) * Quaternion.LookRotation(bone3.position - bone2.position, bone2.rotation * -lower));
		}
		
		#endregion Main Interface
		
		private BoneMap boneMapParent = new BoneMap(), boneMap1 = new BoneMap(), boneMap2 = new BoneMap(), boneMap3 = new BoneMap();
		
		public IKMappingLimb() {}
		
		public IKMappingLimb(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null) {
			SetBones(bone1, bone2, bone3, parentBone);
		}
		
		public void SetBones(Transform bone1, Transform bone2, Transform bone3, Transform parentBone = null) {
			this.bone1 = bone1;
			this.bone2 = bone2;
			this.bone3 = bone3;
			this.parentBone = parentBone;
		}
		
		public void StoreDefaultLocalState() {
			if (parentBone != null) boneMapParent.StoreDefaultLocalState();
			boneMap1.StoreDefaultLocalState();
			boneMap2.StoreDefaultLocalState();
			boneMap3.StoreDefaultLocalState();
		}
		
		public void FixTransforms() {
			if (parentBone != null) boneMapParent.FixTransform();
			boneMap1.FixTransform();
			boneMap2.FixTransform();
			boneMap3.FixTransform();
		}
		
		/*
		 * Initiating and setting defaults
		 * */
		protected override void OnInitiate() {
			// Finding the nodes
			if (parentBone != null) boneMapParent.Initiate(parentBone, solver);
			boneMap1.Initiate(bone1, solver);
			boneMap2.Initiate(bone2, solver);
			boneMap3.Initiate(bone3, solver);

			// Define plane points for the bone maps
			boneMap1.SetPlane(boneMap1.node, boneMap2.node, boneMap3.node);
			boneMap2.SetPlane(boneMap2.node, boneMap3.node, boneMap1.node);

			// Find the swing axis for the parent bone
			if (parentBone != null) boneMapParent.SetLocalSwingAxis(boneMap1);
		}
		
		/*
		 * Presolving the bones and maintaining rotation
		 * */
		public void ReadPose() {
			boneMap1.UpdatePlane();
			boneMap2.UpdatePlane();

			// Clamping weights
			weight = Mathf.Clamp(weight, 0f, 1f);

			// Define plane points for the bone maps
			boneMap3.MaintainRotation();
		}

		public void WritePose() {
			float w = solver.GetIKPositionWeight();
			if (w <= 0) return;
			
			// Swing the parent bone to look at the first node's position
			if (parentBone != null) {
				boneMapParent.Swing(boneMap1.node.solverPosition, w * weight);
			}
			
			// Fix the first bone to it's node
			boneMap1.FixToNode(w * weight);
			
			// Rotate the 2 first bones to the plane points
			LocalSlerp(boneMap1, w);
			LocalSlerp(boneMap2, w);

			// Rotate the third bone to the rotation it had before solving
			boneMap3.RotateToMaintain(w * maintainRotationWeight * weight);
			
			// Rotate the third bone to the effector rotation
			boneMap3.RotateToEffector(w * weight);
		}

		private void LocalSlerp(IKMapping.BoneMap boneMap, float w) {
			if (w * weight >= 1) {
				boneMap.RotateToPlane(w);
				return;
			}

			Quaternion boneMapRotation = boneMap.GetPlaneRotation(w);
			Quaternion boneMapLocalRotation = Quaternion.Inverse(boneMap.transform.parent.rotation) * boneMapRotation;
			boneMap.transform.localRotation = Quaternion.Lerp(boneMap.transform.localRotation, boneMapLocalRotation, weight);
		}

		/*
		 * Ortho-Normalize a vector to the limb direction. Added in 0.2
		 * */
		private Vector3 OrthoToLimb(Vector3 tangent) {
			Vector3 normal = boneMap3.transform.position - boneMap1.transform.position;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}
	}
}
