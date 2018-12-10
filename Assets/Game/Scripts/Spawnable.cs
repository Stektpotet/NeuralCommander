using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
#endif



[SelectionBase]// TODO: Remove this component from objects upon being placed during generation
// attribute collection for each spawnable, note: probability is in procent.
public class Spawnable : MonoBehaviour
{

    [Header("Probabilities")] //TODO make sure probabilities' sum is 1
    [Range(0,1)]
    public float byWallProbability;
    [Range(0,1)]
    public float notWallProbability;
    [Range(0,1)]
    public float spawnByDoorProbability;
    [Range(0,1)]
    public float spawnOnTop;

    [Space(20, order = 1)]
    // defines the desired front angle from the perpendicular angle of wall
    [Range(0,360, order = 2)]
    public int angleMaxOffset;
    public bool allowOtherOnTop;


    // public UnityEngine.Events.UnityEvent onSpawned;
    public RoomType.SpawnListItem[] relativeSpawnList;

#if UNITY_EDITOR
    [ContextMenuItem("Apply mesh bounds", "ApplyMeshBounds")]
#endif
    public Bounds neededSpawnSpace;
#if UNITY_EDITOR

    [ContextMenu("Test Collision")]
    private void TestCollision()
    {
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            Collider[] colliding = new Collider[1]; //need at least collision on 1

            //MeshCollider mCol = collider as MeshCollider;

            //Debug.DrawLine(transform.position + mCol.sharedMesh.bounds.center + mCol.transform.position, transform.position + mCol.transform.position + mCol.sharedMesh.bounds.extents, Color.red, 10);

            //Debug.Log($"positional data: {neededSpawnSpace.center}");
            Debug.Log($"bounds data: {neededSpawnSpace}, center: {neededSpawnSpace.center}, extents: {neededSpawnSpace.extents}, size: {neededSpawnSpace.size}");
            int collisions = Physics.OverlapBoxNonAlloc( neededSpawnSpace.center, neededSpawnSpace.extents * 0.5f, colliding);
            if (collisions == 0)
                Debug.Log("No Collision!");
            else
                Debug.Log($"{name} - {collider} intercepted with {colliding[0]}, {collisions} collisions!");
            //for (int i = 0; i < 10; i++)
            //{
            //    //Try re-locating
            //    Debug.Log($"{name} - {collider} intercepted with {colliding[0]}");
            //    //TODO - RESOLVE MECHANISMS using physics info
            //    //for now lets try to move it up;
            //    //spawnPosition += Vector3.up * colliding[0].bounds.size.y;

            //    collisions = Physics.OverlapBoxNonAlloc(transform.position + mCol.transform.position + mCol.sharedMesh.bounds.center, mCol.sharedMesh.bounds.extents * 0.5f, colliding);
            //    if (collisions == 0) { return true; }
        }
    }

    private void ApplyMeshBounds()
    {
        Debug.Log("Test");
        transform.position = Vector3.zero;
        neededSpawnSpace = new Bounds(transform.position,Vector3.zero);
        foreach (var mRend in GetComponentsInChildren<MeshRenderer>())
        {
            neededSpawnSpace.Encapsulate(mRend.bounds);
        }
    }


    Mesh m;
    Vector3 lastMin;
    private void OnDrawGizmosSelected()
    {
        var c = GetComponentsInChildren<Collider>();

        Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 max = new Vector3(Mathf.NegativeInfinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);

        // TODO: might break | building own min vector instead of finding the actual least min vector
        foreach (Collider collider in c)
        {
            if(min.x > collider.bounds.min.x)
            {
                min.x = collider.bounds.min.x;
            }
            if (min.z > collider.bounds.min.z)
            {
                min.z = collider.bounds.min.z;
            }
            if (min.y > collider.bounds.min.y)
            {
                min.y = collider.bounds.min.y;
            }
            if (max.x < collider.bounds.max.x)
            {
                max.x = collider.bounds.max.x;
            }
            if (max.z < collider.bounds.max.z)
            {
                max.z = collider.bounds.max.z;
            }
            if (max.y < collider.bounds.max.y)
            {
                max.y = collider.bounds.max.y;
            }
        }

        if (m == null || lastMin != min)
        {
            lastMin = min;
            Vector3[] vertices = new Vector3[8] {
            /*0*/    min,
            /*1*/    new Vector3(max.x, min.y, min.z),
            /*2*/    new Vector3(min.x, max.y, min.z),
            /*3*/    new Vector3(min.x, min.y, max.z),
            /*4*/    max,
            /*5*/    new Vector3(min.x, max.y, max.z),
            /*6*/    new Vector3(max.x, min.y, max.z),
            /*7*/    new Vector3(max.x, max.y, min.z)
            };

            neededSpawnSpace.center = gameObject.transform.position + new Vector3(0, (max.y- min.y) * 0.5f, 0);

            int[] triangles = new int[] {
                0,1,3,
                3,1,6,

                4,7,5,
                5,7,2,

                0,2,1,
                1,2,7,

                1,7,6,
                6,7,4,

                6,4,3,
                3,4,5,

                3,5,0,
                0,5,2
            };

            m = new Mesh();
            m.vertices = vertices;
            m.triangles = triangles;
            //UnityEditor.MeshUtility.Optimize(m);

            m.RecalculateNormals();
        }
        Gizmos.color = new Color32(127, 255, 127, 64);
        Gizmos.DrawMesh(m); //TODO fix
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(neededSpawnSpace.center, neededSpawnSpace.size);
        //Gizmos.DrawWireMesh(m);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(neededSpawnSpace.min, neededSpawnSpace.max);

    }
#endif
}
