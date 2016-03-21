using UnityEngine;
using System.Collections;

public enum SteeringState
{
    Stopped,
    Navigating,
    Arriving
}

public enum OrientationQuality
{
    Low,
    High
}

public enum OrientationBehavior
{
    LookForward,
    LookAtTarget,
    None
}

[System.Serializable]
public abstract class SteeringController : MonoBehaviour
{
    public static Vector3 ProjectOnPlane(Vector3 v, Vector3 normal)
    {
        return v - Vector3.Project(v, normal);
    }

    protected static readonly float TURN_EPSILON = 0.9995f;
    protected static readonly float STOP_EPSILON = 0.05f;
	protected static readonly float TURN_ANGLE = 30f;


    protected Vector3 lastPosition = Vector3.zero;
    protected Vector3 target = Vector3.zero;
    public abstract Vector3 Target { get; set; }

    // Whether we're attached to the navmesh
    protected bool attached = true;
    public abstract bool Attached { get; set; }

    [HideInInspector]
    public Quaternion desiredOrientation = Quaternion.identity;

    // Steering Parameters
    public float YOffset = 0.0f;
    public float radius = 0.6f;
    public float height = 2.0f;
    public float stoppingRadius = 0.4f;
    public float arrivingRadius = 3.0f;
    public float acceleration = 2.0f;
    public float maxSpeed = 2.2f;
    public float minSpeed = 0.5f;

    public bool SlowArrival = true;

    public bool ShowDragGizmo = false;
    public bool ShowAgentRadiusGizmo = false;
    public bool ShowTargetRadiusGizmo = false;

    // Orientation Parameters
    public float driveSpeed = 120.0f;
    public float dragRadius = 0.5f;
    public bool planar = true;
    public bool driveOrientation = true;

    public OrientationQuality orientationQuality =
        OrientationQuality.High;
    public OrientationBehavior orientationBehavior =
        OrientationBehavior.LookForward;

    public abstract bool IsAtTarget();
    public abstract bool IsStopped();
    public abstract bool HasArrived();
    public abstract bool CanReach(Vector3 target);
    public abstract void Stop();
    public abstract void Warp(Vector3 target);

    public bool IsFacing()
    {
        Quaternion orientation = transform.rotation;
        Quaternion desired = this.desiredOrientation;

		//Changed to angle to simplify bugfixing
		if (Mathf.Abs(Quaternion.Angle(desired, orientation)) < TURN_ANGLE)
            return true;
        return false;
    }

    public void FacingSnap()
    {
        Quaternion desired = this.desiredOrientation;
        this.transform.rotation = desired;
    }

    public void SetDesiredOrientation(Vector3 target)
    {
        Vector3 difference =
            ProjectOnPlane(
                target - transform.position, 
                Vector3.up);

        this.desiredOrientation = 
            Quaternion.LookRotation(
                difference, 
                Vector3.up);
    }
}