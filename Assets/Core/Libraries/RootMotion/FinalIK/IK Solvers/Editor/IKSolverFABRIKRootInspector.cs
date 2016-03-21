using UnityEngine;
using UnityEditor;
using System.Collections;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverFABRIKRoot
	 * */
	public class IKSolverFABRIKRootInspector : IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = new SerializedContent[4] {
				new SerializedContent(prop.FindPropertyRelative("iterations"), new GUIContent("Iterations", "Solver iterations.")),
				new SerializedContent(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight.")),
				new SerializedContent(prop.FindPropertyRelative("rootPin"), new GUIContent("Root Pin", "Weight of keeping all FABRIK Trees pinned to the root position.")),
				new SerializedContent(prop.FindPropertyRelative("chains"), new GUIContent("Chains", "FABRIK chains.")),
				};
			
			return c;
		}
		
		/*
		 * Draws the custom inspector for IKSolverFABRIKRoot
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, SerializedContent[] content) {
			
			AddClampedInt(content[0], 0, int.MaxValue);
			AddClampedFloat(content[1]);
			AddClampedFloat(content[2]);
			
			EditorGUILayout.Space();

			EditorGUI.indentLevel = 0;
			AddContent(content[3], true);
			
			EditorGUILayout.Space();
		}
		
		/*
		 * Draws the scene view helpers for IKSolverFABRIKRoot
		 * */
		public static void AddScene(IKSolverFABRIKRoot solver, Color color, bool modifiable, ref FABRIKChain selected) {
			// Protect from null reference errors
			if (!solver.IsValid(false)) return;
			
			Handles.color = color;
			
			// Selecting solvers
			if (Application.isPlaying) {
				SelectChain(solver.chains, ref selected, color);
			}
			
			AddSceneChain(solver.chains, color, selected);
			
			// Root pin
			Handles.color = new Color(Mathf.Lerp(1f, color.r, solver.rootPin), Mathf.Lerp(1f, color.g, solver.rootPin), Mathf.Lerp(1f, color.b, solver.rootPin), Mathf.Lerp(0.5f, 1f, solver.rootPin));
			if (solver.GetRoot() != null) {
				Handles.DrawLine(solver.chains[0].ik.solver.bones[0].transform.position, solver.GetRoot().position);
				Handles.CubeCap(0, solver.GetRoot().position, Quaternion.identity, buttonSize * sizeMlp);
			}
		}
		
		#endregion Public methods
		
		private const float sizeMlp = 1f;
		private static Color col, midColor, endColor;
		
		private static void SelectChain(FABRIKChain[] chain, ref FABRIKChain selected, Color color) {
			foreach (FABRIKChain c in chain) {
				if (c.ik.solver.IKPositionWeight > 0 && selected != c) {
					Handles.color = GetChainColor(c, color);
					if (Handles.Button(c.ik.solver.GetIKPosition(), Quaternion.identity, buttonSize * sizeMlp, buttonSize * sizeMlp, Handles.DotCap)) {
						selected = c;
						return;
					}
				}
			}
		}
		
		private static Color GetChainColor(FABRIKChain chain, Color color) {
			float midWeight = chain.pin;
			midColor = new Color(Mathf.Lerp(1f, color.r, midWeight), Mathf.Lerp(1f, color.g, midWeight), Mathf.Lerp(1f, color.b, midWeight), Mathf.Lerp(0.5f, 1f, midWeight));
			
			float endWeight = chain.pull;
			endColor = new Color(Mathf.Lerp(1f, color.r, endWeight), Mathf.Lerp(0f, color.g, endWeight), Mathf.Lerp(0f, color.b, endWeight), Mathf.Lerp(0.5f, 1f, endWeight));
			
			return chain.children.Length == 0? endColor: midColor;
		}
		
		private static void AddSceneChain(FABRIKChain[] chain, Color color, FABRIKChain selected) {
			foreach (FABRIKChain c in chain) {
				col = GetChainColor(c, color);
				
				IKSolverHeuristicInspector.AddScene(c.ik.solver as IKSolverHeuristic, col, selected == c, sizeMlp);
			}
		}
	}
}

