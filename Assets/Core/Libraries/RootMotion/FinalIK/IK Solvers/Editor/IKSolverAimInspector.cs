using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverAim
	 * */
	public class IKSolverAimInspector: IKSolverInspector {

		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = IKSolverHeuristicInspector.FindContent(prop);
			
			Array.Resize(ref c, c.Length + 4);
			c[c.Length - 4] = new SerializedContent(prop.FindPropertyRelative("transform"), new GUIContent("Aim Transform", "The transform that's you want to be aimed at IKPosition. Needs to be a lineal descendant of the bone hierarchy."));
			c[c.Length - 3] = new SerializedContent(prop.FindPropertyRelative("axis"), new GUIContent("Axis", "The local axis of the Transform that you want to be aimed at IKPosition."));
			c[c.Length - 2] = new SerializedContent(prop.FindPropertyRelative("clampWeight"), new GUIContent("Clamp Weight", "Clamping rotation of the solver. 0 is free rotation, 1 is completely clamped to transform axis."));
			c[c.Length - 1] = new SerializedContent(prop.FindPropertyRelative("clampSmoothing"), new GUIContent("Clamp Smoothing", "Number of sine smoothing iterations applied on clamping to make the clamping point smoother."));
			
			return c;
		}
		
		/// <summary>
		/// Draws the custom inspector for IKSolverAim
		/// </summary>
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, SerializedContent[] content) {
			AddContent(content[content.Length - 4]);
			AddContent(content[content.Length - 3], true);
			
			EditorGUILayout.Space();
			
			AddClampedFloat(content[content.Length - 2]);
			AddClampedInt(content[content.Length - 1], 0, 3);
			
			IKSolverHeuristicInspector.AddInspector(prop, editHierarchy, true, content);
		}
		
		/// <summary>
		/// Draws the scene view helpers for IKSolverAim
		/// </summary>
		public static void AddScene(IKSolverAim solver, Color color, bool modifiable) {
			// Protect from null reference errors
			if (!solver.IsValid(false)) return;
			if (solver.transform == null) return;
			
			Handles.color = color;
			GUI.color = color;
			
			// Display the bones
			for (int i = 0; i < solver.bones.Length; i++) {
				IKSolver.Bone bone = solver.bones[i];

				if (i < solver.bones.Length - 1) Handles.DrawLine(bone.transform.position, solver.bones[i + 1].transform.position);
				Handles.SphereCap(0, solver.bones[i].transform.position, Quaternion.identity, jointSize);
			}
			
			if (solver.axis != Vector3.zero) Handles.ConeCap(0, solver.transform.position, Quaternion.LookRotation(solver.transform.rotation * solver.axis), jointSize * 2f);
			
			// Selecting joint and manipulating IKPosition
			if (Application.isPlaying && solver.IKPositionWeight > 0) {
				if (modifiable) {
					Handles.SphereCap(0, solver.IKPosition, Quaternion.identity, selectedSize);
						
					// Manipulating position
					solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
				}
				
				// Draw a transparent line from transform to IKPosition
				Handles.color = new Color(color.r, color.g, color.b, color.a * solver.IKPositionWeight);
				Handles.DrawLine(solver.bones[solver.bones.Length - 1].transform.position, solver.transform.position);
				Handles.DrawLine(solver.transform.position, solver.IKPosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods

	}
}
