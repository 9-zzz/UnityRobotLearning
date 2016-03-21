using UnityEngine;
using TreeSharpPlus;
using System.Collections;

public class _NavigatorScript : MonoBehaviour
{	
	private NavMeshAgent agent;
	private Animator animator;
	private float angleDiff;

	[HideInInspector]
	public Quaternion desiredOrientation{ get; set; }

	/*put together with locomotion*/
	protected LocomotionController locomotion;


	void Start() { this.Initialize(); }

	public void Initialize() 
	{
		agent = this.GetComponent<NavMeshAgent> ();
		animator = this.GetComponent<Animator> ();
        desiredOrientation = transform.rotation;

		/*put together with locomotion*/
		locomotion = new LocomotionController(animator);

	}

	protected void SetupAgentLocomotion()
	{
		if (AgentDone())
		{
            //TODO Resetting path here, otherwise e.g. stepback animation not working properly - CS 03.09.2014
            //Is there a better solution?
            agent.ResetPath();
			locomotion.Do(0, angleDiff);
		}
		else
		{
			float speed = agent.desiredVelocity.magnitude;
			
			Vector3 velocity = Quaternion.Inverse(transform.rotation) * agent.desiredVelocity;
			
			float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;
			
			locomotion.Do(speed, angle);
		}
	}

    void OnAnimatorMove()
    {
        agent.velocity = animator.deltaPosition / Time.deltaTime;
        transform.rotation = animator.rootRotation;
        // get a "forward vector" for each rotation
        var forwardA = transform.rotation * Vector3.forward;
        var forwardB = desiredOrientation * Vector3.forward;
        // get a numeric angle for each vector, on the X-Z plane (relative to world forward)
        var angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
        var angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
        // get the signed difference in these angles
        angleDiff = Mathf.DeltaAngle(angleA, angleB);
    }

	public bool AgentDone()
	{
		return !agent.pathPending && AgentStopping();
	}
	
	protected bool AgentStopping()
	{
		return agent.remainingDistance <= agent.stoppingDistance;
	}

	// Update is called once per frame
	void Update () 
	{
		SetupAgentLocomotion();
	}
	
}
