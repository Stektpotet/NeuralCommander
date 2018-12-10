using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// source: https://unity3d.com/learn/tutorials/topics/scripting/object-pooling

// TODO: force fetch object if too many active objects
// TODO: garbage collect if too many inactive objects

public class GenericPooler : ScriptableObject {

    public bool allowedToGrow;
    private GameObject toBePooled;
    private int poolSize;

    public List<GameObject> ObjectPool;
    private bool initialized;

	// Use this for initialization
	void Awake () {
        initialized = false;
	}
	
    public void Init(GameObject toBePooled, int poolSize)
    {
        // might not need to save these?
        this.toBePooled = toBePooled;
        this.poolSize   = poolSize;

        ObjectPool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            var newObject = Instantiate(toBePooled);
            newObject.SetActive(false);
            ObjectPool.Add(newObject);
        }

        initialized = true;
    }

	public T GetNextObject<T>()
    {

        if (initialized != true)
        {
            Debug.LogError("using uninitialized pooler");
            return default(T);
        }
        if (toBePooled.GetComponent<T>() == null)
        {
            Debug.LogError("pooled object is not of type " + typeof(T).FullName);
            return default(T);
        }

        var nextObj = ObjectPool.Find(obj => !obj.activeSelf);
        if (nextObj != null)
        {
            nextObj.SetActive(true);
            return nextObj.GetComponent<T>();
        }
        if (allowedToGrow)
        {
            var newObject = Instantiate(toBePooled);
            newObject.SetActive(true);
            ObjectPool.Add(newObject);
            return newObject.GetComponent<T>();
        }
        Debug.LogError("pool empty got empty");
        return default(T);   
    }
}
