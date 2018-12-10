using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* TODO:
 * if not armed punish for this activity and exit
 * maybe: if not in cover punish a little bit
*/
public class SuppressActivity : Activity {

    const float blindSupressTime = 5f;

    Vector3 prevCalcPlayerPos;
    Vector3 lastSeenPlayerPos;
    Vector3 prevAgentPos;
    Vector3 aimAtTarget;
    public float distanceUpdateThreshold = 1f;
    bool reachedTarget = false;
    float timeSinceSawPlayer = Mathf.Infinity;

    WeaponPhysicalObject leftWeapon;
    float leftSlerpPos;
    public Transform leftWeaponHand;
    Vector3 leftAimAtTarget;
    Quaternion leftStartSlerpRotation;

    WeaponPhysicalObject rightWeapon;
    float rightSlerpPos;
    public Transform rightWeaponHand;
    Vector3 rightAimAtTarget;
    Quaternion rightStartSlerpRotation;
    bool seePlayer;
    bool prevFrameSeePlayer;

#if ROBOOTCAMP
    public override void EnforceActivityRequirements()
    {
        if(Agent.Sensor.KnowsPlayerPosition)
            return;
        Commander.AddReward(-1);
        Commander.Done();
    }
#endif

    public void SuppressPlayer()
    {
        
        if(seePlayer || Agent.Sensor.IsTakingDmg || Time.realtimeSinceStartup - timeSinceSawPlayer > blindSupressTime)
            Agent.Actions.TurnTowards(Agent.Sensor.player.transform,
                Agent.ComplexActions.torque * 2f, Agent.ComplexActions.maxTorque, Agent.ComplexActions.allowedRotationError);
        else if (lastSeenPlayerPos == Vector3.zero)
            Agent.Actions.TurnTowards(Agent.Sensor.player.transform,
               Agent.ComplexActions.torque * 2f, Agent.ComplexActions.maxTorque, Agent.ComplexActions.allowedRotationError);
        else
            Agent.Actions.TurnTowards(lastSeenPlayerPos,
                Agent.ComplexActions.torque * 2f, Agent.ComplexActions.maxTorque, Agent.ComplexActions.allowedRotationError);

        UpdateWeaponsAndTarget();

        if (leftWeapon != null && leftWeapon.loadedMagazine != null)
        { 

            if (reachedTarget)
                leftSlerpPos += Agent.ComplexActions.torque * Time.fixedDeltaTime * 10f;
            else
                leftSlerpPos += Agent.ComplexActions.torque * Time.fixedDeltaTime * 5;
        
                
            if (leftSlerpPos > 1) leftSlerpPos = 1;

            Agent.Actions.AimAt(Agent.Sensor.leftHand, leftStartSlerpRotation, leftAimAtTarget, leftSlerpPos);

            if (leftSlerpPos >= 1f)
            {
                reachedTarget = true;
                if (seePlayer || lastSeenPlayerPos != Vector3.zero)
                {
                    leftSlerpPos = 0f;
                    leftAimAtTarget = aimAtTarget + GetRandomVectorOffset(3f);
                    leftStartSlerpRotation = leftWeaponHand.rotation;
                }
            }
            if (reachedTarget && lastSeenPlayerPos != Vector3.zero && Time.realtimeSinceStartup - timeSinceSawPlayer < blindSupressTime)
            {
                //Agent.Actions.ActivateObject(leftWeapon, leftWeaponHand);
                bool? leftMagazineLoaded = leftWeapon.loadedMagazine?.IsNotEmpty();
                if (leftMagazineLoaded != null && leftMagazineLoaded == true)
                {
                    Agent.Actions.ActivateObject(leftWeapon, leftWeaponHand);
#if ROBOOTCAMP
        Commander.AddReward(0.001f);
#endif
                }
                else
                {
                    leftWeapon.loadedMagazine.AgentReload(0.5f + Random.value * 1.5f);
                }
            }
        }

        if (rightWeapon != null && rightWeapon.loadedMagazine != null)
        {
            if (reachedTarget)
                rightSlerpPos += Agent.ComplexActions.torque * Time.fixedDeltaTime * 10f;
            else
                rightSlerpPos += Agent.ComplexActions.torque * Time.fixedDeltaTime;
            if (rightSlerpPos > 1) rightSlerpPos = 1;

            Agent.Actions.AimAt(Agent.Sensor.rightHand, rightStartSlerpRotation, rightAimAtTarget, rightSlerpPos);

            if (rightSlerpPos >= 1f)
            {
                reachedTarget = true;
                if (seePlayer || lastSeenPlayerPos != Vector3.zero)
                {
                    rightSlerpPos = 0;
                    rightAimAtTarget = aimAtTarget + GetRandomVectorOffset(3f);
                    rightStartSlerpRotation = rightWeaponHand.rotation;
                }
            }

            if (reachedTarget && lastSeenPlayerPos != Vector3.zero && Time.realtimeSinceStartup - timeSinceSawPlayer < blindSupressTime)
            {
                bool? rightMagazineLoaded = rightWeapon.loadedMagazine?.IsNotEmpty();
                if(rightMagazineLoaded != null && rightMagazineLoaded == true)
                {
                    Agent.Actions.ActivateObject(rightWeapon, rightWeaponHand);
                    Agent.Sensor.CompleteObjective();
                    Commander.AddReward(0.001f);
                }
                else
                {
                    rightWeapon.loadedMagazine.AgentReload(0.5f + Random.value * 1.5f);
                }
            }

        }
        
        return;
    }

    void UpdateWeaponsAndTarget()
    {
        seePlayer = Agent.Sensor.IsSeeingPlayer;

        if (seePlayer)
        {
            lastSeenPlayerPos = Agent.Sensor.player.transform.position;
            timeSinceSawPlayer = Time.realtimeSinceStartup;
        }

        if ((prevCalcPlayerPos - Agent.Sensor.player.transform.position).sqrMagnitude > distanceUpdateThreshold ||
            (prevAgentPos - transform.position).sqrMagnitude > distanceUpdateThreshold || (seePlayer && !prevFrameSeePlayer))
        {
            prevCalcPlayerPos   = Agent.Sensor.player.transform.position;
            prevAgentPos        = transform.position;

            if (leftWeapon == null)
            {
                try
                { leftWeapon = (WeaponPhysicalObject)Agent.Sensor.GetItemInHand(Agent.Sensor.leftHand); }
                catch (System.InvalidCastException) { leftWeapon = null; }
#if ROBOOTCAMP
                if(leftWeapon == null)
                    Commander.AddReward(-0.05f);
#endif
            }
            if (rightWeapon == null)
            {
                try
                { rightWeapon = (WeaponPhysicalObject)Agent.Sensor.GetItemInHand(Agent.Sensor.rightHand); }
                catch (System.InvalidCastException) { rightWeapon = null; }
#if ROBOOTCAMP
                if (rightWeapon == null)
                    Commander.AddReward(-0.05f);
#endif
            }
#if ROBOOTCAMP
            if (!Agent.Sensor.IsArmed)
            {
                Commander.AddReward(-1.00f);
                Commander.Done();
            }
#endif


            leftStartSlerpRotation = leftWeaponHand.rotation;
            rightStartSlerpRotation = rightWeaponHand.rotation;
            reachedTarget           = false;
            leftSlerpPos            = 0f;
            rightSlerpPos           = 0f;

            if ((leftWeapon != null || rightWeapon != null) && (lastSeenPlayerPos != Vector3.zero || Time.realtimeSinceStartup - timeSinceSawPlayer < blindSupressTime) && seePlayer)
            {
            
                aimAtTarget             = GetTargetToHit(lastSeenPlayerPos, 0.1f);
                
                float diffSqrtMagnitude = (transform.position - Agent.Sensor.player.transform.position).sqrMagnitude * 0.3f;
                leftAimAtTarget         = aimAtTarget + GetRandomVectorOffset(diffSqrtMagnitude);
                rightAimAtTarget        = aimAtTarget + GetRandomVectorOffset(diffSqrtMagnitude);
            }
           
        }
        if (!seePlayer && (lastSeenPlayerPos == Vector3.zero || Time.realtimeSinceStartup - timeSinceSawPlayer > blindSupressTime))
        {
            aimAtTarget = GetTargetToHit(lastSeenPlayerPos, 0.1f);
            leftAimAtTarget = Agent.Sensor.leftHand.position + transform.forward;
            rightAimAtTarget = Agent.Sensor.rightHand.position + transform.forward;
        }

        prevFrameSeePlayer = seePlayer;
    }

    Vector3 GetRandomVectorOffset(float offset, float heightModifier = 0.2f)
    {
        return new Vector3((Random.value * 2 - 1) * offset, (Random.value * 2 - 1) * offset * heightModifier, (Random.value * 2 - 1) * offset);
    }
}
