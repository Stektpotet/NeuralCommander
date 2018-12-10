using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class CommanderAgent : Agent
{
    public const int MAX_SQUAD_SIZE = 5;
    
    [Header("Explicit agent input")]
    public Squad squad;

    [MinMaxSlider(1, MAX_SQUAD_SIZE)]
    public Vector2Int unitsInSquad;

    public Color32 squadColor;

    protected int currentDecisionStep = 1;
    protected bool isNewDecisionStep = true;
    protected int squadDataInterval = 5;
    protected int currentSquadIntervalCount = 0;

    public void WakeUpSquad()
    {
        RequestDecision();
    }
    protected bool[] lastDescision = new bool[MAX_SQUAD_SIZE * Squad.NUMER_OF_ACTIVITIES];
    public override void CollectObservations()
    {
        //Debug.Log("Collecting observations");
        currentSquadIntervalCount++;
        if(currentSquadIntervalCount >= squadDataInterval)
        {
            squad.UpdateCollectiveInformation();
            currentSquadIntervalCount = 0;
        }
        AddVectorObs(squad.playerWithinSquadRange); //NOTE THIS WILL ENABLE WALLHACK FOR THE AGNETS!!! TODO: make a sensor that uses the combination of this and the unit.seeingPlayer-observation

        AddVectorObs(squad.physicalObjectsWithinRange.Count > 0);
        AddVectorObs(squad.weaponsWithinRange.Count > 0);

        AddVectorObs(squad.GetActivityHistogram());
        foreach (bool unitDescision in lastDescision)
            AddVectorObs(unitDescision);

        //NOTE maybe add observations on what stuff the player has: weapons, ammo, health etc.

        for (int i = 0; i < MAX_SQUAD_SIZE; i++)
        {
            SquadUnit unit = (i < squad.units.Length) ? squad.units[i] : null;
            foreach (bool observation in Squad.GetAgentBooleanObservation(unit))
            {
                AddVectorObs(observation);
            }
            AddVectorObs(Squad.GetAgentFloatObservations(unit));
        }
    }

    public override void AgentAction( float[] act, string textAction )
    {
        lastDescision.Clear();
        for (int i = 0; i < squad.units.Length; i++)
        {
            if (squad[i] != null)
            {
                int action = (int)act[i];
                int last = squad[i].StateMachineController.GetInteger("RoutineStatus");
                if ((last != action) && !squad[i].Sensor.IsObjectiveCompleted)
                {
                    AddReward(-1);//punish not completing objectives
                }
                squad[i].StateMachineController.SetInteger("RoutineStatus", action);
                lastDescision[i * Squad.NUMER_OF_ACTIVITIES + action] = true;

            }
            //else
            //{
            //    //penalize giving tasks to the dead
            //    for (int j = 0; j < Squad.NUMER_OF_ACTIVITIES; j++)
            //        AddReward(-0.1f);
            //}
            //int bestAction = 0; //first action defaults to best - it might be a good idea to make the first action of the highLevelAPI DoNothing()
            //float bestActionScore = -1;

            //if (squad[i] != null)
            //{
            //    for (int j = 0; j < Squad.NUMER_OF_ACTIVITIES; j++)
            //    {
            //        if (bestActionScore < act[i * Squad.NUMER_OF_ACTIVITIES + j]) // TODO: this gives indexOutOfRangeException on ActivityMap?
            //        {
            //            bestActionScore = act[i * Squad.NUMER_OF_ACTIVITIES + j];
            //            bestAction = j;
            //        }
            //    }
            //    var lastAction = squad[i].StateMachineController.GetInteger("RoutineStatus");
            //    squad[i].StateMachineController.SetInteger("RoutineStatus", bestAction);
            //    if((lastAction != bestAction) && !squad[i].Sensor.IsObjectiveCompleted)
            //    {
            //        AddReward(-1); //punish not completing objectives
            //    }
            //}
            //else
            //{
            //    //penalize giving tasks to the dead
            //    for (int j = 0; j < Squad.NUMER_OF_ACTIVITIES; j++)
            //        AddReward(-0.1f * Mathf.Clamp01(act[i * Squad.NUMER_OF_ACTIVITIES + j]));
            //}
        }
        squad.UpdateUnitStates();

        
#if ROBOOTCAMP
        foreach(var unit in squad.units)
        {
            if(unit != null && unit.transform.position.y < -1)
                Destroy(unit.gameObject);
        }
#endif
    }
    private void FixedUpdate()
    {
        bool allDead = true;
        foreach (var unit in squad.units)
            allDead &= (unit == null);
        if (allDead)
        {
            AddReward(-1);
            Done();
        }
    }
}
