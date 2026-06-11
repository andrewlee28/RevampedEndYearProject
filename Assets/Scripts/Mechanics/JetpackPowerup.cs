using UnityEngine;

public class JetpackPowerup : MonoBehaviour
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

        // Automatically clears the item if the player leaves it alone for too long
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Math sine wave smoothly calculates a fluctuating height over time
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        
        // Update the item's position, keeping X and Z exactly the same
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // Swapped from OnCollisionEnter2D to OnTriggerEnter2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the overlapping object is tagged as "Player"
        if (other.CompareTag("Player"))
        {
            // Try to find the PlayerMovement script on the object that touched it
            PlayerMovement player = other.GetComponent<PlayerMovement>();

            if (player != null)
            {
                // Add 5 seconds to the player's jetpack fuel timer!
                player.jetpackTimer += 5f;
                Debug.Log("Jetpack fuel added!");
            }

            // Make the physical pickup item vanish from the level
            Destroy(gameObject);
        }
    }
}