using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Simple character animation controller for the Hit Reaction demo
	/// </summary>
	public class HitReactionCharacter: MonoBehaviour {

		public string mixingAnim;
		public Transform recursiveMixingTransform;

		void Start() {
			if (mixingAnim != string.Empty) {
				GetComponent<Animation>()[mixingAnim].layer = 1;
				GetComponent<Animation>()[mixingAnim].AddMixingTransform(recursiveMixingTransform, true);
				GetComponent<Animation>().Play(mixingAnim);
			}
		}
	}
}
