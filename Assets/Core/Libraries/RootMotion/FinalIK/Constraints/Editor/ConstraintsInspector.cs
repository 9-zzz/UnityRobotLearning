using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for Constraints
	 * */
	public class ConstraintsInspector: IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Returns all solver SerializedProperties and wraps them into a SerializedContent with names and tooltips.
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = new SerializedContent[4] {
				new SerializedContent(prop.FindPropertyRelative("positionOffsetConstraint.weight"), new GUIContent("Pos Offset Weight", "The weight of pelvis position offset. You can set position offset by bipedIK.solvers.pelvis.positionOffsetConstraint.offset = value.")),
				new SerializedContent(prop.FindPropertyRelative("positionConstraint.weight"), new GUIContent("Pos Weight", "The weight of pelvis position. You can set pelvis position by bipedIK.solvers.pelvis.positionConstraint.position = value.")),
				new SerializedContent(prop.FindPropertyRelative("rotationOffsetConstraint.weight"), new GUIContent("Rot Offset Weight", "The weight of pelvis rotation offset. You can set rotation offset by bipedIK.solvers.pelvis.rotationOffsetConstraint.offset = value.")),
				new SerializedContent(prop.FindPropertyRelative("rotationConstraint.weight"), new GUIContent("Rot Weight", "The weight of pelvis rotation. You can set pelvis rotation by bipedIK.solvers.pelvis.rotationConstraint.rotation = value."))
			};
			
			return c;
		}

		/*
		 * Draws the custom inspector for Constraints
		 * */
		public static void AddInspector(SerializedProperty prop, SerializedContent[] content) {
			if (!prop.isExpanded) return;
			
			// Main properties
			for (int i = 0; i < 4; i++) AddClampedFloat(content[i]);
			
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for Constraints
		 * */
		public static void AddScene(Constraints constraints, Color color, bool modifiable) {
			if (!constraints.IsValid()) return;
			
			Handles.color = color;
			GUI.color = color;
			
			// Transform
			Handles.SphereCap(0, constraints.transform.position, Quaternion.identity, jointSize);
			
			// Target
			Handles.color = new Color(color.r, color.g, color.b, color.a * constraints.positionConstraint.weight);
			Handles.DrawLine(constraints.transform.position, constraints.positionConstraint.position);
			Handles.color = color;
			
			if (Application.isPlaying && modifiable && (constraints.positionConstraint.weight > 0 || constraints.rotationConstraint.weight > 0)) {
				Handles.CubeCap(0, constraints.positionConstraint.position, constraints.rotationConstraint.rotation, selectedSize);
					
				// Manipulating position and rotation
				switch(Tools.current) {
				case Tool.Move:
					constraints.positionConstraint.position = Handles.PositionHandle(constraints.positionConstraint.position, constraints.rotationConstraint.rotation);
					break;
				case Tool.Rotate:
					constraints.rotationConstraint.rotation = Handles.RotationHandle(constraints.rotationConstraint.rotation, constraints.positionConstraint.position);
					break;
				}
			}
			
			Handles.color = Color.white;
			GUI.color = Color.white;
		}

		#endregion Public methods
	}
}
