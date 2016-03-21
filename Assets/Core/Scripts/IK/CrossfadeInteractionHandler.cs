using UnityEngine;
using System.Collections;

using RootMotion.FinalIK;

public delegate void CrossfadeInteractionEvent(
    CrossfadeInteractionHandler sender,
    FullBodyBipedEffector effectorType, 
    InteractionObject interactionObject);

/// <summary>
/// Handles FBBIK interactions for a character.
/// </summary>
public class CrossfadeInteractionHandler
{
    #region Main Interface

    /// <summary>
    /// If not empty, only the targets with the The will be used by 
    /// this interaction system.
    /// </summary>
    public string targetTag = "";
    /// <summary>
    /// The fade in time of the interaction.
    /// </summary>
    public float fadeInTime = 0.3f;
    /// <summary>
    /// The master speed for all interactions.
    /// </summary>
    public float speed = 1f;
    /// <summary>
    /// If true, lerps all the FBBIK channels used by the Interaction System 
    /// back to their default or initial values when not in interaction
    /// </summary>
    public float resetToDefaultsSpeed = 1f;

    /// <summary>
    /// Returns true if any of the effectors are in interaction and not paused.
    /// </summary>
    public bool inInteraction
    {
        get
        {
            if (!IsValid(true)) return false;

            for (int i = 0; i < interactionEffectors.Length; i++)
            {
                if (interactionEffectors[i].inInteraction 
                    && !interactionEffectors[i].isPaused) 
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Determines whether this effector is interaction and not paused
    /// </summary>
    public bool IsInInteraction(FullBodyBipedEffector effectorType)
    {
        if (!IsValid(true)) return false;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType)
            {
                return 
                    interactionEffectors[i].inInteraction 
                    && !interactionEffectors[i].isPaused;
            }
        }
        return false;
    }

    /// <summary>
    /// Determines whether this effector is  paused
    /// </summary>
    public bool IsPaused(FullBodyBipedEffector effectorType)
    {
        if (!IsValid(true)) return false;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType)
            {
                return 
                    interactionEffectors[i].inInteraction 
                    && interactionEffectors[i].isPaused;
            }
        }
        return false;
    }

    /// <summary>
    /// Starts the interaction between an effector and an interaction object.
    /// </summary>
    public void StartInteraction(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject, 
        bool interrupt)
    {
        if (!IsValid(true)) return;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType)
            {
                interactionEffectors[i].Start(
                    interactionObject, 
                    targetTag, 
                    fadeInTime, 
                    interrupt);
                return;
            }
        }
    }

    /// <summary>
    /// Pauses the interaction of an effector.
    /// </summary>
    public void PauseInteraction(FullBodyBipedEffector effectorType)
    {
        if (!IsValid(true)) return;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType) 
                interactionEffectors[i].Pause();
        }
    }

    /// <summary>
    /// Resumes the paused interaction of an effector.
    /// </summary>
    public void ResumeInteraction(FullBodyBipedEffector effectorType)
    {
        if (!IsValid(true)) return;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType) 
                interactionEffectors[i].Resume();
        }
    }

    /// <summary>
    /// Stops the interaction of an effector.
    /// </summary>
    public void StopInteraction(FullBodyBipedEffector effectorType)
    {
        if (!IsValid(true)) return;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType) 
                interactionEffectors[i].Stop();
        }
    }

    /// <summary>
    /// Pauses all the interaction effectors.
    /// </summary>
    public void PauseAll()
    {
        if (!IsValid(true)) return;

        for (int i = 0; i < interactionEffectors.Length; i++) 
            interactionEffectors[i].Pause();
    }

    /// <summary>
    /// Resumes all the paused interaction effectors.
    /// </summary>
    public void ResumeAll()
    {
        if (!IsValid(true)) return;

        for (int i = 0; i < interactionEffectors.Length; i++) 
            interactionEffectors[i].Resume();
    }

    /// <summary>
    /// Stops all interactions.
    /// </summary>
    public void StopAll()
    {
        for (int i = 0; i < interactionEffectors.Length; i++) 
            interactionEffectors[i].Stop();
    }

    /// <summary>
    /// Gets the current interaction object of an effector.
    /// </summary>
    public InteractionObject GetInteractionObject(FullBodyBipedEffector effectorType)
    {
        if (!IsValid(true)) 
            return null;

        for (int i = 0; i < interactionEffectors.Length; i++)
        {
            if (interactionEffectors[i].effectorType == effectorType)
            {
                return interactionEffectors[i].interactionObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the FullBodyBipedIK component.
    /// </summary>
    public CrossfadeFBBIK IK
    {
        get
        {
            return this.fullBody;
        }
    }

    /// <summary>
    /// Called when an InteractionEvent has been started
    /// </summary>
    public event CrossfadeInteractionEvent InteractionStart;
    /// <summary>
    /// Called when an Interaction has been paused
    /// </summary>
    public event CrossfadeInteractionEvent InteractionPause;
    /// <summary>
    /// Called when an Interaction has been triggered
    /// </summary>
    public event CrossfadeInteractionEvent InteractionTrigger;
    /// <summary>
    /// Called when an Interaction has been released
    /// </summary>
    public event CrossfadeInteractionEvent InteractionRelease;
    /// <summary>
    /// Called when an InteractionObject has been picked up.
    /// </summary>
    public event CrossfadeInteractionEvent InteractionPickUp;
    /// <summary>
    /// Called when a paused Interaction has been resumed
    /// </summary>
    public event CrossfadeInteractionEvent InteractionResume;
    /// <summary>
    /// Called when an Interaction has been stopped
    /// </summary>
    public event CrossfadeInteractionEvent InteractionStop;

    #endregion Main Interface

    private CrossfadeFBBIK fullBody; // Reference to the FBBIK component.

    // The array of Interaction Effectors
    private InteractionEffector[] interactionEffectors = 
        new InteractionEffector[9] {
		    new InteractionEffector(FullBodyBipedEffector.Body),
		    new InteractionEffector(FullBodyBipedEffector.LeftFoot),
		    new InteractionEffector(FullBodyBipedEffector.LeftHand),
		    new InteractionEffector(FullBodyBipedEffector.LeftShoulder),
		    new InteractionEffector(FullBodyBipedEffector.LeftThigh),
		    new InteractionEffector(FullBodyBipedEffector.RightFoot),
		    new InteractionEffector(FullBodyBipedEffector.RightHand),
		    new InteractionEffector(FullBodyBipedEffector.RightShoulder),
		    new InteractionEffector(FullBodyBipedEffector.RightThigh)
	    };


    public CrossfadeInteractionHandler(CrossfadeFBBIK fullBody)
    {
        this.fullBody = fullBody;

        // Add to the FBBIK OnPostUpdate delegate to get a call when it 
        // has finished updating
        fullBody.solver.OnPostUpdate += OnPostFBBIK;

        foreach (InteractionEffector e in interactionEffectors)
        {
            e.OnStart = this.OnEffectorStart;
            e.OnTrigger = OnEffectorTrigger;
            e.OnRelease = OnEffectorRelease;
            e.OnPause = OnEffectorPause;
            e.OnPickUp = OnEffectorPickUp;
            e.OnResume = OnEffectorResume;
            e.OnStop = OnEffectorStop;

            e.Initiate(fullBody.solver);
        }
    }

    #region Event Bouncing
    private void OnEffectorStart(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionStart != null)
            this.InteractionStart(this, effectorType, interactionObject);
    }

    private void OnEffectorTrigger(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionTrigger != null)
            this.InteractionTrigger(this, effectorType, interactionObject);
    }

    private void OnEffectorRelease(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionRelease != null)
            this.InteractionRelease(this, effectorType, interactionObject);
    }

    private void OnEffectorPause(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionPause != null)
            this.InteractionPause(this, effectorType, interactionObject);
    }

    private void OnEffectorPickUp(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionPickUp != null)
            this.InteractionPickUp(this, effectorType, interactionObject);
    }

    private void OnEffectorResume(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionResume != null)
            this.InteractionResume(this, effectorType, interactionObject);
    }

    private void OnEffectorStop(
        FullBodyBipedEffector effectorType, 
        InteractionObject interactionObject)
    {
        if (this.InteractionStop != null)
            this.InteractionStop(this, effectorType, interactionObject);
    }
    #endregion

    // Update the interaction
    public void LateUpdate()
    {
        if (fullBody == null) 
            return;

        for (int i = 0; i < interactionEffectors.Length; i++) 
            interactionEffectors[i].Update(fullBody.solver, speed);

        // Interpolate to default pull, reach values
        for (int i = 0; i < interactionEffectors.Length; i++) 
            interactionEffectors[i].ResetToDefaults(
                fullBody.solver, 
                resetToDefaultsSpeed);
    }

    // Used for rotating the hands after FBBIK has finished
    private void OnPostFBBIK()
    {
        if (fullBody == null) return;

        for (int i = 0; i < interactionEffectors.Length; i++) 
            interactionEffectors[i].OnPostFBBIK(fullBody.solver);
    }

    // Remove the delegates
    public void OnDestroy()
    {
        if (fullBody != null) 
            fullBody.solver.OnPostUpdate -= OnPostFBBIK;
    }

    // Is this InteractionSystem valid and initiated
    private bool IsValid(bool log)
    {
        if (this.fullBody == null)
        {
            if (log) 
                Debug.Log("FBBIK is null.");
            return false;
        }
        return true;
    }
}
