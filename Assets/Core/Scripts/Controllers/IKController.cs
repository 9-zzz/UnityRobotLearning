// /// <summary>
///// 
///// </summary>

//using UnityEngine;
//using System;
//using System.Collections;
//using RootMotion.FinalIK;
//using TreeSharpPlus;
  
//[RequireComponent(typeof(Animator))]  

//public class IKControllerOld : MonoBehaviour {
	
//    protected Animator avatar;
	
//    private Transform bodyObj = null;
//    private Transform leftFootObj = null;
//    private Transform rightFootObj = null;
//    private Transform leftHandReachObj = null;
//    private Transform rightHandReachObj = null;
//    private Transform pointObject = null;
//    private Val<Vector3> lookAtObj = null;

//    //smooth transition from one to the other object
//    private Transform oldLeftHandObj = null;
//    private Transform oldRightHandObj = null;
//    private float leftHandObjTransitionTime = 0.0f;
//    private float rightHandObjTransitionTime = 0.0f;

//    private bool animateReaching;

//    private float reachSpeed = 0.3f;
//    private float leftFootWeight = 0;
//    private float rightFootWeight = 0;
//    private float leftHandWeight = 0;
//    private float rightHandWeight = 0;
//    private float lookAtWeight = 0;

//    //for playing animations while doing reaching, pointing, etc.
//    private BodyMecanim body;

//    private SteeringController controller;

//    private OrientationBehavior oldBehavior = OrientationBehavior.LookForward;

//    private bool resetOrientation;

//    public FullBodyBipedIK ik;

//    //15.5 SO changed default to test svn 
//    public bool FFBIK = false;

//    // Use this for initialization
//    void Start () 
//    {
//        avatar = GetComponent<Animator>();
//        ik = GetComponent<FullBodyBipedIK> ();
//        controller = GetComponent<SteeringController>();
//        body = GetComponent<BodyMecanim>();
//    }
    
//    public void setLookAt(Val<Vector3> targ) {
//        lookAtObj = targ;
//    }

//    public void SetReachTarget(Transform target, bool rightHand, bool animate = true)
//    {
//        if (rightHand)
//        {
//            if (rightHandReachObj != target)
//            {
//                oldRightHandObj = rightHandReachObj;
//                rightHandObjTransitionTime = 0.0f;
//                //if we get a new target, we should still store the old one, so we can change the position slowly instead of instantaneously
//            }
//            if (animate)
//            {
//                body.HandAnimation("ReachRight", true);
//            }
//            rightHandReachObj = target;
//        }
//        else
//        {
//            if (leftHandReachObj != target)
//            {
//                leftHandObjTransitionTime = 0.0f;
//                oldLeftHandObj = leftHandReachObj;
//            }
//            leftHandReachObj = target;
//        }
//    }

//    public void LeaveReachTarget(bool rightHand)
//    {
//        if (rightHand)
//        {
//            rightHandReachObj = null;
//            body.HandAnimation("ReachRight", false);
//        }
//        else
//            leftHandReachObj = null;
//    }

//    public void SetPointTarget(Transform target)
//    {
//        body.HandAnimation("Pointing", true);
//        pointObject = target;
//    }

//    public void LeavePointTarget()
//    {
//        pointObject = null;
//        body.HandAnimation("Pointing", false);
//    }

//    public bool rightHandMoving(bool toTarget){
//        if(toTarget)
//            return rightHandWeight <= 1;
//        return rightHandWeight >= 0;
//    }

//    public bool leftHandMoving(bool toTarget){
//        if(toTarget)
//            return leftHandWeight <= 1;
//        return leftHandWeight >= 0;
//    }

//    public bool lookingMoving(bool toTarget){
//        if(toTarget)
//            return lookAtWeight <= 1;
//        return lookAtWeight >= 0;
//    }

//    void OnAnimatorIK(int layerIndex)
//    {	

//        if(avatar)
//        {	
//            if(!FFBIK)
//            {
//                if(rightHandReachObj != null)
//                {
//                    if(rightHandWeight<=1)
//                        rightHandWeight += Time.deltaTime*reachSpeed;
//                    //Debug.Log("weight = " + rightHandWeight);
//                    avatar.SetIKPositionWeight(AvatarIKGoal.RightHand,rightHandWeight);
//                    avatar.SetIKRotationWeight(AvatarIKGoal.RightHand,rightHandWeight);
//                    avatar.SetIKPosition(AvatarIKGoal.RightHand,rightHandReachObj.position);
//                    avatar.SetIKRotation(AvatarIKGoal.RightHand,rightHandReachObj.rotation);
//                }	else {
//                    if(rightHandWeight>=0)
//                        rightHandWeight -= Time.deltaTime*reachSpeed;
//                    //TODO returning isn't smooth, because Effector isn't set anymore after object is set to null...
//                    avatar.SetIKPositionWeight(AvatarIKGoal.RightHand,rightHandWeight);
//                    avatar.SetIKRotationWeight(AvatarIKGoal.RightHand,rightHandWeight);	
//                }

				
//                if(leftHandReachObj != null)
//                {
//                    if(leftHandWeight<=1)
//                        leftHandWeight += Time.deltaTime*reachSpeed;
//                    avatar.SetIKPositionWeight(AvatarIKGoal.LeftHand,leftHandWeight);
//                    avatar.SetIKRotationWeight(AvatarIKGoal.LeftHand,leftHandWeight);
//                    avatar.SetIKPosition(AvatarIKGoal.LeftHand,leftHandReachObj.position);
//                    avatar.SetIKRotation(AvatarIKGoal.LeftHand,leftHandReachObj.rotation);
//                } else {
//                    if(leftHandWeight>=0)
//                        leftHandWeight -= Time.deltaTime*reachSpeed;
//                    avatar.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
//                    avatar.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
//                }
//            }

//            //TODO: Think of a better way to do this
//            //CS, 19.05.2014
//            if(lookAtObj != null)
//            {
//                float angle = AngleTo(lookAtObj.Value);
//                if (angle >= 100)
//                {
//                    //when standing or turning while standing, and the angle is of a certain size, try turning towards the object
//                    AnimatorStateInfo state = avatar.GetCurrentAnimatorStateInfo(0);
//                    if ((state.IsName("Locomotion.Idle") || state.IsName("Locomotion.TurnOnSpot")) && !avatar.IsInTransition(0))
//                    {
//                        oldBehavior = controller.orientationBehavior;
//                        controller.orientationBehavior = OrientationBehavior.None;
//                        controller.SetDesiredOrientation(lookAtObj.Value);
//                        resetOrientation = true;
//                    }
//                }
//                if (angle >= 140) {
//                    //can look at at higher angles than 100 degree, but there is still some cutoff
//                    if (lookAtWeight >= 0)
//                        lookAtWeight -= Time.deltaTime * reachSpeed;
//                    avatar.SetLookAtWeight(lookAtWeight, 0.2f * lookAtWeight, 0.7f * lookAtWeight, 1.0f * lookAtWeight, 0.5f * lookAtWeight);
//                }
//                else
//                {
//                    //check if we need to reset the orientation since we set it before (and it hasn't been changed in between)
//                    if (angle <= 50 && resetOrientation && controller.orientationBehavior == OrientationBehavior.None)
//                    {

//                        controller.orientationBehavior = oldBehavior;
//                        resetOrientation = false;
//                    }
//                    if (lookAtWeight <= 1)
//                        lookAtWeight += Time.deltaTime * reachSpeed;
//                    avatar.SetLookAtWeight(lookAtWeight, 0.2f * lookAtWeight, 0.7f * lookAtWeight, 1.0f * lookAtWeight, 0.5f * lookAtWeight);
//                    avatar.SetLookAtPosition(lookAtObj.Value);
//                }
//            }
//            else
//            {
//                //check if we need to reset the orientation since we set it before (and it hasn't been changed in between
//                if (resetOrientation && controller.orientationBehavior == OrientationBehavior.None)
//                {
//                    resetOrientation = false;
//                    controller.orientationBehavior = oldBehavior;
//                }
//                if(lookAtWeight>=0)
//                    lookAtWeight -= Time.deltaTime*reachSpeed;
//                avatar.SetLookAtWeight(lookAtWeight,0.2f*lookAtWeight,0.7f*lookAtWeight,1.0f*lookAtWeight,0.5f*lookAtWeight);
//            }
//        }
//    }

//    /// <summary>
//    /// Returns the smaller angle between my forward and the direction to target, returning an angle between
//    /// 0 degree and 180 degree.
//    /// </summary>
//    private float AngleTo(Vector3 target)
//    {
//        //take the angle between my forward, and the direction to the target. sets y value to 0 so angle is only in x-z axis
//        Vector3 myForward = transform.forward;
//        myForward.y = 0;
//        Vector3 toTarget = target - transform.position;
//        toTarget.y = 0;
//        return Vector3.Angle(myForward, toTarget);
//    }

//    void LateUpdate () 
//    {

//        rightHandObjTransitionTime += Time.deltaTime;
//        leftHandObjTransitionTime += Time.deltaTime;

//        if(FFBIK)
//        {
//            if(rightHandReachObj != null)
//            {
//                Vector3 position = (oldRightHandObj == null ? rightHandReachObj.position : Vector3.Lerp(oldRightHandObj.position, rightHandReachObj.position, rightHandObjTransitionTime));
//                if(rightHandWeight<=1)
//                    rightHandWeight += Time.deltaTime;
//                HandTo(position, rightHandWeight, ik.solver.rightHandEffector);
//            }	
//            else  
//            {
//                if (rightHandWeight >= 0.0f)
//                {
//                    rightHandWeight -= Time.deltaTime;
//                }
//                ik.solver.rightHandEffector.positionWeight = rightHandWeight; 
//                ik.solver.rightHandEffector.rotationWeight = rightHandWeight;	
//            }


//            if(leftHandReachObj != null)
//            {
//                Vector3 position = (oldLeftHandObj == null ? leftHandReachObj.position : Vector3.Lerp(oldLeftHandObj.position, leftHandReachObj.position, leftHandObjTransitionTime));
//                if(leftHandWeight<=1)
//                    leftHandWeight += Time.deltaTime;
//                HandTo(position, leftHandWeight, ik.solver.leftHandEffector);
//            }	
//            else
//            {
//                if (leftHandWeight >= 0.0f)
//                {
//                    leftHandWeight -= Time.deltaTime;
//                }
//                ik.solver.leftHandEffector.positionWeight = leftHandWeight; 
//                ik.solver.leftHandEffector.rotationWeight = leftHandWeight;	
//            }


//            if (pointObject != null)
//            {
////                Vector3 transformPlane = new Vector3(transform.position.x, 0, transform.position.z);
////                Vector3 toTargetPlane = new Vector3(pointObject.transform.position.x - transform.position.x, 0, pointObject.transform.position.z - transform.position.z);
////                toTargetPlane = Vector3.ClampMagnitude(toTargetPlane, ExtCharacterMecanim.MAX_REACHING_DISTANCE / 2);
////                toTargetPlane.y = pointObject.transform.position.y;
//                GameObject t = new GameObject ();
//                Animator a = GetComponent<Animator> ();
//                Vector3 shPos = a.GetBoneTransform (HumanBodyBones.RightShoulder).position;
//                t.transform.position = shPos + (pointObject.position-shPos).normalized * CharacterMecanim.MAX_REACHING_DISTANCE*0.9f;
//                if (rightHandWeight <= 1)
//                {
//                    rightHandWeight += Time.deltaTime;
//                }
//                HandTo(t.transform.position, rightHandWeight, ik.solver.rightHandEffector);
//                GameObject.Destroy(t);
//            }
//        }
//    }

//    private void HandTo(Vector3 position, float weight, IKEffector effector)
//    {
//        effector.position = position;
//        effector.rotation = Quaternion.LookRotation(position - transform.position);
//        effector.positionWeight = weight;
//        effector.rotationWeight = weight;
//    }
	
//}
