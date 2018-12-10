using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Maybe have let players see if there is ammo in the magazine

public class MagazineObject : PhysicalObject
{

    [SerializeField] private string magazineKey;
    public string MagazineKey { get { return this.magazineKey; } }

    [SerializeField] private int maxAmmo;

    private BulletPooler singletonPooler;

    private int bulletIndex;
    private WeaponPhysicalObject inWeapon;

    private bool reloadRoutineRunning;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        singletonPooler = FindObjectOfType<BulletPooler>();

        Vector3 ammoLocation = new Vector3(0, 0, 0);
        Vector3 locationOffset = new Vector3(0, 0, -0.1f);

    }

    /// <summary>
    /// Called when picking up this item
    /// </summary>
    public void OnLoaded(Transform pickedUpByHand, WeaponPhysicalObject weapon)
    {
        //internal stuff
        isPickedUp = true;
        //NOTE: REMEMBER TO USE RIGIDBODY.MovePosition instead of changing transform.position to avoid weird physics, or lack thereof
        body.isKinematic = true;

        transform.rotation = pickedUpByHand.rotation;
        transform.position = pickedUpByHand.position;
        transform.parent = pickedUpByHand.transform;

        inWeapon = weapon;

        // TODO: should invoke something
    }

    /// <summary>
    /// Called when picking up this item
    /// </summary>
    public override void OnPickUp(Transform pickedUpByHand)
    {
        //internal stuff
        isPickedUp = true;

        body.isKinematic = true;
        transform.parent = pickedUpByHand.transform;

        if (inWeapon != null)
        {
            inWeapon.loadedMagazine = null;
            inWeapon = null;
        }

        //external stuff
        onPickup.Invoke(pickedUpByHand);
    }

    public Bullet Fire()
    {
        if (bulletIndex >= maxAmmo)
            return null;

        
        var bullet = singletonPooler.GetBulletOfType(magazineKey);

        if(bullet == null)
        {
            Debug.LogError("bullet was null", this);
        }

        bulletIndex++;
        return bullet;
    }

    /// <summary>
    /// Reload after said time in seconds
    /// </summary>
    /// <param name="reloadTime">seconds before reload is complete</param>
    public void AgentReload(float reloadTime = 0f)
    {
        if (!reloadRoutineRunning)
        {
            StartCoroutine(ReloadRoutine(reloadTime));
        }
    }

    IEnumerator ReloadRoutine(float reloadTime)
    {
        reloadRoutineRunning = true;

        yield return new WaitForSeconds(reloadTime);

        bulletIndex = 0;

        reloadRoutineRunning = false;
    }

    public bool IsNotEmpty()
    {
        return bulletIndex < maxAmmo;
    }

    private void OnDestroy()
    {
        if(inWeapon != null)
        {
            inWeapon.loadedMagazine = null;
            inWeapon = null;
        }
       
        onDestroy.Invoke();
    }
}
