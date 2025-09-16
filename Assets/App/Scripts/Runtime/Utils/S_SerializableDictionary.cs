using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class S_SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public void OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
        {
            this.Add(keys[i], values[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var kvp in this)
        {
            keys.Add(kvp.Key);
            values.Add(kvp.Value);
        }
    }
}
