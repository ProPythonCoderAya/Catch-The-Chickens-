using Globs;
using UnityEngine;
using UnityEngine.AI;

public class ChickenMovementScript : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("speed");
    public Transform player;
    public Transform miniBot;

    [Header("Wander")]
    public float wanderRadius = 10f;
    public float focusWaitTime = 1.0f;
    public float focusAngle = 90f;

    [Header("Flee")]
    public float fleeDistance = 6f;
    public float fleeSpeed = 5f;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 180f;

    [Header("Hearing")]
    public float hearingRadius;

    [Header("Scanning")]
    public float scanDuration = 1.5f;
    public float stuckThreshold = 0.2f;

    // -------- BOIDS --------
    [Header("Boids")]
    public float separationRadius = 2.0f;
    public float separationStrength = 2.5f;

    public float cohesionRadius = 4.0f;
    public float cohesionStrength = 0.6f; // keep LOW

    private enum State
    {
        Wander,
        Flee,
        Scan
    }

    public ChickenSpawnerScript spawner;
    public Animator animatorRef;

    private State _state;

    private NavMeshAgent _agent;
    private PlayerMovementScript _p;
    private Animator _animator;

    private Vector3 _focusPoint;

    private float _focusTimer;
    private float _scanTimer;
    private float _stuckTimer;

    private Vector3 _lastPosition;

    private Vector3 _lastGoodDestination;
    private bool _hasLastDestination;
    private float _fleeStuckTimer;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _p = player ? player.GetComponent<PlayerMovementScript>() : null;
        _animator = GetComponent<Animator>();

        _state = State.Wander;
        _lastPosition = transform.position;
        _animator = animatorRef ? animatorRef : GetComponent<Animator>();
    }

    void Update()
    {
        if (!player || !_agent || !_animator)
            return;

        float distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        float distanceFromMiniBot = miniBot ? Vector3.Distance(transform.position, miniBot.position) : float.MaxValue;
        if (distanceFromPlayer > 25f && distanceFromMiniBot > 25f)
        {
            gameObject.SetActive(false); // wait for chicken spawner to turn on again
            return;
        }
        
        bool seesPlayer = CheckVision();
        bool hearsPlayer = CheckHearing();

        if (seesPlayer || hearsPlayer)
        {
            _state = State.Flee;
        }

        switch (_state)
        {
            case State.Flee:
                Flee();
                break;

            case State.Scan:
                Scan();
                break;

            case State.Wander:
                CheckIfStuck();
                WanderFocus();
                break;
        }
        
        float speed = _agent.velocity.magnitude;

        _animator.SetFloat(Speed, speed);

        if (speed < 0.5f)
        {
            _animator.speed = 1f;
        } else if (speed >= 0.5f)
        {
            _animator.speed = speed * 1.5f;
        }
    }

    void LateUpdate()
    {
        RotateTowardsMovement();
    }

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.ChickenDied(this);
        }
    }

    // ================= BOIDS =================

    Vector3 GetSeparation()
    {
        if (!spawner) return Vector3.zero;

        Vector3 force = Vector3.zero;
        int count = 0;

        foreach (var c in spawner.chickens)
        {
            if (!c || c == this) continue;

            float dist = Vector3.Distance(transform.position, c.transform.position);

            if (dist < separationRadius && dist > 0.01f)
            {
                force += (transform.position - c.transform.position).normalized / dist;
                count++;
            }
        }

        if (count > 0)
            force /= count;

        return force * separationStrength;
    }

    Vector3 GetCohesion()
    {
        if (!spawner) return Vector3.zero;

        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var c in spawner.chickens)
        {
            if (!c || c == this) continue;

            float dist = Vector3.Distance(transform.position, c.transform.position);

            if (dist < cohesionRadius)
            {
                center += c.transform.position;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;

        center /= count;

        Vector3 dir = (center - transform.position).normalized;

        return dir * cohesionStrength;
    }

    Vector3 ApplyBoids(Vector3 baseDir)
    {
        Vector3 separation = GetSeparation();
        Vector3 cohesion = GetCohesion();

        Vector3 result = baseDir + separation + cohesion;

        result.y = 0;

        if (result.sqrMagnitude < 0.01f)
            return baseDir;

        return result.normalized;
    }

    // ================= FLEE =================

    void Flee()
    {
        _agent.speed = fleeSpeed;

        Vector3 dir = transform.position - player.position;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = Random.insideUnitSphere;
            dir.y = 0;
        }

        dir.Normalize();

        // APPLY BOIDS HERE
        dir = ApplyBoids(dir);

        Vector3 target = player.position + dir * fleeDistance;
        target = SafeNavMesh(target);

        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(target, path);

        bool validPath = path.status == NavMeshPathStatus.PathComplete;
        bool meaningfulMove = Vector3.Distance(transform.position, target) > fleeDistance / 2.0f;

        if (validPath && meaningfulMove)
        {
            _agent.SetDestination(target);

            _lastGoodDestination = target;
            _hasLastDestination = true;
        }
        else if (_hasLastDestination)
        {
            target = _lastGoodDestination;
            meaningfulMove = Vector3.Distance(transform.position, target) > fleeDistance / 2.0f;
            if (meaningfulMove)
            {
                _agent.SetDestination(_lastGoodDestination);
            }
            else
            {
                target = _agent.transform.forward * (fleeDistance * 2f);
                target = SafeNavMesh(target);
                
                _agent.SetDestination(target);
            }
        }
        else
        {
            Vector3 panic = transform.position + Random.insideUnitSphere * (fleeDistance * 2f);
            panic.y = transform.position.y;

            _agent.SetDestination(SafeNavMesh(panic));
        }

        float moved = Vector3.Distance(transform.position, _lastPosition);

        if (moved < 0.05f)
        {
            _fleeStuckTimer += Time.deltaTime;

            if (_fleeStuckTimer > 0.5f)
            {
                Vector3 panic = transform.position + Random.insideUnitSphere * (fleeDistance * 2f);
                panic.y = transform.position.y;

                _agent.SetDestination(SafeNavMesh(panic));
                _fleeStuckTimer = 0f;
            }
        }
        else
        {
            _fleeStuckTimer = 0f;
        }

        _lastPosition = transform.position;

        if (!CheckVision() && !CheckHearing())
        {
            _state = State.Wander;
        }
    }

    // ================= WANDER =================

    void WanderFocus()
    {
        if (_state != State.Wander) return;

        if (!_agent.pathPending && _agent.remainingDistance < 0.3f)
        {
            _focusTimer += Time.deltaTime;

            if (_focusTimer > focusWaitTime)
            {
                _focusTimer = 0f;
                PickNewFocusPoint();
            }
        }

        if (!_agent.hasPath || _agent.remainingDistance < 0.5f)
        {
            if (_focusPoint == Vector3.zero)
                PickNewFocusPoint();
        }
    }

    void PickNewFocusPoint()
    {
        _focusPoint = GetForwardFocusPoint();

        // APPLY BOIDS TO WANDER TARGET
        Vector3 dir = (_focusPoint - transform.position).normalized;
        dir = ApplyBoids(dir);

        Vector3 final = transform.position + dir * Random.Range(2f, wanderRadius);

        _agent.speed = 2f;
        _agent.SetDestination(SafeNavMesh(final));
    }

    Vector3 GetForwardFocusPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(-focusAngle * 0.5f, focusAngle * 0.5f);
            float dist = Random.Range(2f, wanderRadius);

            Quaternion rot = Quaternion.Euler(0, angle, 0);
            Vector3 dir = rot * transform.forward;

            Vector3 candidate = transform.position + dir * dist;

            if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
                return hit.position;
        }

        return transform.position;
    }

    // ================= REST (unchanged) =================

    void Scan()
    {
        _scanTimer -= Time.deltaTime;

        transform.Rotate(0f, 120f * Time.deltaTime, 0f);

        if (_scanTimer <= 0f)
        {
            _state = State.Wander;
            PickNewFocusPoint();
        }
    }

    void StartScan()
    {
        _state = State.Scan;
        _scanTimer = scanDuration;
        _agent.ResetPath();
    }

    void CheckIfStuck()
    {
        float moved = Vector3.Distance(transform.position, _lastPosition);

        if (moved < stuckThreshold && _agent.remainingDistance < 1f)
        {
            _stuckTimer += Time.deltaTime;

            if (_stuckTimer > 1.2f)
            {
                StartScan();
                _stuckTimer = 0f;
            }
        }
        else
        {
            _stuckTimer = 0f;
        }

        _lastPosition = transform.position;
    }

    bool CheckVision()
    {
        if (!Globals.GameManager || !Globals.GameManager.isPlaying) return false;
        if (!player || !_p) return false;
        
        Vector3 dir = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up, dir,
            out RaycastHit hit, viewDistance))
        {
            return hit.transform == player;
        }

        return false;
    }

    bool CheckHearing()
    {
        if (!Globals.GameManager || !Globals.GameManager.isPlaying) return false;
        if (!player || !_p) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > hearingRadius) return false;

        return true;
    }

    void RotateTowardsMovement()
    {
        Vector3 v = _agent.velocity;

        if (v.magnitude > 0.1f)
        {
            Quaternion rot = Quaternion.LookRotation(v.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                Time.deltaTime * 10f
            );
        }
    }

    Vector3 SafeNavMesh(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }
    
    void OnDrawGizmosSelected()
    {
        // ===== BASE POSITION =====
        Vector3 pos = transform.position + Vector3.up * 0.1f;

        // ===== STATE COLOR =====
        Color stateColor = Color.white;

        if (Application.isPlaying)
        {
            switch (_state)
            {
                case State.Wander: stateColor = Color.green; break;
                case State.Flee:   stateColor = Color.red;   break;
                case State.Scan:   stateColor = Color.yellow;break;
            }
        }

        Gizmos.color = stateColor;
        Gizmos.DrawSphere(pos, 0.2f);

        // ===== HEARING RADIUS =====
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        // ===== SEPARATION RADIUS =====
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        // ===== COHESION RADIUS =====
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);

        // ===== VISION CONE =====
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        Vector3 forward = transform.forward;
        float halfAngle = viewAngle * 0.5f;

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawLine(pos, pos + forward * viewDistance);
        Gizmos.DrawLine(pos, pos + leftDir * viewDistance);
        Gizmos.DrawLine(pos, pos + rightDir * viewDistance);

        // draw arc (approx)
        int steps = 20;
        Vector3 prevPoint = pos + leftDir * viewDistance;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;

            Vector3 nextPoint = pos + dir * viewDistance;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        // ===== CURRENT DESTINATION =====
        if (_agent != null && _agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_agent.destination, 0.25f);
            Gizmos.DrawLine(transform.position, _agent.destination);
        }

        // ===== FOCUS POINT =====
        if (_focusPoint != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_focusPoint, 0.2f);
        }

        // ===== PLAYER LINE =====
        if (player)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}