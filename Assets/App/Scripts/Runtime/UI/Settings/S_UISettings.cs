using DG.Tweening;
using FMODUnity;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_UISettings : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Audio")]
    [SerializeField] private EventReference uiSoundClick;

    [TabGroup("References")]
    [SerializeField] private EventReference uiSoundClose;

    [TabGroup("References")]
    [Title("Default")]
    [SerializeField] private GameObject defaultPanelSet;

    [TabGroup("References")]
    [Title("Buttons")]
    [SerializeField] private Button buttonGameplay;

    [TabGroup("References")]
    [SerializeField] private Button buttonGraphics;

    [TabGroup("References")]
    [SerializeField] private Button buttonAudio;

    [TabGroup("References")]
    [SerializeField] private Button buttonApply;

    [TabGroup("References")]
    [SerializeField] private Button buttonReset;

    [TabGroup("References")]
    [SerializeField] private Button buttonReturn;

    [TabGroup("References")]
    [Title("Script")]
    [SerializeField] private S_Settings settings;

    [TabGroup("References")]
    [Title("Text")]
    [SerializeField] private TextMeshProUGUI textGameplay;

    [TabGroup("References")]
    [SerializeField] private TextMeshProUGUI textGraphics;

    [TabGroup("References")]
    [SerializeField] private TextMeshProUGUI textAudio;

    [TabGroup("References")]
    [Title("Selectables")]
    [SerializeField] private Selectable dropDownLanguages;

    [TabGroup("References")]
    [SerializeField] private Selectable toggleDevMode;

    [TabGroup("References")]
    [SerializeField] private Selectable dropDownResolutions;

    [TabGroup("References")]
    [SerializeField] private Selectable dropDownQuality;

    [TabGroup("References")]
    [SerializeField] private Selectable sliderMainVolume;

    [TabGroup("References")]
    [SerializeField] private Selectable sliderUIVolume;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnPlayerPause rseOnPlayerPause;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnCloseWindow rseOnCloseWindow;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnShowMouseCursor rseOnShowMouseCursor;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_Navigation rsoNavigation;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_CurrentWindows rsoCurrentWindows;

    [TabGroup("Outputs")]
    [SerializeField] private RSO_InConsole rsoInConsole;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_Display ssoDisplay;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_UnDisplay ssoUnDisplay;

    private GameObject currentPanelSet = null;

    private bool isClosing = false;

    private void OnEnable()
    {
        rseOnPlayerPause.action += CloseEscape;

        if (defaultPanelSet != null)
        {
            textGameplay.color = Color.red;

            CanvasGroup cg = defaultPanelSet.GetComponent<CanvasGroup>();
            cg.DOKill();

            defaultPanelSet.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);

            currentPanelSet = defaultPanelSet;

            Navigation nav = buttonGameplay.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownLanguages;

            buttonGameplay.navigation = nav;

            nav = buttonGraphics.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownLanguages;

            buttonGraphics.navigation = nav;

            nav = buttonAudio.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownLanguages;

            buttonAudio.navigation = nav;

            nav = buttonApply.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = toggleDevMode;
            nav.selectOnDown = buttonGameplay;

            buttonApply.navigation = nav;

            nav = buttonReset.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = toggleDevMode;
            nav.selectOnDown = buttonGameplay;

            buttonReset.navigation = nav;

            nav = buttonReturn.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = toggleDevMode;
            nav.selectOnDown = buttonGameplay;

            buttonReturn.navigation = nav;
        }

        isClosing = false;
    }

    private void OnDisable()
    {
        rseOnPlayerPause.action -= CloseEscape;

        if (currentPanelSet != null)
        {
            currentPanelSet.SetActive(false);
            currentPanelSet = null;
        }

        isClosing = false;
        textGameplay.color = Color.white;
        textGraphics.color = Color.white;
        textAudio.color = Color.white;
    }

    public void OnDropdownClicked(Selectable uiElement)
    {
        if (uiElement.interactable)
        {
            GameObject blocker = transform.root.Find("Blocker")?.gameObject;
            if (blocker != null)
            {
                Button button = blocker.GetComponent<Button>();
                if (button != null) button.onClick.AddListener(CloseDropDown);
            }
        }
    }

    private void CloseDropDown()
    {
        RuntimeManager.PlayOneShot(uiSoundClick);
    }

    private void CloseEscape()
    {
        if (rsoInConsole.Value && dropDownLanguages?.GetComponent<TMP_Dropdown>()?.IsExpanded == true || dropDownResolutions?.GetComponent<TMP_Dropdown>()?.IsExpanded == true)
        {
            return;
        }

        if (!isClosing)
        {
            if (rsoCurrentWindows.Value[^1] == gameObject) Close();
        }
    }

    public void Close()
    {
        if (!isClosing)
        {
            isClosing = true;
            settings.Close();

            RuntimeManager.PlayOneShot(uiSoundClose);

            rseOnCloseWindow.Call(gameObject);

            if (rsoNavigation.Value.selectablePressOldWindow == null) rsoNavigation.Value.selectableFocus = null;
            else
            {
                rsoNavigation.Value.selectablePressOldWindow?.Select();
                rsoNavigation.Value.selectablePressOldWindow = null;
            }
        }
    }

    #region Panels Management
    private void ClosePanel()
    {
        if (currentPanelSet != null)
        {
            textGameplay.color = Color.white;
            textGraphics.color = Color.white;
            textAudio.color = Color.white;

            CanvasGroup cg = currentPanelSet.GetComponent<CanvasGroup>();
            cg.DOKill();

            GameObject oldpanel = currentPanelSet;

            cg.DOFade(0f, ssoUnDisplay.Value).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() =>
            {
                oldpanel.SetActive(false);
            });

            Navigation nav = buttonGameplay.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = buttonApply;

            buttonGameplay.navigation = nav;

            nav = buttonGraphics.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = buttonApply;

            buttonGraphics.navigation = nav;

            nav = buttonAudio.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = buttonApply;

            buttonAudio.navigation = nav;
        }
    }

    public void OpenPanelGameplay(GameObject panelSet)
    {
        if (currentPanelSet != panelSet)
        {
            ClosePanel();

            textGameplay.color = Color.red;

            CanvasGroup cg = panelSet.GetComponent<CanvasGroup>();
            cg.DOKill();

            panelSet.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);

            currentPanelSet = panelSet;

            Navigation nav = buttonGameplay.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownLanguages;

            buttonGameplay.navigation = nav;

            nav = buttonGraphics.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownLanguages;

            buttonGraphics.navigation = nav;

            nav = buttonAudio.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownLanguages;

            buttonAudio.navigation = nav;

            nav = buttonApply.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = toggleDevMode;
            nav.selectOnDown = buttonGameplay;

            buttonApply.navigation = nav;

            nav = buttonReset.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = toggleDevMode;
            nav.selectOnDown = buttonGameplay;

            buttonReset.navigation = nav;

            nav = buttonReturn.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = toggleDevMode;
            nav.selectOnDown = buttonGameplay;

            buttonReturn.navigation = nav;
        }
    }

    public void OpenPanelGraphics(GameObject panelSet)
    {
        if (currentPanelSet != panelSet)
        {
            ClosePanel();

            textGraphics.color = Color.red;

            CanvasGroup cg = panelSet.GetComponent<CanvasGroup>();
            cg.DOKill();

            panelSet.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);

            currentPanelSet = panelSet;

            Navigation nav = buttonGameplay.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownResolutions;

            buttonGameplay.navigation = nav;

            nav = buttonGraphics.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownResolutions;

            buttonGraphics.navigation = nav;

            nav = buttonAudio.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = dropDownResolutions;

            buttonAudio.navigation = nav;

            nav = buttonApply.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = dropDownQuality;
            nav.selectOnDown = buttonGraphics;

            buttonApply.navigation = nav;

            nav = buttonReset.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = dropDownQuality;
            nav.selectOnDown = buttonGraphics;

            buttonReset.navigation = nav;

            nav = buttonReturn.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = dropDownQuality;
            nav.selectOnDown = buttonGraphics;

            buttonReturn.navigation = nav;
        }
    }

    public void OpenPanelAudio(GameObject panelSet)
    {
        if (currentPanelSet != panelSet)
        {
            ClosePanel();

            textAudio.color = Color.red;

            CanvasGroup cg = panelSet.GetComponent<CanvasGroup>();
            cg.DOKill();

            panelSet.SetActive(true);

            cg.DOFade(1f, ssoDisplay.Value).SetEase(Ease.Linear).SetUpdate(true);

            currentPanelSet = panelSet;

            Navigation nav = buttonGameplay.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = sliderMainVolume;

            buttonGameplay.navigation = nav;

            nav = buttonGraphics.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = sliderMainVolume;

            buttonGraphics.navigation = nav;

            nav = buttonAudio.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnDown = sliderMainVolume;

            buttonAudio.navigation = nav;

            nav = buttonApply.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = sliderUIVolume;
            nav.selectOnDown = buttonAudio;

            buttonApply.navigation = nav;

            nav = buttonReset.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = sliderUIVolume;
            nav.selectOnDown = buttonAudio;

            buttonReset.navigation = nav;

            nav = buttonReturn.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = sliderUIVolume;
            nav.selectOnDown = buttonAudio;

            buttonReturn.navigation = nav;
        }
    }
    #endregion
}