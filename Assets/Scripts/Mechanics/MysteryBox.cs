using UnityEngine;

public class MysteryBox : MonoBehaviour
{
    public GameObject[] items;
    public Transform spawnPoint;
    public float lifetime = 15f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SpawnRandomItem();
            Destroy(gameObject);
        }
    }

    private void SpawnRandomItem()
    {
        if(items.Length == 0)
            return;
        int randomIndex = Random.Range(0, items.Length);
        GameObject itemToSpawn = items[randomIndex];
        Instantiate(itemToSpawn, spawnPoint.position, Quaternion.identity);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
