using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnitSensor : MonoBehaviour {


    //TODO distinguish internal sensors and external. i.e. which ones are used for informing the squad commander, and which ones are used for state machine control
    public bool IsTakingDmg => 
        (Time.realtimeSinceStartup - tookDmgTimer < takingDmgTime);

    public bool IsInCover =>
        CheckIfInCover();
    
    public bool IsGrounded => 
        Physics.Raycast(transform.position, Vector3.down, relativeHeight.y * 1.1f, ~agentLayer);   

    public bool IsCrouching => 
        transform.position.y - foot.position.y < 0.85;
    
    public bool IsWounded =>
        selfObject.Durability < woundedThreshold;

    public bool IsArmed =>
        CheckIfArmed();

    public bool IsLoaded =>
        ((weapon?.loadedMagazine != null) ? weapon.loadedMagazine.IsNotEmpty() : false);

    public bool IsSeeingPlayer => CheckForPlayer();

    public bool IsHearingPlayer =>
        (Time.realtimeSinceStartup - heardPlayerTimer < heardPlayerTimeThreshold);

    public bool KnowsPlayerPosition =>
        (Time.realtimeSinceStartup - knowsPlayerPositionTimer < knowsPlayerPositionTimeThreshold);

    //public bool knowsPlayerPositon;

    // TODO: If this is supposed to be sent to the brain it should maybe be an average over a period of time?
    public bool IsNotMoving =>
        rb.velocity.sqrMagnitude < 0.01f; //TODO for use as observation for the brain this is way too sensitive, we should have a timed property instead

    private bool _objectiveCompleted;
    public bool IsObjectiveCompleted => _objectiveCompleted;
    public void ResetObjective() => _objectiveCompleted = false;
    public void CompleteObjective() => _objectiveCompleted = true;

    public float Health => selfObject.Durability/startDurability;


    [SerializeField] private float woundedThreshold;
    
    public PhysicalObject selfObject;
    public Transform rightHand;
    public PhysicalObject rightStartWeapon;
    public Transform rightWeaponPos;
    public Transform leftHand;
    public PhysicalObject leftStartWeapon;
    public Transform leftWeaponPos;
    public Transform foot;
    private PhysicalObject rightObject;
    private PhysicalObject leftObject;
    public GameObject player;
    public Transform headColliderTransform;
    public float visionRange;
    public float fieldOfView;
    public float takingDmgTime;
    public float heardPlayerTimeThreshold;
    public float knowsPlayerPositionTimeThreshold = 5;
    [Tooltip("Hearing ability of the agent sensor")]
    [SerializeField] private float hearingThreshold;
    [Tooltip("Should be negative value, for sound falloff when further away from player")]
    [SerializeField] private float soundFalloff;

    private Vector3 prevStuckCheckPos;
    private float lastStuckPosSaveTime;
    private const string TAG_PLAYER = "Player";
    private float _originalHeight;
    public float OriginalHeight => _originalHeight;
    
    private WeaponPhysicalObject weapon;

    Rigidbody rb;
    CapsuleCollider col;
    LayerMask notCover;
    LayerMask agentLayer;
    const int LAYER_VISION = 6292279; // everything | ~weapon | ~magazine | ~bullet
    static Vector3 headOffset;

    const float STATE_UPDATE_INTERVAL = 0.2f;
    float lastUpdateCounter;
    private float tookDmgTimer;
    private float heardPlayerTimer;
    private float knowsPlayerPositionTimer;

    private Vector3 relativeHeight;

    private float startDurability=100;

    private void Start()
    {
        startDurability = selfObject.Durability;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        if(player == null)
        {
            Debug.LogError("Agents should be initialized with a reference to the player!");
            player = GameObject.Find("Player");
        }

        if (headOffset == Vector3.zero)
            headOffset = new Vector3(0, col.bounds.extents.y * 1.5f, 0);

        agentLayer = LayerMask.GetMask("Agent");
        notCover = ~LayerMask.GetMask("Player") | ~LayerMask.GetMask("WeaponPickupAble") |
                        ~LayerMask.GetMask("MagazinePickupAble") | ~agentLayer;

        relativeHeight = new Vector3(0, transform.position.y, 0);
        if (relativeHeight.y > 1.6)
        {
            Debug.Log("make sure to spawn agent right above ground");
        }

        if (foot != null)
            _originalHeight = foot.localPosition.y;
        else
            Debug.LogError("foot is null in sensor");
    }

    bool CheckIfArmed()
    {
        if(weapon == null)
        {
            weapon = rightHand.GetComponentInChildren<WeaponPhysicalObject>();
            if (weapon == null)
            {
                weapon = leftHand.GetComponentInChildren<WeaponPhysicalObject>();
            }
        }

        return weapon != null;
    }

    bool CheckIfInCover()
    {
        Vector3 normRelation = (player.transform.position - transform.position).normalized;

        int hitCount = 0;
        for (int i = 0; i < 2; i++)
        {
            for(int j = 1; j <= 3; j++)
            {
                float heigt = col.height * 0.5f;
                Vector3 offset = new Vector3(col.radius - col.radius*2*i, heigt - heigt * 0.4f * j, 0f);
                if(Physics.Raycast(transform.position + offset, normRelation, 1f, notCover))
                {
                    hitCount += 1;
                }
            }
        }

        return hitCount >= 4;
    }


    bool CheckForPlayer()
    {
        Vector3 relation = headColliderTransform.transform.position - transform.position - headOffset; 
        if(relation.magnitude > visionRange)
            return false;

        float angle = Vector3.Angle(relation.normalized, transform.forward);
        if (angle > fieldOfView * 0.5f)
            return false;

        RaycastHit hit;
        if(!Physics.SphereCast(transform.position + headOffset,  0.25f, relation.normalized, out hit, visionRange, LAYER_VISION))
            return false;
        
        if(hit.transform.gameObject.tag != TAG_PLAYER)
            return false;

        knowsPlayerPositionTimer = Time.realtimeSinceStartup;
        return true;
    }

    public void TookDmg()
    {
        tookDmgTimer = Time.realtimeSinceStartup;
        knowsPlayerPositionTimer = tookDmgTimer;
    }

    // Function for "faking" hearing of the player or general noise to confuse agents
    // could be called as broadcast message from player when noise is made.
    public void ReceiveSound(Vector3 position, float loudNess)
    {
        //TODO: Debug and test this
        float sqDistance = (position - transform.position).sqrMagnitude;
        float perceivedLoudness = Mathf.Pow(sqDistance, soundFalloff);

        if(perceivedLoudness > hearingThreshold)
        {
            heardPlayerTimer = Time.realtimeSinceStartup;
        }
    }

    public Vector3 GetVelocity() => rb.velocity;


    public PhysicalObject GetItemInHand(Transform hand)
    {
        if(hand != leftHand && hand != rightHand)
        {
            Debug.LogError("using invalid hand in GetItemInHand");
        }

        return hand.GetComponentInChildren<PhysicalObject>();
    }

}
