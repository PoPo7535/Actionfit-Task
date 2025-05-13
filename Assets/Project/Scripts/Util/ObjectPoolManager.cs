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
    }

    public T GetObject<T>(T obj, Vector3 pos, Quaternion quaternion) where T : Component
    {
        var type = obj.GetType();
        if (false == objectPool.ContainsKey(type))
            objectPool.Add(type, new Queue<Component>());

        if (objectPool[type].Count > 0 && false == objectPool[type].Peek().gameObject.activeSelf)
        {
            var poolObj = objectPool[type].Dequeue();
            poolObj.transform.position = pos;
            poolObj.transform.rotation = quaternion;
            poolObj.gameObject.SetActive(true);
            return poolObj as T;
        }
        
        var newObj = Instantiate(obj, pos, quaternion);
        objectPool[type].Enqueue(newObj);

        return newObj;
    }
    
    public void Release<T>(T obj) where T : Component
    {
        obj.gameObject.SetActive(false);
        var type = typeof(T);
        if (false == objectPool.ContainsKey(type))
            objectPool[type] = new Queue<Component>();
        objectPool[type].Enqueue(obj);
    }
}
