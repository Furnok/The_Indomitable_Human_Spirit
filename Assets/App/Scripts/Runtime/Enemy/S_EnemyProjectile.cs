using Sirenix.OdinInspector;
using UnityEngine;

public class S_EnemyProjectile : MonoBehaviour, I_AttackProvider, I_ReflectableProjectile, I_EnemyTransformProvider
{
    [TabGroup("References")]
    [Title("Filters")]
    [SerializeField, S_TagName] private string tagHurt;

    [TabGroup("References")]
    [Title("Masks")]
    [SerializeField] private LayerMask blockLayer;

    [TabGroup("References")]
    [SerializeField] private LayerMask ownLayer;

    [TabGroup("References")]
    [SerializeField, S_LayerName] private int playerLayer;

    [TabGroup("References")]
    [Title("Rigidbody")]
    [SerializeField] private Rigidbody rb;

    [TabGroup("References")]
    [Title("Materials")]
    [SerializeField] private Material enemyMat;

    [TabGroup("References")]
    [SerializeField] private Material playerMat;

    [TabGroup("References")]
    [Title("Renderer")]
    [SerializeField] private Renderer rendered;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_ProjectileData ssoProjectileData;

    private Transform owner = null;

    private float timeAlive = 0f;

    private Transform target = null;

    private bool isInitialized = false;

    private Vector3 direction = Vector3.zero;
    private S_StructAttackData attackData;
    private Vector3 startPos = Vector3.zero;
    private Vector3 controlPoint = Vector3.zero;
    private Vector3 origin = Vector3.zero;
    private Transform startAimPoint = null;

    private float arcHeightMultiplier => ssoProjectileData.Value.arcHeightMultiplier;
    private float arcDirection => ssoProjectileData.Value.arcDirection;
    private bool randomizeArc => ssoProjectileData.Value.randomizeArc;
    private float arcRandomDirectionMin => ssoProjectileData.Value.arcRandomDirectionMin;
    private float arcRandomDirectionMax => ssoProjectileData.Value.arcRandomDirectionMax;
    private float travelTime;

    public void Initialize(Transform owner, Transform target = null, S_StructAttackData attackData = new())
    {
        this.target = target;

        if (this.target == null)
        {
            Destroy(gameObject);
            return;
        }

        origin = this.target.position;
        this.direction = transform.forward;
        this.attackData = attackData;
        isInitialized = true;
        this.owner = owner;

        owner.gameObject.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
        startAimPoint = aimPointProvider != null ? aimPointProvider.GetAimPoint() : null;

        travelTime = ssoProjectileData.Value.travelTime;
        this.startPos = transform.position;

        Vector3 toTarget = target != null ? (target.position - startPos) : transform.forward * 10f;

        float dist = toTarget.magnitude;

        float baseTravel = ssoProjectileData.Value.travelTime;
        float distRef = 12f;
        float scaledTravel = baseTravel * (dist / distRef);
        scaledTravel = Mathf.Clamp(scaledTravel, 0.12f, baseTravel);
        travelTime = scaledTravel;

        CalculateControlPoint();
    }

    public Transform GetEnemyTransform()
    {
        return owner;
    }

    private void Update()
    {
        if (!isInitialized) return;

        timeAlive += Time.deltaTime;
        float t = timeAlive / travelTime;

        if (timeAlive >= ssoProjectileData.Value.lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 endPos = Vector3.zero;

        if (owner != target && startAimPoint != target)
        {
            endPos = target != null ? origin : startPos + transform.forward * 10f;
        }
        else
        {
            endPos = target != null ? target.position : startPos + transform.forward * 10f;
        }

        Vector3 a = Vector3.Lerp(startPos, controlPoint, Mathf.Clamp01(t));
        Vector3 b = Vector3.Lerp(controlPoint, endPos, Mathf.Clamp01(t));
        Vector3 newPos = Vector3.Lerp(a, b, Mathf.Clamp01(t));
        Vector3 tangent = (b - a).normalized;

        if (t <= 1f)
        {
            transform.position = newPos;
            transform.forward = tangent;
            direction = tangent;
        }
        else
        {
            if (Physics.Raycast(transform.position, Vector3.down, 0.01f, blockLayer))
            {
                Destroy(gameObject);
                return;
            }

            float curveSpeed = (b - a).magnitude / (travelTime * 0.5f);
            transform.position += tangent * curveSpeed * Time.deltaTime;
            transform.forward = tangent;
            direction = tangent;
        }
    }

    public void Reflect(Transform reflectOwner)
    {
        attackData.damage *= ssoProjectileData.Value.reflectDmgMul;
        timeAlive = 0;

        gameObject.layer = playerLayer;
        if (rendered && playerMat) rendered.material = playerMat;

        if (owner != null && owner.gameObject.activeInHierarchy)
        {
            owner.gameObject.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
            target = aimPointProvider != null ? aimPointProvider.GetAimPoint() : owner;

            CalculateControlPoint();
        }
        else
        {
            target = null;
            direction = reflectOwner.forward;
            transform.forward = direction;
        }
    }

    public bool CanReflect()
    {
        return true;
    }

    private void CalculateControlPoint()
    {
        this.startPos = transform.position;

        Vector3 toTarget = target != null ? (target.position - startPos) : transform.forward * 10f;
        Vector3 midPoint = startPos + toTarget * 0.5f;

        Vector3 arcDir = Vector3.up;

        if (arcDirection != 0f || randomizeArc == true)
        {
            Vector3 side = Vector3.Cross(Vector3.up, toTarget.normalized).normalized;
            if (randomizeArc)
            {
                side *= Random.Range(arcRandomDirectionMin, arcRandomDirectionMax);
                arcDir = (Vector3.up + side).normalized;

            }
            else arcDir = (Vector3.up + side * arcDirection).normalized;
        }

        float arcHeight = toTarget.magnitude * 0.25f * arcHeightMultiplier;
        controlPoint = midPoint + arcDir * arcHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagHurt) && other.gameObject.layer != ownLayer && other.TryGetComponent(out I_Damageable damageable))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(attackData.damage);
                Destroy(gameObject);
            }
        }
    }

    public ref S_StructAttackData GetAttackData()
    {
        return ref attackData;
    }
}