using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class ObjectMimicWithDelay : MonoBehaviour
{
    [Header("Settings")]
    public GameObject target;
    [Range(0f, 5f)]
    public float delay = 1.0f;

    [Header("Performance")]
    [Tooltip("If true, variable values (health, speed, etc) are synced instantly. If false, they are ignored to save performance.")]
    public bool syncValuesInstantly = false;

    // A list of matched pairs (Source Component -> Local Component)
    private List<ComponentPair> _trackedComponents = new List<ComponentPair>();

    // History Queue
    private Queue<StateSnapshot> _history = new Queue<StateSnapshot>();

    // Helper class to map a target component to our local component
    private class ComponentPair
    {
        public Component source;
        public Component destination;
        public bool isToggleable; // True if it has an 'enabled' property
    }

    // Struct to hold data for a single frame in history
    private struct StateSnapshot
    {
        public float timestamp;

        // Transform Data
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        // Component States (Active/Inactive)
        public bool[] componentEnabledStates;
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("ObjectMimic: No target assigned.");
            enabled = false;
            return;
        }

        SetupComponents();
    }

    void Update()
    {
        if (target == null) return;

        // 1. Record current state (Transform + Component Active Status)
        RecordSnapshot();

        // 2. Apply state from history (after delay)
        ApplyDelayedSnapshot();

        // 3. (Optional) Sync variable values instantly
        // Note: Delaying variable values (int/float) is extremely expensive memory-wise, 
        // so we usually sync values instantly or not at all.
        if (syncValuesInstantly)
        {
            SyncComponentValues();
        }
    }

    void SetupComponents()
    {
        // Get every component on the target
        Component[] sourceComps = target.GetComponents<Component>();

        foreach (var source in sourceComps)
        {
            // Skip Transform (handled separately) and this script
            if (source is Transform || source is ObjectMimicWithDelay) continue;

            Type type = source.GetType();

            // Get or Add the component locally
            Component dest = GetComponent(type);
            if (dest == null) dest = gameObject.AddComponent(type);

            // Determine if this component can be enabled/disabled
            bool toggleable = (source is Behaviour || source is Renderer || source is Collider);

            _trackedComponents.Add(new ComponentPair
            {
                source = source,
                destination = dest,
                isToggleable = toggleable
            });
        }
    }

    void RecordSnapshot()
    {
        // Create list of enabled states for all tracked components
        bool[] states = new bool[_trackedComponents.Count];

        for (int i = 0; i < _trackedComponents.Count; i++)
        {
            if (_trackedComponents[i].isToggleable)
            {
                states[i] = GetEnabledState(_trackedComponents[i].source);
            }
        }

        _history.Enqueue(new StateSnapshot
        {
            timestamp = Time.time,
            position = target.transform.position,
            rotation = target.transform.rotation,
            scale = target.transform.localScale,
            componentEnabledStates = states
        });
    }

    void ApplyDelayedSnapshot()
    {
        if (delay <= 0f)
        {
            // Instant sync if no delay
            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation;
            transform.localScale = target.transform.localScale;

            for (int i = 0; i < _trackedComponents.Count; i++)
            {
                if (_trackedComponents[i].isToggleable)
                {
                    bool currentState = GetEnabledState(_trackedComponents[i].source);
                    SetEnabledState(_trackedComponents[i].destination, currentState);
                }
            }
            _history.Clear();
            return;
        }

        // Apply history if it is old enough
        if (_history.Count > 0 && Time.time - _history.Peek().timestamp >= delay)
        {
            StateSnapshot snapshot = _history.Dequeue();

            // Apply Transform
            transform.position = snapshot.position;
            transform.rotation = snapshot.rotation;
            transform.localScale = snapshot.scale;

            // Apply Component States
            for (int i = 0; i < _trackedComponents.Count; i++)
            {
                if (_trackedComponents[i].isToggleable)
                {
                    SetEnabledState(_trackedComponents[i].destination, snapshot.componentEnabledStates[i]);
                }
            }
        }
    }

    // --- Helper Helpers ---

    bool GetEnabledState(Component c)
    {
        if (c is Behaviour b) return b.enabled;
        if (c is Collider col) return col.enabled;
        if (c is Renderer r) return r.enabled;
        return true;
    }

    void SetEnabledState(Component c, bool state)
    {
        if (c is Behaviour b) b.enabled = state;
        else if (c is Collider col) col.enabled = state;
        else if (c is Renderer r) r.enabled = state;
    }

    void SyncComponentValues()
    {
        foreach (var pair in _trackedComponents)
        {
            // Copy fields via Reflection (Expensive!)
            FieldInfo[] fields = pair.source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField)))
                {
                    try { field.SetValue(pair.destination, field.GetValue(pair.source)); }
                    catch { }
                }
            }
        }
    }
}