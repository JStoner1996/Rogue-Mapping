using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T prefab;
    private Queue<T> pool = new Queue<T>();

    public ObjectPool(T prefab, int initialSize)
    {
        this.prefab = prefab;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateInstance();
            ReturnToPool(obj);
        }
    }

    public T Get()
    {
        while (pool.Count > 0)
        {
            T obj = pool.Dequeue();

            if (obj == null)
            {
                continue;
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        T newObj = CreateInstance();
        newObj.gameObject.SetActive(true);
        return newObj;
    }

    public void ReturnToPool(T obj)
    {
        if (obj == null)
        {
            return;
        }

        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    private T CreateInstance()
    {
        T obj = Object.Instantiate(prefab);
        obj.gameObject.SetActive(false);
        return obj;
    }
}
