using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverActivityState : AgentActivityState<CoverActivity>
{
    public const string
        STATE_FIND_COVER = "FindCover",
        STATE_MOVE_TO_COVER = "MoveToCover";

    public static int PARAM_FOUND_COVER => PARAM_FOUND_OBJECT;

    public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (stateInfo.IsName(STATE_FIND_COVER))
        {
            bool foundCover = activity.FindCover();
            animator.SetBool(PARAM_MOVING_TO_COVER, foundCover);
            animator.SetBool(PARAM_FOUND_COVER, foundCover);
        }
        else if (stateInfo.IsName(STATE_MOVE_TO_COVER))
        {
            animator.SetBool(PARAM_MOVING_TO_COVER, activity.MoveToCover());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }
        else if (stateInfo.IsName(STATE_WAIT))
            animator.SetBool(PARAM_WAITING, activity.Wait());
    }


    public override void OnStateUpdate( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        animator.SetBool(PARAM_FOUND_COVER, activity.HasCoverObject);

        if (stateInfo.IsName(STATE_MOVE_TO_COVER))
        {
            animator.SetBool(PARAM_MOVING_TO_COVER, activity.MoveToCover());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }
        else if (stateInfo.IsName(STATE_WAIT))
            animator.SetBool(PARAM_WAITING, activity.Wait());
    }
    public override void OnStateExit( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        animator.SetBool(PARAM_FOUND_COVER, activity.HasCoverObject);

        if (stateInfo.IsName(STATE_MOVE_TO_COVER))
        {
            animator.SetBool(PARAM_MOVING_TO_COVER, activity.MoveToCover());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }
        else if (stateInfo.IsName(STATE_WAIT))
            animator.SetBool(PARAM_WAITING, activity.Wait());
    }
}
