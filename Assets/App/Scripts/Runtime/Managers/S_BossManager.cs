using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class S_BossManager : MonoBehaviour
{
    [TabGroup("Settings")]
    [SerializeField] private string sceneToLoadAfterBossP1;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnFinishBossP1 onFinishBossP1;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnLoadScene loadScene;

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
        loadScene.Call(sceneToLoadAfterBossP1);
    }
}