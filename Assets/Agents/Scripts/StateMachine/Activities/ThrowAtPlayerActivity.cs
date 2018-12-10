using System.Linq;
using UnityEngine;

public enum PickUpStatus
{
    PickedUpObject,
    TryingToPickupObject,
    FailedToPickupObject
}

public class ThrowAtPlayerActivity : Activity
{
    [SerializeField] private float heightOffsetThrow = 0.01f;
    [SerializeField] private float maxThrowRange = 150.0f;
    [SerializeField] private float throwSpeed = 25.0f;
    [SerializeField] private float pickUpSpeed = 4.0f;
    [SerializeField] private float throwForce = 25.0f;
    [SerializeField] private float torque = 2.0f;
    [SerializeField] private float maxTorque = 4.0f;
    [SerializeField] private float allowedError = 7.5f;
    public float aimAtLerp = 0;
    public bool newPositionSet;
    private Transform target;
    private Transform handToPickUpWith;
    private Transform handToPickUpWithWeaponPos;
    private PhysicalObject objectToThrow;
    private PhysicalObject previousHeldObject;
    private Vector3 positionToLineOfSightPlayer;
    private int pickupFails = 0;
    private const int MAX_PICKUP_FAILS = 512;
#if ROBOOTCAMP
    public override void EnforceActivityRequirements()
    {
        if (Agent.Sensor.KnowsPlayerPosition)
            return;
        Commander.AddReward(-1);
        Commander.Done();
    }
#endif

    public bool HasObjectToThrow => objectToThrow != null;

    public bool FindObjectToThrow()
    {
        positionToLineOfSightPlayer = Agent.Sensor.player.transform.position;
        //Debug.Log("Trying to find object");
        //List<PhysicalObject> potentialObjects = squadSensor.GetPhysicalObjectsNearby();
        float closestObjectDistance = Mathf.Infinity;
        bool foundObject = false;
        // Should consider size, distance to object, and maybe it's density? (or enum PhysicalObjectType for now)
#if ROBOOTCAMP
    if(Commander.squad.physicalObjectsWithinRange.Count <= 0)
    {
        Commander.AddReward(-1);
        Commander.Done();
    }
#endif
        foreach (PhysicalObject physicalObject in Commander.squad.physicalObjectsWithinRange)
        {
            if (physicalObject == null)
                continue;
            //Debug.Log("Found this: " + physicalObject.name);
            if (physicalObject.objectType == PhysicalObjectType.Furniture || physicalObject.objectType == PhysicalObjectType.Agent ||
               physicalObject.objectType == PhysicalObjectType.Player || physicalObject.isPickedUp)
            {
                continue;
            }
            else
            {
                float distanceToThisObject = (physicalObject.transform.position - Agent.Sensor.transform.position).magnitude;
                if (distanceToThisObject < closestObjectDistance)
                {
                    closestObjectDistance = distanceToThisObject;
                    objectToThrow = physicalObject;
                    foundObject = true;
                }
            }
        }
        //Debug.Log("I will throw this object: " + objectToThrow.name + " :) :)");
#if ROBOOTCAMP
    if(foundObject == false)
        Commander.AddReward(-0.08f);
    else
        Commander.AddReward(0.01f);
#endif
        return foundObject;
    }

    public bool WalkTowardsObject()
    {
        // TODO: Need to use AgentActivity setDestination and GoToDestination here, either to pass it to statemachine moveTowards or call functions
        /*if (newPositionSet == false && HasObjectToThrow)
        {
            //AgentActivity.SetDestination(objectToThrow.transform.position);
            newPositionSet = true;
        }*/
        aimAtLerp = 0;
        if (HasObjectToThrow)
        {
            bool walkTowards = !Agent.ComplexActions.GoTowardsDestination(objectToThrow.transform.position, false, false);
#if ROBOOTCAMP
            if(walkTowards == true)
                Commander.AddReward(0.02f);
#endif

            return walkTowards;
        }

        return false;
    }

    public bool TryPickUp()
    {
        if (pickupFails > MAX_PICKUP_FAILS)
        {
            objectToThrow = null;
            pickupFails = 0;
            return false;
        }

        if (handToPickUpWith != null && objectToThrow != null && objectToThrow.transform.parent == handToPickUpWithWeaponPos)
            return true;
        if (handToPickUpWith != null && Agent.Sensor.GetItemInHand(handToPickUpWith) == null && objectToThrow != null)
        {
            if (Agent.Actions.PickupObject(objectToThrow, handToPickUpWith, handToPickUpWithWeaponPos) == objectToThrow)
            {
                aimAtLerp = 0;
                objectToThrow.transform.position = handToPickUpWithWeaponPos.transform.position;
#if ROBOOTCAMP
                    Commander.AddReward(0.04f);
#endif
                return true;
            }
            else
            {
                aimAtLerp += Time.deltaTime * pickUpSpeed; // Possibly fixedDeltaTime or another variable here?
                if (aimAtLerp > 1.05f)
                {
#if ROBOOTCAMP
                    Commander.AddReward(-0.04f);
#endif
                    // Couldn't pick up object, should give status code maybe? Instead of true/false bool, so state machine can retry another
                    // object or walk towards this object again
                    // TODO: Fix this
                    pickupFails++;
                    return false;
                }
                Agent.Actions.AimAt(handToPickUpWith, objectToThrow.transform.position, aimAtLerp);
                Agent.Actions.TurnTowards(objectToThrow.transform, torque, maxTorque, allowedError);
                pickupFails++;
                return false;
            }
        }
        else if (Agent.Sensor.GetItemInHand(Agent.Sensor.rightHand) == null)
        {
            handToPickUpWith = Agent.Sensor.rightHand;
            handToPickUpWithWeaponPos = handToPickUpWith.Find("WeaponPos");
        }
        else if (Agent.Sensor.GetItemInHand(Agent.Sensor.leftHand) == null)
        {
            handToPickUpWith = Agent.Sensor.leftHand;
            handToPickUpWithWeaponPos = handToPickUpWith.Find("WeaponPos");
        }
        else
        {
            // Should possibly drop item and decide a hand to pick up with
            // For now just picking out lefthand and dropping it's current item
            handToPickUpWith = Agent.Sensor.leftHand;
            handToPickUpWithWeaponPos = handToPickUpWith.Find("WeaponPos");
            previousHeldObject = Agent.Sensor.GetItemInHand(Agent.Sensor.leftHand);
            Agent.Actions.DropObject(previousHeldObject, handToPickUpWith, Agent.Sensor.GetVelocity());
#if ROBOOTCAMP
                    Commander.AddReward(-0.125f);
#endif
        }
        pickupFails++;
        return false;
    }

    public bool TryThrowObject()
    {
        if (Agent.Sensor.IsSeeingPlayer && objectToThrow != null)
        {
            Vector3 throwDirection = Agent.Sensor.player.transform.position - Agent.Sensor.transform.position;
            if (throwDirection.sqrMagnitude < maxThrowRange)
            {
                aimAtLerp += Time.deltaTime * throwSpeed;
                Agent.Actions.AimAt(handToPickUpWith, Agent.Sensor.player.transform.position, aimAtLerp);
                if (aimAtLerp >= 1)
                {
                        throwDirection.y += heightOffsetThrow * throwDirection.sqrMagnitude;
                        Agent.Actions.DropObject(objectToThrow, handToPickUpWith, throwDirection.normalized * throwForce);
                        //Debug.Log("Threw object: " + objectToThrow.name + " at player :) :)");
                        objectToThrow = null;
                        aimAtLerp = 0;
#if ROBOOTCAMP
                        Commander.AddReward(0.06f);
#endif
                        Agent.Sensor.CompleteObjective();
                    return true;
                }
            }
            else
            {

                // Could possible give an int error code and send to a function GetPlayerLineOfSight()
                // If position in moveTo is close to player, no need to update it, move towards
                // else: update new destination path
                if ((positionToLineOfSightPlayer - Agent.Sensor.player.transform.position).sqrMagnitude > 2.0f)
                {
                    Agent.ComplexActions.GoTowardsDestination(positionToLineOfSightPlayer, true, false);
                }
                else
                {
                    positionToLineOfSightPlayer = Agent.Sensor.player.transform.position;
                    Agent.ComplexActions.GoTowardsDestination(positionToLineOfSightPlayer, true, false);
                }
            }
        }
        else
        {
            Agent.Actions.TurnTowards(Agent.Sensor.player.transform, torque, maxTorque, allowedError);
            if ((positionToLineOfSightPlayer - Agent.Sensor.player.transform.position).sqrMagnitude > 2.0f)
            {
                Agent.ComplexActions.GoTowardsDestination(positionToLineOfSightPlayer, true, true);
            }
            else
            {
                positionToLineOfSightPlayer = Agent.Sensor.player.transform.position;
                Agent.ComplexActions.GoTowardsDestination(positionToLineOfSightPlayer, true, true);
            }
        }
        return false;
    }

    //public bool PickupObjectHeldBeforeThrow()
    //public bool GetPlayerLineOfSight()
}