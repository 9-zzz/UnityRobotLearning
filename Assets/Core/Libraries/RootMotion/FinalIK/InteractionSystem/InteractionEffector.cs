using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Managing Interactions for a single FBBIK effector.
	/// </summary>
	[System.Serializable]
	public class InteractionEffector {

		// The type of the effector
		public FullBodyBipedEffector effectorType { get; private set; }

		// Interaction event callbacks
		public InteractionSystem.InteractionEvent OnStart { get; set; }
		public InteractionSystem.InteractionEvent OnTrigger { get; set; }
		public InteractionSystem.InteractionEvent OnRelease { get; set; }
		public InteractionSystem.InteractionEvent OnPause { get; set; }
		public InteractionSystem.InteractionEvent OnPickUp { get; set; }
		public InteractionSystem.InteractionEvent OnResume { get; set; }
		public InteractionSystem.InteractionEvent OnStop { get; set; }

		// Has the interaction been paused?
		public bool isPaused { get; private set; }
		// The current InteractionObject (null if there is no interaction going on)
		public InteractionObject interactionObject { get; private set; }
		// Is this InteractionEffector currently in the middle of an interaction?
		public bool inInteraction { get { return interactionObject != null; }}

		// Internal values
		private Poser poser;
		private IKEffector effector;
		private float timer, length, weight, fadeInSpeed, defaultPull, defaultReach, resetTimer;
		private bool triggered, released, pickedUp, defaults;
		private Vector3 pickUpPosition, pausePositionRelative;
		private Quaternion pickUpRotation, pauseRotationRelative;
		private InteractionTarget interactionTarget;
		private Transform target;

		// The custom constructor
		public InteractionEffector (FullBodyBipedEffector effectorType) {
			this.effectorType = effectorType;
		}

		// Initiate this, get the default values
		public void Initiate(IKSolverFullBodyBiped solver) {
			// Find the effector if we haven't already
			if (effector == null) {
				effector = solver.GetEffector(effectorType);
				poser = effector.bone.GetComponent<Poser>();
			}

			defaultPull = solver.GetChain(effectorType).pull;
			defaultReach = solver.GetChain(effectorType).reach;
		}

		// Interpolate to default values when currently not in interaction
		public void ResetToDefaults(IKSolverFullBodyBiped solver, float speed) {
			if (inInteraction) return;
			if (isPaused) return;
			if (defaults) return; 

			resetTimer = Mathf.Clamp(resetTimer -= Time.deltaTime * speed, 0f, 1f);

			// Pull and Reach
			if (effector.isEndEffector) {
				solver.GetChain(effectorType).pull = Mathf.Lerp(defaultPull, solver.GetChain(effectorType).pull, resetTimer);
				solver.GetChain(effectorType).reach = Mathf.Lerp(defaultReach, solver.GetChain(effectorType).reach, resetTimer);
			}

			// Effector weights
			effector.positionWeight = Mathf.Lerp(0f, effector.positionWeight, resetTimer);
			effector.rotationWeight = Mathf.Lerp(0f, effector.rotationWeight, resetTimer);

			if (resetTimer <= 0f) defaults = true;
		}

		// Pause this interaction
		public void Pause() {
			if (!inInteraction) return;
			isPaused = true;

			pausePositionRelative = target.InverseTransformPoint(effector.position);
			pauseRotationRelative = Quaternion.Inverse(target.rotation) * effector.rotation;

			if (OnPause != null) OnPause(effectorType, interactionObject);
		}

		// Resume a paused interaction
		public void Resume() {
			if (!inInteraction) return;

			isPaused = false;
			if (OnResume != null) OnResume(effectorType, interactionObject);
		}

		// Start interaction
		public void Start(InteractionObject interactionObject, string tag, float fadeInTime, bool interrupt) {
			// If not in interaction, set effector positions to their bones
			if (!inInteraction) {
				effector.position = effector.bone.position;
				effector.rotation = effector.bone.rotation;
			} else if (!interrupt) return;

			// Get the InteractionTarget
			target = interactionObject.GetTarget(effectorType, tag);
			if (target == null) return;
			interactionTarget = target.GetComponent<InteractionTarget>();

			// Start the interaction
			this.interactionObject = interactionObject;
			if (OnStart != null) OnStart(effectorType, interactionObject);

			// Posing the hand/foot
			if (poser != null) {
				if (poser.poseRoot == null) poser.weight = 0f;

				if (interactionTarget != null) poser.poseRoot = target.transform;
				else poser.poseRoot = null;

				poser.AutoMapping();
			}

			// Reset internal values
			timer = 0f;
			weight = 0f;
			fadeInSpeed = fadeInTime > 0f? 1f / fadeInTime: 1000f;
			length = interactionObject.length;
			
			triggered = false;
			released = false;
			isPaused = false;

			pickedUp = false;
			pickUpPosition = Vector3.zero;
			pickUpRotation = Quaternion.identity;

			// Call StartInteraction on the InteractionObject
			interactionObject.StartInteraction(effector.bone);
			if (interactionTarget != null) interactionTarget.RotateTo(effector.bone.position);
		}

		// Update the (possibly) ongoing interaction
		public void Update(IKSolverFullBodyBiped solver, float speed) {
			if (!inInteraction) return;

			// Rotate target
			if (interactionTarget != null && !interactionTarget.rotateOnce) interactionTarget.RotateTo(effector.bone.position);

			if (isPaused) {
				effector.position = target.TransformPoint(pausePositionRelative);
				effector.rotation = target.rotation * pauseRotationRelative;

				// Apply the current interaction state to the solver
				interactionObject.Apply(solver, effectorType, interactionTarget, timer, weight);

				return;
			}

			// Advance the interaction timer and weight
			timer += Time.deltaTime * speed * (interactionTarget != null? interactionTarget.interactionSpeedMlp: 1f);
			weight = Mathf.Clamp(weight + Time.deltaTime * fadeInSpeed, 0f, 1f);

			if (!triggered && timer >= interactionObject.triggerTime) timer = interactionObject.triggerTime;

			// Effector target positions and rotations
			Vector3 targetPosition = pickedUp? pickUpPosition: target.position;
			Quaternion targetRotation = pickedUp? pickUpRotation: target.rotation;

			// Interpolate effector position and rotation
			effector.position = Vector3.Lerp(effector.position, targetPosition, weight * weight);
			effector.rotation = Quaternion.Lerp(effector.rotation, targetRotation, weight * weight);

			// Apply the current interaction state to the solver
			interactionObject.Apply(solver, effectorType, interactionTarget, timer, weight);

			// Hand poser weight
			if (poser != null) poser.weight = pickedUp? 1f: effector.positionWeight;

			// Interaction events
			if (!triggered && timer >= interactionObject.triggerTime) Trigger();
			if (!released && timer >= interactionObject.releaseTime) Release();
			if (timer >= length) Stop();
		}

		// Trigger the interaction object
		private void Trigger() {
			interactionObject.Trigger(effector.bone);

			// Picking up the object
			if (interactionObject.pickUpOnTrigger) {
				pickUpPosition = effector.position;
				pickUpRotation = effector.rotation;

				// Positioning and rotating the interaction object to the effector (not the bone, because it is still at it's animated translation)
				interactionObject.transform.parent = effector.bone;
				interactionObject.transform.localPosition = Quaternion.Inverse(pickUpRotation) * (interactionObject.transform.position - pickUpPosition);
				interactionObject.transform.localRotation = Quaternion.Inverse(pickUpRotation) * interactionObject.transform.rotation;

				pickedUp = true;

				if (OnPickUp != null) OnPickUp(effectorType, interactionObject);
			}

			triggered = true;

			if (OnTrigger != null) OnTrigger(effectorType, interactionObject);
			if (interactionObject.pauseOnTrigger) Pause();
		}

		// Release the interaction object
		private void Release() {
			if (OnRelease != null) OnRelease(effectorType, interactionObject);
			interactionObject.Release();
			released = true;
		}

		// Stop the interaction
		public void Stop() {
			if (!inInteraction) return;

			if (OnStop != null) OnStop(effectorType, interactionObject);

			// Reset the interaction target
			if (interactionTarget != null) interactionTarget.ResetRotation();

			// End event for the interaction object
			interactionObject.EndInteraction(effector.bone);

			// Reset the internal values
			interactionObject = null;
			weight = 0f;
			timer = 0f;
			pickedUp = false;
			triggered = false;
			released = false;
			isPaused = false;
			target = null;
			defaults = false;
			resetTimer = 1f;
			if (poser != null) poser.weight = 0f;
		}

		// Called after FBBIK update
		public void OnPostFBBIK(IKSolverFullBodyBiped fullBody) {
			if (!inInteraction) return;

			// Rotate the hands/feet to the RotateBoneWeight curve
			float rotateBoneWeight = interactionObject.GetValue(InteractionObject.WeightCurve.Type.RotateBoneWeight, interactionTarget, timer) * weight;
			if (rotateBoneWeight > 0f) effector.bone.rotation = Quaternion.Lerp(effector.bone.rotation, effector.rotation, rotateBoneWeight);
		}
	}
}
