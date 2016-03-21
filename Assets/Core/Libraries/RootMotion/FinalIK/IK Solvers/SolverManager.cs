using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Manages solver initiation and updating
	/// </summary>
	[AddComponentMenu("Scripts/SolverManager")]
	public class SolverManager: MonoBehaviour {
		
		#region Main Interface
		
		/// <summary>
		/// The updating frequency
		/// </summary>
		public float timeStep;
		/// <summary>
		/// If true, will fix all the Transforms used by the solver to their default local states in each Update.
		/// </summary>
		public bool fixTransforms = true;

		/// <summary>
		/// Safely disables this component, making sure the solver is still initated. Use this instead of "enabled = false" if you need to disable the component to manually control it's updating.
		/// </summary>
		public void Disable() {
			Initiate();
			enabled = false;
		}
		
		#endregion Main

		protected virtual void InitiateSolver() {}
		protected virtual void UpdateSolver() {}
		protected virtual void FixTransforms() {}
		
		private float lastTime;
		private Animator animator;
		private new Animation animation;
		private bool updateFrame;
		private bool componentInitiated;

		private bool animatePhysics {
			get {
				if (animator != null) return animator.updateMode.Equals(AnimatorUpdateMode.AnimatePhysics);
				if (animation != null) return animation.animatePhysics;
				return false;
			}
		}

		void Start() {
			Initiate();
		}

		void Update() {
			if (animatePhysics) return;

			if (fixTransforms) FixTransforms();
		}

		private void Initiate() {
			if (componentInitiated) return;

			animator = GetComponent<Animator>();
			animation = GetComponent<Animation>();

			InitiateSolver();
			componentInitiated = true;
		}

		/*
		 * Workaround hack for the solver to work with animatePhysics
		 * */
		void FixedUpdate() {
			updateFrame = true;

			if (animatePhysics && fixTransforms) FixTransforms();
		}

		/*
		 * Updating by timeStep
		 * */
		void LateUpdate() {
			// Check if either animatePhysics is false or FixedUpdate has been called
			if (!animatePhysics) updateFrame = true;
			if (!updateFrame) return;
			updateFrame = false;

			if (timeStep == 0) UpdateSolver();
			else {
				if (Time.time >= lastTime + timeStep) {
					UpdateSolver();
					lastTime = Time.time;
				}
			}
		}
	}
}
