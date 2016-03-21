using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK.Demos {

	/// <summary>
	/// Controller for the Mech spider.
	/// </summary>
	public class MechSpiderController: MonoBehaviour {

		public MechSpider mechSpider; // The mech spider
		public Transform cameraTransform; // The camera
		public float speed = 6f; // Horizontal speed of the spider
		public float verticalSpeed = 5f; // Vertical speed of the spider
		public float turnSpeed = 30f; // The speed of turning the spider to align with the camera
		public float height = 4f; // Height from ground
		public float rotateToNormalSpeed = 1f; // The speed of rotating the spider to ground normal
		public float raycastHeight = 2f; // Raycasting height offset

		public Vector3 inputVector {
			get {
				return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			}
		}

		private RaycastHit hit;

		void Update() {
			// Get the ground Raycast hit
			GetGroundHit(out hit);

			// Rotating to ground normal
			Quaternion normalOffset = Quaternion.FromToRotation(transform.up, hit.normal);
			Quaternion rotationTarget = normalOffset * transform.rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, rotationTarget, Time.deltaTime * rotateToNormalSpeed);

			// Moving the spider vertically
			transform.position = Vector3.Lerp(transform.position, hit.point + transform.up * height, Time.deltaTime * verticalSpeed);

			// Read the input
			Vector3 cameraForward = cameraTransform.forward;
			Vector3 camNormal = transform.up;
			Vector3.OrthoNormalize(ref camNormal, ref cameraForward);

			// Moving the spider
			Quaternion cameraLookRotation = Quaternion.LookRotation(cameraForward, transform.up);
			transform.Translate(cameraLookRotation * inputVector.normalized * Time.deltaTime * speed, Space.World);
			
			// Rotating the spider to camera forward
			transform.rotation = Quaternion.RotateTowards(transform.rotation, cameraLookRotation, Time.deltaTime * turnSpeed);
		}

		// Get the Raycast hit to the ground
		private bool GetGroundHit(out RaycastHit hit) {
			Vector3 direction = -transform.up;
			if (Physics.Raycast(transform.position - direction * raycastHeight, direction, out hit, Mathf.Infinity, mechSpider.raycastLayers)) return true;
			return false;
		}
	}

}