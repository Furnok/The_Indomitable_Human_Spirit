using Sirenix.OdinInspector;
using UnityEngine;

public class S_BossReflect : MonoBehaviour
{
    [TabGroup("References")]
    [Title("Body")]
    [SerializeField] private GameObject body;

    private void OnTriggerEnter(Collider other)
    {
        // Search if the Object that Enter is Reflectible Projectile
        if(other.TryGetComponent<I_ReflectableProjectile>(out var reflectable))
        {
            if (reflectable.CanReflect() == false) return;

            var reflectOwner = body != null ? body.transform : transform;
            reflectable.Reflect(reflectOwner);
        }
    }
}