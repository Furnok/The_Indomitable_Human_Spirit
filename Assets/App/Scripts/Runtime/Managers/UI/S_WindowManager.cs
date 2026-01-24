using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class S_WindowManager : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [Title("Default Window")]
    [SerializeField] private GameObject defaultWindow;

    [TabGroup("References")]
    [Title("Windows")]
    [SerializeField] private GameObject mainMenuWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject menuWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject gameWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject settingsWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject consoleBackgroundWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject fadeWindow;

    [TabGroup("References")]
    [Title("Console")]
    [SerializeField] private TMP_InputField inputField;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnOpenWindow rseOnOpenWindow;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCloseWindow rseOnCloseWindow;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnCloseAllWindows rseOnCloseAllWindows;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerPause rseOnPlayerPause;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnFadeIn rseOnFadeIn;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnFadeOut rsOnFadeOut;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayUIGame rseOnDisplayUIGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnUIInputEnabled rseOnUIInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGamePause rseOnGamePause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnHideMouseCursor rseOnHideMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnResetFocus rseOnResetFocus;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameActionInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLookActivated rseOnLookActivated;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_GameInPause rsoGameInPause;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InGame rsoInGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentWindows rsoCurrentWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InConsole rsoInConsole;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_FadeTime ssoFadeTime;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_Display ssoDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_UnDisplay ssoUnDisplay;

    private void Awake()
    {
        rsoCurrentWindows.Value = new();

        fadeWindow.SetActive(true);
    }

    private void OnEnable()
    {
        rseOnOpenWindow.action += AlreadyOpen;
        rseOnCloseWindow.action += CloseWindow;
        rseOnCloseAllWindows.action += CloseAllWindows;
        rseOnPlayerPause.action += PauseGame;
        rseOnFadeIn.action += FadeIn;
        rsOnFadeOut.action += FadeOut;
        rseOnDisplayUIGame.action += DisplayUIGame;
    }

    private void OnDisable()
    {
        rseOnOpenWindow.action -= AlreadyOpen;
        rseOnCloseWindow.action -= CloseWindow;
        rseOnCloseAllWindows.action -= CloseAllWindows;
        rseOnPlayerPause.action -= PauseGame;
        rseOnFadeIn.action -= FadeIn;
        rsOnFadeOut.action -= FadeOut;
        rseOnDisplayUIGame.action -= DisplayUIGame;
    }

    private void Start()
    {
        StartCoroutine(S_Utils.DelayRealTime(1, () => FadeIn()));

        if (defaultWindow == menuWindow)
        {
            if (Gamepad.current == null) rseOnShowMouseCursor.Call();

            rseOnUIInputEnabled.Call();
            rseOnOpenWindow.Call(menuWindow);
            rseOnGamePause.Call(true);
        }
        else if (defaultWindow == mainMenuWindow)
        {
            if (Gamepad.current == null) rseOnShowMouseCursor.Call();

            rseOnUIInputEnabled.Call();
            rsoInGame.Value = false;
            rseOnDisplayUIGame.Call(false);
            mainMenuWindow.SetActive(true);

            CanvasGroup cg = mainMenuWindow.GetComponent<CanvasGroup>();
            cg.DOKill();

            StartCoroutine(S_Utils.DelayRealTime(ssoFadeTime.Value, () =>
            {
                cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);
            }));
        }
        else
        {
            rsoInGame.Value = true;
            DisplayUIGame(true);
            rseOnGameActionInputEnabled.Call();
            rseOnLookActivated.Call(true);
        }
    }

    #region UI Game
    private void DisplayUIGame(bool value)
    {
        CanvasGroup cg = gameWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (value && !gameWindow.activeInHierarchy)
        {
            gameWindow.gameObject.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);
        }
        else if (!value)
        {
            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
            {
                gameWindow.SetActive(false);
            });
        }
    }
    #endregion

    #region Pause Game
    private void PauseGame()
    {
        if (inputField.isFocused) return;

        if (rsoInConsole.Value)
        {
            if (rsoNavigation.Value.selectablePressOld != null)
            {
                rseOnResetFocus.Call();
                rsoNavigation.Value.selectableDefault = rsoNavigation.Value.selectablePressOld;

                if (Gamepad.current != null)
                {
                    rsoNavigation.Value.selectableFocus = rsoNavigation.Value.selectablePressOld;

                    rsoNavigation.Value.selectableDefault.Select();
                }

                rsoNavigation.Value.selectablePressOld = null;
            }
            else
            {
                rsoNavigation.Value.selectableDefault = null;
                rseOnResetFocus.Call();
            }

            rseOnHideMouseCursor.Call();

            if (rsoGameInPause.Value)
            {
                rseOnUIInputEnabled.Call();

                if (Gamepad.current == null) rseOnShowMouseCursor.Call();
            }
            else rseOnGameInputEnabled.Call();

            CanvasGroup cg = consoleBackgroundWindow.GetComponent<CanvasGroup>();
            cg.DOKill();

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                cg.blocksRaycasts = false;
            });

            StartCoroutine(S_Utils.DelayFrame(() => rsoInConsole.Value = false));

            return;
        }

        if (rsoInGame.Value && rsoCurrentWindows.Value.Count < 1)
        {
            if (!menuWindow.activeInHierarchy)
            {
                RuntimeManager.PlayOneShot(uiSound);

                if (Gamepad.current == null) rseOnShowMouseCursor.Call();

                rseOnUIInputEnabled.Call();
                OpenWindow(menuWindow);
                rseOnGamePause.Call(true);
            }
        }
    }
    #endregion

    #region Window Management
    private void AlreadyOpen(GameObject window)
    {
        if (window != null)
        {
            if (!window.activeInHierarchy) OpenWindow(window);
            else CloseWindow(window);
        }
    }

    private void OpenWindow(GameObject window)
    {
        CanvasGroup cg = window.GetComponent<CanvasGroup>();
        cg.DOKill();

        window.SetActive(true);

        cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);

        rsoCurrentWindows.Value.Add(window);
    }

    private void CloseWindow(GameObject window)
    {
        if (window != null && window.activeInHierarchy)
        {
            CanvasGroup cg = window.GetComponent<CanvasGroup>();
            cg.DOKill();

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                window.SetActive(false);
            });

            rsoCurrentWindows.Value.Remove(window);
        }
    }

    private void CloseAllWindows()
    {
        foreach (var window in rsoCurrentWindows.Value)
        {
            CanvasGroup cg = window.GetComponent<CanvasGroup>();
            cg.DOKill();

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                window.SetActive(false);
            });
        }

        rsoCurrentWindows.Value.Clear();
    }
    #endregion

    #region Fade Management
    private void FadeIn()
    {
        CanvasGroup cg = fadeWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        cg.DOFade(0f, ssoFadeTime.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
        {
            fadeWindow.SetActive(false);
        });
    }

    private void FadeOut()
    {
        CanvasGroup cg = fadeWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        fadeWindow.SetActive(true);

        cg.DOFade(1f, ssoFadeTime.Value).SetEase(Ease.Linear).SetUpdate(true);
    }
    #endregion
}