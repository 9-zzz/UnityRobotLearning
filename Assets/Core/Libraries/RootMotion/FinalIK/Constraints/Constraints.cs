using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Contains and manages a set of constraints.
	/// </summary>
	[System.Serializable]
	public class Constraints {
		
		#region Main Interface
		
		/// <summary>
		/// The position offset constraint.
		/// </summary>
		public ConstraintPositionOffset positionOffsetConstraint = new ConstraintPositionOffset();
		/// <summary>
		/// The position constraint.
		/// </summary>
		public ConstraintPosition positionConstraint = new ConstraintPosition();
		/// <summary>
		/// The rotation offset constraint.
		/// </summary>
		public ConstraintRotationOffset rotationOffsetConstraint = new ConstraintRotationOffset();
		/// <summary>
		/// The rotation constraint.
		/// </summary>
		public ConstraintRotation rotationConstraint = new ConstraintRotation();
		
		/// <summary>
		/// Initiate the constraints for the specified transform.
		/// </summary>
		public void Initiate(Transform transform) {
			// Assigning constraint transforms
			positionConstraint.transform = transform;
			positionOffsetConstraint.transform = transform;
			rotationConstraint.transform = transform;
			rotationOffsetConstraint.transform = transform;
			
			// Default values
			positionConstraint.position = transform.position;
			rotationConstraint.rotation = transform.rotation;
		}
		
		/// <summary>
		/// Updates the constraints.
		/// </summary>
		public void UpdateConstraints() {
			positionOffsetConstraint.UpdateConstraint();
			positionConstraint.UpdateConstraint();
			rotationOffsetConstraint.UpdateConstraint();
			rotationConstraint.UpdateConstraint();
		}
		
		/// <summary>
		/// Gets the common transform of all the constraints.
		/// </summary>
		public Transform transform {
			get {
				return positionConstraint.transform;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether all the constraints are valid.
		/// </summary>
		/// <value>
		/// <c>true</c> if is valid; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid() {
			return positionConstraint.isValid && positionOffsetConstraint.isValid && rotationConstraint.isValid && rotationOffsetConstraint.isValid;
		}
		
		#endregion Main Interface
	}
}
