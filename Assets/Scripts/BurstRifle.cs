using UnityEngine;

public class BurstRifle : Gun
{
    void Awake()
    {
        // Custom stats tailored for a precise tactical rifle
        bulletSpeed = 22f;              // Highly accurate, fast-moving rounds
        maxAmmo = 24;                   // Generous magazine size
        reloadDuration = 2f;          // Standard, reliable reload speed
        
        // --- KNOCKBACK MANAGEMENT ---
        knockbackForce = 4.5f;          // Moderate, steady push on the victim
        recoilForce = 2.0f;             // Light, easily manageable shooter recoil
        distanceBasedKnockback = false; // Keeps its full impact even at long range
        
        // --- FIRE RATE MANAGEMENT ---
        fireRate = 0.2f;                // Controlled delay between bursts
    }
}