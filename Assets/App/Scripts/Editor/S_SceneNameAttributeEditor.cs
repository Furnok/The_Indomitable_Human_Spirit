using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace App.Scripts.Utils
{
    [CustomPropertyDrawer(typeof(S_SceneReference))]
    public class S_SceneNameAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty guidProp = property.FindPropertyRelative("sceneGUID");
            SerializedProperty pathProp = property.FindPropertyRelative("scenePath");

            EditorGUI.BeginProperty(position, label, property);

            // Get all scenes in build
            var buildScenePaths = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            var buildScenes = buildScenePaths
                .Select(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path))
                .Where(scene => scene != null)
                .ToArray();

            // Get current scene from GUID
            string guid = guidProp.stringValue;
            string resolvedPath = AssetDatabase.GUIDToAssetPath(guid);
            SceneAsset currentScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(resolvedPath);

            // Keep scenePath in sync with GUID
            pathProp.stringValue = resolvedPath;

            // Show popup
            int currentIndex = Array.IndexOf(buildScenes, currentScene);
            string[] sceneNames = buildScenes.Select(s => s.name).ToArray();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);

            if (newIndex >= 0 && newIndex < buildScenes.Length)
            {
                SceneAsset selectedScene = buildScenes[newIndex];
                string path = AssetDatabase.GetAssetPath(selectedScene);
                string newGUID = AssetDatabase.AssetPathToGUID(path);

                guidProp.stringValue = newGUID;
                pathProp.stringValue = path; // keep in sync
            }

            EditorGUI.EndProperty();
        }
    }
}