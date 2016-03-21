using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Object than the InteractionSystem can interact with.
	/// </summary>
	public class InteractionObject : MonoBehaviour {

		#region Main Interface

		/// <summary>
		/// Gets the length of the interaction (the longest curve)
		/// </summary>
		public float length { get; private set; }

		/// <summary>
		/// Call if you have changed the curves in play mode or added/removed InteractionTargets
		/// </summary>
		public void Initiate() {
			for (int i = 0; i < weightCurves.Length; i++) {
				if (weightCurves[i].curve.length > 0) {
					float l = weightCurves[i].curve.keys[weightCurves[i].curve.length - 1].time;
					length = Mathf.Clamp(length, l, length);
				}
			}
			
			targets = targetsRoot.GetComponentsInChildren<InteractionTarget>();
		}

		/// <summary>
		/// Gets the look at target (returns otherLookAtTarget if not null).
		/// </summary>
		public Transform lookAtTarget {
			get {
				if (otherLookAtTarget != null) return otherLookAtTarget;
				return transform;
			}
		}


		#endregion Main Interface

		/*
		 * Calls a Mecanim animation
		 * */
		[System.Serializable]
		public class AnimatorEvent {
			public string animationState;
			public float crossfadeTime = 0.3f;
			public int layer;
			public bool resetNormalizedTime;
			
			public void Activate(Animator animator) {
				if (animationState == "") return;
				
				if (resetNormalizedTime) animator.CrossFade(animationState, crossfadeTime, layer, 0f);
				else animator.CrossFade(animationState, crossfadeTime, layer);
			}
		}
		
		/*
		 * A Weight curve for various FBBIK channels
		 * */
		[System.Serializable]
		public class WeightCurve {
			
			[System.Serializable]
			public enum Type {
				PositionWeight,
				RotationWeight,
				PositionOffsetX,
				PositionOffsetY,
				PositionOffsetZ,
				Pull,
				Reach,
				RotateBoneWeight,
			}
			
			public Type type;
			public AnimationCurve curve;
			
			public float GetValue(float timer) {
				return curve.Evaluate(timer);
			}
		}
		
		/*
		 * Multiplies a weight curve and uses the result for another FBBIK channel. (to reduce the amount of work with AnimationCurves)
		 * */
		[System.Serializable]
		public class Multiplier {
			public WeightCurve.Type curve;
			public float multiplier = 1f;
			public WeightCurve.Type result;
			
			public float GetValue(WeightCurve weightCurve, float timer) {
				return weightCurve.GetValue(timer) * multiplier;
			}
		}

		// The weight curves for the interaction.
		public WeightCurve[] weightCurves;
		// The weight curve multipliers for the interaction.
		public Multiplier[] multipliers;
		// The look at target. If null, will look at this GameObject
		public Transform otherLookAtTarget;
		// The root Transform of the InteractionTargets. If null, will use this GameObject.
		public Transform otherTargetsRoot;
		// Will this object be picked up on trigger? This only works as expected if a single effector is interacting with this object. For 2-handed pick-ups please see the "Interaction PickUp2Handed" demo.
		public bool pickUpOnTrigger;
		// Will the interaction be paused on trigger?
		public bool pauseOnTrigger;
		// Trigger time since interaction start.
		public float triggerTime = 0.3f;
		// Release time since interaction start
		public float releaseTime = 0.6f;
		// Animations played on events
		public AnimatorEvent onStartAnimation, onTriggerAnimation, onReleaseAnimation, onEndAnimation;
		// SendMessage recipients
		public GameObject[] onInteractionStartRecipients, onInteractionTriggerRecipients, onInteractionReleaseRecipients, onInteractionEndRecipients;

		[SerializeField] Animator animator; // Reference to the animator component

		// Returns all the InteractionTargets of this object
		public InteractionTarget[] GetTargets() {
			return targets;
		}

		// Returns the InteractionTarget of effector type and tag
		public Transform GetTarget(FullBodyBipedEffector effectorType, string tag) {
			if (tag == string.Empty || tag == "") return GetTarget(effectorType);
			
			for (int i = 0; i < targets.Length; i++) {
				if (targets[i].effectorType == effectorType && targets[i].tag == tag) return targets[i].transform;
			}

			return transform;
		}

		// Starts the interaction process, animation and sends the OnInteractionStart message
		public void StartInteraction(Transform t) {
			if (animator != null) onStartAnimation.Activate(animator);
			
			foreach (GameObject r in onInteractionStartRecipients) r.SendMessage("OnInteractionStart", t, SendMessageOptions.RequireReceiver);
		}

		// Trigger event, animation and sends the OnInteractionTrigger message
		public void Trigger(Transform t) {
			if (animator != null) {
				// disable root motion because it may become a child of another Animator. Workaround for a Unity bug with an error message: "Transform.rotation on 'gameobject name' is no longer valid..."
				if (pickUpOnTrigger) animator.applyRootMotion = false;
				
				onTriggerAnimation.Activate(animator);
			}
			
			foreach (GameObject r in onInteractionTriggerRecipients) r.SendMessage("OnInteractionTrigger", t, SendMessageOptions.RequireReceiver);
		}

		// Release event, animation and OnInteractionRelease message 
		public void Release() {
			if (animator != null) {
				onReleaseAnimation.Activate(animator);
			}
			
			foreach (GameObject r in onInteractionReleaseRecipients) r.SendMessage("OnInteractionRelease", SendMessageOptions.RequireReceiver);
		}

		// Ends the interaction process, animation and sends the OnInteractionEnd message
		public void EndInteraction(Transform t) {
			if (animator != null) onEndAnimation.Activate(animator);
			
			foreach (GameObject r in onInteractionEndRecipients) r.SendMessage("OnInteractionEnd", t, SendMessageOptions.RequireReceiver);
		}

		// Applies the weight curves and multipliers to the FBBIK solver
		public void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, InteractionTarget target, float timer, float weight) {

			for (int i = 0; i < weightCurves.Length; i++) {
				float mlp = target == null? 1f: target.GetValue(weightCurves[i].type);

				Apply(solver, effector, weightCurves[i].type, weightCurves[i].GetValue(timer), weight * mlp);
			}

			for (int i = 0; i < multipliers.Length; i++) {
				if (multipliers[i].curve == multipliers[i].result) {
					if (!Warning.logged) Warning.Log("InteractionObject Multiplier 'Curve' " + multipliers[i].curve.ToString() + "and 'Result' are the same.", transform);
				}

				int curveIndex = GetWeightCurveIndex(multipliers[i].curve);
					
				if (curveIndex != -1) {
					float mlp = target == null? 1f: target.GetValue(multipliers[i].result);

					Apply(solver, effector, multipliers[i].result, multipliers[i].GetValue(weightCurves[curveIndex], timer), weight * mlp);
				} else {
					if (!Warning.logged) Warning.Log("InteractionObject Multiplier curve " + multipliers[i].curve.ToString() + "does not exist.", transform);
				}
			}
		}

		// Gets the value of a weight curve/multiplier
		public float GetValue(WeightCurve.Type weightCurveType, InteractionTarget target, float timer) {
			int index = GetWeightCurveIndex(weightCurveType);

			if (index != -1) {
				float mlp = target == null? 1f: target.GetValue(weightCurveType);

				return weightCurves[index].GetValue(timer) * mlp;
			}

			for (int i = 0; i < multipliers.Length; i++) {
				if (multipliers[i].result == weightCurveType) {

					int wIndex = GetWeightCurveIndex(multipliers[i].curve);
					if (wIndex != -1) {
						float mlp = target == null? 1f: target.GetValue(multipliers[i].result);

						return multipliers[i].GetValue(weightCurves[wIndex], timer) * mlp;
					}
				}
			}

			return 0f;
		}

		private InteractionTarget[] targets = new InteractionTarget[0];
		
		void Awake() {
			Initiate();
		}

		// Apply the curve to the specified solver, effector, with the value and weight.
		private void Apply(IKSolverFullBodyBiped solver, FullBodyBipedEffector effector, WeightCurve.Type type, float value, float weight) {
			switch(type) {
			case WeightCurve.Type.PositionWeight:
				solver.GetEffector(effector).positionWeight = Mathf.Lerp(solver.GetEffector(effector).positionWeight, value, weight);
				return;
			case WeightCurve.Type.RotationWeight:
				solver.GetEffector(effector).rotationWeight = Mathf.Lerp(solver.GetEffector(effector).rotationWeight, value, weight);
				return;
			case WeightCurve.Type.PositionOffsetX:
				solver.GetEffector(effector).position += solver.GetRoot().rotation * Vector3.right * value * weight;
				return;
			case WeightCurve.Type.PositionOffsetY:
				solver.GetEffector(effector).position += solver.GetRoot().rotation * Vector3.up * value * weight;
				return;
			case WeightCurve.Type.PositionOffsetZ:
				solver.GetEffector(effector).position += solver.GetRoot().rotation * Vector3.forward * value * weight;
				return;
			case WeightCurve.Type.Pull:
				solver.GetChain(effector).pull = Mathf.Lerp(solver.GetChain(effector).pull, value, weight);
				return;
			case WeightCurve.Type.Reach:
				solver.GetChain(effector).reach = Mathf.Lerp(solver.GetChain(effector).reach, value, weight);
				return;
			}
		}

		private Transform GetTarget(FullBodyBipedEffector effectorType) {
			for (int i = 0; i < targets.Length; i++) {
				if (targets[i].effectorType == effectorType) return targets[i].transform;
			}
			return transform;
		}
		
		private int GetWeightCurveIndex(WeightCurve.Type weightCurveType) {
			for (int i = 0; i < weightCurves.Length; i++) {
				if (weightCurves[i].type == weightCurveType) return i;
			}
			return -1;
		}
		
		private int GetMultiplierIndex(WeightCurve.Type weightCurveType) {
			for (int i = 0; i < multipliers.Length; i++) {
				if (multipliers[i].result == weightCurveType) return i;
			}
			return -1;
		}

		private Transform targetsRoot {
			get {
				if (otherTargetsRoot != null) return otherTargetsRoot;
				return transform;
			}
		}
	}
}
