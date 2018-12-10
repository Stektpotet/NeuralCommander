using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SquadGenerator {

    //TODO add support for multiple
    public static void CreateNewSquad(GameObject squad, GameObject unitPrefab, HashSet<Vector2Int> tiles, 
        Vector2Int minMax, System.Random random, MLAgents.Brain Brain, Room room, Vector2Int offset, GameObject player)
    {
        if (unitPrefab == null)
        {
            Debug.LogError("create squad need unit prefab");
            return;
        }
        CommanderAgent prefabCommander = squad.GetComponent<CommanderAgent>();
        if (prefabCommander == null)
        {
            Debug.LogError("create squad need squad prefab");
            return;
        }
        if (prefabCommander.squad.units.Length > 0)
        {
            Debug.LogError("squad prefab needs to contain no units");
            return;
        }
        if (player == null)
        {
            Debug.LogError("Squad needs to be initialized with player reference");
            return;
        }

        GameObject newSquadObject           = Object.Instantiate(squad, Vector3.zero, Quaternion.identity);
        CommanderAgent commanderAgent       = newSquadObject.GetComponent<CommanderAgent>();
        commanderAgent.squad.currentRoom    = room;        
        commanderAgent.GiveBrain(Brain);
        commanderAgent.squadColor = Random.ColorHSV(0, 1, 0.25f, 1f);

        int squadSize       = random.NextInclusive(minMax.x, minMax.y);
        int tilesPerUnit    = 6;
        int maxUnits        = tiles.Count / tilesPerUnit;

        if (squadSize > maxUnits)                       squadSize = maxUnits;
        if (squadSize > CommanderAgent.MAX_SQUAD_SIZE)  squadSize = CommanderAgent.MAX_SQUAD_SIZE;

        

        for (int i = 0; i < squadSize; i++)
        {
            Vector2 tile = tiles.GetRandom(random);
            Vector3 position = new Vector3((tile.x * 2) + offset.x, 1.055f, (tile.y * 2) + offset.y);
            var unit = Object.Instantiate(unitPrefab, position, Quaternion.identity, newSquadObject.transform);
            SquadUnit newUnit = unit.GetComponent<SquadUnit>();
            var unitSensor = newUnit.GetComponent<UnitSensor>();
            unitSensor.player = player;
            unitSensor.headColliderTransform = player.transform;
            SetMaterialColor(newUnit, commanderAgent.squadColor);
            commanderAgent.squad.AddUnit(newUnit);
        }

        return;
    }

    public static void AddUnitToSquad(CommanderAgent commander, GameObject[] unitPrefabs,
        HashSet<Vector2Int> tiles, Room room, Vector2Int offset, System.Random random, GameObject player)
    {
        int unitsAlive = 0;
        foreach(var unit in commander.squad.units)
        { if(unit!= null) unitsAlive++; }


        if (unitsAlive == CommanderAgent.MAX_SQUAD_SIZE) //NOTE: Not using NumberOfAliveUnits because it might not be correct at the point this is called
        {
            Debug.LogError("Trying to add more units than allowed to a squad");
            return;
        }
        Vector2 tile = tiles.GetRandom(random);
        Vector3 position = new Vector3((tile.x * 2) + offset.x, 1.055f, (tile.y * 2) + offset.y);
        var newUnit = Object.Instantiate(unitPrefabs.GetRandom(random), position, Quaternion.identity, commander.transform).GetComponent<SquadUnit>();
        var unitSensor = newUnit.GetComponent<UnitSensor>();
        unitSensor.player = player;
        unitSensor.headColliderTransform = player.transform;
        SetMaterialColor(newUnit, commander.squadColor);
        commander.squad.AddUnit(newUnit);
    }

    public static void AttachNewSquadTo(CommanderAgent commander, GameObject[] unitPrefabs,
        HashSet<Vector2Int> tiles, Vector2Int minMax, System.Random random, Room room, Vector2Int offset, GameObject player)
    {

        if (unitPrefabs == null)
        {
            Debug.LogError("create squad need unit prefab");
            return;
        }

        //Cleanup of previous squad============================
        foreach (var unit in commander.squad.units)
            if(unit != null)
            {
                commander.squad.MarkDead(unit.squadUnitIndex);
                Object.Destroy(unit.gameObject);
            }
        //commander.squad.units.Clear(); -> after array conversion we leave garbage here...
        //=====================================================

        commander.squad = new Squad()
        {
            currentRoom = room,
            squadSensor = commander.GetComponent<SquadSensor>(),
        };
        
        commander.squadColor = Random.ColorHSV(0, 1, 0.25f, 1f);

        int squadSize = random.NextInclusive(minMax.x, minMax.y);
        int tilesPerUnit = 6;
        int maxUnits = tiles.Count / tilesPerUnit;

        if (squadSize > maxUnits) squadSize = maxUnits;
        if (squadSize > CommanderAgent.MAX_SQUAD_SIZE) squadSize = CommanderAgent.MAX_SQUAD_SIZE;

        for (int i = 0; i < squadSize; i++)
        {
            Vector2 tile = tiles.GetRandom(random);
            Vector3 position = new Vector3((tile.x * 2) + offset.x, 1.055f, (tile.y * 2) + offset.y);
            var newUnit = Object.Instantiate(unitPrefabs.GetRandom(random), position, Quaternion.identity, commander.transform).GetComponent<SquadUnit>();
            var unitSensor = newUnit.GetComponent<UnitSensor>();
            unitSensor.player = player;
            unitSensor.headColliderTransform = player.transform;
            SetMaterialColor(newUnit, commander.squadColor);
            commander.squad.AddUnit(newUnit);
        }
        return;
    }

    private static void SetMaterialColor(SquadUnit unit, Color c)
    {
        foreach(var r in unit.GetComponentsInChildren<MeshRenderer>())
        { r.materials[0].SetColor("_Color", c); }
    }

}
