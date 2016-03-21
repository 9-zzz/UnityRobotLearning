using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IK solvers extending IKSolverHeuristic
	 * */
	public class IKSolverHeuristicInspector: IKSolverInspector {

		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = new SerializedContent[5] {
				new SerializedContent(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight for smooth blending.")),
				new SerializedContent(prop.FindPropertyRelative("tolerance"), new GUIContent("Tolerance", "Minimum offset from last reached position. Will stop solving if offset is less than tolerance. If tolerance is zero, will iterate until maxIterations.")),
				new SerializedContent(prop.FindPropertyRelative("maxIterations"), new GUIContent("Max Iterations", "Max solver iterations per frame.")),
				new SerializedContent(prop.FindPropertyRelative("useRotationLimits"), new GUIContent("Use Rotation Limits", "If true, rotation limits (if excisting) will be applied on each iteration.")),
				new SerializedContent(prop.FindPropertyRelative("bones"), new GUIContent("Bones", string.Empty))
				};
			
			return c;
		}
		
		/*
		 * Draws the custom inspector for IKSolverHeuristic
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool editWeights, SerializedContent[] content) {
			AddClampedFloat(content[0]);
			AddClampedFloat(content[1], 0f, Mathf.Infinity);
			AddClampedInt(content[2], 1, int.MaxValue);
			AddContent(content[3]);
			
			EditorGUILayout.Space();
			weights = editWeights;
			if (editHierarchy || editWeights) AddArray(content[4], editHierarchy, false, null, OnAddToArrayBone, DrawArrayElementLabelBone);
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverHeuristic
		 * */
		public static void AddScene(IKSolverHeuristic solver, Color color, bool modifiable, float sizeMlp = 1f) {
			// Protect from null reference errors
			if (!solver.IsValid(false)) return;
			
			Handles.color = color;
			GUI.color = color;
			
			// Display the bones
			for (int i = 0; i < solver.bones.Length; i++) {
				IKSolver.Bone bone = solver.bones[i];

				if (i < solver.bones.Length - 1) Handles.DrawLine(bone.transform.position, solver.bones[i + 1].transform.position);
				Handles.SphereCap(0, solver.bones[i].transform.position, Quaternion.identity, jointSize);
			}
			
			// Selecting joint and manipulating IKPosition
			if (Application.isPlaying && solver.IKPositionWeight > 0) {
				if (modifiable) {
					Handles.CubeCap(0, solver.IKPosition, solver.GetRoot().rotation, selectedSize * sizeMlp);
						
					// Manipulating position
					solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
				}
				
				// Draw a transparent line from last bone to IKPosition
				Handles.color = new Color(color.r, color.g, color.b, color.a * solver.IKPositionWeight);
				Handles.DrawLine(solver.bones[solver.bones.Length - 1].transform.position, solver.IKPosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
		
		private static bool weights;
		
		private static void DrawArrayElementLabelBone(SerializedProperty bone, bool editHierarchy) {
			AddObjectReference(bone.FindPropertyRelative("transform"), GUIContent.none, editHierarchy, 0, weights? 200: 300);
			if (weights) AddWeightSlider(bone.FindPropertyRelative("weight"), 0f, 1f, GUILayout.MinWidth(50));
		}
		
		private static void OnAddToArrayBone(SerializedProperty bone) {
			bone.FindPropertyRelative("weight").floatValue = 1f;
		}
		
		private static void AddWeightSlider(SerializedProperty prop, float min = 0f, float max = 1f, params GUILayoutOption [] options) {
			GUILayout.Label("Weight", GUILayout.Width(45));
			prop.floatValue = GUILayout.HorizontalSlider(prop.floatValue, min, max, options);
			EditorGUILayout.PropertyField(prop, new GUIContent(string.Empty, string.Empty), GUILayout.Width(50));
			prop.floatValue = Mathf.Clamp(prop.floatValue, min, max);
		}
	}
}
