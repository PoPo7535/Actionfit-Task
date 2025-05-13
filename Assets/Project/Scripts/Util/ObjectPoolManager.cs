using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;
    private Dictionary<Type,Queue<Component>> objectPool = new();
    public void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public T GetObject<T>(T obj, Vector3 pos, Quaternion rotation = default) where T : Component
    {
        rotation = rotation == default ? Quaternion.identity : rotation;
        var type = obj.GetType();
        if (false == objectPool.ContainsKey(type))
            objectPool.Add(type, new Queue<Component>());

        if (objectPool[type].Count > 0 && false == objectPool[type].Peek().gameObject.activeSelf)
        {
            var poolObj = objectPool[type].Dequeue();
            poolObj.transform.position = pos;
            poolObj.transform.rotation = rotation;
            poolObj.gameObject.SetActive(true);
            return poolObj as T;
        }
        
        var newObj = Instantiate(obj, pos, rotation);
        newObj.transform.SetParent(transform, true);
        objectPool[type].Enqueue(newObj);

        return newObj;
    }
    
    public T GetObject<T>(T obj, Transform tr ) where T : Component
    {
        var type = obj.GetType();
        if (false == objectPool.ContainsKey(type))
            objectPool.Add(type, new Queue<Component>());

        if (objectPool[type].Count > 0 && false == objectPool[type].Peek().gameObject.activeSelf)
        {
            var poolObj = objectPool[type].Dequeue();
            poolObj.transform.position = tr.position;
            poolObj.transform.rotation = tr.rotation;
            poolObj.gameObject.SetActive(true);
            return poolObj as T;
        }
        
        var newObj = Instantiate(obj, tr);
        objectPool[type].Enqueue(newObj);
        return newObj;
    }
    
    public void Release<T>(T obj) where T : Component
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform, true);
        var type = typeof(T);
        if (false == objectPool.ContainsKey(type))
            objectPool[type] = new Queue<Component>();
        objectPool[type].Enqueue(obj);
        
    }
}
