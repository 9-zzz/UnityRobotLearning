using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverLimb
	 * */
	public class IKSolverLimbInspector: IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = IKSolverTrigonometricInspector.FindContent(prop);
			Array.Resize(ref c, c.Length + 4);
			
			c[c.Length - 1] = new SerializedContent(prop.FindPropertyRelative("goal"), new GUIContent("Avatar IK Goal", "Avatar IK Goal here is only used by the 'Arm' bend modifier."));
			c[c.Length - 2] = new SerializedContent(prop.FindPropertyRelative("maintainRotationWeight"), new GUIContent("Maintain Rotation Weight", "Weight of rotating the last bone back to the rotation it had before solving IK."));
			c[c.Length - 3] = new SerializedContent(prop.FindPropertyRelative("bendModifier"), new GUIContent("Bend Modifier", "Bend normal modifier."));
			c[c.Length - 4] = new SerializedContent(prop.FindPropertyRelative("bendModifierWeight"), new GUIContent("Bend Modifier Weight", "Weight of the bend modifier."));
			
			return c;
		}
		
		/*
		 * Draws the custom inspector for IKSolverLimb
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool showReferences, SerializedContent[] content) {
			// Draw the trigonometric IK inspector
			IKSolverTrigonometricInspector.AddInspector(prop, editHierarchy, showReferences, content);
			
			EditorGUILayout.Space();
			
			if (showReferences && editHierarchy) AddContent(content[content.Length - 1]);
			AddClampedFloat(content[content.Length - 2]);
			
			// Bend normal modifier.
			AddContent(content[content.Length - 3]);
			AddClampedFloat(content[content.Length - 4]);
			
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverLimb
		 * */
		public static void AddScene(IKSolverLimb solver, Color color, bool modifiable) {
			IKSolverTrigonometricInspector.AddScene(solver as IKSolverTrigonometric, color, modifiable);
		}
		
		#endregion Public methods
	}
}

