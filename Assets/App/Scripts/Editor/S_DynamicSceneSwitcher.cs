using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;

public class S_DynamicSceneSwitcher : EditorWindow
{
    private Vector2 scrollPosition = Vector2.zero;
    private string[] scenePaths = new string[0];
    private string searchQuery = "";

    private GUIStyle searchFieldStyle = null;
    private GUIStyle buttonStyle = null;
    private GUIStyle buttonRefreshStyle = null;
    private float lastWidth = 0;

    [MenuItem("Tools/Dynamic Scenes Switcher")]
    public static void ShowWindow()
    {
        var window = GetWindow<S_DynamicSceneSwitcher>("Dynamic Scene Switcher");
        window.minSize = new Vector2(300, 300);
        window.maxSize = new Vector2(1000, 1000);
        window.position = new Rect(100, 100, 400, 400);
    }

    private void OnEnable()
    {
        RefreshSceneList();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void RefreshSceneList()
    {
        scenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.StartsWith("Assets/"))
            .ToArray();
    }

    private void SetupStyles()
    {
        if (Mathf.Approximately(lastWidth, position.width)) return;

        lastWidth = position.width;

        float scale = Mathf.Clamp(position.width / 400f, 0.75f, 2f);

        searchFieldStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = 14,
            padding = new RectOffset(6, 6, 4, 4),
            fixedHeight = 25
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            margin = new RectOffset(20, 20, 5, 5),
            padding = new RectOffset(10, 10, 8, 8),
            fontSize = Mathf.RoundToInt(14 * scale),
            alignment = TextAnchor.MiddleCenter
        };

        buttonRefreshStyle = new GUIStyle(GUI.skin.button)
        {
            margin = new RectOffset(5, 5, 5, 5),
            padding = new RectOffset(15, 15, 10, 10),
            fontSize = Mathf.RoundToInt(16 * scale),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter
        };
    }

    private void OnGUI()
    {
        SetupStyles();

        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Search Scenes:", EditorStyles.boldLabel);

        EditorGUILayout.Space(5);

        searchQuery = EditorGUILayout.TextField(searchQuery, searchFieldStyle);

        EditorGUILayout.Space(15);

        if (scenePaths.Length == 0)
        {
            EditorGUILayout.LabelField("No Scenes Found!", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            foreach (string scenePath in scenePaths.Where(path =>
                string.IsNullOrEmpty(searchQuery) ||
                Path.GetFileNameWithoutExtension(path).ToLower().Contains(searchQuery.ToLower())))
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                if (GUILayout.Button(sceneName, buttonStyle) &&
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(scenePath);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Refresh Scenes", buttonRefreshStyle))
        {
            RefreshSceneList();
            Repaint();
        }
    }
}