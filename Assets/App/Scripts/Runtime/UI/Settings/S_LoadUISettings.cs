using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_LoadUISettings : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Save")]
    [SerializeField, S_SaveName] private string saveSettingsName;

    [TabGroup("References")]
    [Title("Settings Script")]
    [SerializeField] private S_Settings settings;

    [TabGroup("References")]
    [Title("Selectables")]
    [SerializeField] private TMP_Dropdown dropDownLanguages;

    [TabGroup("References")]
    [SerializeField] private Toggle toggleHoldLockTarget;

    [TabGroup("References")]
    [SerializeField] private Toggle toggleControllerRumble;

    [TabGroup("References")]
    [SerializeField] private Toggle toggleActivateTuto;

    [TabGroup("References")]
    [SerializeField] private Toggle toggleDevMode;

    [TabGroup("References")]
    [SerializeField] private TMP_Dropdown dropDownResolutions;

    [TabGroup("References")]
    [SerializeField] private Toggle toggleFullscreen;

    [TabGroup("References")]
    [SerializeField] private TMP_Dropdown dropDownQuality;

    [TabGroup("References")]
    [SerializeField] private List<Slider> listSliderVolume;

    [TabGroup("References")]
    [Title("Texts")]
    [SerializeField] private List<TextMeshProUGUI> listTextsVolume;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSaveData rseOnSaveData;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_SettingsSaved rsoSettingsSaved;

    private void OnEnable()
    {
        LoadUI();
    }

    public void ResetSettings()
    {
        rsoSettingsSaved.Value = new();

        StartCoroutine(S_Utils.DelayFrame(() => LoadUI()));
    }

    public void LoadUI()
    {
        LoadLanguages();

        LoadHoldLockTarget();

        LoadControllerRumble();

        LoadActivateTuto();

        LoadDevMode();

        LoadResolutions();

        LoadFullScreen();

        LoadQuality();

        LoadVolumes();

        StartCoroutine(S_Utils.DelayFrame(() => settings.Setup(listSliderVolume, listTextsVolume)));
    }

    private void LoadLanguages()
    {
        dropDownLanguages.value = rsoSettingsSaved.Value.languageIndex;
    }

    private void LoadHoldLockTarget()
    {
        toggleHoldLockTarget.isOn = rsoSettingsSaved.Value.holdLockTarget;
    }

    private void LoadControllerRumble()
    {
        toggleControllerRumble.isOn = rsoSettingsSaved.Value.controllerRumble;
    }

    private void LoadActivateTuto()
    {
        toggleActivateTuto.isOn = rsoSettingsSaved.Value.activateTuto;
    }

    private void LoadDevMode()
    {
        toggleDevMode.isOn = rsoSettingsSaved.Value.devMode;
    }

    private int GetResolutions(int index)
    {
        List<Resolution> resolutionsPC = new(Screen.resolutions);

        resolutionsPC = resolutionsPC
            .Where(r => r.width >= 1280 && r.height >= 720)
            .OrderByDescending(r => r.width * r.height)
            .ThenByDescending(r => r.refreshRateRatio.value)
            .ToList();

        int currentResolutionIndex = 0;
        Resolution recommended = Screen.currentResolution;

        dropDownResolutions.GetComponent<TMP_Dropdown>().ClearOptions();

        List<string> options = new();

        for (int i = 0; i < resolutionsPC.Count; i++)
        {
            Resolution res = resolutionsPC[i];

            string option = $"{res.width}x{res.height} {res.refreshRateRatio.value:F2}Hz";

            options.Add(option);

            if (index < 0)
            {
                if (res.width == recommended.width && res.height == recommended.height && Mathf.Approximately((float)res.refreshRateRatio.value, (float)recommended.refreshRateRatio.value))
                {
                    currentResolutionIndex = i;
                }
            }
            else if (i == index) currentResolutionIndex = i;
        }

        dropDownResolutions.GetComponent<TMP_Dropdown>().AddOptions(options);
        dropDownResolutions.GetComponent<TMP_Dropdown>().RefreshShownValue();

        return currentResolutionIndex;
    }

    private void LoadResolutions()
    {
        dropDownResolutions.value = GetResolutions(rsoSettingsSaved.Value.resolutionIndex);
    }

    private void LoadFullScreen()
    {
        toggleFullscreen.isOn = rsoSettingsSaved.Value.fullScreen;
    }

    private void LoadQuality()
    {
        dropDownQuality.value = rsoSettingsSaved.Value.qualityIndex;
    }

    private void LoadVolumes()
    {
        for (int i = 0; i < rsoSettingsSaved.Value.listVolumes.Count; i++)
        {
            listSliderVolume[i].value = rsoSettingsSaved.Value.listVolumes[i].volume;
            listTextsVolume[i].text = $"{rsoSettingsSaved.Value.listVolumes[i].volume}%";
        }
    }
}