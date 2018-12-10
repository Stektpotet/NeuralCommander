using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Could add situations here where player for example would target unit's weapons instead of the unit's themselves
public enum PlayerSituation
{
    AimBotSetupSingleTarget,
    WalkingTowardsRoom,
    HidingFromAgents,
    MovingAroundInRoom
}

public class SimulatePlayerActions : UnitActions {

    const int MASK_VISION = 6292279;
    const int LAYER_AGENT = 21;
    public float torque = 3.0f;
    public float maxTorque = 6.0f;
    public float allowedError = 60.0f;

    public float aimOffset = 0.05f;

    public float aimSpeed = 7.0f;
    public float moveSpeed = 5.0f;
    public float jumpHeight = 200.0f;
    public float awarenessRadius = 20.0f;
    public float calculateNewTargetCooldown = 2.0f;
    public float calculateNewTargetTimer;
    public float cooldownToStart = 2.0f;
    public float timerToStart;


    public float moveWhenUnseenTime = 5.0f;
    private float moveTimer = 0;
    public Vector3[] teleportTargets;
    private int currentPositionIndex;

    public float randomAimOffset = 1.5f; // Maybe not use?
    private float leftHandLerpPos;
    private float rightHandLerpPos;

    public Transform leftHand;
    public Transform rightHand;
    public Transform leftHandWeaponPos;
    public Transform rightHandWeaponPos;

    public Transform destination;
    public Transform target;
    public WeaponPhysicalObject leftPickupTarget;
    public WeaponPhysicalObject rightPickupTarget;
    public LayerMask agentLayerMask = 2097152;  //Agent layer mask

    public PlayerSituation playerSituation = PlayerSituation.AimBotSetupSingleTarget;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        rbCollider = rb.GetComponent<Collider>();
        leftHandWeaponPos = transform.GetChild(0).GetChild(0);
        rightHandWeaponPos = transform.GetChild(1).GetChild(0);
        PickupObject(leftPickupTarget, leftHand, leftHandWeaponPos);
        Physics.IgnoreCollision(leftPickupTarget.GetComponent<Collider>(), rbCollider);
        PickupObject(rightPickupTarget, rightHand, rightHandWeaponPos);
        Physics.IgnoreCollision(rightPickupTarget.GetComponent<Collider>(), rbCollider);
        calculateNewTargetTimer = 0;
        timerToStart = 0;
        leftHandLerpPos = 0;
        rightHandLerpPos = 0;
    }
	
	// Update is called once per frame
	void Update () {
        if (target == null)
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > moveWhenUnseenTime)
            {
                TeleportNext();
                moveTimer = 0;
            }
        }
        else moveTimer = 0;

        // Doing this to make sure to reload and weapons being picked up
        /*timerToStart += Time.deltaTime;
        if(timerToStart > cooldownToStart)
        {*/
            //timerToStart = cooldownToStart;
            if (playerSituation == PlayerSituation.AimBotSetupSingleTarget)
            {
                DoAimBotSetupSingleTarget();
            }
        //}
        if(transform.position.y < -10)
        {
            Destroy(gameObject);
        }
	}

    private void TeleportNext()
    {
        currentPositionIndex = (currentPositionIndex + 1) % teleportTargets.Length;
        transform.position = teleportTargets[currentPositionIndex];
    }

    private void DoAimBotSetupSingleTarget()
    {
        if(target == null || (Time.realtimeSinceStartup - calculateNewTargetTimer) > calculateNewTargetCooldown)
        {
            target = GetNewTarget();
            leftHandLerpPos = 0;
            rightHandLerpPos = 0;
        }
        if(target != null)
        {
            float aimOffsetY = (target.position - transform.position).magnitude * aimOffset;
            Vector3 AimPosition = new Vector3(target.position.x - randomAimOffset + Random.value * randomAimOffset * 2, target.position.y + aimOffsetY - randomAimOffset + Random.value * randomAimOffset * 2, target.position.z - randomAimOffset + Random.value * randomAimOffset * 2);
            TurnTowards(target, torque, maxTorque, allowedError);
            rightHandLerpPos = leftHandLerpPos += Time.deltaTime * aimSpeed;
            
            if(rightHandLerpPos >= 1)
            {
                rightHandLerpPos = leftHandLerpPos = 1;
            }
            AimAt(leftHand, AimPosition, leftHandLerpPos);
            AimAt(rightHand, AimPosition, rightHandLerpPos);

            if (HasAmmoInGun(leftPickupTarget))
                ActivateObject(leftPickupTarget, leftHandWeaponPos);
            else
                ReloadWeapon(leftPickupTarget, 1.0f);

            if (HasAmmoInGun(rightPickupTarget))
                ActivateObject(rightPickupTarget, rightHandWeaponPos);
            else
                ReloadWeapon(rightPickupTarget, 1.0f);
        }

    }

    private Transform GetNewTarget()
    {
        float shortestDistance = Mathf.Infinity;
        Transform bestTarget = null;
        RaycastHit hit;
        foreach(Collider agent in Physics.OverlapSphere(transform.position, awarenessRadius, agentLayerMask))
        {
            float distance = (agent.transform.position - transform.position).sqrMagnitude;
            if (distance < shortestDistance && Physics.Raycast(transform.position, (agent.transform.position - transform.position).normalized, out hit, distance, MASK_VISION)) {
                if (hit.collider.gameObject.layer == LAYER_AGENT)
                {
                    shortestDistance = distance;
                    bestTarget = agent.transform;
                }
            }
        }

        return bestTarget;
    }

    public bool HasAmmoInGun(WeaponPhysicalObject weaponObject)
    {
        return weaponObject.loadedMagazine.IsNotEmpty();
    }

}
