using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This class is mainly for collecting collective information within a squad
/// </summary>
public class SquadSensor : MonoBehaviour {

    const int PHYSICAL_OBJECT_LAYERMASK = 512;
    const int WEAPON_OBJECT_LAYERMASK = 2048;
    const int PLAYER_LAYERMASK = 4194304;

    private Vector3 _centerOfMassPosition;
    public Vector3 CenterOfMassPosition => _centerOfMassPosition;
    [SerializeField] private float squadRadius;

    // Could possibly be a void that just updates the position the squad holds
    public void UpdateCenterOfMassPosition(SquadUnit[] members)
    {
        Vector3 centerPosition = new Vector3(0, 0, 0);
        int memberCount = 0;
        foreach (SquadUnit member in members)
        {
            if(member != null)
            {
                memberCount += 1;
                centerPosition += member.transform.position;
            }
        }
        centerPosition /= memberCount;

        _centerOfMassPosition = centerPosition;
    }

    public Collider[] GetObjectsWithinSquadRadius(LayerMask layerMask)
    {
        return Physics.OverlapSphere(CenterOfMassPosition, squadRadius, layerMask);
    }
    public Collider[] GetObjectsWithinSquadRadiusSorted( LayerMask layerMask )
    {
        return Physics.OverlapSphere(CenterOfMassPosition, squadRadius, layerMask).OrderByDescending(c => (CenterOfMassPosition - c.transform.position).sqrMagnitude).ToArray();
    }

    public List<PhysicalObject> GetPhysicalObjectsNearby( bool sorted = false )
    {
        List<PhysicalObject> physicalObjects = new List<PhysicalObject>();
        if (sorted)
        {
            foreach (Collider physObj in GetObjectsWithinSquadRadiusSorted(PHYSICAL_OBJECT_LAYERMASK))
            {
                physicalObjects.Add(physObj.GetComponent<PhysicalObject>());
            }
        }
        else
        {
            foreach (Collider physObj in GetObjectsWithinSquadRadius(PHYSICAL_OBJECT_LAYERMASK))
            {
                physicalObjects.Add(physObj.GetComponent<PhysicalObject>());
            }
        }
        return physicalObjects;
    }

    public List<WeaponPhysicalObject> GetWeaponsNearby( bool sorted = false )
    {
        List<WeaponPhysicalObject> weapons = new List<WeaponPhysicalObject>();
        if (sorted)
        {
            foreach (Collider weapon in GetObjectsWithinSquadRadiusSorted(WEAPON_OBJECT_LAYERMASK))
            {
                weapons.Add(weapon.GetComponent<WeaponPhysicalObject>());
            }
        }
        else
        {
            foreach (Collider weapon in GetObjectsWithinSquadRadius(WEAPON_OBJECT_LAYERMASK))
            {
                weapons.Add(weapon.GetComponent<WeaponPhysicalObject>());
            }
        }
        return weapons;
    }

    public bool IsPlayerWithinSquadRange(Vector3 playerPos)
    {
        float playerCOMRelation = (CenterOfMassPosition - playerPos).sqrMagnitude;
        return squadRadius*squadRadius > playerCOMRelation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(CenterOfMassPosition, squadRadius);
    }
}
