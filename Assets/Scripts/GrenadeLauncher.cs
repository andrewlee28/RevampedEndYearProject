using UnityEngine;

public class GrenadeLauncher : Gun
{
    void Awake()
    {
        // Custom stats tailored for a heavy explosive weapon
        bulletSpeed = 7f;               // Very slow, looping/heavy projectile feel
        maxAmmo = 3;                    // Extremely limited magazine capacity
        reloadDuration = 4f;          // Painfully long reload time
        
        // --- KNOCKBACK MANAGEMENT ---
        knockbackForce = 17f;           // Explosive force! Launches victims across the map
        recoilForce = 9f;               // Massive kickback that pushes the shooter backward
        distanceBasedKnockback = false; // The explosion hits just as hard at any distance
        
        // --- FIRE RATE MANAGEMENT ---
        fireRate = 1.5f;                // Slow pump action between single grenades
    }
}