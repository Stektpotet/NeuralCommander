using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentActivityState<T> : StateMachineBehaviour where T : Activity
{
    public const string
        STATE_WAIT = "Wait";

    public static int
        PARAM_ROUTINE_STATUS      = Animator.StringToHash("RoutineStatus"),
        PARAM_PERFORM_SCAN        = Animator.StringToHash("PerformScan"),
        PARAM_FOUND_OBJECT        = Animator.StringToHash("FoundObject"),
        PARAM_NEXT_TO_OBJECT      = Animator.StringToHash("NextToObject"),
        PARAM_OBJECT_PICKED_UP    = Animator.StringToHash("ObjectPickedUp"),
        PARAM_OBJECT_THROWN       = Animator.StringToHash("ObjectThrown"),
        PARAM_MOVING_TO_COVER     = Animator.StringToHash("MovingToCover"),
        PARAM_FLANK_STEP_SUCCESS  = Animator.StringToHash("Flank StepSuccess"),
        PARAM_WAITING             = Animator.StringToHash("Waiting"),
        PARAM_OBJECTIVE_COMPLETE  = Animator.StringToHash("ObjectiveComplete"),
        PARAM_KNOWS_PLAYER_POSITION = Animator.StringToHash("KnowsPlayerPosition");

    protected T activity;
    protected int stateStepCounter = 0;
    const int STATE_STEPS_BEFORE_RETHINK = 512; //i.e. 512 physics-frames before rethink

    public override sealed void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        activity = animator.GetComponent<T>();
        activity.Agent.Sensor.ResetObjective();
#if ROBOOTCAMP
        activity.EnforceActivityRequirements();
#endif
    }

    public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
    {
        activity = animator.GetComponent<T>();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateStepCounter >= STATE_STEPS_BEFORE_RETHINK) //TODO maybe put this in CommanderAgent instead
        {
            Debug.LogWarning("Re-think!");
            activity.Commander.RequestDecision();
            stateStepCounter = 0;
        }
        stateStepCounter++;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        activity.Commander.RequestDecision();
    }
    public override sealed void OnStateMachineExit( Animator animator, int stateMachinePathHash )
    {
        foreach(var param in animator.parameters)
        {
            if(param.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(param.nameHash, false);
        }
    }
}