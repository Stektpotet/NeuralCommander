using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BootCampSetup : MonoBehaviour
{
    public int maxRoomSize = 30;
    public TrainingRoom[] levels;
    public RoomType[] roomTypes;
    

    public TrainingRoom SetupEnvironment(System.Random rand)
    {
        TrainingRoom level = Instantiate(levels.GetRandom(rand).gameObject, transform.parent).GetComponent<TrainingRoom>();
        SpawnSpawnables(level, rand);
        return level;
    }

    public void SpawnSpawnables(TrainingRoom room, System.Random rand)
    {
        var roomType = roomTypes.GetRandom(rand);
        HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>(room.unitSpaceFloors);
        HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>(room.unitSpaceWalls);

        Debug.Log("LEVEL SPAWNED AT: " + transform.position);

        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)) + room.Position;
        Debug.Log($"LEVEL SPAWN: {pos}");
        RoomType.RoomSet roomSet = roomType.roomSets.GetRandom(rand);
        var spawned = SpawnGenerator.SpawnItemsInRoom(rand, roomType, roomType.roomSets.GetRandom(rand), floorTiles, wallTiles, pos);
        foreach (var item in spawned)
        {
            item.transform.SetParent(room.transform);
        }
    }

}
