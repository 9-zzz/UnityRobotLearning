using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

	namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverFullBody
	 * */
	public class IKSolverFullBodyInspector : IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = new SerializedContent[5] {
				new SerializedContent(prop.FindPropertyRelative("IKPositionWeight"), new GUIContent("Weight", "Solver weight for smooth blending.")),
				new SerializedContent(prop.FindPropertyRelative("iterations"), new GUIContent("Iterations", "Solver iterations per frame.")),
				new SerializedContent(prop.FindPropertyRelative("effectors"), new GUIContent("Effectors", string.Empty)),
				new SerializedContent(prop.FindPropertyRelative("bendConstraints"), new GUIContent("Bend Constraints", string.Empty)),
				new SerializedContent(prop.FindPropertyRelative("chain"), new GUIContent("Chain", string.Empty)),
			};
			
			return c;
		}
		
		/*
		 * Draws the custom inspector for IKSolverFullBody
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool editWeights, SerializedContent[] content) {
			AddClampedFloat(content[0]);
			AddClampedInt(content[1], 1, int.MaxValue);	
		}

		/*
		 * Draws the scene view helpers for IKSolverFullBody
		 * */
		public static void AddScene(IKSolverFullBody solver, Color color, bool modifiable, ref int selectedEffector, float size) {
			if (!solver.IsValid(false)) return;
			if (!Application.isPlaying) return;
			
			// Effectors
			for (int i = 0; i < solver.effectors.Length; i++) {
				bool rotate = solver.effectors[i].isEndEffector;
				float weight = rotate? Mathf.Max(solver.effectors[i].positionWeight, solver.effectors[i].rotationWeight): solver.effectors[i].positionWeight;
				
				if (weight > 0 && selectedEffector != i) {
					Handles.color = color;
					
					if (rotate) {
						if (Handles.Button(solver.effectors[i].position, solver.effectors[i].rotation, size * 0.5f, size * 0.5f, Handles.DotCap)) {
							selectedEffector = i;
							return;
						}
					} else {
						if (Handles.Button(solver.effectors[i].position, solver.effectors[i].rotation, size, size, Handles.SphereCap)) {
							selectedEffector = i;
							return;
						}
					}
				}
			}
			
			for (int i = 0; i < solver.effectors.Length; i++) IKEffectorInspector.AddScene(solver.effectors[i], color, modifiable && i == selectedEffector, size);
		}
		
		#endregion Public methods
		
		public static void AddChain(FBIKChain[] chain, int index, Color color, float size) {
			Handles.color = color;
			
			for (int i = 0; i < chain[index].nodes.Length - 1; i++) {
				Handles.DrawLine(GetNodePosition(chain[index].nodes[i]), GetNodePosition(chain[index].nodes[i + 1]));
				Handles.SphereCap(0, GetNodePosition(chain[index].nodes[i]), Quaternion.identity, size);
			}
			
			Handles.SphereCap(0, GetNodePosition(chain[index].nodes[chain[index].nodes.Length - 1]), Quaternion.identity, size);

			for (int i = 0; i < chain[index].children.Length; i++) {
				Handles.DrawLine(GetNodePosition(chain[index].nodes[chain[index].nodes.Length - 1]), GetNodePosition(chain[chain[index].children[i]].nodes[0]));
			}
		}
		
		private static Vector3 GetNodePosition(IKSolver.Node node) {
			if (Application.isPlaying) return node.solverPosition;
			return node.transform.position;
		}
	}
}
