using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_UIMenu : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Levels")]
    [SerializeField] private S_SceneReference levelName;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [Title("Windows")]
    [SerializeField] private GameObject settingsWindow;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerPause rseOnPlayerPause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnOpenWindow rseOnOpenWindow;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCloseAllWindows rseOnCloseAllWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnQuitGame rseOnQuitGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLoadScene rseOnLoadScene;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGamePause rseOnGamePause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnResetFocus rseOnResetFocus;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnFadeOut rseOnFadeOut;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnHideMouseCursor rseOnHideMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnNeedCursor rseOnNeedCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InGame rsoInGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentWindows rsoCurrentWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InConsole rsoInConsole;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_GameInPause rsoGameInPause;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_FadeTime ssoFadeTime;

    private bool isTransit = false;

    private void OnEnable()
    {
        rseOnPlayerPause.action += CloseEscape;

        isTransit = false;
    }

    private void OnDisable()
    {
        rseOnPlayerPause.action -= CloseEscape;

        isTransit = false;
    }

    private void CloseEscape()
    {
        if (rsoCurrentWindows.Value.Count > 0 && rsoCurrentWindows.Value[^1] == gameObject && !rsoInConsole.Value) ResumeGame();
    }

    public void ResumeGame()
    {
        if (!isTransit)
        {
            RuntimeManager.PlayOneShot(uiSound);

            rseOnHideMouseCursor.Call();
            rseOnNeedCursor.Call(false);

            rseOnGameInputEnabled.Call();
            rseOnCloseAllWindows.Call();
            rsoNavigation.Value.selectableDefault = null;
            rseOnResetFocus.Call();
            rsoInGame.Value = true;
            rseOnGamePause.Call(false);
        }
    }

    public void Settings()
    {
        if (!isTransit)
        {
            rsoNavigation.Value.selectableFocus = null;
            rseOnOpenWindow.Call(settingsWindow);
        }
    }

    public void MainMenu()
    {
        if (!isTransit)
        {
            isTransit = true;
            rseOnFadeOut.Call();

            StartCoroutine(S_Utils.DelayRealTime(ssoFadeTime.Value, () =>
            {
                rseOnCloseAllWindows.Call();
                rsoNavigation.Value.selectableFocus = null;

                rseOnGamePause.Call(false);

                if (!string.IsNullOrEmpty(levelName.Name))
                {
                    rseOnLoadScene.Call(levelName.Name);
                }
                else
                {
                    Scene currentScene = SceneManager.GetActiveScene();
                    rseOnLoadScene.Call(currentScene.name);
                }
            }));
        }
    }

    public void QuitGame()
    {
        if (!isTransit)
        {
            isTransit = true;

            rseOnFadeOut.Call();

            StartCoroutine(S_Utils.DelayRealTime(ssoFadeTime.Value, () =>
            {
                rseOnQuitGame.Call();
                rsoNavigation.Value.selectableFocus = null;
            }));
        }
    }
}