using UnityEngine;

public class Shotgun : Gun
{
    void Awake()
    {
        // Custom default stats for the Shotgun
        bulletSpeed = 10f;
        maxAmmo = 6;
        reloadDuration = 2f;
        recoilForce = 3f;
        knockbackForce = 3f; // Base damage/knockback at normal range
        fireRate = 1f; // Must wait 'x' seconds between shots
        
        // Distance configuration
        distanceBasedKnockback = true;
        maxKnockbackRange = 3f;        // Multiplier smoothly drops until 4 units away
        maxKnockbackMultiplier = 2.5f; // Point-blank shots get multiplied by 3.5x!
    }

    public override void Fire(GameObject bulletPrefab, Transform firePoint, float direction)
    {
        float[] angles = { -10f, -5f, 0f, 5f, 10f };

        foreach (float angle in angles)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.startPosition = firePoint.position;
                bulletScript.knockbackForce = knockbackForce;
                bulletScript.distanceBasedKnockback = distanceBasedKnockback;
                bulletScript.maxKnockbackRange = maxKnockbackRange;
                bulletScript.maxKnockbackMultiplier = maxKnockbackMultiplier;
            }

            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                Vector2 bulletDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
                bulletRb.linearVelocity = bulletDirection * bulletSpeed * direction;
            }

            Vector3 bulletScale = bullet.transform.localScale;
            bulletScale.x = Mathf.Abs(bulletScale.x) * direction;
            bullet.transform.localScale = bulletScale;

            PlayerMovement owner = firePoint.GetComponentInParent<PlayerMovement>();
            if (owner != null)
            {
                bullet.name = owner.gameObject.name + "_Bullet";
            }

            Destroy(bullet, 3f);
        }
    }
}