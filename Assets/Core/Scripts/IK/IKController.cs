using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RootMotion.FinalIK;

public class IKController : MonoBehaviour 
{
    /// <summary>
    /// Controls IK for head look
    /// </summary>
    private class LookAtIKController
    {
        private CrossfadeLookAtIK lookAt;

        private Vector3 oldTarget;
        private Transform curTarget;

        private Interpolator<Vector3> targetInterp;
        private Interpolator<float> weightInterp;
        private Interpolator<float> controlInterp;

        private float weight; // Current weight

        public LookAtIKController(
            CrossfadeLookAtIK lookAt,
            float bodyWeightMax,
            float bodyWeightDelay)
        {
            this.lookAt = lookAt;

            this.targetInterp =
                new Interpolator<Vector3>(
                    Vector3.zero, Vector3.zero, Vector3.Lerp);

            this.weightInterp =
                new Interpolator<float>(
                    0.0f, 1.0f, Mathf.Lerp);

            this.controlInterp =
                new Interpolator<float>(
                    0.0f, bodyWeightMax, Mathf.Lerp);

            this.controlInterp.ForceMax();
        }

        public void LookAt(Vector3 target, float delay)
        {
            this.weightInterp.ToMax(delay);

            this.targetInterp.SetValues(
                this.lookAt.solver.IKPosition,
                target);
            this.targetInterp.ForceMin();
            this.targetInterp.ToMax(delay);
        }

        public void LookStop(float delay)
        {
            this.weightInterp.ToMin(delay);
        }

        public void Update()
        {
            this.lookAt.solver.IKPositionWeight =
                this.weightInterp.Value;
            this.lookAt.solver.IKPosition = 
                this.targetInterp.Value;
            this.lookAt.solver.bodyWeight =
                this.controlInterp.Value;
        }

        public void LateUpdate()
        {
            this.lookAt.GetIKSolver().Update();
        }

        public void FullBody(float delay)
        {
            this.controlInterp.ToMax(delay);
        }

        public void HeadOnly(float delay)
        {
            this.controlInterp.ToMin(delay);
        }

        public bool IsFullBody()
        {
            return this.controlInterp.State == InterpolationState.Max;
        }
    }

    private enum BodyIKState
    {
        Online,
        Swapping,
        Stopping,
        Offline,
    }

    /// <summary>
    /// Controlls IK for the body
    /// </summary>
    private class BodyIKController
    {
        /// <summary>
        /// Time to take when swapping controllers
        /// </summary>
        public float SwapTime = 0.5f;

        /// <summary>
        /// Called when an InteractionEvent has been started
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyStart;
        /// <summary>
        /// Called when an Interaction has been paused
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyPause;
        /// <summary>
        /// Called when an Interaction has been triggered
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyTrigger;
        /// <summary>
        /// Called when an Interaction has been released
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyRelease;
        /// <summary>
        /// Called when an InteractionObject has been picked up.
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyPickUp;
        /// <summary>
        /// Called when a paused Interaction has been resumed
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyResume;
        /// <summary>
        /// Called when an Interaction has been stopped
        /// </summary>
        public event InteractionSystem.InteractionEvent BodyStop;

        private CrossfadeFBBIK ikPrimary = null;
        private CrossfadeFBBIK ikSecondary = null;

        private CrossfadeInteractionHandler handlerPrimary = null;
        private CrossfadeInteractionHandler handlerSecondary = null;

        private BodyIKState state;
        public BodyIKState State { get { return this.state; } }

        private float swapTimeFinish;

        public Dictionary<FullBodyBipedEffector, InteractionObject> primaryEffectors;
        public Dictionary<FullBodyBipedEffector, InteractionObject> secondaryEffectors;

        public BodyIKController(CrossfadeFBBIK[] iks, float swapTime)
        {
            this.primaryEffectors =
                new Dictionary<FullBodyBipedEffector, InteractionObject>();
            this.secondaryEffectors =
                new Dictionary<FullBodyBipedEffector, InteractionObject>();

            this.ikPrimary = iks[0];
            this.ikSecondary = iks[1];
            this.SwapTime = swapTime;

            this.handlerPrimary = new CrossfadeInteractionHandler(ikPrimary);
            this.handlerSecondary = new CrossfadeInteractionHandler(ikSecondary);

            this.RegisterWithHandler(this.handlerPrimary);
            this.RegisterWithHandler(this.handlerSecondary);

            this.state = BodyIKState.Offline;
        }

        public void Update()
        {
            if (this.state == BodyIKState.Swapping)
            {
                if (Time.time > this.swapTimeFinish)
                {
                    foreach (var kv in this.secondaryEffectors)
                        this.handlerSecondary.StopInteraction(kv.Key);
                    this.secondaryEffectors.Clear();
                    this.state = BodyIKState.Online;
                }
            }
            else if (this.state == BodyIKState.Stopping)
            {
                foreach (var kv in this.secondaryEffectors)
                    this.handlerSecondary.StopInteraction(kv.Key);
                this.secondaryEffectors.Clear();

                foreach (var kv in this.primaryEffectors)
                    this.handlerSecondary.StopInteraction(kv.Key);
                this.secondaryEffectors.Clear();

                if (this.IsActive(this.ikPrimary) == false)
                    this.state = BodyIKState.Offline;
            }
        }

        public void LateUpdate()
        {
            this.handlerPrimary.LateUpdate();
            this.handlerSecondary.LateUpdate();

            this.ikPrimary.GetIKSolver().Update();
            this.ikSecondary.GetIKSolver().Update();
        }

        public void StartInteraction(FullBodyBipedEffector effector, InteractionObject obj)
        {
            if (this.state == BodyIKState.Offline)
            {
                this.primaryEffectors.Add(effector, obj);
                this.handlerPrimary.StartInteraction(effector, obj, true);
                this.state = BodyIKState.Online;
            }
            else if (this.state == BodyIKState.Online
                || this.state == BodyIKState.Swapping)
            {
                // Is this effector already being used?
                if (this.primaryEffectors.ContainsKey(effector))
                {
                    this.PerformSwap(effector, obj);
                }
                else
                {
                    this.primaryEffectors.Add(effector, obj);
                    this.handlerPrimary.StartInteraction(effector, obj, true);
                }
            }
            else if (this.state == BodyIKState.Stopping)
            {
                this.primaryEffectors.Add(effector, obj);
                this.handlerPrimary.StartInteraction(effector, obj, true);
                this.state = BodyIKState.Online;
            }
        }

        public void ResumeInteraction(FullBodyBipedEffector effector)
        {
            if (this.state == BodyIKState.Online
                || this.state == BodyIKState.Swapping)
            {
                // TODO: What if we swap immediately after? The interaction
                //       will get stuck at the trigger again.
                this.handlerPrimary.ResumeInteraction(effector);
            }
        }

        private void PerformSwap(FullBodyBipedEffector effector, InteractionObject obj)
        {
            // Move all the effectors to the secondary IK solver
            foreach (var kv in this.primaryEffectors)
            {
                if (kv.Key != effector)
                {
                    this.handlerSecondary.StartInteraction(kv.Key, kv.Value, true);
                    this.secondaryEffectors[kv.Key] = kv.Value;
                }
            }
            this.handlerSecondary.StartInteraction(effector, obj, true);
            this.secondaryEffectors[effector] = obj;

            // Swap solvers
            this.Swap();

            // Store the intermediate state
            float time = Time.time;
            this.swapTimeFinish = time + this.SwapTime;
            this.state = BodyIKState.Swapping;
        }

        public void StopInteraction(FullBodyBipedEffector effector)
        {
            if (this.primaryEffectors.ContainsKey(effector) == true)
            {
                this.primaryEffectors.Remove(effector);
                this.handlerPrimary.StopInteraction(effector);

                // If this is our last active effector, shut down
                if (this.primaryEffectors.Count == 0)
                    this.state = BodyIKState.Stopping;
            }
        }

        private bool IsActive(CrossfadeFBBIK fbbik)
        {
            foreach (IKEffector eff in fbbik.solver.effectors)
                if (eff.positionWeight > 0.05f || eff.rotationWeight > 0.05f)
                    return true;
            return false;
        }

        private void RegisterWithHandler(CrossfadeInteractionHandler handler)
        {
            handler.InteractionStart += this.OnInteractionStart;
            handler.InteractionTrigger += this.OnInteractionTrigger;
            handler.InteractionRelease += this.OnInteractionRelease;
            handler.InteractionPause += this.OnInteractionPause;
            handler.InteractionPickUp += this.OnInteractionPickUp;
            handler.InteractionResume += this.OnInteractionResume;
            handler.InteractionStop += this.OnInteractionStop;
        }

        private void Swap()
        {
            CrossfadeFBBIK ikTemp = this.ikPrimary;
            this.ikPrimary = this.ikSecondary;
            this.ikSecondary = ikTemp;

            CrossfadeInteractionHandler handlerTemp = this.handlerPrimary;
            this.handlerPrimary = this.handlerSecondary;
            this.handlerSecondary = handlerTemp;

            Dictionary<FullBodyBipedEffector, InteractionObject> temp =
                this.primaryEffectors;
            this.primaryEffectors = this.secondaryEffectors;
            this.secondaryEffectors = temp;
        }

        #region Event Bounce
        private void OnInteractionStart(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyStart != null)
                this.BodyStart(effectorType, interactionObject);
        }

        private void OnInteractionTrigger(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyTrigger != null)
                this.BodyTrigger(effectorType, interactionObject);
        }

        private void OnInteractionRelease(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyRelease != null)
                this.BodyRelease(effectorType, interactionObject);
        }

        private void OnInteractionPause(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyPause != null)
                this.BodyPause(effectorType, interactionObject);
        }

        private void OnInteractionPickUp(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyPickUp != null)
                this.BodyPickUp(effectorType, interactionObject);
        }

        private void OnInteractionResume(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyResume != null)
                this.BodyResume(effectorType, interactionObject);
        }

        private void OnInteractionStop(
            CrossfadeInteractionHandler handler,
            FullBodyBipedEffector effectorType,
            InteractionObject interactionObject)
        {
            if (this.BodyStop != null)
                this.BodyStop(effectorType, interactionObject);
        }
        #endregion
    }

    /// <summary>
    /// Called when an InteractionEvent has been started
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionStart;
    /// <summary>
    /// Called when an Interaction has been paused
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionPause;
    /// <summary>
    /// Called when an Interaction has been triggered
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionTrigger;
    /// <summary>
    /// Called when an Interaction has been released
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionRelease;
    /// <summary>
    /// Called when an InteractionObject has been picked up.
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionPickUp;
    /// <summary>
    /// Called when a paused Interaction has been resumed
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionResume;
    /// <summary>
    /// Called when an Interaction has been stopped
    /// </summary>
    public event InteractionSystem.InteractionEvent InteractionStop;

    /// <summary>
    /// Default delay time
    /// </summary>
    public float DefaultDelay = 0.5f;

    /// <summary>
    /// Maximum weight for body control
    /// </summary>
    public float BodyWeightMax = 0.5f;

    private LookAtIKController lookController;
    private BodyIKController bodyController;

    void Awake()
    {
        this.lookController = new LookAtIKController(
            this.GetComponent<CrossfadeLookAtIK>(),
            this.BodyWeightMax,
            this.DefaultDelay);
        this.bodyController = new BodyIKController(
            this.GetComponents<CrossfadeFBBIK>(),
            this.DefaultDelay);
        this.RegisterWithBodyController();
    }

    void Update()
    {
        this.bodyController.Update();
        this.lookController.Update();

        if (this.bodyController.State == BodyIKState.Offline
            && this.lookController.IsFullBody() == false)
            this.lookController.FullBody(this.DefaultDelay);
    }

    void LateUpdate()
    {
        this.bodyController.LateUpdate();
        this.lookController.LateUpdate();
    }

    public void LookAt(Vector3 target, float delay)
    {
        this.lookController.LookAt(target, delay);
    }

    public void LookStop(float delay)
    {
        this.lookController.LookStop(delay);
    }

    public void LookAt(Vector3 target)
    {
        this.lookController.LookAt(target, this.DefaultDelay);
    }

    public void LookStop()
    {
        this.lookController.LookStop(this.DefaultDelay);
    }

    public void StartInteraction(
        FullBodyBipedEffector effector, 
        InteractionObject obj)
    {
        this.bodyController.StartInteraction(effector, obj);
        this.lookController.HeadOnly(this.DefaultDelay);
    }

    public void ResumeInteraction(
        FullBodyBipedEffector effector)
    {
        this.bodyController.ResumeInteraction(effector);
        this.lookController.HeadOnly(this.DefaultDelay);
    }

    public void StopInteraction(FullBodyBipedEffector effector)
    {
        this.bodyController.StopInteraction(effector);
    }

    private void RegisterWithBodyController()
    {
        this.bodyController.BodyStart += this.OnInteractionStart;
        this.bodyController.BodyTrigger += this.OnInteractionTrigger;
        this.bodyController.BodyRelease += this.OnInteractionRelease;
        this.bodyController.BodyPause += this.OnInteractionPause;
        this.bodyController.BodyPickUp += this.OnInteractionPickUp;
        this.bodyController.BodyResume += this.OnInteractionResume;
        this.bodyController.BodyStop += this.OnInteractionStop;
    }

    #region Event Bounce
    private void OnInteractionStart(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionStart != null)
            this.InteractionStart(effectorType, interactionObject);
    }

    private void OnInteractionTrigger(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionTrigger != null)
            this.InteractionTrigger(effectorType, interactionObject);
    }

    private void OnInteractionRelease(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionRelease != null)
            this.InteractionRelease(effectorType, interactionObject);
    }

    private void OnInteractionPause(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionPause != null)
            this.InteractionPause(effectorType, interactionObject);
    }

    private void OnInteractionPickUp(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionPickUp != null)
            this.InteractionPickUp(effectorType, interactionObject);
    }

    private void OnInteractionResume(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionResume != null)
            this.InteractionResume(effectorType, interactionObject);
    }

    private void OnInteractionStop(
        FullBodyBipedEffector effectorType,
        InteractionObject interactionObject)
    {
        if (this.InteractionStop != null)
            this.InteractionStop(effectorType, interactionObject);
    }
    #endregion
}
