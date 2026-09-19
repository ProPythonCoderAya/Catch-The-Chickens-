using UnityEngine;
using UnityEngine.AI;
using Globs;

public class ChickenHunterAI : MonoBehaviour
{
    [Header("Targeting")]
    public float detectionRange = 50f;

    [Header("Attacking")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float damage = 100f;

    private NavMeshAgent _agent;
    private ChickenMovementScript _currentTarget;
    private float _lastAttackTime;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        FindClosestChicken();

        if (_currentTarget == null)
        {
            _agent.ResetPath();
            return;
        }

        _agent.SetDestination(_currentTarget.transform.position);

        float distance = Vector3.Distance(
            transform.position,
            _currentTarget.transform.position
        );

        if (distance <= attackRange)
        {
            AttackChicken();
        }
    }

    void FindClosestChicken()
    {
        float closestDistance = detectionRange;
        ChickenMovementScript closestChicken = null;

        foreach (ChickenMovementScript chicken in Globals.ChickenSpawnerScript.chickens)
        {
            if (chicken == null)
                continue;

            if (!chicken.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                chicken.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestChicken = chicken;
            }
        }

        _currentTarget = closestChicken;
    }

    void AttackChicken()
    {
        if (Time.time < _lastAttackTime + attackCooldown)
            return;

        _lastAttackTime = Time.time;

        ChickenSoundScript health =
            _currentTarget.GetComponent<ChickenSoundScript>();

        if (health != null && health.health > 0)
        {
            health.Hit(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}