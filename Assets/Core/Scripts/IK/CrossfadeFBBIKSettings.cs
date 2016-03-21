using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

/// <summary>
/// Manages FBBIK settings that are not visible in the FBBIK custom inspector.
/// </summary>
public class CrossfadeFBBIKSettings : MonoBehaviour
{

    /// <summary>
    /// Settings for a limb
    /// </summary>
    [System.Serializable]
    public class Limb
    {
        public FBIKChain.ReachSmoothing reachSmoothing; // Smoothing of the Reach effect (since 0.2)
        public float maintainRelativePositionWeight; // Weight of maintaining the limb's position relative to the body part that it is attached to (since 0.2, used to be IKEffector.Mode.MaintainRelativePosition)
        public float mappingWeight = 1f;

        // Apply the settings
        public void Apply(FullBodyBipedChain chain, IKSolverFullBodyBiped solver)
        {
            solver.GetChain(chain).reachSmoothing = reachSmoothing;
            solver.GetEndEffector(chain).maintainRelativePositionWeight = maintainRelativePositionWeight;
            solver.GetLimbMapping(chain).weight = mappingWeight;
        }
    }

    [HideInInspector]
    public CrossfadeFBBIK ik1; // Reference to the FBBIK component 1

    [HideInInspector]
    public CrossfadeFBBIK ik2; // Reference to the FBBIK component 2

    public bool disableAfterStart; // If true, will not update after Start
    public Limb leftArm, rightArm, leftLeg, rightLeg; // The Limbs

    public float rootPin = 0f; // Weight of pinning the root node to it's animated position
    public bool bodyEffectChildNodes = true; // If true, the body effector will also drag the thigh effectors

    void Awake()
    {
        CrossfadeFBBIK[] iks = this.GetComponents<CrossfadeFBBIK>();
        this.ik1 = iks[0];
        this.ik2 = iks[1];
    }

    // Apply all the settings to the FBBIK solver
    public void UpdateSettings()
    {
        leftArm.Apply(FullBodyBipedChain.LeftArm, ik1.solver);
        rightArm.Apply(FullBodyBipedChain.RightArm, ik1.solver);
        leftLeg.Apply(FullBodyBipedChain.LeftLeg, ik1.solver);
        rightLeg.Apply(FullBodyBipedChain.RightLeg, ik1.solver);

        ik1.solver.chain[0].pin = rootPin;
        ik1.solver.bodyEffector.effectChildNodes = bodyEffectChildNodes;

        leftArm.Apply(FullBodyBipedChain.LeftArm, ik2.solver);
        rightArm.Apply(FullBodyBipedChain.RightArm, ik2.solver);
        leftLeg.Apply(FullBodyBipedChain.LeftLeg, ik2.solver);
        rightLeg.Apply(FullBodyBipedChain.RightLeg, ik2.solver);

        ik2.solver.chain[0].pin = rootPin;
        ik2.solver.bodyEffector.effectChildNodes = bodyEffectChildNodes;
    }

    void Start()
    {
        UpdateSettings();
        if (disableAfterStart) this.enabled = false;
    }

    void Update()
    {
        UpdateSettings();
    }
}