using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverTrigonometric
	 * */
	public class IKSolverTrigonometricInspector: IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = new SerializedContent[6] {
				new SerializedContent(prop.FindPropertyRelative("bone1.transform"), new GUIContent("Bone 1", "The first bone in the hierarchy (thigh or upper arm).")),
				new SerializedContent(prop.FindPropertyRelative("bone2.transform"), new GUIContent("Bone 2", "The second bone in the hierarchy (calf or forearm).")),
				new SerializedContent(prop.FindPropertyRelative("bone3.transform"), new GUIContent("Bone 3", "The last bone in the hierarchy (foot or hand).")),
				new SerializedContent(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Position Weight", "Solver weight for smooth blending.")),
				new SerializedContent(prop.FindPropertyRelative("IKRotationWeight"), new GUIContent("Rotation Weight", "Weight of last bone's rotation to IKRotation.")),
				new SerializedContent(prop.FindPropertyRelative("bendNormal"), new GUIContent("Bend Normal", "Bend plane normal."))
			};
			
			return c;
		}
		
		/*
		 * Draws the custom inspector for IKSolverTrigonometric
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences, SerializedContent[] content) {
			EditorGUI.indentLevel = 0;
			
			// Bone references
			if (showReferences) {
				EditorGUILayout.Space();
				for (int i = 0; i < 3; i++) {
					AddObjectReference(content[i].prop, content[i].guiContent, editHierarchy, 100);
				}
				EditorGUILayout.Space();
			}

			AddClampedFloat(content[3]);
			AddClampedFloat(content[4]);
			
			if (showReferences) AddContent(content[5], true);
		}
		
		public static void AddTestInspector(SerializedObject obj) {
			SerializedProperty solver = obj.FindProperty("solver");
			EditorGUILayout.PropertyField(solver.FindPropertyRelative("bendNormal"));
		}
		
		/*
		 * Draws the scene view helpers for IKSolverTrigonometric
		 * */
		public static void AddScene(IKSolverTrigonometric solver, Color color, bool modifiable) {
			if (!solver.IsValid(true)) return;
			
			Handles.color = color;
			GUI.color = color;
			
			Vector3 bendPosition = solver.bone2.transform.position;
			Vector3 endPosition = solver.bone3.transform.position;
			
			// Chain lines
			Handles.DrawLine(solver.bone1.transform.position, bendPosition);
			Handles.DrawLine(bendPosition, endPosition);
			
			// Joints
			Handles.SphereCap(0, solver.bone1.transform.position, Quaternion.identity, jointSize);
			Handles.SphereCap(0, bendPosition, Quaternion.identity, jointSize);
			Handles.SphereCap(0, endPosition, Quaternion.identity, jointSize);
			
			if (Application.isPlaying && (solver.IKPositionWeight > 0 || solver.IKRotationWeight > 0)) {
				if (modifiable) {
					Handles.CubeCap(0, solver.IKPosition, solver.IKRotation, selectedSize);
						
					// Manipulating position and rotation
					switch(Tools.current) {
					case Tool.Move:
						solver.IKPosition = Handles.PositionHandle(solver.IKPosition, Quaternion.identity);
						break;
					case Tool.Rotate:
						solver.IKRotation = Handles.RotationHandle(solver.IKRotation, solver.IKPosition);
						break;
					}
				}
				
				// Target
				Handles.color = new Color(color.r, color.g, color.b, color.a * Mathf.Max(solver.IKPositionWeight, solver.IKRotationWeight));
				Handles.DrawLine(endPosition, solver.IKPosition);
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}
		
		#endregion Public methods
	}
}

