using UnityEngine;

public class BoxSpawner : MonoBehaviour
{   
    [Header("Spawn Settings")]
    public GameObject boxPrefab;
    public float spawnInterval = 15f;

    [Header("Platform Spawn Zones")]
    // We changed Transform[] to SpawnZone[]
    public SpawnZone[] spawnPoints; 

    void Start()
    {
        InvokeRepeating(nameof(SpawnBox), 0f, spawnInterval);
    }

    private void SpawnBox()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // 1. Pick a random platform zone
        int randomIndex = Random.Range(0, spawnPoints.Length);
        SpawnZone selectedZone = spawnPoints[randomIndex];

        if (selectedZone == null) return;

        // 2. Use that SPECIFIC zone's width and height!
        float randomX = Random.Range(-selectedZone.zoneWidth / 2f, selectedZone.zoneWidth / 2f);
        float randomY = Random.Range(-selectedZone.zoneHeight / 2f, selectedZone.zoneHeight / 2f);

        Vector3 finalSpawnPosition = selectedZone.transform.position + new Vector3(randomX, randomY, 0f);

        if (boxPrefab != null)
        {
            Instantiate(boxPrefab, finalSpawnPosition, Quaternion.identity);
        }
    }
}