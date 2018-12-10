using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody))]
public class UnitActions : MonoBehaviour {

    protected Rigidbody rb;
    protected Collider rbCollider;
    private const int NOT_AGENT_MASK = 4202295;

    // Use this for initialization
    void Start () {
        rb              = GetComponent<Rigidbody>();
        rbCollider      = rb.GetComponent<Collider>();
    }
	
    public void MoveTowards(Vector3 target, float moveSpeed)
    {
        Vector3 moveVector = (target - transform.position);
        moveVector.y = 0;
        moveVector = moveVector.normalized * moveSpeed;

        // if direction is not 
        //if(rb.velocity.normalized != (target - transform.position).normalized)
        //{
            rb.AddForce(moveVector);
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        //}

    }
    public void TurnTowards( Vector3 target, float torque, float maxTorque, float allowedError )
    {
        Vector3 relation = target - transform.position;

        float angle = Vector3.SignedAngle(relation, transform.forward, transform.up);
        float absAngle = Mathf.Abs(angle);

        if (Mathf.Abs(angle) < allowedError)
        {
            rb.angularVelocity = new Vector3(0, 0, 0);
            Vector3 lookat = target;
            lookat.y = transform.position.y;
            transform.LookAt(lookat);
            return;
        }
        if (angle < -allowedError)
        {
            if (rb.angularVelocity.y < maxTorque)
            {
                rb.AddTorque(new Vector3(0, torque, 0));
                return;
            }
        }
        if (angle > allowedError)
        {
            if (rb.angularVelocity.y > -maxTorque)
            {
                rb.AddTorque(new Vector3(0, -torque, 0));
                return;
            }
        }
    }

    public void RotateTo( Quaternion rotation ) => rb.MoveRotation(rotation);

    public void TurnTowards( Transform target, float torque, float maxtorque, float allowedError )
        => TurnTowards(target.position, torque, maxtorque, allowedError);

    public void LookAt(Transform target)
    {
        transform.LookAt(target);
    }

    public void Jump(float jumpForce)
    {
        rb.AddForce(new Vector3(0f, jumpForce, 0f));
    }

    /// <summary>
    /// will try to pick up object using hand transform
    /// </summary>
    /// <param name="pickObject">object to be picked up</param>
    /// <param name="hand">hand that pick up object</param>
    /// <returns>object it picked up</returns>
    public PhysicalObject PickupObject(PhysicalObject pickObject, Transform hand, Transform weaponPos)
    {
        RaycastHit hit;
        //Debug.DrawRay(hand.position, (pickObject.transform.position - hand.position).normalized, Color.blue, 2f);
        if (!Physics.Raycast(hand.position, (pickObject.transform.position - hand.position).normalized, out hit, 2f * hand.localScale.x, NOT_AGENT_MASK))
        {
            return null;
        }

        if (hit.transform.gameObject.name != pickObject.gameObject.name)
        {
            return null;
        }
            

        pickObject.OnPickUp(weaponPos);
        return pickObject;
    }

    /// <summary>
    /// Drop object that is in hand
    /// </summary>
    /// <param name="pickObject">object to drop</param>
    /// <param name="hand">hand to do action</param>
    /// <param name="velocity">velocity to apply to object</param>
    /// <returns>object that was dropped</returns>
    public PhysicalObject DropObject(PhysicalObject pickObject, Transform hand, Vector3 velocity)
    {
        pickObject.OnDrop(hand, velocity);
        return pickObject;

    }

    public void ActivateObject(PhysicalObject pickObject, Transform hand)
    {
        pickObject.OnActivate(hand);
    }

    public void AimAt(Transform hand, Vector3 targetPosition, float t)
    {
        // carefull shooting direcly at target will probaly ending in bullets dropping to the groundt
        //hand.LookAt(target
        var desiredRot = Quaternion.LookRotation((targetPosition - hand.position).normalized, Vector3.up);
        hand.rotation = Quaternion.Slerp(hand.rotation, desiredRot, t);
    }

    public void AimAt(Transform hand, Quaternion startRotation, Vector3 targetPosition, float t)
    {
        var desiredRot = Quaternion.LookRotation((targetPosition - hand.position).normalized, Vector3.up);
        hand.rotation = Quaternion.Slerp(startRotation, desiredRot, t);
    }

    public void Crouch(Transform leg, float originalHeight, float amount)
    {
        Vector3 pos = leg.localPosition;
        leg.localPosition = new Vector3(pos.x, originalHeight + amount * leg.localScale.y, pos.z);
    }

    public void StandUp(Transform leg, float originalHeight)
    {
        Vector3 pos = leg.localPosition;
        leg.localPosition = new Vector3(pos.x, originalHeight, pos.z);
    }

    public void ReloadWeapon(PhysicalObject weaponObject, float reloadTimer = 0)
    {
        WeaponPhysicalObject weapon = (WeaponPhysicalObject) weaponObject;
        weapon?.loadedMagazine?.AgentReload(reloadTimer);
    }

    public void BreakSpeed(float amount)
    {
        rb.velocity = Vector3.zero;
       /*amount = Mathf.Abs(amount);
        if (amount > 1) amount = 1;
        
        rb.AddForce(-rb.velocity * amount);*/
    }
}
