using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using System.Collections.Generic;

[System.Serializable]
public class AnimationCommand
{
    public Animator targetAnimator;
    [Tooltip("The name of the Trigger parameter in the Animator Controller.")]
    public string triggerName;
}

[System.Serializable]
public class CinematicShot
{
    [Header("Camera Settings")]
    public string shotName;
    public CinemachineVirtualCameraBase camera;
    public int targetPriority = 100;

    [Header("Animation")]
    public List<AnimationCommand> triggersToFire;

    [Header("Game Logic")]
    public UnityEvent onShotStart;
}