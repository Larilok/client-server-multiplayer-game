﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    //public static ObjectPool SharedInstance;
    public List<GameObject> pool;
    public GameObject pooledObject;
    public int pooledAmmount;
    
    void Awake()
    {
        //SharedInstance = this;
    }

    private void Start()
    {
        pool = new List<GameObject>();
        for (int i = 0; i < pooledAmmount; i++)
        {
            GameObject obj = Instantiate(pooledObject);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }
    public List<GameObject> GetActiveObjectsInPositionList(Vector3 wantedPos)
    {
        List<GameObject> newPool = pool.FindAll(gO =>
        {
            if (gO.activeInHierarchy)
            {
                if (gO.transform.position == wantedPos) return true;
                else return false;
            }
            else return false;
        });
        return newPool;
    }
    public GameObject GetObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy) return pool[i];
        }
        Debug.Log("HAVE TO ADD OBJECTS!");
        return AddAndReturnObject();
    }

    private GameObject AddAndReturnObject()
    {
        GameObject obj = Instantiate(pooledObject);
        obj.SetActive(false);
        pool.Add(obj);
        return obj;
    }
}
