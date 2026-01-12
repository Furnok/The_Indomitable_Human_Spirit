using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public class S_ClassSettingsSaved
{
    [Title("Langues")]
    public int languageIndex = 0;

    [Title("Target")]
    public bool holdLockTarget = false;

    [Title("Target")]
    public bool controllerRumble = true;

    [Title("Screen")]
    public int resolutionIndex = -1;

    public bool fullScreen = true;

    public int qualityIndex = 0;

    [Title("Audio")]
    public List<S_ClassVolume> listVolumes = new()
    {
        new S_ClassVolume { name = "Master", volume = 100f },
        new S_ClassVolume { name = "Music", volume = 100f },
        new S_ClassVolume { name = "Sounds", volume = 100f },
        new S_ClassVolume { name = "UI", volume = 100f }
    };

    public S_ClassSettingsSaved Clone()
    {
        S_ClassSettingsSaved copy = (S_ClassSettingsSaved)MemberwiseClone();

        copy.listVolumes = new();

        foreach (var vol in listVolumes)
        {
            copy.listVolumes.Add(new S_ClassVolume
            {
                name = vol.name,
                volume = vol.volume
            });
        }

        return copy;
    }
}