using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class holding shared knowledge and methods of inferring shared knowledge fopr the squad
/// Update on Desicion steps for the commander agent
/// </summary>
[System.Serializable]
public class Squad
{
    public const int NUMER_OF_ACTIVITIES = 4;

    public SquadSensor squadSensor;
    public SquadUnit[] units = new SquadUnit[CommanderAgent.MAX_SQUAD_SIZE]; //needs to be of constans size for usage in the ANN input and output
    public Dictionary<SquadUnit, int> UnitActivity = new Dictionary<SquadUnit, int>(NUMER_OF_ACTIVITIES+1); //+idle
    public SquadUnit this[int i] => units[i];
    public bool playerWithinSquadRange;
    public List<PhysicalObject> physicalObjectsWithinRange = new List<PhysicalObject>();
    public List<WeaponPhysicalObject> weaponsWithinRange = new List<WeaponPhysicalObject>();

    private int[] numberOfUnitsInActivity = new int[NUMER_OF_ACTIVITIES]; //TODO have a const for number of activities
    int _numberOfAliveUnits = 0;
    public int NumberOfAliveUnits => _numberOfAliveUnits;

    public Room currentRoom;
    
    public void AddUnit(SquadUnit unit)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null)
            {
                unit.squadUnitIndex = i;
                units[i] = unit;
                return;
            }
        }
        Debug.LogError("THERES NO MORE SPACE FOR UNITS, YOU MUST'VE FFFED UP SOMEWHERE!");
    }
    public void MarkDead(int unitIndex) => units[unitIndex] = null;

    public int NumberOfOtherAgentsInSameActivity(SquadUnit agent)
    {
        int activity = agent.StateMachineController.GetInteger("RoutineStatus");
        return (activity < 0) ? 0 : numberOfUnitsInActivity[activity] - 1; // -1 to account for self
    }


    public static bool[] GetAgentBooleanObservation(SquadUnit unit)
    {
        if(unit == null)
        {
            bool[] deadState = new bool[SquadUnit.SENSOR_COUNT];
            deadState[0] = true;
            return deadState;
        }
        return unit.GetObservations();
    }

    public static float[] GetAgentFloatObservations( SquadUnit unit )
    {
        return (unit == null) ? new float[1] { 0 } : unit.GetFloatObservations();
    }

    public float[] GetActivityHistogram()
    {
        float[] histogram = new float[NUMER_OF_ACTIVITIES];
        for (int i = 0; i < NUMER_OF_ACTIVITIES; i++)
            histogram[i] = (float)numberOfUnitsInActivity[i] / units.Length; //NOTE/TODO: units count does not properly map to number units still alive
        return histogram;
    }

    public List<Vector3> RandomPointsInRoom()
    {
        List<Vector3> points = new List<Vector3>();
        UnityEngine.AI.NavMeshHit[] hits = new UnityEngine.AI.NavMeshHit[units.Length];
        SquadUnit unit;
        for (int i = 0; i < units.Length; i++)
        {
            unit = units[i];
            Vector3 samplePos = unit.transform.position + new Vector3(Random.value * 4, 0, Random.value * 4) ;
            if(UnityEngine.AI.NavMesh.SamplePosition(samplePos, out hits[i], 4, 0))
                points.Add(hits[i].position);
        }

        return points;
    }
    
    public void UpdateUnitStates()
    {
        _numberOfAliveUnits = 0;
        System.Array.Clear(numberOfUnitsInActivity, 0, numberOfUnitsInActivity.Length);
        foreach (var u in units)
        {
            if (u == null) return;
            _numberOfAliveUnits++;
            int activity = u.StateMachineController.GetInteger("RoutineStatus");
            UnitActivity[u] = activity;
            if (activity >= 0)
                numberOfUnitsInActivity[activity]++;
        }
    }

    public void UpdateCollectiveInformation()
    {
        squadSensor.UpdateCenterOfMassPosition(units);
        physicalObjectsWithinRange = squadSensor.GetPhysicalObjectsNearby(true);
        weaponsWithinRange = squadSensor.GetWeaponsNearby();
        foreach (var u in units)
        {
            if (u == null) continue;
            playerWithinSquadRange = squadSensor.IsPlayerWithinSquadRange(u.Sensor.player.transform.position);
            break;
        }
    }
}
