using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S_UIMainMenu : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Save")]
    [SerializeField, S_SaveName] private string saveName;

    [TabGroup("References")]
    [Title("Levels")]
    [SerializeField] private S_SceneReference levelName;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [Title("Buttons")]
    [SerializeField] private Button buttonStart;

    [TabGroup("References")]
    [SerializeField] private Button buttonContinue;

    [TabGroup("References")]
    [SerializeField] private Button buttonSettings;

    [TabGroup("References")]
    [Title("Windows")]
    [SerializeField] private GameObject settingsWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject creditsWindow;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDataTemp rseOnDataTemp;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCameraIntro rseOnCameraIntro;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnOpenWindow rseOnOpenWindow;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCloseAllWindows rseOnCloseAllWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnQuitGame rseOnQuitGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnFadeIn rseOnFadeIn;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnFadeOut rseOnFadeOut;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnHideMouseCursor rseOnHideMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDataLoad rseOnDataLoad;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDisplayUIGame rseOnDisplayUIGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLoadData rseOnLoadData;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLoadScene rseOnLoadScene;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnSkipIntro rseOnSkipIntro;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnTPCam rseOnTPCam;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InGame rsoInGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_DataTempSaved rsoDataTempSaved;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PlayerRespawnPosition rsoPlayerRespawnPosition;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_FadeTime ssoFadeTime;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_UnDisplay ssoUnDisplay;

    private bool isTransit = true;

    private void OnEnable()
    {
        rseOnDataTemp.action += SetupMenu;

        isTransit = false;
        rsoPlayerRespawnPosition.Value = new();
    }

    private void OnDisable()
    {
        rseOnDataTemp.action -= SetupMenu;

        isTransit = false;
    }

    private void SetupMenu()
    {
        if (rsoDataTempSaved.Value.haveSave)
        {
            buttonContinue.gameObject.SetActive(true);

            Navigation nav = buttonStart.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = buttonContinue;

            buttonStart.navigation = nav;

            Navigation nav2 = buttonSettings.navigation;
            nav2.mode = Navigation.Mode.Explicit;

            nav2.selectOnUp = buttonContinue;

            buttonSettings.navigation = nav2;
        }
    }

    public void StartGame()
    {
        if (!isTransit)
        {
            isTransit = true;

            RuntimeManager.PlayOneShot(uiSound);

            rseOnHideMouseCursor.Call();

            rseOnCloseAllWindows.Call();

            CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();
            cg.DOKill();

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                gameObject.SetActive(false);
                rsoNavigation.Value.selectableFocus = null;

                rseOnCameraIntro.Call();
                rsoInGame.Value = true;
            });
        }
    }

    public void ContinueGame()
    {
        if (!isTransit)
        {
            isTransit = true;

            rseOnHideMouseCursor.Call();

            rseOnCloseAllWindows.Call();

            rseOnLoadData.Call(saveName, false);

            rseOnFadeOut.Call();

            StartCoroutine(S_Utils.DelayRealTime(ssoFadeTime.Value, () =>
            {
                rseOnDataLoad.Call();
                rseOnDisplayUIGame.Call(true);
                rseOnGameInputEnabled.Call();
                rseOnSkipIntro.Call();
                rsoInGame.Value = true;
                rseOnTPCam.Call();

                StartCoroutine(S_Utils.DelayRealTime(0.8f, () => 
                {
                    gameObject.SetActive(false);
                    rsoNavigation.Value.selectableFocus = null;

                    rseOnFadeIn.Call();
                }));
            }));
        }
    }

    public void Settings()
    {
        if (!isTransit)
        {
            rseOnCloseAllWindows.Call();
            rsoNavigation.Value.selectableFocus = null;
            rseOnOpenWindow.Call(settingsWindow);
        }
    }

    public void Credits()
    {
        if (!isTransit)
        {
            rseOnCloseAllWindows.Call();
            rsoNavigation.Value.selectableFocus = null;
            rseOnOpenWindow.Call(creditsWindow);
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

    public void GYM()
    {
        if (!isTransit)
        {
            isTransit = true;

            rseOnHideMouseCursor.Call();

            rseOnFadeOut.Call();

            StartCoroutine(S_Utils.DelayRealTime(ssoFadeTime.Value, () =>
            {
                rseOnCloseAllWindows.Call();
                rsoNavigation.Value.selectableFocus = null;

                rseOnLoadScene.Call(levelName.Name);
            }));
        }
    }
}