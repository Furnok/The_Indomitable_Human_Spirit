using UnityEngine;

namespace BT.ScriptablesObject
{
    public class StaticScriptableObject<T> : ScriptableObject
    {
        [SerializeField] private T _value = default;

        public T Value
        {
            get => _value;
        }
    }
}