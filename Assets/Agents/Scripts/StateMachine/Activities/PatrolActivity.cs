using System.Linq;
using UnityEngine;

public class PatrolActivity : Activity
{
    

    [Range(0,1)]
    Vector3 rotationAxis;

    Vector3[] patrolRoute;
    int patrolRouteIndex = 0;
    Vector3 currentDestination = Vector3.zero;

    public float scanTime = 3;
    float scanTimer = 0;
    bool scanning;
    Quaternion scanRotation = new Quaternion(0,1,0,2 * Mathf.PI);

#if ROBOOTCAMP
    public override void EnforceActivityRequirements()
    {
        if (Agent.Sensor.KnowsPlayerPosition)
        {
            //YOU ALREADY KNOW WHERE THE PLAYER IS!! dont patrol ffs
            Commander.AddReward(-1);
            Commander.Done();
        }
    }
#endif

    public void CreatePatrol()
    {
        if (patrolRoute == null) //TODO: or there's a new room? -> maybe broadcast through FSM variables?
        {
            patrolRoute = Commander.squad.currentRoom.corners.Select(v => new Vector3(v.x, 0, v.y)).ToArray();
            patrolRouteIndex = (patrolRouteIndex + Agent.squadUnitIndex + (Commander.squad.NumberOfOtherAgentsInSameActivity(Agent) / patrolRoute.Length)) % patrolRoute.Length;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>True when unit is at </returns>
    public bool Patrol()
    {
        if (Agent.Sensor.IsSeeingPlayer)
            Agent.Sensor.CompleteObjective();
        if (!Agent.ComplexActions.GoTowardsDestination(patrolRoute[patrolRouteIndex], false, true)) //if the agent is done moving to destination
        {
            Commander.AddReward(0.01f);
            if (++patrolRouteIndex >= patrolRoute.Length) // start from the first point again
                patrolRouteIndex = 0;
            //the unit got to a point in the patrol route
            return true;
        }
        return false;
    }

    public bool Scan()
    {
        if (scanTimer > scanTime)
        {
            scanTimer = 0;
            return false;
        }
        Agent.Actions.RotateTo(Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis((scanTimer / scanTime) * 360, Vector3.up), (scanTimer / scanTime)));
        scanTimer += Time.fixedDeltaTime;
        if (Agent.Sensor.IsSeeingPlayer)
        {
            scanTimer = 0;
            Agent.Sensor.CompleteObjective();
            return false;
        }
        return true;
        //Agent.transform.Rotate(rotationAxis, scanRotationDelta * 360);

    }
}    
