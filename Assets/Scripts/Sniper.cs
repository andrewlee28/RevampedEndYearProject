using UnityEngine;

public class Sniper : Gun
{
    void Awake()
    {
        // Custom default stats for the Sniper
        bulletSpeed = 20f;              // Super fast bullet trajectory
        maxAmmo = 5;                    // Low capacity
        reloadDuration = 5f;            // Long reload penalty
        fireRate = 2f; // Must wait 1.5 seconds between shots
        
        // --- KNOCKBACK MANAGEMENT ---
        recoilForce = 5f;              // Pushes the shooter back heavily
        knockbackForce = 17.5f;           // Massive punch that blasts the victim away
        distanceBasedKnockback = false; // Disables falloff so it hits hard at any range
    }
}