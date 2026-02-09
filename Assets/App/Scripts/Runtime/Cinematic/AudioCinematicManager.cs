using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Cinemachine; // Required for Cinemachine 3.0
using FMODUnity;
using FMOD.Studio;

public class AudioCinematicManager : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private KeyCode startKey = KeyCode.Return;

    [Header("FMOD Settings")]
    [SerializeField] private EventReference cinematicAudioEvent;
    [Tooltip("Prefix for markers to trigger shots (e.g. 'Cut_')")]
    [SerializeField] private string markerPrefix = "Cut_";

    [Header("Sequence Data")]
    [SerializeField] private List<CinematicShot> shotList;

    private EventInstance audioInstance;
    private int currentShotIndex = -1;
    private bool isCinematicRunning = false;

    // FMOD Callback vars
    private GCHandle timelineHandle;
    private EVENT_CALLBACK beatCallback;
    private bool pendingCut = false;

    private void Start()
    {
        // Reset all cameras to priority 0 on start
        foreach (var shot in shotList)
        {
            if (shot.camera != null) shot.camera.Priority = 0;
        }
    }

    private void Update()
    {
        // 1. Input Check
        if (!isCinematicRunning && Input.GetKeyDown(startKey))
        {
            StartCinematic();
        }

        // 2. FMOD Thread Sync
        if (pendingCut)
        {
            pendingCut = false;
            AdvanceShot();
        }
    }

    public void StartCinematic()
    {
        if (isCinematicRunning) return;
        isCinematicRunning = true;

        // Prepare Index
        currentShotIndex = -1;
        pendingCut = false;

        // Create FMOD Instance
        audioInstance = RuntimeManager.CreateInstance(cinematicAudioEvent);

        // Bind Callback
        timelineHandle = GCHandle.Alloc(this);
        beatCallback = new EVENT_CALLBACK(AudioCallback);
        audioInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));
        audioInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        // Start Audio
        audioInstance.start();

        // *** NEW: Force the first shot immediately ***
        // This takes us from Index -1 to Index 0 instantly.
        AdvanceShot();
    }

    public void StopCinematic()
    {
        isCinematicRunning = false;
        audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        audioInstance.release();

        if (timelineHandle.IsAllocated) timelineHandle.Free();
    }

    private void OnDestroy()
    {
        StopCinematic();
    }

    private void AdvanceShot()
    {
        currentShotIndex++;

        // If we run out of shots, just log and let audio finish.
        if (currentShotIndex >= shotList.Count)
        {
            Debug.Log($"Sequence finished (Index {currentShotIndex}). Audio continuing...");
            return;
        }

        CinematicShot activeShot = shotList[currentShotIndex];

        // --- 1. Camera Logic ---
        for (int i = 0; i < shotList.Count; i++)
        {
            var shot = shotList[i];
            if (shot.camera != null)
            {
                if (i == currentShotIndex)
                {
                    // A. Priority
                    shot.camera.Priority = activeShot.targetPriority;

                    // B. Spline Reset (Cinemachine 3.0 / Unity 6)
                    // We look for 'CinemachineSplineDolly' on the Camera object
                    var splineDolly = shot.camera.GetComponent<CinemachineSplineDolly>();
                    if (splineDolly != null)
                    {
                        splineDolly.CameraPosition = 0f; // Reset position to 0
                    }
                }
                else
                {
                    shot.camera.Priority = 0;
                }
            }
        }

        // --- 2. Animation Trigger Logic ---
        foreach (var cmd in activeShot.triggersToFire)
        {
            if (cmd.targetAnimator != null && !string.IsNullOrEmpty(cmd.triggerName))
            {
                cmd.targetAnimator.SetTrigger(cmd.triggerName);
            }
        }

        // --- 3. Events ---
        activeShot.onShotStart?.Invoke();
    }

    // --- FMOD CALLBACK ---
    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT AudioCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        EventInstance instance = new EventInstance(instancePtr);
        IntPtr timelineInfoPtr;
        instance.getUserData(out timelineInfoPtr);

        if (timelineInfoPtr != IntPtr.Zero)
        {
            GCHandle handle = GCHandle.FromIntPtr(timelineInfoPtr);
            AudioCinematicManager manager = handle.Target as AudioCinematicManager;

            if (manager != null && type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            {
                var parameter = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));

                // FMOD 2.02+ String Wrapper Fix
                string markerName = parameter.name;

                if (!string.IsNullOrEmpty(markerName) && markerName.Contains(manager.markerPrefix))
                {
                    manager.pendingCut = true;
                }
            }
        }
        return FMOD.RESULT.OK;
    }
}