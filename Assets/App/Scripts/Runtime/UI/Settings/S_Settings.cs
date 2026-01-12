using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class S_Settings : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Save")]
    [SerializeField, S_SaveName] private string saveSettingsName;

    [TabGroup("References")]
    [Title("Script")]
    [SerializeField] private S_LoadUISettings loadUISettings;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSaveData rseOnSaveData;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnConsole rseOnConsole;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCancelTargeting rseOnCancelTargeting;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_SettingsSaved rsoSettingsSaved;

    private bool isLoaded = false;
    private bool isSave = false;

    private List<Slider> listSlidersAudios = new();
    private List<TextMeshProUGUI> listTextAudios = new();

    private RSO_SettingsSaved rsoSettingsSavedOld;

    private Bus audioMaster;
    private Bus audioMusic;
    private Bus audioSounds;
    private Bus audioUI;

    private int step = 1;
    private float stepFloat = 1f;
    private bool startMove = false;
    private float holdTime = 0f;
    private bool isStick = false;
    private float maxStep = 3f;
    private float accelerationTime = 3f;

    private void Awake()
    {
        audioMaster = RuntimeManager.GetBus("bus:/");
        audioMusic = RuntimeManager.GetBus("bus:/Music");
        audioSounds = RuntimeManager.GetBus("bus:/Sounds");
        audioUI = RuntimeManager.GetBus("bus:/UI");
    }

    private void OnEnable()
    {
        rsoSettingsSavedOld = ScriptableObject.CreateInstance<RSO_SettingsSaved>();
        rsoSettingsSavedOld.Value = rsoSettingsSaved.Value.Clone();

        isLoaded = false;
        isSave = false;
    }

    private void Update()
    {
        if (isLoaded && (EventSystem.current.currentSelectedGameObject == listSlidersAudios[0].gameObject || EventSystem.current.currentSelectedGameObject == listSlidersAudios[1].gameObject || EventSystem.current.currentSelectedGameObject == listSlidersAudios[2].gameObject || EventSystem.current.currentSelectedGameObject == listSlidersAudios[3].gameObject) && Gamepad.current != null && startMove)
        {
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();

            bool stickActive = Mathf.Abs(leftStick.x) > 0.5f;
            bool stick2Active = Mathf.Abs(rightStick.x) > 0.5f;

            if (stickActive || stick2Active)
            {
                if (!isStick)
                {
                    isStick = true;
                    holdTime = 0f;
                    step = 1;
                    stepFloat = 1f;
                }

                holdTime += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(holdTime / accelerationTime);
                stepFloat = Mathf.Lerp(1f, maxStep, t);

                step = (int)Mathf.Clamp(Mathf.FloorToInt(stepFloat), 1, maxStep);
            }
            else if (isStick)
            {
                startMove = false;
                step = 1;
                stepFloat = 1f;
                holdTime = 0f;
            }
            else
            {
                step = 1;
                stepFloat = 1f;
                holdTime = 0f;
            }
        }
        else
        {
            isStick = false;
            step = 1;
            stepFloat = 1f;
            holdTime = 0f;
        }
    }

    public void Setup(List<Slider> listSlidersVolumes, List<TextMeshProUGUI> listTextVolumes)
    {
        listSlidersAudios = listSlidersVolumes;
        listTextAudios = listTextVolumes;

        isLoaded = true;
    }

    public void UpdateLanguages(int index)
    {
        if (isLoaded && rsoSettingsSaved.Value.languageIndex != index) rsoSettingsSaved.Value.languageIndex = index;
    }

    public void UpdateHoldLockTarget(bool value)
    {
        if (isLoaded && rsoSettingsSaved.Value.holdLockTarget != value) rsoSettingsSaved.Value.holdLockTarget = value;

        if (rsoSettingsSaved.Value.holdLockTarget) rseOnCancelTargeting.Call();
    }

    public void UpdateControllerRumble(bool value)
    {
        if (isLoaded && rsoSettingsSaved.Value.controllerRumble != value) rsoSettingsSaved.Value.controllerRumble = value;
    }

    public void UpdateActivateTuto(bool value)
    {
        if (isLoaded && rsoSettingsSaved.Value.activateTuto != value) rsoSettingsSaved.Value.activateTuto = value;
    }

    public void UpdateDevMode(bool value)
    {
        if (isLoaded && rsoSettingsSaved.Value.devMode != value)
        {
            rsoSettingsSaved.Value.devMode = value;

            if (!rsoSettingsSaved.Value.devMode) rseOnConsole.Call();
        }
    }

    private Resolution GetResolutions(int index)
    {
        List<Resolution> resolutionsPC = new(Screen.resolutions);

        resolutionsPC = resolutionsPC
            .Where(r => r.width >= 1280 && r.height >= 720)
            .OrderByDescending(r => r.width * r.height)
            .ThenByDescending(r => r.refreshRateRatio.value)
            .ToList();

        Resolution resolution = resolutionsPC[0];

        for (int i = 0; i < resolutionsPC.Count; i++)
        {
            Resolution res = resolutionsPC[i];

            if (i == index) resolution = res;
        }

        return resolution;
    }

    public void UpdateResolutions(int index)
    {
        if (isLoaded && rsoSettingsSaved.Value.resolutionIndex != index) rsoSettingsSaved.Value.resolutionIndex = index;
    }

    public void UpdateFullscreen(bool value)
    {
        if (isLoaded && rsoSettingsSaved.Value.fullScreen != value) rsoSettingsSaved.Value.fullScreen = value;
    }

    public void UpdateQuality(int index)
    {
        if (isLoaded && rsoSettingsSaved.Value.qualityIndex != index) rsoSettingsSaved.Value.qualityIndex = index;
    }

    public void UpdateMainVolume(float value)
    {
        if (isLoaded && rsoSettingsSaved.Value.listVolumes[0].volume != value)
        {
            float newValue = 0;

            if (Gamepad.current != null)
            {
                startMove = true;

                float diff = Mathf.Sign(value - rsoSettingsSaved.Value.listVolumes[0].volume);
                newValue = Mathf.RoundToInt(diff * step);
                newValue = Mathf.Clamp(rsoSettingsSaved.Value.listVolumes[0].volume + newValue, listSlidersAudios[0].minValue, listSlidersAudios[0].maxValue);
                listSlidersAudios[0].SetValueWithoutNotify(newValue);
            }
            else newValue = value;

            rsoSettingsSaved.Value.listVolumes[0].volume = newValue;

            listTextAudios[0].text = $"{newValue}%";

            audioMaster.setVolume(rsoSettingsSaved.Value.listVolumes[0].volume / 100);
        }
    }

    public void UpdateMusicVolume(float value)
    {
        if (isLoaded && rsoSettingsSaved.Value.listVolumes[1].volume != value)
        {
            float newValue = 0;

            if (Gamepad.current != null)
            {
                startMove = true;

                float diff = Mathf.Sign(value - rsoSettingsSaved.Value.listVolumes[1].volume);
                newValue = Mathf.RoundToInt(diff * step);
                newValue = Mathf.Clamp(rsoSettingsSaved.Value.listVolumes[1].volume + newValue, listSlidersAudios[1].minValue, listSlidersAudios[1].maxValue);
                listSlidersAudios[1].SetValueWithoutNotify(newValue);
            }
            else newValue = value;

            rsoSettingsSaved.Value.listVolumes[1].volume = newValue;

            listTextAudios[1].text = $"{newValue}%";

            audioMusic.setVolume(rsoSettingsSaved.Value.listVolumes[1].volume / 100);
        }
    }

    public void UpdateSoundsVolume(float value)
    {
        if (isLoaded && rsoSettingsSaved.Value.listVolumes[2].volume != value)
        {
            float newValue = 0;

            if (Gamepad.current != null)
            {
                startMove = true;

                float diff = Mathf.Sign(value - rsoSettingsSaved.Value.listVolumes[2].volume);
                newValue = Mathf.RoundToInt(diff * step);
                newValue = Mathf.Clamp(rsoSettingsSaved.Value.listVolumes[2].volume + newValue, listSlidersAudios[2].minValue, listSlidersAudios[2].maxValue);
                listSlidersAudios[2].SetValueWithoutNotify(newValue);
            }
            else newValue = value;

            rsoSettingsSaved.Value.listVolumes[2].volume = newValue;

            listTextAudios[2].text = $"{newValue}%";

            audioSounds.setVolume(rsoSettingsSaved.Value.listVolumes[2].volume / 100);
        }
    }

    public void UpdateUIVolume(float value)
    {
        if (isLoaded && rsoSettingsSaved.Value.listVolumes[3].volume != value)
        {
            float newValue = 0;

            if (Gamepad.current != null)
            {
                startMove = true;

                float diff = Mathf.Sign(value - rsoSettingsSaved.Value.listVolumes[3].volume);
                newValue = Mathf.RoundToInt(diff * step);
                newValue = Mathf.Clamp(rsoSettingsSaved.Value.listVolumes[3].volume + newValue, listSlidersAudios[3].minValue, listSlidersAudios[3].maxValue);
                listSlidersAudios[3].SetValueWithoutNotify(newValue);
            }
            else newValue = value;

            rsoSettingsSaved.Value.listVolumes[3].volume = newValue;

            listTextAudios[3].text = $"{newValue}%";

            audioUI.setVolume(rsoSettingsSaved.Value.listVolumes[3].volume / 100);
        }
    }

    public void SaveSettings()
    {
        isSave = true;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[rsoSettingsSaved.Value.languageIndex];

        Resolution resolution = GetResolutions(rsoSettingsSaved.Value.resolutionIndex);

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);

        if (rsoSettingsSaved.Value.fullScreen) Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else Screen.fullScreenMode = FullScreenMode.Windowed;

        Screen.fullScreen = rsoSettingsSaved.Value.fullScreen;

        QualitySettings.SetQualityLevel(rsoSettingsSaved.Value.qualityIndex, true);

        rseOnSaveData.Call(saveSettingsName, true);
    }

    public void Close()
    {
        if (!isSave)
        {
            rsoSettingsSaved.Value = rsoSettingsSavedOld.Value.Clone();

            audioMaster.setVolume(rsoSettingsSaved.Value.listVolumes[0].volume / 100);
            audioMusic.setVolume(rsoSettingsSaved.Value.listVolumes[1].volume / 100);
            audioSounds.setVolume(rsoSettingsSaved.Value.listVolumes[2].volume / 100);
            audioUI.setVolume(rsoSettingsSaved.Value.listVolumes[3].volume / 100);

            loadUISettings.LoadUI();
        }
    }
}