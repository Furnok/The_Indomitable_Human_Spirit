using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class S_UIGameWin : MonoBehaviour
{
    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCloseAllWindows rseOnCloseAllWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnQuitGame rseOnQuitGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLoadScene rseOnLoadScene;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGamePause rseOnGamePause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnFadeOut rseOnFadeOut;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnNeedCursor rseOnNeedCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_FadeTime ssoFadeTime;

    private bool isTransit = false;

    private void OnEnable()
    {
        if (Gamepad.current == null) rseOnShowMouseCursor.Call();
        rseOnNeedCursor.Call(true);

        isTransit = false;
    }

    private void OnDisable()
    {
        isTransit = false;
    }

    public void Form()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSfv-M9c5vk5ZcO6x142nddZ2P9dlYln1nQq41wDwrQ_snus3A/viewform");
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

                Scene currentScene = SceneManager.GetActiveScene();
                rseOnLoadScene.Call(currentScene.name);
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