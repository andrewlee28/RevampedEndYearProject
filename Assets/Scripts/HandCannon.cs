using UnityEngine;

public class HandCannon : Gun
{
    void Awake()
    {
        // Custom stats tailored for a high-risk, pocket-shotgun style weapon
        bulletSpeed = 16f;              // Snappy, immediate projectile speed
        maxAmmo = 2;                    // Only two barrels to fire before reloading
        reloadDuration = 1.0f;          // Very fast, frantic double-barrel reload
        
        // --- KNOCKBACK MANAGEMENT ---
        knockbackForce = 4.0f;          // Decent baseline knockback at standard range
        recoilForce = 8.0f;             // Massive shooter recoil (fires a heavy slug)
        
        // --- DISTANCE FALLOFF CONFIGURATION ---
        distanceBasedKnockback = true;
        maxKnockbackRange = 3.5f;       // The scaling effect completely stops after 3.5 units
        maxKnockbackMultiplier = 5.0f;  // Point-blank shots get multiplied by 5x! ($4 \times 5 = 20$ Force)
        
        // --- FIRE RATE MANAGEMENT ---
        fireRate = 0.15f;               // Can pull the trigger on both barrels almost instantly
    }
}