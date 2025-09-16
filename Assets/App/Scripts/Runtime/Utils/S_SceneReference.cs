using System;
using System.IO;
using UnityEngine;

[Serializable]
public class S_SceneReference
{
    [SerializeField] private string sceneGUID = "";

    public string Name
    {
        get
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(sceneGUID);
            return Path.GetFileNameWithoutExtension(path);
        }
    }

    public string GUID => sceneGUID;
}