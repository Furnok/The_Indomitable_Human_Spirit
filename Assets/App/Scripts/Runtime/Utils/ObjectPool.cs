using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly T prefab;
    private readonly Transform parentTransform;
    private readonly Queue<T> pool = new Queue<T>();

    public ObjectPool(T prefab, int initialSize, Transform parentTransform = null)
    {
        this.prefab = prefab;
        this.parentTransform = parentTransform;

        for (int i = 0; i < initialSize; i++)
        {
            T instance = Object.Instantiate(prefab, parentTransform);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }
    }

    public T Get()
    {
        T instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
        }
        else
        {
            instance = Object.Instantiate(prefab, parentTransform);
        }

        instance.gameObject.SetActive(true);
        return instance;
    }

    public void ReturnToPool(T instance)
    {
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
    }
}
