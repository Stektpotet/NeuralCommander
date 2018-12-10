using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class TrainingRoom : Room
{
    public Vector2Int[] unitSpaceFloors;
    public Vector2Int[] unitSpaceWalls;
    public Vector2Int Position => bounds.position;

#if UNITY_EDITOR
    
    [ContextMenu("Gather Room Bounds")]
    private void GatherRoomBounds()
    {
        int xMin = int.MaxValue, xMax = int.MinValue,
              yMin = int.MaxValue, yMax = int.MinValue;
        if (corners.Length == 1)
        {
            xMin = corners[0].x;
            xMax = corners[0].x;
            yMin = corners[0].y;
            yMax = corners[0].y;
        }
        else
        {
            foreach (var c in corners)
            {
                if (c.x < xMin) xMin = c.x;
                else if (c.x > xMax) xMax = c.x;
                if (c.y < yMin) yMin = c.y;
                else if (c.y > yMax) yMax = c.y;
            }
        }
        bounds = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
    }
    [ContextMenu("Gather Room Data")]
    private void GatherRoomData()
    {
        HashSet<Vector2Int> cornerTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
        HashSet<Vector2Int> wallTiles = new HashSet<Vector2Int>();

        for (int y = 0; y < 40; y++)
        {
            for (int x = 0; x < 40; x++)
            {
                if (
                  Physics.Raycast(
                        new Vector3(transform.position.x + x * 2, transform.position.y + 1, transform.position.z + y * 2),
                        Vector3.down, 2
                  ))
                {
                    Debug.DrawRay(new Vector3(transform.position.x + x * 2, transform.position.y + 1, transform.position.z + y * 2), Vector3.down * 2, Color.green, 4);
                    floorTiles.Add(new Vector2Int(x, y));
                }
            }
        }
        //Pick out walltiles
        foreach (var tile in floorTiles)
        {
            if (
                !floorTiles.Contains(tile + Vector2Int.up) ||
                !floorTiles.Contains(tile + Vector2Int.down) ||
                !floorTiles.Contains(tile + Vector2Int.right) ||
                !floorTiles.Contains(tile + Vector2Int.left)
              )
            {
                wallTiles.Add(tile);
            }

        }
        floorTiles.ExceptWith(wallTiles); //exclude the walltiles from floorTiles


        /*
         *    |
         *  --+--
         *    |
         */ 
         


        //Pick out corners
        foreach (var tile in wallTiles)
        {
            TileType cornerMask = 0;
            if (!wallTiles.Contains(tile + Vector2Int.up))      cornerMask |= TileType.WALL_NORTH;
            if (!wallTiles.Contains(tile + Vector2Int.down))    cornerMask |= TileType.WALL_SOUTH;
            if (!wallTiles.Contains(tile + Vector2Int.right))   cornerMask |= TileType.WALL_EAST;
            if (!wallTiles.Contains(tile + Vector2Int.left))    cornerMask |= TileType.WALL_WEST;

            if ((cornerMask & TileType.CORNER_NORTHEAST) == TileType.CORNER_NORTHEAST ||
                (cornerMask & TileType.CORNER_NORTHWEST) == TileType.CORNER_NORTHWEST ||
                (cornerMask & TileType.CORNER_SOUTHEAST) == TileType.CORNER_SOUTHEAST ||
                (cornerMask & TileType.CORNER_SOUTHWEST) == TileType.CORNER_SOUTHWEST)
            {
                cornerTiles.Add(tile);
            }
        }
        wallTiles.ExceptWith(cornerTiles);
        corners = cornerTiles.Select(t => t * 2).ToArray();
        unitSpaceFloors = floorTiles.ToArray();
        unitSpaceWalls = wallTiles.ToArray();
        GatherRoomBounds();
    }

    private void OnDrawGizmosSelected()
    {
        if (unitSpaceFloors != null)
        {
            Gizmos.color = Color.white;
            foreach (var tile in unitSpaceFloors)
                Gizmos.DrawCube(new Vector3(transform.position.x + tile.x*2, 0, transform.position.z + tile.y*2), Vector3.one*0.125f);
            Gizmos.color = Color.red;
            foreach (var tile in unitSpaceWalls)
                Gizmos.DrawCube(new Vector3(transform.position.x + tile.x * 2, 0, transform.position.z + tile.y * 2), Vector3.one*0.5f);
            Gizmos.color = Color.green;
            foreach (var tile in corners)
                Gizmos.DrawCube(new Vector3(transform.position.x + tile.x, 0, transform.position.z + tile.y), Vector3.one);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(new Vector3(transform.position.x + bounds.center.x, 0, transform.position.z + bounds.center.y), new Vector3(bounds.size.x, 0.25f, bounds.size.y));
        }
    }

#endif
}
