using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoverActivity : Activity
{
    public const int ROUTINE_IDENTIFIER = 2;

    [SerializeField] private LayerMask coverLayers;
    PhysicalObject usingAsCover;
    Vector3 coverPosition;

    public bool HasCoverObject 
        => usingAsCover != null;

    private bool FindCoverAlone()
    {
        PhysicalObject closestCover = null;
        float sqrDistance = float.MaxValue;

        foreach (var potentialCover in Commander.squad.physicalObjectsWithinRange)
        {
            float potentialCoverSqrDistance = (transform.position - potentialCover.transform.position).sqrMagnitude;
            if (potentialCover.objectType == PhysicalObjectType.Furniture
              && sqrDistance > potentialCoverSqrDistance
               )
            {
                closestCover = potentialCover;
                sqrDistance = potentialCoverSqrDistance;
            }
        }

        if (closestCover != null)
        {
            usingAsCover = closestCover;
            return true;
        }
        return false;
    }
    private bool FindCoverWithOthers()
    {
        PhysicalObject closestCover = null;
        float sqrDistance = float.MaxValue;

        foreach (var potentialCover in Commander.squad.physicalObjectsWithinRange)
        {
            float potentialCoverSqrDistance = (transform.position - potentialCover.transform.position).sqrMagnitude;
            if (potentialCover.objectType == PhysicalObjectType.Furniture
              && sqrDistance > potentialCoverSqrDistance
               )
            {
                bool used = false;
                foreach(var unit in Commander.squad.units)
                {
                    if (unit == Agent) continue;
                    if(unit?.GetComponent<CoverActivity>().usingAsCover == potentialCover)
                    { used = true; break; }
                }
                
                if(!used)
                {
                    closestCover = potentialCover;
                    sqrDistance = potentialCoverSqrDistance;
                }
            }
        }

        if (closestCover != null)
        {
            usingAsCover = closestCover;

            return true;
        }
        return false;
    }

    public bool FindCover()
    {


#if ROBOOTCAMP
        //Avoid having all agents in same activity
        if (Commander.squad.NumberOfOtherAgentsInSameActivity(Agent) > 0)
            Commander.AddReward(-0.01f * Commander.squad.NumberOfOtherAgentsInSameActivity(Agent));
#endif

        //Avoid having all agents in same activity
        if (Commander.squad.physicalObjectsWithinRange.Count == 0)
        {
#if ROBOOTCAMP
            Commander.AddReward(-1); //you should never go here if there's no object to hide behind
            Commander.Done();
#endif
            Agent.Sensor.CompleteObjective(); //allow the agent to make new decision
        }

        if (Commander.squad.NumberOfOtherAgentsInSameActivity(Agent) > 0)
            return FindCoverWithOthers();
        else
            return FindCoverAlone();

    }

    public bool MoveToCover()
    {
        if (usingAsCover == null)
            return false;
        //coverPosition = (Agent.Sensor.player?.transform.position - usingAsCover.transform.position) ?? Vector3.zero;
        coverPosition = usingAsCover.transform.position - (Agent.Sensor.player.transform.position - usingAsCover.transform.position).normalized;

        if (Agent.ComplexActions.GoTowardsDestination(coverPosition, false, false))
        {

            return true;
        }
        else
        {
#if ROBOOTCAMP
            Commander.AddReward(0.1f);
#endif
            Agent.Sensor.CompleteObjective();
            return false; //if the agent is done moving to destination
        }
    }
    
}
