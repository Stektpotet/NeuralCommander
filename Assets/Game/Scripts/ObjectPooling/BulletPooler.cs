using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPooler : MonoBehaviour {

    [SerializeField] private GameObject[] bulletTypes;
    Dictionary<string, int> magazineKeytoIndex;
    public List<GenericPooler> bulletPool;

    void Start()
    {
        magazineKeytoIndex = new Dictionary<string, int>();

        for (int i = 0; i < bulletTypes.Length; i++)
        {
            Bullet bulletType = bulletTypes[i].GetComponent<Bullet>();
            if (bulletType == null)
            {
                // make sure all game objects are valid 
                Debug.LogError(string.Format("added non bullet to bullet pool at index: {0}", i));
                return;
            }
            magazineKeytoIndex.Add(bulletType.MagazineKey, i);
            GenericPooler currentPool = ScriptableObject.CreateInstance<GenericPooler>();
            currentPool.Init(bulletTypes[i], 30);
            currentPool.allowedToGrow = true;
            bulletPool.Add(currentPool);
        }


    }

    public Bullet GetBulletOfType(string magazineKey)
    {
        if (!magazineKeytoIndex.ContainsKey(magazineKey))
        {
            Debug.LogError("attemting to retrive nonpooled bullet: " + magazineKey);
            return null;
        }

        int index = magazineKeytoIndex[magazineKey];
        Bullet bullet = bulletPool[index].GetNextObject<Bullet>();
        if (bullet == null)
        { 
            Debug.LogError("attemting to retrive non bullet object: " + magazineKey);
            return null;
        }

        bullet.ResetRigid();

        return bullet;

    }
}
