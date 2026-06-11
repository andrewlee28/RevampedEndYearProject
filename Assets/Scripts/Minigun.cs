using UnityEngine;

public class Minigun : Gun
{
    void Awake()
    {
        // Custom stats tailored for a heavy Minigun
        bulletSpeed = 15f;              // Fast, streaming projectiles
        maxAmmo = 100;                  // Massive ammunition belt
        reloadDuration = 7f;          // Long, punishing reload time due to weapon size
        
        // --- KNOCKBACK MANAGEMENT ---
        knockbackForce = 2.5f;          // Lower per-bullet knockback, but hits rapidly!
        recoilForce = 0.75f;             // Constant heavy push back against the shooter
        distanceBasedKnockback = false; // Keeps pressure up regardless of distance
        
        // --- FIRE RATE MANAGEMENT ---
        fireRate = 0.05f;               // Extreme high speed (roughly 16 rounds per second!)
    }
}