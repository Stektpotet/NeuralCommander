using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Note: just used for testing of AgentActions for now.
[RequireComponent(typeof(UnitActions))]
public class UnitBehaviour : MonoBehaviour {
    public UnitActions actionSet;
    public UnitComplexActions activitySet;

    public float moveSpeed;
    public float jumpHeight;

    // debug
    public Transform target;
    public PhysicalObject rightPickupTarget;
    public PhysicalObject leftPickupTarget;
    public bool doPickupRight;
    public bool doPickupLeft;
    public bool doJump; 
    public bool doDrop;
    public bool doActivateRight;
    public bool doActivateLeft;
    public bool doAimAtRight;
    public bool doAimAtLeft;
    public bool doMoveTowards;
    public bool doCrouch;
    public bool doStandup;
    public bool doReload;
    public bool doOrientate;
    public bool setDestinationWithStrafe;
    public bool setDestinationWithoutStrafe;
    float rightSlerpPos = 0;
    float leftSlerpPos = 0;

    public Animator animator;
    public int routineStatus;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leg;
    public Transform destination;
    private float originalHeight;
    private bool newDestination;


    // TODO: this remove script



    // Use this for initialization
    void Start () {

        actionSet = GetComponent<UnitActions>();
        originalHeight = leg.localPosition.y;

    }
	
	// Update is called once per frame
	void FixedUpdate () {

        // ALL of this is debug 
        // TODO: remove
        if(doMoveTowards)
        {
            actionSet.MoveTowards(target.position, moveSpeed);
        }
        if (doJump)
        {
            actionSet.Jump(jumpHeight);
            doJump = false;
        }
        if (doPickupRight)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), rightPickupTarget.GetComponent<Collider>());
            actionSet.PickupObject(rightPickupTarget,rightHand, rightHand.Find("WeaponPos"));
            doPickupRight = false;
        }
        if (doPickupLeft)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), leftPickupTarget.GetComponent<Collider>());
            actionSet.PickupObject(leftPickupTarget,leftHand, leftHand.Find("WeaponPos"));
            doPickupLeft = false;
        }
        /*if (doDrop)
        {
            Vector3 velocity = hand.GetComponent<Rigidbody>().velocity;
            actionSet.DropObject(pickupTarget, hand.transform, velocity);
            doDrop = false;
        }*/
        if(doActivateRight)
        {
            actionSet.ActivateObject(rightPickupTarget, rightHand.Find("WeaponPos"));
        }
        if (doActivateLeft)
        {
            actionSet.ActivateObject(leftPickupTarget, leftHand.Find("WeaponPos"));
        }
        if (doAimAtRight)
        {

            rightSlerpPos += 0.2f;
            actionSet.AimAt(rightHand, target.position, rightSlerpPos);
            if(rightSlerpPos > 1)
            {
                rightSlerpPos = 0;
            }
            //doAimAtRight = false;
        }
        if (doAimAtLeft)
        {

            leftSlerpPos += 0.2f;
            actionSet.AimAt(leftHand, target.position, leftSlerpPos);
            if (leftSlerpPos > 1)
            {
                leftSlerpPos = 0;
            }
        }
        if (doCrouch)
        {
            actionSet.Crouch(leg, originalHeight, 0.3f);
            doCrouch = false;
        }
        if (doStandup)
        {
            actionSet.StandUp(leg, originalHeight);
            doStandup = false;
        }
        if (doReload)
        {
            actionSet.ReloadWeapon(rightPickupTarget);
            actionSet.ReloadWeapon(leftPickupTarget);
            doReload = false;
        }
        if (doOrientate)
        {
            actionSet.TurnTowards(target, 1.0f, 2.0f, 7f);
        }
        if (setDestinationWithStrafe)
        {
            setDestinationWithoutStrafe = false;
            setDestinationWithStrafe = activitySet.GoTowardsDestination(destination.position);
            actionSet.TurnTowards(destination, 1.0f, 2.0f, 7f);
        }
        if (setDestinationWithoutStrafe)
        {
            setDestinationWithStrafe = false;
            setDestinationWithoutStrafe = activitySet.GoTowardsDestination(destination.position, false);
            //actionSet.TurnTowards(destination, 1.0f, 2.0f, 7f);
        }
        //animator.SetInteger("RoutineStatus", routineStatus);
        

    }

    void ProccessSituation()
    {

    }
}
