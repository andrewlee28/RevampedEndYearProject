using System.Collections;
using UnityEngine;

public class BoxSpawner : MonoBehaviour
{   
    [System.Serializable]
    public class SpawnableItem
    {
        public string name;
        public GameObject prefab;
        [Header("Timing (in seconds)")]
        public float minSpawnTime = 15f;
        public float maxSpawnTime = 30f;
        public float minInitialDelay = 5f;
        public float maxInitialDelay = 15f;
    }

    [Header("All Spawnable Items")]
    public SpawnableItem[] itemsToSpawn;

    [Header("Platform Spawn Zones")]
    public SpawnZone[] spawnPoints; 

    void Start()
    {
        // Start a separate random timer loop for every item in your list
        foreach (SpawnableItem item in itemsToSpawn)
        {
            if (item.prefab != null)
            {
                StartCoroutine(SpawnRoutine(item));
            }
        }
    }

    IEnumerator SpawnRoutine(SpawnableItem item)
    {
        // 1. Pick a random delay before this specific item spawns for the first time
        float initialDelay = Random.Range(item.minInitialDelay, item.maxInitialDelay);
        yield return new WaitForSeconds(initialDelay);

        // 2. Loop to handle all future spawns for this specific item
        while (true)
        {
            SpawnItem(item.prefab);

            // 3. Pick a new random wait time before spawning it again
            float randomWait = Random.Range(item.minSpawnTime, item.maxSpawnTime);
            yield return new WaitForSeconds(randomWait);
        }
    }

    private void SpawnItem(GameObject prefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Pick a random platform zone
        int randomIndex = Random.Range(0, spawnPoints.Length);
        SpawnZone selectedZone = spawnPoints[randomIndex];

        if (selectedZone == null) return;

        // Calculate the exact spot on that platform (your original formula)
        float randomX = Random.Range(-selectedZone.zoneWidth / 2f, selectedZone.zoneWidth / 2f);
        float randomY = Random.Range(-selectedZone.zoneHeight / 2f, selectedZone.zoneHeight / 2f);

        Vector3 finalSpawnPosition = selectedZone.transform.position + new Vector3(randomX, randomY, 0f);

        // If it's your specific WeaponBox, we still want it to drop from the sky!
        if (prefab.name.Contains("WeaponBox") || prefab.name.Contains("weaponbox"))
        {
            finalSpawnPosition.y += 10f;
        }

        Instantiate(prefab, finalSpawnPosition, Quaternion.identity);
    }
}