using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Blast Settings")]
    public float fuseTime = 2.5f;        // Seconds before the blast wave activates
    public float explosionRadius = 1.8f; // Small radius (only players standing very close)
    public float blastForce = 5.0f;      // Medium push force (not launching them into orbit)

    private string throwerName = "";    

    void Start()
    {
        // Start the countdown timer as soon as it drops
        Invoke("ActivateBlastWave", fuseTime);
    }

    public void Initialize(string creatorName)
    {
        throwerName = creatorName;
    }

    void ActivateBlastWave()
    {
        // Find physical objects within our small radius circle
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D obj in objectsInRange)
        {
            // Check if the object caught in the zone is a player
            PlayerMovement player = obj.GetComponent<PlayerMovement>();
            
            if (player != null)
            {
                // Calculate direction away from the bomb's center
                Vector2 blastDirection = (obj.transform.position - transform.position).normalized;
                
                // Safety check: if player is perfectly centered, push them straight up
                if (blastDirection == Vector2.zero)
                {
                    blastDirection = Vector2.up;
                }

                // Apply the gentle knockback to the player script
                player.TakeBlastKnockback(blastDirection * blastForce);
            }
        }

        // Cleanly remove the bomb object from the scene
        Destroy(gameObject);
    }

    // Visualizes the blast zone area in your Scene view when you click the prefab
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}