using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnOption
{
    public GameObject enemyPrefab;
    public float spawnWeight = 1f; // Higher = more likely
    public int maxCount = 5;       // Individual cap

    [HideInInspector]
    public int currentCount = 0;   // Runtime tracking
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<EnemySpawnOption> enemies = new List<EnemySpawnOption>();
    public float spawnInterval = 2f;
    public int maxEnemies = 10;

    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(10, 0, 10);

    private int currentEnemies = 0;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        EnemySpawnOption option = GetRandomEnemy();
        if (option == null) return;

        Vector3 spawnPos = transform.position + GetRandomPointInArea();
        GameObject enemy = Instantiate(option.enemyPrefab, spawnPos, Quaternion.identity);

        currentEnemies++;
        option.currentCount++;

        EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();
        tracker.spawner = this;
        tracker.spawnOption = option;
    }

    EnemySpawnOption GetRandomEnemy()
    {
        float totalWeight = 0f;

        // Only include enemies under their individual cap
        foreach (var e in enemies)
        {
            if (e.currentCount < e.maxCount)
            {
                totalWeight += e.spawnWeight;
            }
        }

        if (totalWeight <= 0f) return null;

        float randomValue = Random.Range(0, totalWeight);

        float currentWeight = 0f;
        foreach (var e in enemies)
        {
            if (e.currentCount >= e.maxCount) continue;

            currentWeight += e.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return e;
            }
        }

        return null;
    }

    Vector3 GetRandomPointInArea()
    {
        return new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            spawnAreaSize.y,
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
    }

    public void OnEnemyDied(EnemySpawnOption option)
    {
        currentEnemies--;
        currentEnemies = Mathf.Max(0, currentEnemies);

        if (option != null)
        {
            option.currentCount--;
            option.currentCount = Mathf.Max(0, option.currentCount);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}