using UnityEngine;
using System.Collections;

using RootMotion.FinalIK;

/// <summary>
/// Rotates a hierarchy of bones to face a target
/// </summary>
public class CrossfadeLookAtIK : IK
{
    /// <summary>
    /// The LookAt %IK solver.
    /// </summary>
    public IKSolverLookAt solver = new IKSolverLookAt();

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
