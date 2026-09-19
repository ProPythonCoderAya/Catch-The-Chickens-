using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Globs;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

public class ChickenSpawnerScript : MonoBehaviour
{
    [Header("Chicken")]
    public GameObject chickenPrefab;

    [Header("Spawning")]
    public int maxChickens = 10;
    [Min(0.1f)]
    public float spawnDelay = 2f;
    public float spawnRadius = 10f;

    public int luckyChickenChance;
    
    [CanBeNull]
    public Transform miniBot;
    
    [Space]
    public List<ChickenMovementScript> chickens = new();

    private int _currentChickens;
    private Transform _chickenRoot;

    private void Awake()
    {
        Globals.ChickenSpawnerScript = this;
    }

    void Start()
    {
        _chickenRoot = new GameObject("Chickens").transform;
        _chickenRoot.SetParent(transform);
        for (int i = 0; i < maxChickens; i++) // spawn all at first
        {
            if (_currentChickens < maxChickens)
            {
                SpawnChicken();
            }
        }
        StartCoroutine(SpawnLoop()); // then keep spawning when player kills
    }

    private void Update()
    {
        if (!Globals.Player) return;

        foreach (ChickenMovementScript chicken in chickens)
        {
            if (!chicken || chicken.gameObject.activeSelf) continue;

            float distanceFromPlayer = Vector3.Distance(Globals.Player.transform.position, chicken.transform.position);
            float distanceFromMiniBot = miniBot ? Vector3.Distance(Globals.Player.transform.position, miniBot.position) : float.MaxValue;

            if (distanceFromPlayer < 25f || distanceFromMiniBot < 25f)
            {
                chicken.gameObject.SetActive(true);
            }
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (_currentChickens < maxChickens)
            {
                SpawnChicken();
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnChicken()
    {
        if (!Globals.Player) return;

        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0,
            Random.Range(-spawnRadius, spawnRadius)
        );

        GameObject chicken = Instantiate(chickenPrefab, randomPos, Quaternion.identity);
        chicken.name = $"Chicken_{_currentChickens}";
        chicken.transform.SetParent(_chickenRoot);
        
        _currentChickens++;

        // Tell chicken who spawned it
        ChickenMovementScript chickenScript = chicken.GetComponent<ChickenMovementScript>();
        chickens.Add(chickenScript);
        if (chickenScript)
        {
            chickenScript.spawner = this;
            chickenScript.player = Globals.Player.transform;
            chickenScript.miniBot = miniBot;
        }
        
        ChickenHealthScript chickenHealthScript = chicken.GetComponentInChildren<ChickenHealthScript>();
        if (chickenHealthScript)
        {
            chickenHealthScript.targetCamera = Globals.MainCamera;
        }

        ChickenSoundScript chickenSoundScript = chicken.GetComponent<ChickenSoundScript>();
        if (chickenSoundScript)
        {
            chickenSoundScript.lucky = Random.Range(0, 100) < luckyChickenChance;
        }
    }

    public void ChickenDied(ChickenMovementScript chicken)
    {
        chickens.Remove(chicken);
        _currentChickens--;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}