using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    public float lifetime = 15f;
    
    void Start()
    {
        // Force to front rendering fix
        SpriteRenderer spriteRen = GetComponent<SpriteRenderer>();
        if (spriteRen != null)
        {
            spriteRen.sortingOrder = 500;
        }

        // Auto-destroys if left uncollected after 15 seconds
        Destroy(gameObject, lifetime);
    } 
    void Update()
    {
    }

    // CHANGED: From OnTriggerEnter2D to OnCollisionEnter2D
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Check the collider tag of whatever bumped into the box
        if (other.collider.CompareTag("Player"))
        {
            PlayerMovement player = other.collider.GetComponent<PlayerMovement>();

            if (player != null)
            {
                player.EquipRandomWeapon();
            }

            // Destroy the box instantly so the player doesn't trip over it
            Destroy(gameObject);
        }
    }
}