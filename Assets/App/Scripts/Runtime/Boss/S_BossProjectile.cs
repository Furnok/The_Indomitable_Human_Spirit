using Sirenix.OdinInspector;
using UnityEngine;

public class S_BossProjectile : MonoBehaviour, I_AttackProvider, I_ReflectableProjectile, I_EnemyTransformProvider
{
    [TabGroup("References")]
    [Title("Filter")]
    [SerializeField, S_TagName] private string tagHurt;

    [TabGroup("References")]
    [Title("Mask")]
    [SerializeField] private LayerMask blockLayer;

    [TabGroup("References")]
    [SerializeField, S_LayerName] private int playerLayer;

    [TabGroup("References")]
    [SerializeField, S_LayerName] private int enemyLayer;

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

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnEndFly onEndFly;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnBossStun onBossStun;

    private Transform owner = null;
    private Transform player = null;

    private float timeAlive = 0f;
    private int reflectCount = 0;

    private Transform target = null;

    private bool isInitialized = false;

    private Vector3 direction = Vector3.zero;
    private S_StructEnemyAttackData attackData;
    private Vector3 startPos = Vector3.zero;
    private Vector3 controlPoint = Vector3.zero;
    private Vector3 origin = Vector3.zero;
    private Transform startAimPoint = null;
    private bool _canReflect = true;
    private bool bossHit= false;

    private float arcHeightMultiplier => ssoProjectileData.Value.arcHeightMultiplier;
    private float arcDirection => ssoProjectileData.Value.arcDirection;
    private bool randomizeArc => ssoProjectileData.Value.randomizeArc;
    private float arcRandomDirectionMin => ssoProjectileData.Value.arcRandomDirectionMin;
    private float arcRandomDirectionMax => ssoProjectileData.Value.arcRandomDirectionMax;
    private float travelTime => ssoProjectileData.Value.travelTime;

    public void Initialize(Transform owner, Transform target = null, S_StructEnemyAttackData attackData = new())
    {
        this.target = target;
        this.player = target;
        this.direction = transform.forward;
        this.attackData = attackData;
        isInitialized = true;
        this.owner = owner;
        origin = target.position;

        if (ssoProjectileData.Value.reflectMax <= 0)
        {
            _canReflect = false;
        }

        owner.gameObject.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);
        startAimPoint = aimPointProvider != null ? aimPointProvider.GetAimPoint() : null;

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

    public bool CanReflect()
    {
        return _canReflect;
    }

    public void Reflect(Transform reflectOwner)
    {

        attackData.damage *= ssoProjectileData.Value.reflectDmgMul;
        timeAlive = 0;

        if (gameObject.layer == enemyLayer)
        {
            gameObject.layer = playerLayer;

            if (rendered && playerMat) rendered.material = playerMat;
        }
        else
        {
            if (_canReflect == false) return;

            reflectCount++;

            if (reflectCount >= ssoProjectileData.Value.reflectMax) _canReflect = false;

            gameObject.layer = enemyLayer;

            if (rendered && playerMat) rendered.material = enemyMat;
        }


        if (owner != null && owner.gameObject.activeInHierarchy || player != null && player.gameObject.activeInHierarchy)
        {
            Transform targetReflect;

            if (target == owner) targetReflect = player;
            else targetReflect = owner;

            targetReflect.gameObject.TryGetComponent<I_AimPointProvider>(out I_AimPointProvider aimPointProvider);

            target = aimPointProvider != null ? aimPointProvider.GetAimPoint() : targetReflect;
            
            CalculateControlPoint();
        }
        else
        {
            target = null;
            direction = reflectOwner.forward;
            transform.forward = direction;
        }
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
            else
            {
                arcDir = (Vector3.up + side * arcDirection).normalized;
            }
        }

        float arcHeight = toTarget.magnitude * 0.25f * arcHeightMultiplier;
        controlPoint = midPoint + arcDir * arcHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagHurt) && other.TryGetComponent(out I_Damageable damageable))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(attackData.damage);
                bossHit = true;
                Debug.Log("Get Stun");
                onBossStun.Call(S_EnumBossState.Stun);
                Destroy(gameObject);
            }
        }
    }
    private void OnDestroy()
    {
        if(bossHit) return;
        Debug.Log("Fly End");
        onEndFly.Call();
    }
    public ref S_StructEnemyAttackData GetAttackData()
    {
        return ref attackData;
    }
}