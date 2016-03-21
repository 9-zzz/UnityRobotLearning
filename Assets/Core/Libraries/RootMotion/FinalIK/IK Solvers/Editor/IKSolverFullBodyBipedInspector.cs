using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

namespace RootMotion.FinalIK {

	/*
	 * Custom inspector and scene view tools for IKSolverFullBodyBiped
	 * */
	public class IKSolverFullBodyBipedInspector : IKSolverInspector {
		
		#region Public methods
		
		/*
		 * Returns all solver SeiralizedProperties
		 * */
		public static SerializedContent[] FindContent(SerializedProperty prop) {
			SerializedContent[] c = IKSolverFullBodyInspector.FindContent(prop);
			Array.Resize(ref c, c.Length + 7);
			
			c[c.Length - 1] = new SerializedContent(prop.FindPropertyRelative("pullBodyVertical"), new GUIContent("Pull Body Vertical", "Weight of hand effectors pulling the body vertically."));
			c[c.Length - 2] = new SerializedContent(prop.FindPropertyRelative("limbMappings"), GUIContent.none);
			c[c.Length - 3] = new SerializedContent(prop.FindPropertyRelative("spineMapping"), GUIContent.none);
			c[c.Length - 4] = new SerializedContent(prop.FindPropertyRelative("boneMappings"), GUIContent.none);
			c[c.Length - 5] = new SerializedContent(prop.FindPropertyRelative("rootNode"), new GUIContent("Root Node", "Select one of the bones in the (lower) spine."));
			c[c.Length - 6] = new SerializedContent(prop.FindPropertyRelative("spineStiffness"), new GUIContent("Spine Stiffness", "The bend resistance of the spine."));
			c[c.Length - 7] = new SerializedContent(prop.FindPropertyRelative("pullBodyHorizontal"), new GUIContent("Pull Body Horizontal", "Weight of hand effectors pulling the body horizontally."));
			return c;
		}
		
		public static void AddReferences(bool editHierarchy, SerializedContent[] content) {
			// RootNode
			if (editHierarchy) {
				AddContent(content[content.Length - 5]);
			}
		}
		
		/*
		 * Draws the custom inspector for IKSolverFullBodybiped
		 * */
		public static void AddInspector(SerializedProperty prop, bool editHierarchy, bool editWeights, SerializedContent[] content) {
			IKSolverFullBodyInspector.AddInspector(prop, editHierarchy, editWeights, content);
			
			// Spine Iteration
			AddClampedInt(content[content.Length - 3].prop.FindPropertyRelative("iterations"), new GUIContent("Spine Mapping Iterations", "Iterations of FABRIK solver mapping the spine to the FBIK nodes."), 1, 10);
			
			// Spine Stiffness
			AddClampedFloat(content[content.Length - 6]);
			
			// Pull Body
			AddClampedFloat(content[content.Length - 1], -1f, 1f);
			AddClampedFloat(content[content.Length - 7], -1f, 1f);

			// Chain
			EditorGUILayout.Space();
			EditorGUI.indentLevel = 0;

			EditorGUILayout.PropertyField(content[4].prop, new GUIContent("Chain", "The node chain."));
			if (content[4].prop.isExpanded) {
				AddPulls(content[4].prop);
				EditorGUILayout.Space();
			}
			
			// Effectors
			EditorGUI.indentLevel = 0;
			
			EditorGUILayout.PropertyField(content[2].prop, new GUIContent("Effectors", "Effectors for manipulating the node chain."));
			if (content[2].prop.isExpanded) {
				
				// Body
				AddEffector(content[2].prop.GetArrayElementAtIndex(0), new GUIContent("Body"), false);
				
				// Left Shoulder
				AddEffector(content[2].prop.GetArrayElementAtIndex(1),new GUIContent("L Shoulder"), false);
				
				// Right Shoulder
				AddEffector(content[2].prop.GetArrayElementAtIndex(2),new GUIContent("R Shoulder"), false);
				
				// Left Thigh
				AddEffector(content[2].prop.GetArrayElementAtIndex(3),new GUIContent("L Thigh"), false);
				
				// Right Thigh
				AddEffector(content[2].prop.GetArrayElementAtIndex(4),new GUIContent("R Thigh"), false);
				
				// Left Hand
				AddEffector(content[2].prop.GetArrayElementAtIndex(5), new GUIContent("L Hand"));
				
				// Right Hand
				AddEffector(content[2].prop.GetArrayElementAtIndex(6), new GUIContent("R Hand"));
				
				// Left Foot
				AddEffector(content[2].prop.GetArrayElementAtIndex(7), new GUIContent("L Foot"));
				
				// Right Foot
				AddEffector(content[2].prop.GetArrayElementAtIndex(8), new GUIContent("R Foot"));
				
				EditorGUILayout.Space();
			}
			
			EditorGUI.indentLevel = 0;
			
			// Mappings
			EditorGUILayout.PropertyField(content[content.Length - 3].prop, new GUIContent("Mapping", "Options for mapping bones to their solver positions."));
			if (content[content.Length - 3].prop.isExpanded) {
				AddSpineMapping(content[content.Length - 3].prop, new GUIContent("Spine"));
				
				AddMapping(content[content.Length - 2].prop.GetArrayElementAtIndex(0), new GUIContent("Left Hand"));
				AddMapping(content[content.Length - 2].prop.GetArrayElementAtIndex(1), new GUIContent("Right Hand"));
				AddMapping(content[content.Length - 2].prop.GetArrayElementAtIndex(2), new GUIContent("Left Foot"));
				AddMapping(content[content.Length - 2].prop.GetArrayElementAtIndex(3), new GUIContent("Right Foot"));
				
				AddMapping(content[content.Length - 4].prop.GetArrayElementAtIndex(0), new GUIContent("Head"));
				
			}
		}


		/*
		 * Draws the scene view helpers for IKSolverFullBodyBiped
		 * */
		public static void AddScene(IKSolverFullBodyBiped solver, Color color, bool modifiable, ref int selectedEffector, Transform root) {
			if (!solver.IsValid(false)) return;

			float heightF = Vector3.Distance(solver.chain[1].nodes[0].transform.position, solver.chain[1].nodes[1].transform.position) + 
				Vector3.Distance(solver.chain[3].nodes[0].transform.position, solver.chain[3].nodes[1].transform.position);

			float size = Mathf.Clamp(heightF * 0.075f, 0.001f, Mathf.Infinity);

			// Chain
			if (!Application.isPlaying) {
				for (int i = 0; i < solver.chain.Length; i++) {
					IKSolverFullBodyInspector.AddChain(solver.chain, i, color, size);
				}

				Handles.DrawLine(solver.chain[1].nodes[0].transform.position, solver.chain[2].nodes[0].transform.position);
				Handles.DrawLine(solver.chain[3].nodes[0].transform.position, solver.chain[4].nodes[0].transform.position);

				AddLimbHelper(solver.chain[1], size);
				AddLimbHelper(solver.chain[2], size);
				AddLimbHelper(solver.chain[3], size, root);
				AddLimbHelper(solver.chain[4], size, root);
			}
			
			// Effectors
			IKSolverFullBodyInspector.AddScene(solver, color, modifiable, ref selectedEffector, size);
		}

		/*
		 * Scene view handles to help with limb setup
		 * */
		private static void AddLimbHelper(FBIKChain chain, float size, Transform root = null) {
			Vector3 cross = Vector3.Cross((chain.nodes[1].transform.position - chain.nodes[0].transform.position).normalized, (chain.nodes[2].transform.position - chain.nodes[0].transform.position).normalized);

			Vector3 bendDirection = -Vector3.Cross(cross.normalized, (chain.nodes[2].transform.position - chain.nodes[0].transform.position).normalized);

			if (bendDirection != Vector3.zero) {
				Color c = Handles.color;
				bool inverted = root != null && Vector3.Dot(root.forward, bendDirection.normalized) < 0f;

				// Inverted bend direction
				if (inverted) {
					GUI.color = new Color(1f, 0.75f, 0.75f);
					Handles.color = Color.yellow;

					if (Handles.Button(chain.nodes[1].transform.position, Quaternion.identity, size * 0.5f, size, Handles.DotCap)) {
						Warning.logged = false;
						Warning.Log("The bend direction of this limb appears to be inverted. Please rotate this bone so that the limb is bent in it's natural bending direction. If this limb is supposed to be bent in the direction pointed by the arrow, ignore this warning.", root, true);
					}
				}

				Handles.ArrowCap(0, chain.nodes[1].transform.position, Quaternion.LookRotation(bendDirection), size * 2f);

				GUI.color = Color.white;
				Handles.color = c;
			} else {
				// The limb is completely stretched out
				Color c = Handles.color;
				Handles.color = Color.red;
				GUI.color = new Color(1f, 0.75f, 0.75f);

				if (Handles.Button(chain.nodes[1].transform.position, Quaternion.identity, size * 0.5f, size, Handles.DotCap)) {
					Warning.logged = false;
					Warning.Log("The limb is completely stretched out. Full Body Biped IK does not know which way the limb should be bent. Please rotate this bone slightly in it's bending direction.", root, true);
				}

				GUI.color = Color.white;
				Handles.color = c;
			}
		}
		
		#endregion Public methods
		
		private static SerializedProperty spineChildren, reach;
		
		private static void AddPulls(SerializedProperty chain) {
			if (chain.arraySize < 5) return;
			
			AddPull(chain.GetArrayElementAtIndex(1), new GUIContent("Left Arm", string.Empty));
			AddPull(chain.GetArrayElementAtIndex(2), new GUIContent("Right Arm", string.Empty));
			AddPull(chain.GetArrayElementAtIndex(3), new GUIContent("Left Leg", string.Empty));
			AddPull(chain.GetArrayElementAtIndex(4), new GUIContent("Right Leg", string.Empty));		
		}
		
		private static void AddLabel(GUIContent guiContent) {
			EditorGUI.indentLevel = 1;
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(guiContent, GUILayout.Width(95));
		}
		
		private static void AddPull(SerializedProperty pull, GUIContent guiContent, bool addReach = true) {
			AddLabel(guiContent);

			EditorGUILayout.LabelField(new GUIContent("Pull", "The weight of pulling other chains."), GUILayout.Width(45));
			pull.FindPropertyRelative("pull").floatValue = GUILayout.HorizontalSlider(pull.FindPropertyRelative("pull").floatValue, 0f, 1f, GUILayout.Width(50));
			
			GUILayout.Space(20);
			
			if (addReach) {
				EditorGUILayout.LabelField(new GUIContent("Reach", "Pulls the first node closer to the last node of the chain."), GUILayout.Width(65));
				pull.FindPropertyRelative("reach").floatValue = GUILayout.HorizontalSlider(pull.FindPropertyRelative("reach").floatValue, 0f, 1f, GUILayout.Width(50));
			}
			
			GUILayout.EndHorizontal();
		}
		
		private static void AddEffector(SerializedProperty effector, GUIContent guiContent, bool rotation = true) {
			AddLabel(guiContent);
			
			int w = 65;
			
			EditorGUILayout.LabelField(new GUIContent("Position", "Position weight."), GUILayout.Width(w));
			effector.FindPropertyRelative("positionWeight").floatValue = GUILayout.HorizontalSlider(effector.FindPropertyRelative("positionWeight").floatValue, 0f, 1f, GUILayout.Width(50));
			
			if (rotation) {
				EditorGUILayout.LabelField(new GUIContent("Rotation", "Rotation weight."), GUILayout.Width(w));
				effector.FindPropertyRelative("rotationWeight").floatValue = GUILayout.HorizontalSlider(effector.FindPropertyRelative("rotationWeight").floatValue, 0f, 1f, GUILayout.Width(50));
			}

			GUILayout.EndHorizontal();
		}
		
		private static void AddMapping(SerializedProperty mapping, GUIContent guiContent) {
			AddLabel(guiContent);
			
			int w = 190;
			
			EditorGUILayout.LabelField(new GUIContent("Maintain Rotation Weight", "The weight of maintaining the bone's animated rotation in world space."), GUILayout.Width(w));
			mapping.FindPropertyRelative("maintainRotationWeight").floatValue = GUILayout.HorizontalSlider(mapping.FindPropertyRelative("maintainRotationWeight").floatValue, 0f, 1f, GUILayout.Width(50));
			
			GUILayout.EndHorizontal();
		}

		private static void AddSpineMapping(SerializedProperty mapping, GUIContent guiContent) {
			AddLabel(guiContent);
			
			int w = 190;

			EditorGUILayout.LabelField(new GUIContent("Twist Weight", "The weight of spine twist."), GUILayout.Width(w));
			mapping.FindPropertyRelative("twistWeight").floatValue = GUILayout.HorizontalSlider(mapping.FindPropertyRelative("twistWeight").floatValue, 0f, 1f, GUILayout.Width(50));
			
			GUILayout.EndHorizontal();
		}
	}
}
