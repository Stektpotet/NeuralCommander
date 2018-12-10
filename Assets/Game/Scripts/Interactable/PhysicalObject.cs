using UnityEngine;
using UnityEngine.Events;

public enum PhysicalObjectType
{
    Furniture,
    Weapon,
    Magazine,
    Decor,
    Agent,
    Player,
    Misc
}

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PhysicalObject : MonoBehaviour
{

    public Rigidbody body; // public becuase of playerstorage TODO: fix

    [SerializeField] private bool indestrucable;
    [SerializeField] private float durability;      //How much this object can take before being broken, -1 is unbreakable;
    public float Durability => durability;
    [SerializeField] private float density;         //How dense an object is determines how much damage it applies to other objects
    [SerializeField] private float dmgThreshold;

    protected Vector3 velocity;
    private Vector3 torque;

    public bool isPickedUp;
    public PhysicalObjectType objectType;

    [System.Serializable] public class ColliderEvent : UnityEvent<Collider> { };
    [System.Serializable] public class HandEvent : UnityEvent<Transform> { };

    public UnityEvent
        onDestroy,
        onHolding;
    public ColliderEvent
        onHit,
        onDurabilityLoss;
    public HandEvent
        onPickup,
        onDrop,
        onPlayerDrop, // for getting dropped item transform
        onActivate;

    private void Start() => body = GetComponent<Rigidbody>();

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (isPickedUp && (collision.gameObject.tag == "Hand"))
            return; //ignore this collision

        if (!indestrucable)
        {
            float otherDensity = collision.gameObject.GetComponent<PhysicalObject>()?.density ?? 0.1f;
            if (otherDensity <= 0) return;
            float potentialDamage = otherDensity * collision.relativeVelocity.magnitude;
            if (potentialDamage > dmgThreshold)
            {
                durability -= potentialDamage;
                onDurabilityLoss.Invoke(collision.collider);
            }
                
            if (durability <= 0) Destroy(gameObject);
           
        }
        onHit.Invoke(collision.collider);
    }

    /// <summary>
    /// Called when picking up this item
    /// </summary>
    public virtual void OnPickUp(Transform pickedUpByHand)
    {
        //internal stuff
        isPickedUp = true;
        //NOTE: REMEMBER TO USE RIGIDBODY.MovePosition instead of changing transform.position to avoid weird physics, or lack thereof
        body.isKinematic = true;
        transform.parent = pickedUpByHand.transform;

        //external stuff
        onPickup.Invoke(pickedUpByHand);
    }

    /// <summary>
    /// Called every fixedupdate frame when holding this
    /// </summary>
    public virtual void OnHolding(Transform heldByHand)
    {
        //internal

        //maybe calculate velocity or not?;

        body.velocity = new Vector3(0, 0, 0);

        //external stuff
        onHolding.Invoke();
    }

    public virtual void OnDrop(Transform droppedByHand, Vector3 velocity)
    {
        //internal stuff
        isPickedUp = false;

        //NOTE: REMEMBER TO USE RIGIDBODY.MovePosition instead of changing transform.position to avoid weird physics, or lack thereof
        body.isKinematic = false;
        transform.parent = null;

        body.velocity = velocity;   //droppedByHand.GetTrackedObjectVelocity();

        //external stuff
        onDrop.Invoke(droppedByHand);
    }

    public virtual void OnPlayerDrop(Transform droppedItem) // TODO: investigate if we need hand that did drop at all
    {
        //external stuff
        onPlayerDrop.Invoke(droppedItem);
    }

    public void OnEnable()
    {
        if (body == null)
            body = GetComponent<Rigidbody>();
    }

    private void OnDestroy() => onDestroy.Invoke();

    public virtual void OnActivate(Transform heldByHand) => onActivate.Invoke(heldByHand);
}
