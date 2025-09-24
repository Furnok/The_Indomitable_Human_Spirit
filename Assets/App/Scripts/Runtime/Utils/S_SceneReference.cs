using System;
using UnityEngine;

[Serializable]
public class S_SceneReference
{
    [SerializeField] private string sceneGUID = "";
    [SerializeField] private string scenePath = "";

    public string GUID => sceneGUID;

    public string Path => scenePath;

    public string Name => System.IO.Path.GetFileNameWithoutExtension(scenePath);
}