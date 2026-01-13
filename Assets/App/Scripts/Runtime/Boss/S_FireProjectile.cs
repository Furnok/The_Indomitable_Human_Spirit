using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class S_FireProjectile : MonoBehaviour, I_AttackProvider
{
    [TabGroup("References")]
    [Title("Filter")]
    [SerializeField, S_TagName] private string tagHurt;

    [TabGroup("Settings")]
    [Title("Parameters")]
    [SerializeField] private float lifeTime;

    [TabGroup("Settings")]
    [SerializeField] private float speed;



    private S_StructEnemyAttackData attackData;
    private bool isInitialized = false;
    private float timeAlive = 0f;

    public void Initialize(Quaternion rotation, S_StructEnemyAttackData attackData = new())
    {
        transform.rotation = rotation;
        this.attackData = attackData;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        timeAlive += Time.deltaTime;

        if (timeAlive >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Fire Projectile Hit 1");
        if (other.CompareTag(tagHurt) && other.TryGetComponent(out I_Damageable damageable))
        {
            Debug.Log("Fire Projectile Hit 2");
            if (damageable != null)
            {
                Debug.Log("Fire Projectile Hit 3");
                damageable.TakeDamage(attackData.damage);
                Destroy(gameObject);
            }
        }
    }

    public ref S_StructEnemyAttackData GetAttackData()
    {
        return ref attackData;
    }
}