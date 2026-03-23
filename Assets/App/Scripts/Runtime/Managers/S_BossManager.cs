using Sirenix.OdinInspector;
using UnityEngine;

public class S_BossManager : MonoBehaviour
{
    [TabGroup("Settings")]
    [SerializeField] private string sceneToLoadAfterBossP1;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnFinishBossP1 onFinishBossP1;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnFadeOut rseOnFadeOut;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLoadScene loadScene;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_FadeTime ssoFadeTime;

    private void OnEnable()
    {
        onFinishBossP1.action += OnFinishBossP1;
    }
    private void OnDisable()
    {
        onFinishBossP1.action -= OnFinishBossP1;
    }
    private void OnFinishBossP1()
    {
        rseOnFadeOut.Call();

        StartCoroutine(S_Utils.DelayRealTime(ssoFadeTime.Value, () =>
        {
            loadScene.Call(sceneToLoadAfterBossP1);
        }));
    }
}