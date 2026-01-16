using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_SceneManagement : MonoBehaviour
{
    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnLoadScene rseOnLoadScene;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnQuitGame rseOnQuitGame;

    private bool isLoading = false;

    private void OnEnable()
    {
        rseOnLoadScene.action += LoadLevel;
        rseOnQuitGame.action += QuitGame;
    }

    private void OnDisable()
    {
        rseOnLoadScene.action -= LoadLevel;
        rseOnQuitGame.action -= QuitGame;
    }

    private void LoadLevel(string sceneName)
    {
        if (isLoading) return;

        isLoading = true;

        Transition(sceneName);
    }

    private void Transition(string sceneName)
    {
        StartCoroutine(S_Utils.LoadSceneAsync(sceneName, LoadSceneMode.Single, () =>
        {
            isLoading = false;
        }));
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}