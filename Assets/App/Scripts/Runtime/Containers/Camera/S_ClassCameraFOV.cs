using Sirenix.OdinInspector;
using System;

[Serializable]
public class S_ClassCameraFOV
{
    [Title("FOV")]
    public float value = 0;

    [SuffixLabel("s", Overlay = true)]
    public float time = 0;

    public bool reset = false;
}