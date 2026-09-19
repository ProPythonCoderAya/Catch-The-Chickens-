using System;
using UnityEngine;
using System.Collections;
using Globs;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChickenSoundScript : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("speed");
    public AudioSource audioSource;
    
    [Header("Animator")]
    public Animator animatorRef;
    
    public AudioClip[] quackSounds;
    
    public AudioClip[] hurtSounds;

    public Slider healthSlider;

    public float minDelay = 3f;
    public float maxDelay = 8f;
    
    [HideInInspector] public float health = 100f;
    [HideInInspector] public bool lucky;
    
    private ChickenMovementScript _chickenMovementScript;
    private ChickenVisualScript _chickenVisualScript;
    private NavMeshAgent _chickenNavMeshAgent;
    private Animator _animator;
    private bool _isDead;

    void Start()
    {
        healthSlider.maxValue = 100f;
        _chickenNavMeshAgent = GetComponent<NavMeshAgent>();
        _chickenMovementScript = GetComponent<ChickenMovementScript>();
        _chickenVisualScript = GetComponent<ChickenVisualScript>();
        _animator = animatorRef;
        StartCoroutine(RandomCluckLoop());
    }

    void Update()
    {
        healthSlider.value = health;
    }

    IEnumerator RandomCluckLoop()
    {
        while (!_isDead)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            PlayRandom();
        }
    }

    void PlayRandom()
    {
        if (_isDead) return;
        if (quackSounds.Length == 0) return;

        var clip = quackSounds[Random.Range(0, quackSounds.Length)];
        audioSource.PlayOneShot(clip);
    }

    public void Hit(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            Die();
            return;
        }
        
        _chickenVisualScript.FlashRed();
        _chickenNavMeshAgent.velocity += _chickenNavMeshAgent.transform.forward * 5f + new Vector3(0f, 2f, 0f); // boost chicken forward
        
        if (hurtSounds.Length == 0) return;
        var clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void Die()
    {
        _isDead = true;
        _chickenMovementScript.enabled = false;
        _chickenNavMeshAgent.isStopped = true;

        var chickensCount = Globals.ChickensCountScript;
        chickensCount.IncrementChickenCount();

        if (lucky)
        {
            chickensCount.IncrementChickenCount();
            chickensCount.IncrementChickenCount();
            chickensCount.IncrementChickenCount();
            chickensCount.IncrementChickenCount();
        }

        if (hurtSounds.Length == 0)
        {
            StartCoroutine(DeathAnimation(() => Destroy(gameObject)));
            return;
        }

        StartCoroutine(DeathAnimation(() =>
        {
            Destroy(gameObject);
        }));
    }

    private IEnumerator DeathAnimation(Action action)
    {
        var clip = hurtSounds[Random.Range(0, hurtSounds.Length)];
        audioSource.PlayOneShot(clip);
        var clipLength = clip.length;
        
        _animator.SetFloat(Speed, 0f);
        _chickenVisualScript.FlashRed(clipLength);
        _chickenVisualScript.TurnRight(clipLength);
        yield return new WaitForSeconds(clipLength);
        action();
    }
}
