using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; // tolist

//TODO Rename CommonAgentActivities
public class UnitComplexActions : MonoBehaviour {

    const int WALKABLE_AREA_MASK = 0;
    public UnitActions actions;
    public UnitSensor sensor;

    public float jumpCooldown = 0.15f;
    private float lastJumpTime;

    private float lastStuckIncrement;
    private int stuckIteration;
    public float stuckIterationTime = 0.3f;
    

    private NavMeshPath     path;
    private List<Vector3>   pathCornerList = new List<Vector3>();
    private int cornerIndex;

    public float cornerArriveThreshold = 0.1f;
    public float moveSpeed = 10f;
    public float maxJumpForce = 500f;

    public float torque = 0.5f;
    public float maxTorque = 1.0f;
    public float allowedRotationError = 12.5f;
    private Vector3 relativeHeight;
    private Vector3 obstaclesCheckExtend;
    private LayerMask agentLayer;
    private LayerMask physicalLayer;

    private Vector3 prevDestination;



    // Use this for initialization
    void Start () {
        actions         = GetComponent<UnitActions>();
        if (actions == null) Debug.LogError("actions is null in complexactions");

        sensor          = GetComponent<UnitSensor>();
        if (sensor == null) Debug.LogError("sensor is null in complexactions");

        path            = new NavMeshPath();
        relativeHeight  = new Vector3(0, transform.position.y, 0);
        if(relativeHeight.y > 1.6)
        {
            Debug.Log("make sure to spawn agent right above ground");
        }

        stuckIteration          = -1;
        obstaclesCheckExtend    = new Vector3(0.2f, transform.position.y, 1f);
        lastJumpTime            = Time.timeSinceLevelLoad;
        lastStuckIncrement      = Time.realtimeSinceStartup;
        agentLayer              = LayerMask.GetMask("Agent");
        physicalLayer           = LayerMask.GetMask("PickupAble");


        if (sensor.rightStartWeapon != null)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), sensor.rightStartWeapon.GetComponent<Collider>());
            actions.PickupObject(sensor.rightStartWeapon, sensor.rightHand, sensor.rightWeaponPos);
        }
        if (sensor.leftStartWeapon != null)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), sensor.leftStartWeapon.GetComponent<Collider>());
            actions.PickupObject(sensor.leftStartWeapon, sensor.leftHand, sensor.leftWeaponPos);
        }
    }

    private void OnDrawGizmos()
    {
        if (cornerIndex >= pathCornerList.Count || pathCornerList.Count < 1)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 diff = transform.position - relativeHeight - pathCornerList[cornerIndex];
        Vector3 diffxz = diff;
        diffxz.y = 0;
        Gizmos.DrawCube((transform.position) - (diffxz.normalized * obstaclesCheckExtend.z), obstaclesCheckExtend);

        //newDestination = GoToDestination();
        for (int i = 0; i < pathCornerList.Count - 1; i++)
        {
            Color cornerColor = Color.green;
            if(i < cornerIndex)
            {
                cornerColor = Color.red;
            }

            Debug.DrawLine(pathCornerList[i], pathCornerList[i+1], cornerColor, 0.5f);
        }
    }
   

    private void SetDestination(Vector3 destination)
    {
        NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path); // TODO: replace this, it's very heavy and will cause frame drops
        cornerIndex         = 1;
        pathCornerList      = path.corners.ToList();
    }


    /// <summary>
    /// Function for iterating trough path and moving towards corners
    /// </summary>
    /// <returns>true if there are more points still, false otherwise</returns>
    public bool GoTowardsDestination(Vector3 destination, bool strafe = true, bool doObstacleRoutine = true)
    {
        Vector3 destinationInNavmesh    = destination;
        destinationInNavmesh.y          = 0.1f;

        Vector3 forward = sensor.GetVelocity();
        forward.y       = 0;
        forward         = forward.normalized;
        Vector3 right   = Quaternion.Euler(0, 90, 0) * forward;

        if (sensor.foot != null && sensor.OriginalHeight > 0)
            actions.StandUp(sensor.foot, sensor.OriginalHeight);
        
        if (pathCornerList.Count <= 0 || (prevDestination - destinationInNavmesh).sqrMagnitude > 0.1f)
        {
            SetDestination(destinationInNavmesh);

            if (pathCornerList.Count <= 0)
            {
                Vector3 samplePos = SamplePosition(destination, 5.0f); // This one seems to be rather heavy
                if(!samplePos.Equals(destination))
                {
                    SetDestination(samplePos);
                }
                if (pathCornerList.Count <= 0)
                {
                    StuckRoutine(right, forward, 6f);
                    cornerIndex = 0;
                    return false;
                }
            }

            prevDestination = destinationInNavmesh;
        }

        // TODO: Look into why this gives null reference at certain times
        Vector3 diff = transform.position - relativeHeight - pathCornerList[cornerIndex];

        Vector3 diffxz = diff;
        diffxz.y = 0;

        if (diff.magnitude < cornerArriveThreshold || 
            (diffxz.magnitude < cornerArriveThreshold * 0.66f && diff.y - transform.position.y < cornerArriveThreshold*2f))
        {
            cornerIndex++;
        }

        if (cornerIndex >= pathCornerList.Count)
        {
            pathCornerList.Clear();
            return false;
        }      

      
        Debug.DrawLine(transform.position, transform.position + forward, Color.red);
        Debug.DrawLine(transform.position, transform.position + right, Color.red);

        if(doObstacleRoutine)
            ObstacleRoutine(diffxz, destination);


        if (cornerIndex >= pathCornerList.Count) // quick fix: because ObstacleRoutine can change path
        {
            pathCornerList.Clear();
            return false;
        }

        if (!strafe)
            actions.TurnTowards(pathCornerList[cornerIndex], torque, maxTorque, allowedRotationError);

        if (sensor.IsNotMoving) // not stuck
        {
            StuckRoutine(right, forward);
        }
        else
        {
            stuckIteration = -1;
            if (pathCornerList.Count > 0)
                actions.MoveTowards(pathCornerList[cornerIndex], moveSpeed);
        }

        return true;
    }

    private void StuckRoutine(Vector3 right, Vector3 forward, float moveSpeedModifier = 3f)
    {
        if(Time.realtimeSinceStartup - stuckIterationTime > stuckIteration)
        {
            stuckIteration += 1;
            lastStuckIncrement = Time.realtimeSinceStartup;
        }

        switch (stuckIteration)
        {
            case 0:
                if (pathCornerList.Count > 0)
                    actions.MoveTowards(pathCornerList[cornerIndex], moveSpeed * moveSpeedModifier);
                break;
            case 1:
                actions.MoveTowards(transform.position + right, moveSpeed * moveSpeedModifier);
                break;
            case 2:
                actions.MoveTowards(transform.position - right, moveSpeed * moveSpeedModifier);
                break;
            case 3:
                actions.MoveTowards(transform.position - forward, moveSpeed * moveSpeedModifier);
                break;
            /*case 4:
                // SamplePosition towards next corner and insert valid position in corners
                break;
            case 5:
                // panic shoot 
                break;*/
            default:
                stuckIteration = 0;
                break;
        }

    }

    void ObstacleRoutine(Vector3 diffxz, Vector3 destination)
    {
        List<Collider> obstacles = Physics.OverlapBox((transform.position) - (diffxz.normalized * obstaclesCheckExtend.z), obstaclesCheckExtend, transform.rotation, physicalLayer).ToList();

        if (obstacles.Count > 0 && obstacles.Exists(col => col.gameObject.layer != agentLayer))
        {
            bool didJump = false;
            if (Time.timeSinceLevelLoad - lastJumpTime > jumpCooldown && sensor.IsGrounded) // TODO: check if possible to jump over
            {

                Collider theObstacle = obstacles.First(col => col.gameObject.layer != agentLayer);
                float height = theObstacle.bounds.extents.y + theObstacle.bounds.center.y;
                float jumpforce = 280f * height;

                if (jumpforce < maxJumpForce)
                {
                    actions.Jump(jumpforce);
                    didJump = true;
                    lastJumpTime = Time.timeSinceLevelLoad;
                }
            }

            if (!didJump && sensor.IsGrounded)
            {
                SetDestination(destination);
            }

            // else if you can shoot through, do so
        }
    }

    // This seems to be a bit too heavy atm
    private Vector3 SamplePosition(Vector3 position, float range) 
    {
        
        NavMeshHit hit = new NavMeshHit();
        if (NavMesh.SamplePosition(position, out hit, range, WALKABLE_AREA_MASK))
        {
            Debug.DrawRay(hit.position, Vector3.up, Color.blue, 5);
            return hit.position;
        }
        return position;
    }
}
