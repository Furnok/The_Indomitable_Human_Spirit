using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class S_DataManagement : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Saves")]
    [SerializeField, S_SaveName] private string saveSettingsName;

    [TabGroup("References")]
    [SerializeField, S_SaveName] private string saveNames;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnLoadData rseOnLoadData;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSaveData rseOnSaveData;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDeleteData rseOnDeleteData;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDataTemp rseOnDataTemp;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_SettingsSaved rsoSettingsSaved;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_DataSaved rsoDataSaved;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_DataTempSaved rsoDataTempSaved;

    private static readonly string EncryptionKey = "ajekoBnPxI9jGbnYCOyvE9alNy9mM/Kw";
    private static readonly string SaveDirectory = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Saves");
    private static readonly bool fileCrypted = true;

    private Bus audioMaster;
    private Bus audioMusic;
    private Bus audioSounds;
    private Bus audioUI;

    private void Awake()
    {
        Application.targetFrameRate = 120;

        audioMaster = RuntimeManager.GetBus("bus:/");
        audioMusic = RuntimeManager.GetBus("bus:/Music");
        audioSounds = RuntimeManager.GetBus("bus:/Sounds");
        audioUI = RuntimeManager.GetBus("bus:/UI");

        rsoSettingsSaved.Value = new();
        rsoDataSaved.Value = new();
        rsoDataTempSaved.Value = new();
    }

    private void OnEnable()
    {
        rseOnSaveData.action += SaveToJson;
        rseOnLoadData.action += LoadFromJson;
        rseOnDeleteData.action += DeleteData;
    }

    private void OnDisable()
    {
        rseOnSaveData.action -= SaveToJson;
        rseOnLoadData.action -= LoadFromJson;
        rseOnDeleteData.action -= DeleteData;

        rsoSettingsSaved.Value = null;
        rsoDataSaved.Value = null;
        rsoDataTempSaved.Value = null;
    }

    private void Start()
    {
        Directory.CreateDirectory(SaveDirectory);

        if (saveSettingsName != null)
        {
            if (FileAlreadyExist(saveSettingsName)) LoadFromJson(saveSettingsName, true);
            else SaveToJson(saveSettingsName, true);
        }

        if (FileAlreadyExist(saveNames)) LoadTempFromJson(saveNames);
    }

    #region Encryption
    private static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
        aes.GenerateIV();

        using MemoryStream memoryStream = new();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (CryptoStream cryptoStream = new(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (StreamWriter writer = new(cryptoStream)) writer.Write(plainText);

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static string Decrypt(string cipherText)
    {
        try
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.IV = buffer[..(aes.BlockSize / 8)];

            using MemoryStream memoryStream = new(buffer[(aes.BlockSize / 8)..]);
            using CryptoStream cryptoStream = new(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader reader = new(cryptoStream);

            return reader.ReadToEnd();
        }
        catch
        {
            throw;
        }
    }
    #endregion

    #region File Management
    private string GetFilePath(string name)
    {
        return Path.Combine(SaveDirectory, $"{name}.json");
    }

    private bool FileAlreadyExist(string name)
    {
        return File.Exists(GetFilePath(name));
    }
    #endregion

    #region Save & Load & Delete
    private void SaveToJson(string name, bool isSetting)
    {
        if (name == null) return;

        string filePath = GetFilePath(name);

        string dataToSave = "";

        if (isSetting) dataToSave = JsonUtility.ToJson(rsoSettingsSaved.Value);
        else dataToSave = JsonUtility.ToJson(rsoDataSaved.Value);

        File.WriteAllText(filePath, fileCrypted ? Encrypt(dataToSave) : dataToSave);
    }

    private void LoadFromJson(string name, bool isSettings)
    {
        if (!FileAlreadyExist(name)) return;

        string filePath = GetFilePath(name);
        string jsonContent;

        try
        {
            jsonContent = File.ReadAllText(filePath);

            if (fileCrypted) jsonContent = Decrypt(jsonContent);

            if (string.IsNullOrWhiteSpace(jsonContent)) throw new Exception(); 
        }
        catch
        {
            SaveToJson(name, isSettings);
            return;
        }

        try
        {
            if (isSettings)
            {
                rsoSettingsSaved.Value = JsonUtility.FromJson<S_ClassSettingsSaved>(jsonContent);

                if (rsoSettingsSaved.Value == null) throw new Exception();

                StartCoroutine(S_Utils.DelayFrame(() => LoadSettings()));
            }
            else
            {
                rsoDataSaved.Value = JsonUtility.FromJson<S_ClassDataSaved>(jsonContent);

                if (rsoDataSaved.Value == null) throw new Exception();
            }
        }
        catch
        {
            if (isSettings) SaveToJson(name, isSettings);
        }
    }

    private void LoadTempFromJson(string name)
    {
        if (!FileAlreadyExist(name)) return;

        string filePath = GetFilePath(name);
        string jsonContent;

        try
        {
            jsonContent = File.ReadAllText(filePath);

            if (fileCrypted) jsonContent = Decrypt(jsonContent);

            if (string.IsNullOrWhiteSpace(jsonContent)) throw new Exception();
        }
        catch
        {
            return;
        }

        try
        {
            if (JsonUtility.FromJson<S_ClassDataSaved>(jsonContent) == null) throw new Exception();
            else
            {
                rsoDataTempSaved.Value.haveSave = true;

                StartCoroutine(S_Utils.DelayRealTime(0.1f, () => rseOnDataTemp.Call()));
            }
        }
        catch
        {
            return;
        }
    }

    private void DeleteData(string name)
    {
        if (FileAlreadyExist(name))
        {
            string filePath = GetFilePath(name);

            File.Delete(filePath);
        }
    }
    #endregion

    #region Load Settings
    private Resolution GetResolutions(int index)
    {
        List<Resolution> resolutionsPC = new(Screen.resolutions);
        resolutionsPC.Reverse();

        Resolution resolution = resolutionsPC[0];
        Resolution recommended = Screen.currentResolution;

        for (int i = 0; i < resolutionsPC.Count; i++)
        {
            Resolution res = resolutionsPC[i];

            if (index < 0)
            {
                if (res.width == recommended.width && res.height == recommended.height && Mathf.Approximately((float)res.refreshRateRatio.value, (float)recommended.refreshRateRatio.value))
                {
                    resolution = res;
                }
            }
            else if (i == index) resolution = res;
        }

        return resolution;
    }

    private void LoadSettings()
    {
        StartCoroutine(LangueSetup());

        Resolution resolution = GetResolutions(rsoSettingsSaved.Value.resolutionIndex);

        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);

        if (rsoSettingsSaved.Value.fullScreen) Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else Screen.fullScreenMode = FullScreenMode.Windowed;

        Screen.fullScreen = rsoSettingsSaved.Value.fullScreen;

        QualitySettings.SetQualityLevel(rsoSettingsSaved.Value.qualityIndex, true);

        audioMaster.setVolume(rsoSettingsSaved.Value.listVolumes[0].volume / 100);
        audioMusic.setVolume(rsoSettingsSaved.Value.listVolumes[1].volume / 100);
        audioSounds.setVolume(rsoSettingsSaved.Value.listVolumes[2].volume / 100);
        audioUI.setVolume(rsoSettingsSaved.Value.listVolumes[3].volume / 100);
    }

    private IEnumerator LangueSetup()
    {
        var initOperation = LocalizationSettings.InitializationOperation;
        yield return initOperation;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[rsoSettingsSaved.Value.languageIndex];
    }
    #endregion
}
