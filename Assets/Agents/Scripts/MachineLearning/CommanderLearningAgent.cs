using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class CommanderLearningAgent : CommanderAgent
{
   
    BootCampSetup setup;

    [Header("Data for learning")]
    public GameObject[] unitPrefabs; //NOTE can be changed to [] if we want more than 1 type of unit in squad
    public GameObject simulatedPlayerPrefab;

    private GameObject instantiatedPlayer;
    
    [Header("Episodes Before:")]
    [Space(10.0f)]
    [LabelAs("Full Reset")]
    public int episodesForNewRoom = 50;
    [LabelAs("New Spawnables")]
    public int episodesForNewSpawnables = 20;
    [LabelAs("New Squad")]
    public int episodesForNewSquad = 3; //I.e. if the episode is completed without all units dying, keep going... - this might teach them to value their health

#if UNITY_EDITOR
    private void OnValidate()
    {
        episodesForNewRoom          = Mathf.Max(1, episodesForNewRoom);
        episodesForNewSpawnables    = Mathf.Max(1, episodesForNewSpawnables);
        episodesForNewSquad         = Mathf.Max(1, episodesForNewSquad);
    }
#endif

    int episodeCounter = 0;

    System.Random random;
    TrainingRoom trainingRoom;
    UnityEngine.AI.NavMeshDataInstance navMeshHandle;

    [ContextMenu("Test NewRoom()")]
    private void NewRoom()
    {
#if UNITY_EDITOR
        random = new System.Random(Time.realtimeSinceStartup.GetHashCode());
        if (trainingRoom != null)
        {
            DestroyImmediate(trainingRoom.gameObject);
#else
            Destroy(trainingRoom.gameObject);
#endif
        }
        if (setup == null) setup = GetComponent<BootCampSetup>();
        trainingRoom = setup.SetupEnvironment(random);

        var navMeshSurface = transform.parent.GetComponent<UnityEngine.AI.NavMeshSurface>();
        navMeshSurface.collectObjects = UnityEngine.AI.CollectObjects.Children;
        navMeshSurface.BuildNavMesh();
        if (navMeshHandle.valid)
            UnityEngine.AI.NavMesh.RemoveNavMeshData(navMeshHandle);
        navMeshHandle = UnityEngine.AI.NavMesh.AddNavMeshData(navMeshSurface.navMeshData);
        
        NewSimulatedPlayer();
        NewSquad();
    }
    [ContextMenu("Test NewSpawnables()")]
    private void NewSpawnables()
    {
        if (trainingRoom != null)
        {
            foreach (Transform child in trainingRoom.transform)
            {
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
        setup.SpawnSpawnables(trainingRoom, random);
        
    }
    private void NewSquad()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>(trainingRoom.unitSpaceFloors);
        tiles.UnionWith(trainingRoom.unitSpaceWalls);
        Vector2Int offset = new Vector2Int(Mathf.RoundToInt(transform.position.x) + trainingRoom.Position.x, Mathf.RoundToInt(transform.position.z) + trainingRoom.Position.y);
        squad.currentRoom = trainingRoom;
        SquadGenerator.AttachNewSquadTo(this, unitPrefabs, tiles, unitsInSquad, random, trainingRoom, offset, instantiatedPlayer);
    }

    [ContextMenu("Test NewSquadUnits()")]
    private void NewSquadUnits()
    {
        //NOTE: it might be useful to just fill in empty positions of the squad instead.
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>(trainingRoom.unitSpaceFloors);
        tiles.UnionWith(trainingRoom.unitSpaceWalls);
        Vector2Int offset = new Vector2Int(Mathf.RoundToInt(transform.position.x) + trainingRoom.Position.x, Mathf.RoundToInt(transform.position.z) + trainingRoom.Position.y);
        squad.currentRoom = trainingRoom;
        for (int i = 0; i < MAX_SQUAD_SIZE - squad.NumberOfAliveUnits; i++)
        {
            SquadGenerator.AddUnitToSquad(this, unitPrefabs, tiles, trainingRoom, offset, random, instantiatedPlayer);
        }
    }

    // This could take in an argument to change the simulated player behaviour
    private void NewSimulatedPlayer()
    {
        if(instantiatedPlayer != null)
        {
            Destroy(instantiatedPlayer);
        }
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>(trainingRoom.unitSpaceFloors);
        tiles.UnionWith(trainingRoom.unitSpaceWalls);

        Vector2 tile = tiles.GetRandom(random);
        Vector3 position = new Vector3((tile.x * 2) + transform.position.x + trainingRoom.Position.x, 2.055f, tile.y * 2 + transform.position.z + trainingRoom.Position.y);


        instantiatedPlayer = Instantiate(simulatedPlayerPrefab, position, Quaternion.identity, transform.parent);
        SimulatePlayerActions playerSim = instantiatedPlayer.GetComponent<SimulatePlayerActions>();
        playerSim.teleportTargets = new Vector3[8];
        playerSim.teleportTargets[0] = position;
        for (int i = 1; i < 8; i++)
        {
            tile = tiles.GetRandom(random);
            playerSim.teleportTargets[i] = new Vector3((tile.x * 2) + transform.position.x + trainingRoom.Position.x, 2.055f, tile.y * 2 + transform.position.z + trainingRoom.Position.y);
        }
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        setup = GetComponent<BootCampSetup>();
        random = new System.Random(Time.realtimeSinceStartup.GetHashCode());
    }
    public override void AgentReset()
    {
        lastDescision.Clear();
        WakeUpSquad();
        //FROM heavy-load resetting to light-load resetting
        if ((episodeCounter %  episodesForNewRoom) == 0)
        {
            NewRoom();
            episodeCounter = 1;
        }
        else if ((episodeCounter % episodesForNewSpawnables) == 0)
        {
            NewSpawnables();
            episodeCounter++;
        }
        else if ((episodeCounter % episodesForNewSquad) == 0)
        {
            NewSquadUnits();
            episodeCounter++;
        }
        episodeCounter++;

        if (instantiatedPlayer == null)
            NewSimulatedPlayer();
    }
    
}
