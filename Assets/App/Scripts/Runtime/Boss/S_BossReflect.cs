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

            Debug.Log("Boss reflected a projectile!" + reflectable.CanReflect());

            var reflectOwner = body != null ? body.transform : transform;
            reflectable.Reflect(reflectOwner);
        }
    }
}