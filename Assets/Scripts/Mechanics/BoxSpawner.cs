using UnityEngine;

public class BoxSpawner : MonoBehaviour
{   
    // Spawn Settings
    public GameObject boxPrefab;
    public float spawnInterval = 15f;
    // Spawn Area
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -3f;
    public float maxY = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(SpawnBox), 0f, spawnInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void SpawnBox()
    {
        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        Instantiate(boxPrefab, randomPosition, Quaternion.identity);
    }
}
