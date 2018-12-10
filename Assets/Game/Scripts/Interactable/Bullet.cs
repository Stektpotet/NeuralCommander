using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PhysicalObject {

    [SerializeField] private string magazineKey;
    public string MagazineKey{   get{    return this.magazineKey;   }   }

    public float bulletImpactThreshold;
    public float bulletIdleThreshold;

    private float idleCounter;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (!gameObject.activeSelf)
        {
            return;
        }

        if (collision.relativeVelocity.magnitude >= bulletImpactThreshold)
        {
            gameObject.SetActive(false);       
        }

        idleCounter = 0f;
    }

    public void ResetRigid()
    {
        body.angularVelocity = Vector3.zero;
        body.velocity = Vector3.zero;

    }

    private void FixedUpdate()
    {
        if (gameObject.activeSelf)
        {
            idleCounter += Time.fixedDeltaTime;

            if (idleCounter >= bulletIdleThreshold)
            {
                gameObject.SetActive(false);
                idleCounter = 0f;
            }
        }
    }
}
