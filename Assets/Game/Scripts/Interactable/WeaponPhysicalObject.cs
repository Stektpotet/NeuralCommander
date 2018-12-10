using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Maybe extend the reload mechanic to require some sort of bolting
// TODO: Gun should use LookAt on a gameobject or virtual point as of now the holding location and trigger location must align to work
// TODO: Laser guns(raycast)
// TODO: haptic feedback

public class WeaponPhysicalObject : PhysicalObject {

    public Vector3 triggerHeldPositionOffset;
    public Vector3 triggerHeldRotationOffset;

    public Collider triggerArea;
    public Collider offhandArea;

    public Vector3 barrelEndOffset;
    public Vector3 magazineLocationOffset;
    public Vector3 magazineLocationSize;
    public float firePower;
    public LayerMask magazineLayer;

    [Tooltip("useful for making new guns")]
    public bool showTheseGizmos;

    [SerializeField] private string magazineKey;
    public string MagazineKey => magazineKey;

    [Tooltip("firerate = 1 == 1 bullet pr sec & firerate = 0.25 == 4 bullets pr sec")]
    public float fireRate; // example: firerate = 1 means 1 bullet pr sec

    public MagazineObject loadedMagazine;
    private GameObject magazineTransform;
    private GameObject bulletTransform;
    private float fireCooldown;

    private GameObject triggerHand;
    private GameObject offhand;

    private const string PlayerString = "Player";

    // make serialized


    private void Start()
    {
        body = GetComponent<Rigidbody>();
        magazineTransform = new GameObject();
        magazineTransform.name = "magazineTransformObject";
        magazineTransform.transform.parent = transform;
        bulletTransform = new GameObject();

        if (triggerArea == null)
        {
            Debug.LogError("weapon missing trigger collider");
            return;
        }
    }


    public void FixedUpdate()
    {
        fireCooldown -= Time.fixedDeltaTime;
        if (fireCooldown < 0)
            fireCooldown = 0;

        Transform magTransform = magazineTransform.transform;
        magTransform.rotation = transform.rotation;
        magTransform.position = transform.position + magazineLocationOffset.z * transform.forward + magazineLocationOffset.y * transform.up + magazineLocationOffset.x * transform.right;

        // magazine logics
        if (loadedMagazine != null)
        {
            loadedMagazine.OnHolding(magTransform);
        }
        else
        {
            Vector3 halfSize = magazineLocationSize * 0.5f;
            Collider[] colliders = Physics.OverlapBox(magTransform.position, halfSize, Quaternion.Euler(transform.forward), magazineLayer);

            foreach (Collider collider in colliders)
            {
                MagazineObject magazine = collider.GetComponent<MagazineObject>();

                // if we find legal magazine that isn't already picked up
                if (magazine != null && magazine.MagazineKey == magazineKey && !magazine.isPickedUp)
                {
                    // TODO: require a relative velocity for the gun to reload & play a sound on reload
                    loadedMagazine = magazine;
                    loadedMagazine.OnLoaded(magTransform, this);
                }

            }
        }

        // two hand logic
        if (triggerHand == null)
        {
            offhand = null;
        }

        if (offhandArea != null && offhand != null)
        {
            Vector3 handsVector = offhand.transform.position - triggerHand.transform.position;
            //transform.rotation = Quaternion.LookRotation(handsVector, transform.up);
            Quaternion newRotation = Quaternion.LookRotation(handsVector, transform.up);
            Vector3 euler = transform.rotation.eulerAngles;
            float zRotaion = triggerHand.transform.rotation.eulerAngles.z - euler.z;
            transform.rotation = newRotation * Quaternion.AngleAxis(zRotaion, Vector3.forward);
            if (!offhandArea.bounds.Contains(offhand.transform.position))
            {
                offhand = null;
            }
        }
        else if(triggerHand != null)
        { //TODO loook into optimizing?
            if (triggerHand.transform.tag != PlayerString)
                transform.rotation = triggerHand.transform.rotation;
            else
                transform.rotation = triggerHand.transform.rotation * Quaternion.Euler(triggerHeldRotationOffset);
        }



    }

    // used for setting up weapon
    public void OnDrawGizmos()
    {
        if (showTheseGizmos)
        {
            // shoot vector
            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawRay(transform.position + barrelEndOffset.z * transform.forward + barrelEndOffset.y * transform.up + barrelEndOffset.x * transform.right, transform.forward);

            // magazine location
            Gizmos.color = new Color(1, 0, 0, 0.8f);
            Gizmos.DrawCube(transform.position + magazineLocationOffset.z * transform.forward + magazineLocationOffset.y * transform.up + magazineLocationOffset.x * transform.right, magazineLocationSize);
        }
    }

    /// <summary>
    /// Called when picking up this item
    /// </summary>
    public void OnStabilizing(Transform pickedUpByHand)
    {
        if (offhand == null)
            offhand = pickedUpByHand.gameObject;
        else
            offhand = null;
    }


    /// <summary>
    /// Called when picking up this item
    /// </summary>
    public override void OnPickUp(Transform pickedUpByHand)
    {
        //internal stuff

        body.isKinematic = true;

        if (triggerArea.bounds.Contains(pickedUpByHand.position) || pickedUpByHand.tag != "Player")
        {
            if (pickedUpByHand.tag != "Player")
                transform.rotation = pickedUpByHand.rotation;
            else
                transform.rotation = pickedUpByHand.rotation * Quaternion.Euler(triggerHeldRotationOffset);

            transform.position  = pickedUpByHand.position + body.transform.TransformDirection(triggerHeldPositionOffset);
            transform.parent    = pickedUpByHand.transform;
            triggerHand         = pickedUpByHand.gameObject;
        }
        else if (offhandArea != null && offhandArea.bounds.Contains(pickedUpByHand.position))
        {
            Debug.LogError("error in OnPickUP weapon");
        }
        else if(triggerHand == null)
        {
            transform.parent = pickedUpByHand.transform;
        }

        //external stuff
        onPickup.Invoke(pickedUpByHand);
    }

    public override void OnDrop(Transform droppedByHand, Vector3 velocity)
    {
        if(offhand?.transform == droppedByHand)
        {
            offhand = null;
            return;
        }
     
        triggerHand = null;
        offhand = null;
        base.OnDrop(droppedByHand, velocity);
        return; 
          
    }

    /// <summary>
    /// Called every fixedupdate frame when holding this
    /// </summary>
    public override void OnHolding(Transform heldByHand)
    {
        //internal
       

        //external stuff
        onHolding.Invoke();
    }

    /// <summary>
    /// Called every fixedupdate frame when holding this
    /// </summary>
    public override void OnActivate(Transform heldByHand)
    {
        //internal

        // Order of the ifs matter
        if (triggerHand == null)
            return;

        if (heldByHand != triggerHand.transform)
            return;

        if (loadedMagazine == null)
            return;

        if (fireCooldown > 0)
            return;

        Bullet bulletToFire = loadedMagazine.Fire();
        if (bulletToFire == null)
        {
            // TODO: play sound if no bullet left
            return;
        }
            

        fireCooldown = fireRate;
        Transform bltTransform = bulletTransform.transform;
        bltTransform.position = transform.position + barrelEndOffset.z * transform.forward + barrelEndOffset.y * transform.up + barrelEndOffset.x * transform.right;
        bulletToFire.transform.position = bltTransform.position;
        bulletToFire.transform.rotation = transform.rotation;
        bulletToFire.OnDrop(bltTransform, firePower * transform.forward);

        //external stuff
        onActivate.Invoke(heldByHand);
    }

    public bool IsTriggerColliding(Vector3 pos)
    {
        return triggerArea.bounds.Contains(pos);
    }

    public bool IsStabilizingColliding(Vector3 pos)
    {
        if (offhandArea == null) return false;
        return offhandArea.bounds.Contains(pos);
    }

    public bool IsDefaultColliding(Vector3 pos)
    {
        return !IsTriggerColliding(pos) && !IsStabilizingColliding(pos);
    } 
}
