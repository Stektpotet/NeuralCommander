using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuppressActivityState : AgentActivityState<SuppressActivity>
{
     // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (stateInfo.IsName("SuppressPlayer"))
            activity.SuppressPlayer();
        animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (stateInfo.IsName("SuppressPlayer"))
            activity.SuppressPlayer();
        animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        animator.SetBool(PARAM_KNOWS_PLAYER_POSITION, activity.Agent.Sensor.KnowsPlayerPosition);
    }

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (stateInfo.IsName("SuppressPlayer"))
            activity.SuppressPlayer();
        animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
    }
    
}
