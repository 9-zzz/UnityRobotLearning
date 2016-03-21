using UnityEngine;
using System.Collections;

using RootMotion;
using RootMotion.FinalIK;

/// <summary>
/// Full Body %IK System designed specifically for bipeds
/// </summary>
[AddComponentMenu("Scripts/RootMotion/IK/Crossfade FBBIK")]
public class CrossfadeFBBIK : IK
{

    // Reinitiates the solver to the current references
    [ContextMenu("Reinitiate")]
    void Reinitiate()
    {
        SetReferences(references, solver.rootNode);
    }

    /// <summary>
    /// The biped definition. Don't change refences directly in runtime, use SetReferences(BipedReferences references) instead.
    /// </summary>
    public BipedReferences references = new BipedReferences();

    /// <summary>
    /// The FullBodyBiped %IK solver.
    /// </summary>
    public IKSolverFullBodyBiped solver = new IKSolverFullBodyBiped();

    /// <summary>
    /// Sets the solver to new biped references.
    /// </summary>
    /// /// <param name="references">Biped references.</param>
    /// <param name="rootNode">Root node. if null, will try to detect the root node bone automatically. </param>
    public void SetReferences(BipedReferences references, Transform rootNode)
    {
        this.references = references;
        solver.SetToReferences(this.references, rootNode);
    }

    public override IKSolver GetIKSolver()
    {
        return solver as IKSolver;
    }

    protected override void UpdateSolver()
    {
        if (!GetIKSolver().initiated) InitiateSolver();
        if (!GetIKSolver().initiated) return;

        //GetIKSolver().Update();
    }
}
