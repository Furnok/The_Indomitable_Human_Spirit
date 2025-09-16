using System;
using UnityEngine;

namespace BT.ScriptablesObject
{
    public class RuntimeScriptableEvent : ScriptableObject
    {
        public event Action action;

        public void Call() => action?.Invoke();
    }

    public class RuntimeScriptableEvent<T> : ScriptableObject
    {
        public event Action<T> action;

        public void Call(T t) => action?.Invoke(t);
    }

    public class RuntimeScriptableEvent<T1, T2> : ScriptableObject
    {
        public event Action<T1, T2> action;

        public void Call(T1 t1, T2 t2) => action?.Invoke(t1, t2);
    }

    public class RuntimeScriptableEvent<T1, T2, T3> : ScriptableObject
    {
        public event Action<T1, T2, T3> action;

        public void Call(T1 t1, T2 t2, T3 t3) => action?.Invoke(t1, t2, t3);
    }
}