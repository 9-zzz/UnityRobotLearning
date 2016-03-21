using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverLookAt
	 * */
	public class IKSolverLookAtInspector: IKSolverInspector {

		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = new SerializedContent[12] {
				new SerializedContent(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight for smooth blending.")),
				new SerializedContent(prop.FindPropertyRelative("bodyWeight"), new GUIContent("Body Weight", "Weight of rotating spine to target.")),
				new SerializedContent(prop.FindPropertyRelative("headWeight"), new GUIContent("Head Weight", "Weight of rotating head to target.")),
				new SerializedContent(prop.FindPropertyRelative("eyesWeight"), new GUIContent("Eyes Weight", "Weight of rotating eyes to target.")),
				new SerializedContent(prop.FindPropertyRelative("clampWeight"), new GUIContent("Clamp Weight", "Clamping rotation of spine and head. 0 is free rotation, 1 is completely clamped to forward.")),
				new SerializedContent(prop.FindPropertyRelative("clampWeightHead"), new GUIContent("Clamp Weight Head", "Clamping rotation of the head. 0 is free rotation, 1 is completely clamped to forward.")),
				new SerializedContent(prop.FindPropertyRelative("clampWeightEyes"), new GUIContent("Clamp Weight Eyes", "Clamping rotation of the eyes. 0 is free rotation, 1 is completely clamped to forward.")),
				new SerializedContent(prop.FindPropertyRelative("clampSmoothing"), new GUIContent("Clamp Smoothing", "Number of sine smoothing iterations applied on clamping to make the clamping point smoother.")),
				new SerializedContent(prop.FindPropertyRelative("head.transform"), new GUIContent("Head", "The head bone.")),
				new SerializedContent(prop.FindPropertyRelative("spine"), new GUIContent("Spine", string.Empty)),
				new SerializedContent(prop.FindPropertyRelative("eyes"), new GUIContent("Eyes", string.Empty)),
				new SerializedContent(prop.FindPropertyRelative("spineWeightCurve"), new GUIContent("Spine Weight Curve", "Weight distribution between spine bones (first bone is evaluated at time 0.0, last bone is at 1.0)."))
			};
			
			return c;
		}
		
		/*
		 * Draws the custom inspector for IKSolverLookAt
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences, SerializedContent[] content) {
			// Main properties
			for (int i = 0; i < 7; i++) AddClampedFloat(content[i]);
			AddClampedInt(content[7], 0, 3);

			// Spine Weight curve
			AddContent(content[11]);
			
			// References
			if (showReferences) {
				EditorGUILayout.Space();
				AddContent(content[8]);
				
				EditorGUILayout.Space();
				AddArray(content[9], editHierarchy, false, null, null, DrawArrayElementLabelBone);
				
				EditorGUILayout.Space();
				AddArray(content[10], editHierarchy, false, null, null, DrawArrayElementLabelBone);
			}
			
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverLookAt
		 * */
		public static void AddScene(IKSolverLookAt solver, Color color, bool modifiable) {
			// Protect from null reference errors
			if (!solver.IsValid(false)) return;
			
			// Display the Spine
			if (solver.spine.Length > 0) {
				Handles.color = color;
				GUI.color = color;
				
				for (int i = 0; i < solver.spine.Length; i++) {
					IKSolverLookAt.LookAtBone bone = solver.spine[i];
					
					if (i < solver.spine.Length - 1) Handles.DrawLine(bone.transform.position, solver.spine[i + 1].transform.position);
					Handles.SphereCap(0, bone.transform.position, Quaternion.identity, jointSize);
				}
				
				// Draw a transparent line from last bone to IKPosition
				if (Application.isPlaying) {
					Handles.color = new Color(color.r, color.g, color.b, color.a * solver.IKPositionWeight * solver.bodyWeight);
					Handles.DrawLine(solver.spine[solver.spine.Length - 1].transform.position, solver.IKPosition);
				}
			}
			
			// Display the eyes
			if (solver.eyes.Length > 0) {
				for (int i = 0; i < solver.eyes.Length; i++) {
					DrawLookAtBoneInScene(solver.eyes[i], solver.IKPosition, color, solver.IKPositionWeight * solver.eyesWeight);
				}
			}
			
			// Display the head
			if (solver.head.transform != null) {
				DrawLookAtBoneInScene(solver.head, solver.IKPosition, color, solver.IKPositionWeight * solver.headWeight);
			}
			
			Handles.color = color;
			GUI.color = color;
			
			// Selecting joint and manipulating IKPosition
			if (Application.isPlaying && solver.IKPositionWeight > 0) {
				if (modifiable) {
					Handles.SphereCap(0, solver.IKPosition, Quaternion.identity, selectedSize);
						
					// Manipulating position
					solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
				}
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
		
		private static void DrawArrayElementLabelBone(SerializedProperty bone, bool editHierarchy) {
			AddObjectReference(bone.FindPropertyRelative("transform"), GUIContent.none, editHierarchy, 0, 300);
		}
		
		private static void DrawLookAtBoneInScene(IKSolverLookAt.LookAtBone bone, Vector3 IKPosition, Color color, float lineWeight) {
			Handles.color = color;
			GUI.color = color;
					
			Handles.SphereCap(0, bone.transform.position, Quaternion.identity, jointSize);
			
			// Draw a transparent line from last bone to IKPosition
			if (Application.isPlaying && lineWeight > 0) {
				Handles.color = new Color(color.r, color.g, color.b, color.a * lineWeight);
				Handles.DrawLine(bone.transform.position, IKPosition);
			}
		}
	}
}

