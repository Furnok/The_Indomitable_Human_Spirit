using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

public class S_PlayerProjectile : MonoBehaviour
{

    [TabGroup("References")]
    [SerializeField] private Transform _playerProjectile;

    [TabGroup("References")]
    [SerializeField] private MeshRenderer _meshRendererProjectile;

    [TabGroup("References")]
    [SerializeField] ParticleSystem _trailParticle;

    [TabGroup("References")]
    [Title("Filters")]
    [SerializeField, S_TagName] private string tagHurt;

    [TabGroup("References")]
    [Title("Sounds")]
    [SerializeField] private EventReference _projectileTravelSound;

    [TabGroup("References")]
    [SerializeField] private EventReference _projectileImpactSound;

    [TabGroup("Inputs")]
    [SerializeField] private RSE_OnEnemyTargetDied _onEnemyTargetDied;

    [TabGroup("Outputs")]
    [SerializeField] private RSE_OnDespawnProjectile rseOnDespawnProjectile;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerAttackSteps _playerAttackSteps;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerStats _playerStats;

    [TabGroup("Outputs")]
    [SerializeField] private SSO_PlayerConvictionData _PlayerConvictionData;

    private float _lifeTime => _playerStats.Value.projectileLifeTime;

    private float _arcHeightMultiplier => _projectileData.arcHeightMultiplier;
    private float _arcDirection => _projectileData.arcDirection;
    private bool _randomizeArc => _projectileData.randomizeArc;

    private AnimationCurve _arcCurve => _projectileData.speedAnimationCurve;

    private Transform _target = null;
    private float _timeAlive = 0f;
    private float _speed = 0;
    
    private float _damage = 0;
    private float _convictionUsed = 0;
    private Vector3 _direction = Vector3.zero;
    private bool _isInitialized = false;
    private Vector3 _startPos = Vector3.zero;
    private Vector3 _controlPoint = Vector3.zero;

    private S_StructDataProjectile _projectileData;
    private float _arcRandomDirectionMin = 0;
    private float _arcRandomDirectionMax = 0;
    private float _travelTime = 0;

    //private int _attackStep = 0;
    private Material _projectileMat = null;
    EventInstance _travelSoundInstance;

    private void Awake()
    {
        _projectileMat = _meshRendererProjectile.material;
    }

    public void Initialize(float damage, S_StructDataProjectileVisuals visualsData, Transform target = null, int attackStep = 0, float convictionUsed = 0)
    {
        this._target = target;
        this._direction = transform.forward;
        //_attackStep = attackStep;
        _speed = _playerAttackSteps.Value.Find(x => x.step == attackStep).speed;
        _damage = damage;
        _convictionUsed = convictionUsed;
        _projectileData = _playerAttackSteps.Value.Find(x => x.step == attackStep).projectileData;
        _arcRandomDirectionMin = _projectileData.arcRandomDirectionMin;
        _arcRandomDirectionMax = _projectileData.arcRandomDirectionMax;

        // Apply visuals
        _playerProjectile.localScale = Vector3.one * visualsData.ScaleProjectile;
        _projectileMat.color = visualsData.ColorProjectile;

        if (_randomizeArc)
        {
            if (_arcRandomDirectionMax < _arcRandomDirectionMin)
            {
                float temp = _arcRandomDirectionMax;
                _arcRandomDirectionMax = _arcRandomDirectionMin;
                _arcRandomDirectionMin = temp;
            }
        }

        _travelTime = _projectileData.travelTime;
        this._startPos = transform.position;

        Vector3 toTarget = target != null ? (target.position - _startPos) : transform.forward * 10f;

        float dist = toTarget.magnitude;

        float baseTravel = _projectileData.travelTime;
        float distRef = 8f;
        float scaledTravel = baseTravel * (dist / distRef);
        scaledTravel = Mathf.Clamp(scaledTravel, 0.12f, baseTravel);
        _travelTime = scaledTravel;

        Vector3 midPoint = _startPos + toTarget * 0.5f;

        // Default arc direction (top)
        Vector3 arcDir = Vector3.up;

        if (_arcDirection != 0f || _randomizeArc == true)
        {
            Vector3 side = Vector3.Cross(Vector3.up, toTarget.normalized).normalized;
            if (_randomizeArc)
            {
                side *= Random.Range(_arcRandomDirectionMin, _arcRandomDirectionMax);
                arcDir = (Vector3.up + side).normalized;
            }
            else arcDir = (Vector3.up + side * _arcDirection).normalized;
        }

        float arcHeight = toTarget.magnitude * 0.25f * _arcHeightMultiplier;
        _controlPoint = midPoint + arcDir * arcHeight;

        _isInitialized = true;

        var mainModule = _trailParticle.main;
        mainModule.startColor = visualsData.ColorProjectile;

        var shapeModule = _trailParticle.shape;
        shapeModule.scale = _playerProjectile.localScale;

        _trailParticle.Play();

        // Play travel sound
        if (!_projectileTravelSound.IsNull)
        {
            _travelSoundInstance = RuntimeManager.CreateInstance(_projectileTravelSound);
            _travelSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            _travelSoundInstance.start();
        }
    }

    private void OnEnable()
    {
        _onEnemyTargetDied.action += OnTargetDie;
    }

    private void OnDisable()
    {
        _isInitialized = false;
        _timeAlive = 0f;
        _target = null;
        _direction = Vector3.zero;

        _onEnemyTargetDied.action -= OnTargetDie;

        StopTravelSound();
    }

    private void Update()
    {
        if (!_isInitialized) return;

        _timeAlive += Time.deltaTime;
        float t01 = _timeAlive / _travelTime;
        t01 = Mathf.Clamp01(t01);

        if (_timeAlive >= _lifeTime)
        {
            rseOnDespawnProjectile.Call(this);

            StopTravelSound();
            return;
        }

        Vector3 endPos = _target != null ? _target.position : _startPos + transform.forward * 10f;

        float easedT = _arcCurve != null ? _arcCurve.Evaluate(t01) : t01;
        // Use easedT for Bézier
        Vector3 a = Vector3.Lerp(_startPos, _controlPoint, easedT);
        Vector3 b = Vector3.Lerp(_controlPoint, endPos, easedT);
        Vector3 newPos = Vector3.Lerp(a, b, easedT);
        Vector3 tangent = (b - a).normalized;

        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            transform.position = newPos;
            transform.forward = tangent;

            _direction = tangent;
        }
        else
        {
            transform.position += _direction * _speed * Time.deltaTime;
            transform.forward = _direction;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagHurt) && other.TryGetComponent(out I_Damageable damageable))
        {
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
                rseOnDespawnProjectile.Call(this);

                StopTravelSound();

                if (!_projectileImpactSound.IsNull)
                {
                    EventInstance impactSound = RuntimeManager.CreateInstance(_projectileImpactSound);
                    impactSound.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
                    impactSound.setParameterByName("ConvictionAccumulated", _convictionUsed / _PlayerConvictionData.Value.maxConviction);
                    impactSound.start();
                }

                Debug.Log($"Hit enemy for {_damage} damage.");
            }
        }
        else
        {
            StopTravelSound();

            rseOnDespawnProjectile.Call(this);
        }
    }

    private void OnTargetDie(GameObject enemyDie)
    {
        if (_target != null && enemyDie == _target.gameObject && enemyDie != null) _target = null;
    }

    void StopTravelSound()
    {
        if (!_projectileTravelSound.IsNull && _travelSoundInstance.isValid())
        {
            _travelSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _travelSoundInstance.release();
        }
    }
}

public struct S_StructDataProjectileVisuals
{
    public float ScaleProjectile;
    public Color ColorProjectile;
}