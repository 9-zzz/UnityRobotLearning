using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Motion Absorb demo character controller.
	/// </summary>
	public class MotionAbsorbCharacter : MonoBehaviour {

		public Animator animator;
		public MotionAbsorb motionAbsorb;
		public Transform cube; // The cube we are hitting
		public float cubeRandomPosition = 0.1f; // Randomizing cube position after each hit

		private Vector3 cubeDefaultPosition;
		
		void Start() {
			// Storing the default position of the cube
			cubeDefaultPosition = cube.position;
		}

		void Update () {
			// Set motion absorb weight
			motionAbsorb.weight = animator.GetFloat("MotionAbsorbWeight"); // NB! Using Mecanim curves is PRO only
		}

		// Mecanim event
		void SwingStart() {
			// Reset the cube
			cube.GetComponent<Rigidbody>().MovePosition(cubeDefaultPosition + UnityEngine.Random.insideUnitSphere * cubeRandomPosition);
			cube.GetComponent<Rigidbody>().MoveRotation(Quaternion.identity);
			cube.GetComponent<Rigidbody>().velocity = Vector3.zero;
			cube.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
	}
}
