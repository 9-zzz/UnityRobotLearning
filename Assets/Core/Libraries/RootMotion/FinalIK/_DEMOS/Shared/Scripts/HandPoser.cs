// THIS FILE CONTAINS EDITS FROM THE ORIGINAL
// Search for "EDIT:" to see changed areas

using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

// EDIT: Adding usings
using System.Collections.Generic;
using System.Linq;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Posing the children of a Transform to match the children of another Transform
	/// </summary>
	public class HandPoser : Poser {

		private Transform _poseRoot;
		private Transform[] children;
		private Transform[] poseChildren;
		
		void Start() {
			// Find the children
            // EDIT: Allowing filtering out of bones with the "Marker" tag - AS
            this.children = 
                new List<Transform>(
                    this.transform.GetComponentsInChildren<Transform>()
                        .Where((Transform child) => child.CompareTag("Marker") == false))
                        .ToArray();
		}

		public override void AutoMapping() {
			if (poseRoot == null) poseChildren = new Transform[0];
			else poseChildren = (Transform[])poseRoot.GetComponentsInChildren<Transform>();

			_poseRoot = poseRoot;
		}
		
		void LateUpdate() {
			if (weight <= 0f) return;
			if (localPositionWeight <= 0f && localRotationWeight <= 0f) return;

			// Get the children, if we don't have them already
			if (_poseRoot != poseRoot) AutoMapping();

			if (poseRoot == null) return;

			// Something went wrong

            // EDIT: Adding better debug output
			if (children.Length != poseChildren.Length) {
                Warning.Log(
                    "Number of children does not match with the pose."
                    + " Did you tag markers with the \"Marker\" tag?",
                    this.transform);
				return;
			}

			// Calculate weights
			float rW = localRotationWeight * weight;
			float pW = localPositionWeight * weight;

			// Lerping the localRotation and the localPosition
			for (int i = 0; i < children.Length; i++) {
				if (children[i] != transform) {
					children[i].localRotation = Quaternion.Lerp(children[i].localRotation, poseChildren[i].localRotation, rW);
					children[i].localPosition = Vector3.Lerp(children[i].localPosition, poseChildren[i].localPosition, pW);
				}
			}
		}
	}
}
