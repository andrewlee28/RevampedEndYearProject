using UnityEngine;

public class HealthPowerup : MonoBehaviour
{
    public float lifetime = 15f;

    [Header("Hover Settings")]
    public float floatSpeed = 3f;     // How fast it bobs up and down
    public float floatHeight = 0.2f;  // How high/low it goes from its starting spot
    
    private Vector3 startPosition;

    void Start()
    {
        // Save the exact spot where the item spawned on the platform
        startPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Math sine wave smoothly calculates a fluctuating height over time
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        // Update the item's position, keeping X and Z exactly the same
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // Swapped to OnTriggerEnter2D so the player can walk through it smoothly
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // Give the player an extra life!
                player.lives++;
                Debug.Log("Health collected! Lives: " + player.lives);
            }

            Destroy(gameObject);
        }
    }
}