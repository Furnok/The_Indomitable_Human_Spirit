using UnityEditor;
using UnityEngine;
using System.IO;

//[InitializeOnLoad]
public static class S_ProjectInitializer
{
    static S_ProjectInitializer()
    {
        string rootFolder = $"App";

        // List of Subfolders to Create
        string[] folders = new string[]
        {
            rootFolder,

            $"{rootFolder}/Animations",

            $"{rootFolder}/Arts",
            $"{rootFolder}/Arts/Sprites",
            
            $"{rootFolder}/Audio",
            $"{rootFolder}/Audio/Musics",
            $"{rootFolder}/Audio/SFX",
            $"{rootFolder}/Audio/UI",

            $"{rootFolder}/Inputs",

            $"{rootFolder}/Prefabs",
            $"{rootFolder}/Prefabs/UI",
            $"{rootFolder}/Prefabs/Managers",

            $"{rootFolder}/Scenes",
            $"{rootFolder}/Scenes/Tests",

            $"{rootFolder}/Scripts",
            $"{rootFolder}/Scripts/Editor",
            $"{rootFolder}/Scripts/Managers",
            $"{rootFolder}/Scripts/UI",
            $"{rootFolder}/Scripts/Utils",
            $"{rootFolder}/Scripts/Wrapper",
            $"{rootFolder}/Scripts/Wrapper/RSE",
            $"{rootFolder}/Scripts/Wrapper/RSO",
            $"{rootFolder}/Scripts/Wrapper/SSO",

            $"{rootFolder}/SO",
            $"{rootFolder}/SO/RSE",
            $"{rootFolder}/SO/RSO",
            $"{rootFolder}/SO/SSO",

            $"Plugins",

            $"Resources",

            $"ScriptTemplates",

            $"Settings",
        };

		// Check if the Root Folder Already Exists; if not, Create All the Subfolders
		string rootPath = Path.Combine(Application.dataPath, rootFolder);

        if (!Directory.Exists(rootPath))
        {
			Directory.CreateDirectory(rootPath);

			foreach (var folder in folders)
			{
				string folderPath = Path.Combine(Application.dataPath, folder);

				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}
			}

			// Refresh Unity AssetDatabase 
			AssetDatabase.Refresh();
		}
    }
}
