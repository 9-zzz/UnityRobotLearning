using UnityEditor;
using UnityEngine;
using System.Collections;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector for FullBodyBipedIK.
	 * */
	[CustomEditor(typeof(FullBodyBipedIK))]
	public class FullBodyBipedIKInspector : IKInspector {

		private FullBodyBipedIK script { get { return target as FullBodyBipedIK; }}
		private int selectedEffector;
		private SerializedProperty references;
		private bool autodetected;

		private static Color color {
			get {
				return new Color(0f, 0.75f, 1f);
			}
		}

		protected override MonoBehaviour GetMonoBehaviour(out int executionOrder) {
			executionOrder = 9999;
			return script;
		}
		
		protected override SerializedContent[] FindContent() {
			references = serializedObject.FindProperty("references");
			
			return IKSolverFullBodyBipedInspector.FindContent(solver);
		}

		protected override void OnEnableVirtual() {
			// Autodetecting References
			if (script.references.IsEmpty(false) && script.enabled) {
				BipedReferences.AutoDetectReferences(ref script.references, script.transform, new BipedReferences.AutoDetectParams(true, false));

				script.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(script.references);

				Initiate();

				if (Application.isPlaying) Warning.Log("Biped references were auto-detected on a FullBodyBipedIK component that was added in runtime. Note that this only happens in the Editor and if the GameObject is selected (for quick and convenient debugging). If you want to add FullBodyBipedIK dynamically in runtime via script, you will have to use BipedReferences.AutodetectReferences() for automatic biped detection.", script.transform);
				
				references.isExpanded = !script.references.isValid;
				content[5].prop.isExpanded = false;
				content[content.Length - 3].prop.isExpanded = false;
			}
		}

		protected override void AddInspector() {
			if (!Application.isPlaying) {
				// Editing References, if they have changed, reinitiate.
				if (BipedReferencesInspector.AddModifiedInspector(references)) Initiate();	

				if (script.references.isValid) {
					IKSolverFullBodyBipedInspector.AddReferences(true, content);
					
					// Draw the inspector for IKSolverFullBody
					if (script.solver.IsValid(false)) IKSolverFullBodyBipedInspector.AddInspector(solver, !Application.isPlaying, false, content);
				}

				// Reinitiate if rootNode has changed
				if (serializedObject.ApplyModifiedProperties()) Initiate();
			} else {
				BipedReferencesInspector.AddModifiedInspector(references);	
				IKSolverFullBodyBipedInspector.AddReferences(true, content);

				if (script.solver.initiated) IKSolverFullBodyBipedInspector.AddInspector(solver, !Application.isPlaying, false, content);
			}

			EditorGUILayout.Space();
		}

		private void Initiate() {
			Warning.logged = false;

			if (!BipedIsValid(script.references, script.solver, script.transform, true)) return;
			Warning.logged = false;

			script.solver.SetToReferences(script.references, script.solver.rootNode);
		}
		
		void OnSceneGUI() {
			// Draw the scene veiw helpers
			if (!script.references.isValid) return;

			IKSolverFullBodyBipedInspector.AddScene(script.solver, color, true, ref selectedEffector, script.transform);
		}

		private bool BipedIsValid(BipedReferences references, IKSolverFullBodyBiped solver, Transform context, bool log) {
			BipedReferences.CheckSetup(script.references);

			if (!references.isValid) {
				//if (log) Warning.Log("BipedReferences contains one or more missing Transforms.", context, true);
				return false;
			}
			if (references.spine.Length == 0) {
				//if (log) Warning.Log("Biped has no spine bones.", context, true);
				return false;
			}
			
			if (solver.rootNode == null) {
				//if (log) Warning.Log("Root Node bone is null.", context, true);
				return false;
			}
			
			Vector3 toRightShoulder = references.rightUpperArm.position - references.leftUpperArm.position;
			Vector3 shoulderToRootNode = solver.rootNode.position - references.leftUpperArm.position;
			float dot = Vector3.Dot(toRightShoulder.normalized, shoulderToRootNode.normalized);
			
			if (dot > 0.95f) {
				if (log) Warning.Log ("The root node, the left upper arm and the right upper arm bones should ideally form a triangle that is as close to equilateral as possible. " +
					"Currently the root node bone seems to be very close to the line between the left upper arm and the right upper arm bones. This might cause unwanted behaviour like the spine turning upside down when pulled by a hand effector." +
					"Please set the root node bone to be one of the lower bones in the spine.", context, true);
			}
			
			Vector3 toRightThigh = references.rightThigh.position - references.leftThigh.position;
			Vector3 thighToRootNode = solver.rootNode.position - references.leftThigh.position;
			dot = Vector3.Dot(toRightThigh.normalized, thighToRootNode.normalized);
			
			if (dot > 0.95f && log) {
				Warning.Log ("The root node, the left thigh and the right thigh bones should ideally form a triangle that is as close to equilateral as possible. " +
					"Currently the root node bone seems to be very close to the line between the left thigh and the right thigh bones. This might cause unwanted behaviour like the hip turning upside down when pulled by an effector." +
					"Please set the root node bone to be one of the higher bones in the spine.", context, true);
			}

			/*
			Vector3 shoulderCross = Vector3.Cross(toRightShoulder, shoulderToRootNode);
			Vector3 charCross = Vector3.Cross(references.rightHand.position - references.leftHand.position, references.rightFoot.position - references.leftHand.position);
			
			float shoulderInvertDot = Vector3.Dot(shoulderCross.normalized, charCross.normalized);
			
			if (shoulderInvertDot < 0 && log) {
				Warning.Log("The triangle formed by the root node, left upper arm and right upper arm bones seems to be flipped. The root node bone should be below the upper arm bones.", context, true);
			}
			
			Vector3 thighCross = Vector3.Cross(toRightThigh, thighToRootNode);
			
			float thighInvertDot = Vector3.Dot(thighCross.normalized, charCross.normalized);
			
			if (thighInvertDot > 0 && log) {
				Warning.Log("The triangle formed by the root node, left thigh and right thigh bones seems to be flipped. The root node bone should be above the thigh bones.", context, true);
			}
			*/

			return true;
		}
	}
}
