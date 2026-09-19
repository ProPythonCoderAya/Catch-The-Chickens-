using UnityEngine;
using System.Collections.Generic;

public class PlayerInteractScript : MonoBehaviour
{
    private PlayerInputActions _input;
    
    public ChickensCountScript chickensCount;

    public float damage = 50f;

    public float attackSpeed = 0.5f;

    public float killAmount = 1f;
    
    public PlayerMovementScript playerMovementScript;

    // Chickens currently inside trigger
    private readonly List<GameObject> _chickensInRange = new();
    private readonly List<GameObject> _chickensToKill = new();

    private float _hitTimer;
    
    private void Update()
    {
        _hitTimer -= Time.deltaTime;
        if (_hitTimer > 0f) return;
        if (!playerMovementScript.canMove) return;
        if (_input.Player.Interact.IsPressed())
        {
            OnInteract();
            _hitTimer = attackSpeed;
        }
    }

    void Awake()
    {
        _input = new PlayerInputActions();
    }

    void OnEnable()
    {
        _input.Enable();
    }

    void OnDisable()
    {
        _input.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chicken"))
        {
            _chickensInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chicken"))
        {
            _chickensInRange.Remove(other.gameObject);
        }
    }

    private void OnInteract()
    {
        if (_chickensInRange.Count == 0) return;
        
        _chickensInRange.RemoveAll(c => !c);
        
        _chickensToKill.Clear();
        // Pick the closest chickens
        for (int i = 0; i < killAmount; i++)
        {
            float minDist = float.MaxValue;
            GameObject closest = null;
            foreach (var chicken in _chickensInRange)
            {
                if (!chicken) continue;

                var dist = Vector3.Distance(transform.position, chicken.transform.position);
                if (!(dist < minDist)) continue;
                if (_chickensToKill.Contains(chicken)) continue;
                minDist = dist;
                closest = chicken;
            }
            if (closest)
                _chickensToKill.Add(closest);
        }
        
        foreach (var chicken in _chickensToKill)
        {
            HitChicken(chicken, _chickensToKill.Count);
        }
    }

    private void HitChicken(GameObject chicken, float amount)
    {
        if (!chicken) return;

        if (!chicken.TryGetComponent(out ChickenSoundScript chickenSound))
            return;
        
        if (chickenSound.health <= 0f) return;

        chickenSound.Hit(damage / amount);
    }
}
