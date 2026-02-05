using Sirenix.OdinInspector;
using UnityEngine;

public class S_ColliderBoss : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Collider")]
    [SerializeField] private Collider boxCollider;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnStartBossP1 rseOnStartBossP1;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnEndBossP1 rseOnEndBossP1;

    private void OnEnable()
    {
        rseOnStartBossP1.action += ColliderActivated;
        rseOnEndBossP1.action += ColliderDeactivated;
    }

    private void OnDisable()
    {
        rseOnStartBossP1.action -= ColliderActivated;
        rseOnEndBossP1.action -= ColliderDeactivated;
    }

    private void ColliderActivated()
    {
        boxCollider.enabled = true;
    }

    private void ColliderDeactivated()
    {
        boxCollider.enabled = false;
    }
}