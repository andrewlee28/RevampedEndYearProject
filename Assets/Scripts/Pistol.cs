using UnityEngine;

public class Pistol : Gun
{
    void Awake()
    {
        // Custom stats tailored for a standard Pistol
        bulletSpeed = 10f;              // Decent, snappy bullet speed
        maxAmmo = 12;                   // Standard magazine capacity
        reloadDuration = 1.5f;          // Quick, agile reload
        
        // --- KNOCKBACK MANAGEMENT ---
        knockbackForce = 2.5f;          // Low knockback (as requested, won't launch opponents)
        recoilForce = 1.5f;             // Very controllable shooter recoil
        distanceBasedKnockback = false; // Regular static knockback at any range
        
        // --- FIRE RATE MANAGEMENT ---
        fireRate = 0.5f;               // Can be fired cleanly 2 times a second
    }
}