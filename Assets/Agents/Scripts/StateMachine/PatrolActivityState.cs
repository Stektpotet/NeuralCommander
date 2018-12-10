using UnityEngine;
using System.Linq;
using UnityEngine.Animations;

public class PatrolActivityState : AgentActivityState<PatrolActivity>
{

    public const string
            STATE_PATROL = "Patrol",
            STATE_SCAN = "Scan";


    public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (stateInfo.IsName(STATE_PATROL))
        {
            activity.CreatePatrol();
            animator.SetBool(PARAM_PERFORM_SCAN, activity.Patrol());
        }
        else if (stateInfo.IsName(STATE_SCAN))
        {
            animator.SetBool(PARAM_PERFORM_SCAN, activity.Scan());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }
    }

    public override void OnStateUpdate( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (stateInfo.IsName(STATE_PATROL))
            animator.SetBool(PARAM_PERFORM_SCAN, activity.Patrol());
        else if (stateInfo.IsName(STATE_SCAN))
        {
            animator.SetBool(PARAM_PERFORM_SCAN, activity.Scan());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }


    }
    
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (stateInfo.IsName(STATE_PATROL))
            animator.SetBool(PARAM_PERFORM_SCAN, activity.Patrol());
    }
}