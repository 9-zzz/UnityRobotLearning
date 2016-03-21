using UnityEngine;
using System.Collections;






public class UnitySteeringController : SteeringController
{
    // The navmesh agent attached to us
    private NavMeshAgent navAgent = null;
	private _NavigatorScript navScript = null;

    // Used when we detach from the NavMesh
    private Vector3 cachedPosition;

    public override Vector3 Target
    {
        get
        {
            return this.target;
        }
        set
        {
            if (this.navAgent != null && this.attached == true)
            {
                this.navAgent.SetDestination(value);
                this.target = value;
            }
        }
    }

    public override bool Attached
    {
        get
        {
            return this.attached;
        }
        set
        {
            this.attached = value;
            if (value == true)
                this.Attach();
            else
                this.Detach();
        }
    }

    void Awake()
    {

		this.navScript = transform.GetComponent<_NavigatorScript>();
		if (this.navScript == null)
			this.navScript = transform.gameObject.AddComponent<_NavigatorScript>();
    }

    void Start()
    {
        //TODO left this out, led to weird behavior with crowds when they were pushing each other, CS 16.05.2014 
        //this.Target = transform.position;

        //this can't be done in awake, as we can't initialize our nav mesh agent then. We first need to set all parent transforms.
        this.navAgent = transform.GetComponent<NavMeshAgent>();
        if (this.navAgent == null)
        {
            this.navAgent = this.gameObject.AddComponent<NavMeshAgent>();
        }
        this.navAgent.updateRotation = false;

        this.navAgent.height = this.height;
        this.navAgent.radius = this.radius;
        this.navAgent.acceleration = this.acceleration;
        this.navAgent.speed = this.maxSpeed;
        this.navAgent.stoppingDistance = this.stoppingRadius;
    }

    void Update()
    {
        this.HandleOrientation();
        this.lastPosition = transform.position;

        this.navAgent.height = this.height;
        this.navAgent.radius = this.radius;
        this.navAgent.acceleration = this.acceleration;
        this.navAgent.stoppingDistance = this.stoppingRadius;

        if (this.navAgent.enabled == true)
        {
            float remaining = navAgent.remainingDistance;
            if (this.SlowArrival == true && remaining <= this.arrivingRadius)
            {
                float speed = this.maxSpeed
                    * (remaining / this.arrivingRadius);
                if (speed < minSpeed)
                    speed = minSpeed;
                this.navAgent.speed = speed;
            }
            else
            {
                this.navAgent.speed = this.maxSpeed;
            }
        }
    }

    public override bool IsAtTarget()
    {
        return (transform.position - this.target).magnitude 
            <= (this.stoppingRadius + STOP_EPSILON);
    }

    public override bool IsStopped()
    {
        return (this.navAgent.velocity.sqrMagnitude < STOP_EPSILON);
    }

    public override bool HasArrived()
    {
        return this.IsAtTarget() && this.IsStopped();
    }

    public override bool CanReach(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        this.navAgent.CalculatePath(target, path);
        return (path.status == NavMeshPathStatus.PathComplete);
    }

    protected void Detach()
    {
        this.navAgent.radius = 0.0f;
        this.navAgent.enabled = false;
        this.navAgent.updatePosition = false;
        this.cachedPosition = transform.position;
    }

    protected void Attach()
    {
        this.navAgent.enabled = true;
        this.navAgent.radius = this.radius;
        this.navAgent.Move(this.cachedPosition - transform.position);
        this.navAgent.updatePosition = true;
    }

    private void HandleOrientation()
    {
        switch (this.orientationBehavior)
        {
            case OrientationBehavior.LookForward:
                this.desiredOrientation = this.CalcHeadingOrientation();
                break;
            case OrientationBehavior.LookAtTarget:
                this.desiredOrientation = this.CalcTargetOrientation();
                break;
        }

        switch (this.orientationQuality)
        {
            case OrientationQuality.Low:
                transform.rotation = desiredOrientation;
                break;
            case OrientationQuality.High:
			navScript.desiredOrientation = desiredOrientation;
//			---Replaced turning with doing a turning animation, SO 08.05.14
//                transform.rotation = Quaternion.RotateTowards(
//                    transform.rotation,
//                    desiredOrientation,
//                    driveSpeed * Time.deltaTime);
                break;
        }
    }

    public override void Stop()
    {
        this.navAgent.Stop();
    }

    public override void Warp(Vector3 target)
    {
        this.navAgent.Warp(target);
    }

    public void Move(Vector3 translation)
    {
        this.navAgent.Move(translation);
    }

    internal void OnDrawGizmos()
    {
        if (this.ShowAgentRadiusGizmo == true)
        {
            Vector3 top = this.transform.position 
                + (Vector3.up * this.height);
            Vector3 bottom = this.transform.position;
            if (Application.isPlaying == false)
            {
                top.y += this.YOffset;
                bottom.y += this.YOffset;
            }

            float diameter = this.radius * 2.0f;
            Matrix4x4 trs = Matrix4x4.TRS(
                Vector3.Lerp(bottom, top, 0.5f),
                Quaternion.LookRotation(Vector3.forward),
                new Vector3(
                    diameter, this.height / 2.0f, diameter));

            GizmoDraw.DrawCylinder(trs, (Color.green + Color.white) / 2);
        }

        if (this.ShowTargetRadiusGizmo == true)
        {
            Vector3 target =
                (this.Target == Vector3.zero)
                    ? transform.position
                    : this.Target;

            float stoppingDiameter = this.stoppingRadius * 2.0f;
            float arrivingDiameter = this.arrivingRadius * 2.0f;

            Matrix4x4 holdingTrs = Matrix4x4.TRS(
                target,
                Quaternion.LookRotation(Vector3.forward),
                new Vector3(
                    stoppingDiameter, 0.0f, stoppingDiameter));

            Matrix4x4 totalTrs = Matrix4x4.TRS(
                target,
                Quaternion.LookRotation(Vector3.forward),
                new Vector3(
                    arrivingDiameter, 0.0f, arrivingDiameter));

            GizmoDraw.DrawCylinder(holdingTrs, Color.blue);
            GizmoDraw.DrawCylinder(totalTrs, Color.red);
        }

        if (this.ShowDragGizmo == true)
        {
            Vector3 pointArm = this.CalcDragArm();
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.position, 0.05f);
            Gizmos.DrawLine(transform.position, transform.position + pointArm);
            Gizmos.DrawSphere(transform.position + pointArm, 0.07f);

            if (driveOrientation)
            {
                Vector3 desiredPointArm =
                    desiredOrientation * new Vector3(0, 0, -1);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position,
                    transform.position + desiredPointArm);
                Gizmos.DrawSphere(transform.position + desiredPointArm, 0.06f);
            }
        }
    }

    private Vector3 CalcDragArm()
    {
        Vector3 dragPoint = lastPosition - transform.forward * dragRadius;
        Vector3 pointArm = dragPoint - transform.position;
        pointArm = pointArm.normalized * dragRadius;
        if (this.planar == true)
            pointArm.y = 0;
        return pointArm;
    }

    private Quaternion CalcHeadingOrientation()
    {
        Vector3 heading;
        switch (this.orientationQuality)
        {
            case OrientationQuality.High:
                heading = -CalcDragArm();
                break;
            default:
                heading = transform.position - lastPosition;
                if (this.planar == true)
                    heading.y = 0;
                break;
        }
        return Quaternion.LookRotation(heading);
    }

    private Quaternion CalcTargetOrientation()
    {
        Vector3 toTarget = this.Target - transform.position;
        if (this.planar == true)
            toTarget.y = 0;
        return Quaternion.LookRotation(toTarget);
    }
}