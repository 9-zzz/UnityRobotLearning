using UnityEditor;
using UnityEngine;
using System.Collections;

using RootMotion;
using RootMotion.FinalIK;


/*
    * Custom inspector for CrossfadeLookAtIK.
    * */
[CustomEditor(typeof(CrossfadeLookAtIK))]
public class CrossfadeLookAtIKInspector : IKInspector
{

    private CrossfadeLookAtIK script { get { return target as CrossfadeLookAtIK; } }

    protected override MonoBehaviour GetMonoBehaviour(out int executionOrder)
    {
        executionOrder = 9997;
        return script;
    }

    protected override SerializedContent[] FindContent()
    {
        return IKSolverLookAtInspector.FindContent(solver);
    }

    protected override void OnApplyModifiedProperties()
    {
        if (!Application.isPlaying) script.solver.Initiate(script.transform);
    }

    protected override void AddInspector()
    {
        // Draw the inspector for IKSolverTrigonometric
        IKSolverLookAtInspector.AddInspector(solver, !Application.isPlaying, true, content);
    }

    void OnSceneGUI()
    {
        // Draw the scene veiw helpers
        IKSolverLookAtInspector.AddScene(script.solver, new Color(0f, 1f, 1f, 1f), true);
    }
}

