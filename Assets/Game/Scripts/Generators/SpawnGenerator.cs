
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Math = System.Math;

// bounds (collision box that is generated) does not rotate when spawnable rotates


// note: this class does change collection (workingList) while iterating, therefore we use for and not foreach as that would give compile issues
public static class SpawnGenerator
{
    const int MAX_ITEM_STACK = 3; // TODO: evaluate where to put this variable

    // place all the required items within a room
    public static HashSet<GameObject> SpawnItemsInRoom(Random rand, RoomType roomType, RoomType.RoomSet roomSet, HashSet<Vector2Int> floorPos, HashSet<Vector2Int> wallPos, Vector2Int finalizeOffset = default(Vector2Int)) //Defaults to zero
    {
        HashSet<GameObject> spawnedObjects = new HashSet<GameObject>();
  
        // we copying the list as we are changing the data of roomtype otherwise
        List<RoomType.SpawnListItem> workingList = new List<RoomType.SpawnListItem>(roomType.spawnList);
        if(roomSet.extraSpawnList != null)
            workingList.AddRange(roomSet.extraSpawnList);

        HashSet<Vector2Int> onTopPos = new HashSet<Vector2Int>();
        HashSet<Vector2Int> workingFloor = new HashSet<Vector2Int>(floorPos);
        HashSet<Vector2Int> workingWall = new HashSet<Vector2Int>(wallPos);


        for (int i = 0; i < workingList.Count; i++)
        {
            float relativeSpawnAmount = (500 / (workingFloor.Count + workingWall.Count));
            int min = Mathf.RoundToInt(workingList[i].Min / relativeSpawnAmount);
            int max = Mathf.RoundToInt(workingList[i].Max / relativeSpawnAmount);

            Debug.Log("onTop:" + workingList[i].Spawnable.spawnOnTop);
            HashSet<Vector2Int> allowedSpawnPos = new HashSet<Vector2Int>(); // HashSet has optimal lookuptimes   O(1)            

            // TODO: validate that probability sum is one
            int totalPos = workingFloor.Count + wallPos.Count + onTopPos.Count;
            allowedSpawnPos.UnionWith(FindNewPos((int)Math.Round((totalPos * workingList[i].Spawnable.notWallProbability)), workingFloor, finalizeOffset, rand, 2));
            allowedSpawnPos.UnionWith(FindNewPos((int)Math.Round((totalPos * workingList[i].Spawnable.byWallProbability)), workingWall, finalizeOffset, rand, 2));
            allowedSpawnPos.UnionWith(FindNewPos((int)Math.Round((totalPos * workingList[i].Spawnable.spawnOnTop)), onTopPos, Vector2Int.zero, rand)); //NOTE CHANGE OFFSET TO 0

            // chose an amount to spawn a Spawnable type
            int spawnCount = rand.NextInclusive(min, max);


            for (int j = 0; j < spawnCount && allowedSpawnPos.Count > 0; j++)
            {
                // TODO: roll pos in tile
                Vector2Int tile = allowedSpawnPos.GetRandom(rand);
                Vector3 spawnPosition = new Vector3(tile.x, 0, tile.y);

                Quaternion objectQuaternion = Quaternion.Euler(0, rand.NextInclusive(0, workingList[i].Spawnable.angleMaxOffset), 0);
                Vector3? forward = null;

                // if we are about to spawn on the side of a object, we need a forward vector
                bool isOnTopPos = onTopPos.Contains(tile);

                if (!isOnTopPos)
                {
                    forward = objectQuaternion * new Vector3(0, 0, 1);
                }
              

                if (TryResolvePositionPhysically(workingList[i].Spawnable, ref spawnPosition, forward, objectQuaternion))
                {

                    spawnedObjects.Add(GameObject.Instantiate(workingList[i].Spawnable.gameObject, spawnPosition, objectQuaternion));

                    // TODO: rounding bugs!!!
                    Vector2Int tileToRemove = new Vector2Int((int)(tile.x * 0.5f), (int)(tile.y * 0.5f));
                    workingFloor.RemoveWhere(pos => pos == tileToRemove);
                    workingWall.RemoveWhere(pos => pos == tileToRemove);
                 
                    allowedSpawnPos.RemoveWhere(pos => pos == tile);
                        
                    if (!workingList[i].Spawnable.allowOtherOnTop && isOnTopPos)
                    {
                        onTopPos.Remove(tile);
                    }
                    else if (workingList[i].Spawnable.allowOtherOnTop)
                    {
                        onTopPos.Add(tile);

                        if (workingList[i].Spawnable.spawnOnTop > rand.NextFloat())
                        {
                            allowedSpawnPos.Add(tile);
                        }
                    }

                spawnCount -= SpawnRelatives(workingList[i].Spawnable, new Vector2(spawnPosition.x, spawnPosition.z), objectQuaternion, rand, ref spawnedObjects, ref workingList, min, max);
                    
            }

            }
  
        }

        // if we are not debugging, we delete scripts used for placement
        if (Application.isPlaying)
        {
            foreach (var spawnedObject in spawnedObjects)
            {
                UnityEngine.Object.Destroy(spawnedObject.GetComponent<Spawnable>());
            }
        }
       

        return spawnedObjects;
    }

    // find the distribution amount for this set in the allowed pool
    private static HashSet<Vector2Int> FindNewPos(int prefCount, HashSet<Vector2Int> posSet, Vector2Int offset, Random rand, int posModifier = 1)
    {
        HashSet<Vector2Int> allowedPosSet = new HashSet<Vector2Int>();

        for (int i = 0; i < prefCount && i < posSet.Count; i++)
        {
            Vector2Int newPos;
            do
            {
                Vector2 newPosFloat = posSet.GetRandom(rand) * posModifier + offset;
                
                newPos = new Vector2Int((int)newPosFloat.x, (int)newPosFloat.y);
            } while (allowedPosSet.Contains(newPos));
            allowedPosSet.Add(newPos);
        }

        return allowedPosSet;
    }

    // TODO: know issue with rotation
    private static bool TryResolvePositionPhysically(Spawnable spawnable, ref Vector3 spawnPosition, Vector3? forward, Quaternion rotation)
    {
        Collider[] colliding = new Collider[2]; //need at least collision on 1
        Debug.Log($"{spawnable.name}, {spawnable.neededSpawnSpace.extents}");
        
        Debug.DrawLine(spawnPosition + spawnable.neededSpawnSpace.min, spawnPosition + spawnable.neededSpawnSpace.max, Color.red, 10);

        //Debug.Log($"{ spawnPosition + mCol.transform.position + mCol.sharedMesh.bounds.center }");
        int collisions = Physics.OverlapBoxNonAlloc(spawnPosition + spawnable.neededSpawnSpace.center, spawnable.neededSpawnSpace.extents * 0.5f, colliding, rotation);
        if ( collisions == 0) { return true; }
        Vector3? right = Quaternion.Euler(0, 90, 0) * forward;

        if (forward == null) // if we are supposed to spawn on top of another spawnable
        {
            for (int i = 0; i < MAX_ITEM_STACK; i++)
            {
                spawnPosition += Vector3.up * colliding[0].bounds.size.y;
                Debug.Log($"{ spawnPosition + spawnable.neededSpawnSpace.center }");
                collisions = Physics.OverlapBoxNonAlloc(spawnPosition + spawnable.neededSpawnSpace.center, spawnable.neededSpawnSpace.extents * 0.5f, colliding, rotation);
                if (collisions == 0) { return true; }
            }

            return false;
        }
        else // if we are spawning on the side of the object
        {
            spawnPosition += new Vector3(forward.Value.x * colliding[0].bounds.size.x, 0, forward.Value.z * colliding[0].bounds.size.z);
            Debug.Log($"{ spawnPosition + spawnable.neededSpawnSpace.center }");
            collisions = Physics.OverlapBoxNonAlloc(spawnPosition + spawnable.neededSpawnSpace.center, spawnable.neededSpawnSpace.extents * 0.5f, colliding, rotation);
            if (collisions == 0) { return true; }
        }
        return false;
    }


    // This function is tasked with spawning items relative to already placed item
    // will return a list of spawned items so that SpawnItemsInRoom know what's left to spawn
    public static int SpawnRelatives(Spawnable spawned, Vector2 spawnedPos, Quaternion spawnedRot, Random rand, 
                                        ref HashSet<GameObject> spawnedObjects, ref List<RoomType.SpawnListItem> availableItems,
                                        int min, int max, int currentRecusion = 0)
    {
        int spawnedOfSameCount = 0;
        if (currentRecusion < 2)
        {
            int spawnedIndex = availableItems.FindIndex(spawnableItem => spawnableItem.Spawnable == spawned);

            for (int i = 0; i < spawned.relativeSpawnList.Length; i++)
            {
                int relationIndex = availableItems.FindIndex(spawnableItem => spawnableItem.Spawnable == spawned.relativeSpawnList[i].Spawnable);
                if (relationIndex != -1 && spawnedIndex <= relationIndex)
                {

                    int spawnCount = rand.NextInclusive(min, max);
                    int spawnedObjectsCount = 0;
                    
                    // if the rotation is set (not random)
                    if (spawned.relativeSpawnList[i].AngleIteration != 0)
                    {
                        for (int j = 0; j < spawnCount && Math.Abs(j * spawned.relativeSpawnList[i].AngleIteration) < 360; j++)
                        {
                            Quaternion objectQuaternion = Quaternion.Euler(0, j * spawned.relativeSpawnList[i].AngleIteration + spawnedRot.eulerAngles.y, 0);
                            Vector3 spawnPosition = new Vector3(spawnedPos.x, 0, spawnedPos.y);
                            Vector3 forward = objectQuaternion * new Vector3(0, 0, 1);
                            if (TryResolvePositionPhysically(spawned.relativeSpawnList[i].Spawnable, ref spawnPosition, -forward, objectQuaternion))
                            {
                                spawnedObjects.Add(GameObject.Instantiate(spawned.relativeSpawnList[i].Spawnable.gameObject, spawnPosition, objectQuaternion));
                                SpawnRelatives(spawned.relativeSpawnList[i].Spawnable, new Vector2Int((int)spawnPosition.x, (int)spawnPosition.z), objectQuaternion, rand, ref spawnedObjects, ref availableItems, min, max, ++currentRecusion);
                            }

                            spawnedObjectsCount = j;
                        }

                        int newMin = ((min - spawnedObjectsCount) > 0) ? min - spawnedObjectsCount : 0;
                        int newMax = ((max - spawnedObjectsCount) > 0) ? max - spawnedObjectsCount : 0;

                        RoomType.SpawnListItem updatedItem = new RoomType.SpawnListItem(availableItems[relationIndex].AngleIteration, new Vector2Int(newMin, newMax), availableItems[relationIndex].Spawnable);
                        availableItems[relationIndex] = updatedItem;

                        if (spawnedIndex == relationIndex) { spawnedOfSameCount++; }
                    }
                    else
                    {
                        for (int j = 0; j < spawnCount; j++)
                        {
                            Quaternion objectQuaternion = Quaternion.Euler(0, rand.NextInclusive(0, availableItems[relationIndex].Spawnable.angleMaxOffset), 0);
                            float xPos = spawnedPos.x + (rand.NextFloat() * (spawned.neededSpawnSpace.extents.x * 0.75f)) * (rand.NextBool() ? -1 : 1);
                            float zPos = spawnedPos.y + (rand.NextFloat() * (spawned.neededSpawnSpace.extents.z * 0.75f)) * (rand.NextBool() ? -1 : 1);
                            Vector3 spawnPosition = new Vector3(xPos, 0, zPos);

                            if (TryResolvePositionPhysically(spawned.relativeSpawnList[i].Spawnable, ref spawnPosition, null, objectQuaternion))
                            {
                                spawnedObjects.Add(GameObject.Instantiate(spawned.relativeSpawnList[i].Spawnable.gameObject, spawnPosition, objectQuaternion));
                                SpawnRelatives(spawned.relativeSpawnList[i].Spawnable, new Vector2Int((int)spawnPosition.x, (int)spawnPosition.z), objectQuaternion, rand, ref spawnedObjects, ref availableItems, min, max, ++currentRecusion);
                            }

                            spawnedObjectsCount = j;
                        }

                        int newMin = ((min - spawnedObjectsCount) > 0) ? min - spawnedObjectsCount : 0;
                        int newMax = ((max - spawnedObjectsCount) > 0) ? max - spawnedObjectsCount : 0;

                        RoomType.SpawnListItem updatedItem = new RoomType.SpawnListItem(availableItems[relationIndex].AngleIteration, new Vector2Int(newMin, newMax), availableItems[relationIndex].Spawnable);
                        availableItems[relationIndex] = updatedItem;

                        if (spawnedIndex == relationIndex) { spawnedOfSameCount++; }
                    }
                }
            }
        }
       
        return spawnedOfSameCount;
    }

}

