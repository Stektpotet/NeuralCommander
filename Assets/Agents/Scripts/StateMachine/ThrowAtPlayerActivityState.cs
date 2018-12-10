using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThrowAtPlayerActivityState : AgentActivityState<ThrowAtPlayerActivity>
{

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (!activity.HasObjectToThrow) // Object was never found or has been destroyed
        {
            animator.SetBool("FoundObject", false);
            animator.SetBool("ObjectPickedUp", false); // Should possibly set a separate status for the animator like "ObjectDestroyed"
        }

        if (stateInfo.IsName("FindObjectToThrow"))
            animator.SetBool("FoundObject", activity.FindObjectToThrow());
        else if (stateInfo.IsName("GoTowardsObject"))
            animator.SetBool("NextToObject", activity.WalkTowardsObject());
        else if (stateInfo.IsName("TryPickUpObject"))
        {
            // TODO: Fix this and look at statemachine, some transitions should maybe be removed
            /*if (activity.aimAtLerp > 1) // This means agent couldn't pick up object
            {
                animator.SetBool("FoundObject", false);
                animator.SetBool("ObjectPickedUp", false);
                animator.SetBool("NextToObject", false);
            }*/
            //else
            animator.SetBool("ObjectPickedUp", activity.TryPickUp());
        }
        else if (stateInfo.IsName("ThrowObject"))
        {
            animator.SetBool("ObjectThrown", activity.TryThrowObject());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }
        animator.SetBool(PARAM_KNOWS_PLAYER_POSITION, activity.Agent.Sensor.KnowsPlayerPosition);
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
        if (!activity.HasObjectToThrow) // Object was never found or has been destroyed
        {
            animator.SetBool("FoundObject", false);
            animator.SetBool("ObjectPickedUp", false); // Should possibly set a separate status for the animator like "ObjectDestroyed"
        }

        if (stateInfo.IsName("FindObjectToThrow"))
            animator.SetBool("FoundObject", activity.FindObjectToThrow());
        else if (stateInfo.IsName("GoTowardsObject"))
            animator.SetBool("NextToObject", activity.WalkTowardsObject());
        else if (stateInfo.IsName("TryPickUpObject"))
        {
            // TODO: Fix this and look at statemachine, some transitions should maybe be removed
            /*if (activity.aimAtLerp > 1) // This means agent couldn't pick up object
            {
                animator.SetBool("FoundObject", false);
                animator.SetBool("ObjectPickedUp", false);
                animator.SetBool("NextToObject", false);
            }*/
            //else
            animator.SetBool("ObjectPickedUp", activity.TryPickUp());
        }
        else if (stateInfo.IsName("ThrowObject"))
        {
            animator.SetBool("ObjectThrown", activity.TryThrowObject());
            animator.SetBool(PARAM_OBJECTIVE_COMPLETE, activity.Agent.Sensor.IsObjectiveCompleted);
        }
        animator.SetBool(PARAM_KNOWS_PLAYER_POSITION, activity.Agent.Sensor.KnowsPlayerPosition);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        OnStateUpdate(animator, stateInfo, layerIndex);
    }
}
