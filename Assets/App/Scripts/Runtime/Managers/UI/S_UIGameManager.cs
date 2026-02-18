using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class S_UIGameManager : MonoBehaviour
{
    [TabGroup("Settings")]
    [Title("Game Over")]
    [SerializeField] private bool haveGameOver;

    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSound;

    [TabGroup("References")]
    [Title("Sliders")]
    [SerializeField] private Slider sliderBossHealth;

    [TabGroup("References")]
    [SerializeField] private Slider sliderHealth;

    [TabGroup("References")]
    [SerializeField] private Image sliderConviction;

    [TabGroup("References")]
    [SerializeField] private Slider sliderPlayerAttackSteps;

    [TabGroup("References")]
    [Title("Skip")]
    [SerializeField] private GameObject skipWindow;

    [TabGroup("References")]
    [SerializeField] private Image imageSkip;

    [TabGroup("References")]
    [Title("Rect Transform Conviction")]
    [SerializeField] private RectTransform sliderFillAreaRectTransform;

    [TabGroup("References")]
    [SerializeField] private RectTransform ticksParent;

    [TabGroup("References")]
    [Title("Prefabs")]
    [SerializeField] private GameObject tickPrefab;

    [TabGroup("References")]
    [Title("Extract")]
    [SerializeField] private GameObject extractWindow;

    [TabGroup("References")]
    [Title("Console")]
    [SerializeField] private GameObject consoleWindow;

    [TabGroup("References")]
    [SerializeField] private GameObject consoleBackgroundWindow;

    [TabGroup("References")]
    [SerializeField] private Selectable buttonSend;

    [TabGroup("References")]
    [SerializeField] private TMP_InputField inputField;

    [TabGroup("References")]
    [Title("Game Over")]
    [SerializeField] private GameObject gameWinWindow;

    [TabGroup("References")]
    [Title("Game Over")]
    [SerializeField] private GameObject gameOverWindow;

    [TabGroup("References")]
    [Title("Dialogue")]
    [SerializeField] private GameObject dialogueWindow;

    [TabGroup("References")]
    [SerializeField] private TextMeshProUGUI textDialogue;

    [TabGroup("References")]
    [Title("Save")]
    [SerializeField] private GameObject saveWindow;

    [TabGroup("References")]
    [Title("Error")]
    [SerializeField] private GameObject errorWindow;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayBossHealth rseOnDisplayBossHealth;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnBossHealthSetup rseOnBossHealthSetup;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnUpdateBossHealth rseOnUpdateBossHealth;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerHealthUpdate rseOnPlayerHealthUpdate;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerConvictionUpdate rseOnPlayerConvictionUpdate;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplaySkip rseOnDisplaySkip;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSkipHold rseOnSkipHold;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnOpenExtractWindow rseOnOpenExtractWindow;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDialogueDisplay rseOnDialogueDisplay;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnConsole rseOnConsole;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerDeath rseOnPlayerDeath;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnSaveDisplay rseOnSaveDisplay;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnFinishBossP1 rseOnFinishBossP1;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnDisplayError rseOnDisplayError;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnUIInputEnabled rseOnUIInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGameInputEnabled rseOnGameInputEnabled;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDisplayExtract rseOnDisplayExtract;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnOpenWindow rseOnOpenWindow;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnHideMouseCursor rseOnHideMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnResetFocus rseOnResetFocus;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnGamePause rseOnGamePause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnPlayerRespawn rseOnPlayerRespawn;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_PreconsumedConviction rsoPreconsumedConviction;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_ConsoleDisplay rsoConsoleDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InConsole rsoInConsole;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InGame rsoInGame;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_SettingsSaved rsoSettingsSaved;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats ssoPlayerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData ssoPlayerConvictionData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerAttackSteps ssoPlayerAttackSteps;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_Extract ssoExtractText;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_CameraData ssoCameraData;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_Display ssoDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_UnDisplay ssoUnDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_AnimationSlider ssoAnimationSlider;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_GameOver ssoGameOver;

    private Tween healthTween = null;
    private Tween convictionTween = null;
    private Tween preconvictionTween = null;
    private Tween skipTween = null;
    private Tween bossHealthTween = null;

    private Coroutine resetCoroutine = null;
    private Coroutine resetSaveCoroutine = null;
    private Coroutine errorCoroutine = null;

    private Material materialConviction = null;

    private void Awake()
    {
        sliderHealth.maxValue = ssoPlayerStats.Value.maxHealth;

        materialConviction = new Material(sliderConviction.material);
        sliderConviction.material = materialConviction;

        materialConviction.SetFloat("_FillAmount", ssoPlayerConvictionData.Value.maxConviction / ssoPlayerConvictionData.Value.maxConviction);

        sliderPlayerAttackSteps.maxValue = ssoPlayerConvictionData.Value.maxConviction;

        StartCoroutine(BuildTicksNextFrame());
    }

    private void OnEnable()
    {
        rseOnBossHealthSetup.action += SetBossHealthSetup;
        rseOnDisplayBossHealth.action += DisplayBossHealth;
        rseOnUpdateBossHealth.action += SetBossHealthSliderValue;
        rseOnPlayerHealthUpdate.action += SetHealthSliderValue;
        rseOnPlayerConvictionUpdate.action += SetConvictionSliderValue;
        rsoPreconsumedConviction.onValueChanged += SetPreconvictionSliderValue;
        rseOnDisplaySkip.action += DisplaySkip;
        rseOnSkipHold.action += SetSkipHoldValue;
        rseOnOpenExtractWindow.action += DiplayExtract;
        rseOnConsole.action += Console;
        rseOnPlayerDeath.action += GameOver;
        rseOnDialogueDisplay.action += DisplayDialogue;
        rseOnSaveDisplay.action += DisplaySave;
        rseOnFinishBossP1.action += GameWin;
        rseOnDisplayError.action += DisplayError;
    }

    private void OnDisable()
    {
        rseOnBossHealthSetup.action -= SetBossHealthSetup;
        rseOnDisplayBossHealth.action -= DisplayBossHealth;
        rseOnUpdateBossHealth.action -= SetBossHealthSliderValue;
        rseOnPlayerHealthUpdate.action -= SetHealthSliderValue;
        rseOnPlayerConvictionUpdate.action -= SetConvictionSliderValue;
        rsoPreconsumedConviction.onValueChanged -= SetPreconvictionSliderValue;
        rseOnDisplaySkip.action -= DisplaySkip;
        rseOnSkipHold.action -= SetSkipHoldValue;
        rseOnOpenExtractWindow.action -= DiplayExtract;
        rseOnConsole.action -= Console;
        rseOnPlayerDeath.action -= GameOver;
        rseOnDialogueDisplay.action -= DisplayDialogue;
        rseOnSaveDisplay.action -= DisplaySave;
        rseOnFinishBossP1.action -= GameWin;
        rseOnDisplayError.action -= DisplayError;

        healthTween?.Kill();
        convictionTween?.Kill();
        preconvictionTween?.Kill();
        bossHealthTween?.Kill();
    }

    #region UI Game
    private void DisplayBossHealth(bool value)
    {
        CanvasGroup cg = sliderBossHealth.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (value && !sliderBossHealth.gameObject.activeInHierarchy)
        {
            sliderBossHealth.gameObject.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);
        }
        else if (!value)
        {
            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
            {
                sliderBossHealth.gameObject.SetActive(false);
            });
        }
    }

    private void SetBossHealthSetup(float health)
    {
        sliderBossHealth.maxValue = health;
        sliderBossHealth.value = health;
    }

    private void SetBossHealthSliderValue(float health)
    {
        bossHealthTween?.Kill();

        bossHealthTween = sliderBossHealth.DOValue(health, ssoAnimationSlider.Value).SetEase(Ease.OutCubic);
    }

    private void SetHealthSliderValue(float health)
    {
        healthTween?.Kill();

        healthTween = sliderHealth.DOValue(health, ssoAnimationSlider.Value).SetEase(Ease.OutCubic);
    }

    private void SetConvictionSliderValue(float conviction)
    {
        convictionTween?.Kill();

        materialConviction.SetFloat("_FillAmount", conviction / ssoPlayerConvictionData.Value.maxConviction);
    }

    private void SetPreconvictionSliderValue(float preconvition)
    {
        preconvictionTween?.Kill();

        preconvictionTween = sliderPlayerAttackSteps.DOValue(preconvition, ssoAnimationSlider.Value).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    private IEnumerator BuildTicksNextFrame()
    {
        yield return null;

        CreateStepTicks();
    }

    private void CreateStepTicks()
    {
        for (int i = ticksParent.childCount - 1; i >= 0; i--) Destroy(ticksParent.GetChild(i).gameObject);

        float maxConv = ssoPlayerConvictionData.Value.maxConviction;

        var fillRect = sliderFillAreaRectTransform;

        if (ticksParent != fillRect)
        {
            ticksParent.SetParent(fillRect, worldPositionStays: false);
            ticksParent.anchorMin = new Vector2(0, 0.5f);
            ticksParent.anchorMax = new Vector2(1, 0.5f);
            ticksParent.pivot = new Vector2(0.5f, 0.5f);
            ticksParent.offsetMin = Vector2.zero;
            ticksParent.offsetMax = Vector2.zero;
            ticksParent.anchoredPosition = Vector2.zero;
        }

        foreach (var step in ssoPlayerAttackSteps.Value)
        {
            float normalized = Mathf.Clamp01(step.ammountConvitionNeeded / maxConv);

            var tickGO = Instantiate(tickPrefab, ticksParent);
            var rt = (RectTransform)tickGO.transform;

            rt.anchorMin = new Vector2(normalized, 0.5f);
            rt.anchorMax = new Vector2(normalized, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
    }
    #endregion

    #region Skip
    private void DisplaySkip(bool value)
    {
        CanvasGroup cg = skipWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (value && !skipWindow.activeInHierarchy)
        {
            skipWindow.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);
        }
        else if (!value) skipWindow.SetActive(false);
    }

    private void SetSkipHoldValue(float value)
    {
        skipTween?.Kill();

        skipTween = imageSkip.DOFillAmount(value / ssoCameraData.Value.holdSkipTime, ssoAnimationSlider.Value).SetEase(Ease.OutCubic);
    }
    #endregion

    #region Extract
    private void DiplayExtract(int index)
    {
        RuntimeManager.PlayOneShot(uiSound);

        rseOnUIInputEnabled.Call();
        rseOnOpenWindow.Call(extractWindow);
        rseOnDisplayExtract.Call(ssoExtractText.Value[index]);
    }
    #endregion

    #region Console
    private void Console()
    {
        if (rsoSettingsSaved.Value.devMode)
        {
            if (!rsoConsoleDisplay.Value)
            {
                rsoNavigation.Value.selectablePressOld = rsoNavigation.Value.selectableFocus;
                rsoNavigation.Value.selectableDefault = null;
                rseOnResetFocus.Call();

                if (Gamepad.current == null) rseOnShowMouseCursor.Call();

                RuntimeManager.PlayOneShot(uiSound);

                rsoConsoleDisplay.Value = true;
                rsoInConsole.Value = true;

                rseOnUIInputEnabled.Call();

                CanvasGroup cg = consoleWindow.GetComponent<CanvasGroup>();
                cg.DOKill();

                consoleWindow.SetActive(true);

                cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);
            }
            else
            {
                if (!rsoInConsole.Value)
                {
                    rsoNavigation.Value.selectablePressOld = rsoNavigation.Value.selectableFocus;
                    rsoNavigation.Value.selectableDefault = null;
                    rseOnResetFocus.Call();

                    rsoNavigation.Value.selectableDefault = buttonSend;

                    if (Gamepad.current != null) buttonSend?.Select();

                    if (Gamepad.current == null) rseOnShowMouseCursor.Call();

                    rsoInConsole.Value = true;

                    rseOnUIInputEnabled.Call();

                    CanvasGroup cg = consoleBackgroundWindow.GetComponent<CanvasGroup>();
                    cg.DOKill();

                    cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);
                    cg.blocksRaycasts = true;
                }
                else
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

                    inputField.text = "";
                    inputField.caretPosition = 0;

                    RuntimeManager.PlayOneShot(uiSound);

                    CanvasGroup cg = consoleWindow.GetComponent<CanvasGroup>();
                    cg.DOKill();

                    cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
                    {
                        rseOnHideMouseCursor.Call();

                        rsoConsoleDisplay.Value = false;
                        rsoInConsole.Value = false;

                        if (!rsoInGame.Value)
                        {
                            rseOnUIInputEnabled.Call();

                            if (Gamepad.current == null) rseOnShowMouseCursor.Call();
                        }
                        else rseOnGameInputEnabled.Call();

                        consoleWindow.SetActive(false);
                    });
                }
            }
        }
        else if (consoleWindow.activeInHierarchy)
        {
            inputField.text = "";
            inputField.caretPosition = 0;

            RuntimeManager.PlayOneShot(uiSound);

            CanvasGroup cg = consoleWindow.GetComponent<CanvasGroup>();
            cg.DOKill();

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                rseOnHideMouseCursor.Call();

                rsoConsoleDisplay.Value = false;
                rsoInConsole.Value = false;

                if (!rsoInGame.Value)
                {
                    rseOnUIInputEnabled.Call();

                    if (Gamepad.current == null) rseOnShowMouseCursor.Call();
                }
                else rseOnGameInputEnabled.Call();

                consoleWindow.SetActive(false);
            });
        }
    }
    #endregion

    #region Game Over
    private void GameWin()
    {
        StartCoroutine(S_Utils.Delay(ssoDisplay.Value, () =>
        {
            rseOnUIInputEnabled.Call();
            rseOnGamePause.Call(true);

            CanvasGroup cg = gameWinWindow.GetComponent<CanvasGroup>();
            cg.DOKill();

            gameWinWindow.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);
        }));
    }

    private void GameOver()
    {
        StartCoroutine(S_Utils.Delay(ssoGameOver.Value, () =>
        {
            if (haveGameOver)
            {
                rseOnUIInputEnabled.Call();
                rseOnGamePause.Call(true);

                CanvasGroup cg = gameOverWindow.GetComponent<CanvasGroup>();
                cg.DOKill();

                gameOverWindow.SetActive(true);

                cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);
            }
            else rseOnPlayerRespawn.Call();
        }));
    }
    #endregion

    #region Dialogue
    private void DisplayDialogue(S_ClassDialogue dialogue)
    {
        CanvasGroup cg = dialogueWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (dialogueWindow.activeInHierarchy)
        {
            cg.DOFade(0f, 0).SetEase(Ease.Linear);
            dialogueWindow.gameObject.SetActive(false);
        }

        dialogueWindow.gameObject.SetActive(true);

        cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);

        textDialogue.text = dialogue.text;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        resetCoroutine = StartCoroutine(ResetDiplay(dialogue.duration));
    }

    private IEnumerator ResetDiplay(float duration)
    {
        yield return new WaitForSeconds(duration);

        CanvasGroup cg = dialogueWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        cg.DOFade(0f, ssoDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
        {
            dialogueWindow.gameObject.SetActive(false);
        });
    }
    #endregion

    #region Save
    private void DisplaySave()
    {
        CanvasGroup cg = saveWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (saveWindow.activeInHierarchy)
        {
            cg.DOFade(0f, 0).SetEase(Ease.Linear);
            saveWindow.gameObject.SetActive(false);
        }

        saveWindow.gameObject.SetActive(true);

        cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear);

        if (resetSaveCoroutine != null)
        {
            StopCoroutine(resetSaveCoroutine);
            resetSaveCoroutine = null;
        }

        resetSaveCoroutine = StartCoroutine(ResetDisplaySave());
    }

    private IEnumerator ResetDisplaySave()
    {
        yield return new WaitForSeconds(2);

        CanvasGroup cg = saveWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        cg.DOFade(0f, ssoDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
        {
            saveWindow.gameObject.SetActive(false);
        });
    }
    #endregion

    private void DisplayError()
    {
        CanvasGroup cg = errorWindow.GetComponent<CanvasGroup>();
        cg.DOKill();

        if (errorCoroutine != null)
        {
            StopCoroutine(errorCoroutine);
            errorCoroutine = null;
        }

        errorWindow.gameObject.SetActive(false);
        cg.alpha = 0;

        cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear)
        .OnStart(() =>
        {
            errorWindow.gameObject.SetActive(true);
        })
        .OnComplete(() =>
        {
            errorCoroutine = StartCoroutine(TimeError(cg));
        });   
    }

    private IEnumerator TimeError(CanvasGroup cg)
    {
        yield return new WaitForSeconds(2);

        cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).OnComplete(() =>
        {
            sliderBossHealth.gameObject.SetActive(false);
        });
    }
}