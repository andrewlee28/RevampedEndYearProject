using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float bulletSpeed = 12f;
    public int maxAmmo = 15;
    public float reloadDuration = 2f;
    public float recoilForce = 4f;
    public float knockbackForce = 2f;

    [Header("Distance Knockback Settings")]
    public bool distanceBasedKnockback = false;
    
    [Tooltip("The max distance where knockback scaling happens. Beyond this, it hits at base knockbackForce.")]
    public float maxKnockbackRange = 3f; 
    
    [Tooltip("The multiplier applied when point-blank (0 distance). Scales down smoothly to 1x at max range.")]
    public float maxKnockbackMultiplier = 3f;
    [Header("Fire Rate Settings")]
    [Tooltip("The minimum time delay (in seconds) between shots.")]
    public float fireRate = 0.2f; // Default baseline (good for a pistol)

    public virtual void Fire(GameObject bulletPrefab, Transform firePoint, float direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

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
            bulletRb.linearVelocity = new Vector2(direction * bulletSpeed, 0f);
        }

        // Flip bullet sprite matching shooting direction
        Vector3 bulletScale = bullet.transform.localScale;
        bulletScale.x = Mathf.Abs(bulletScale.x) * direction;
        bullet.transform.localScale = bulletScale;

        // Mark who fired it
        PlayerMovement owner = firePoint.GetComponentInParent<PlayerMovement>();
        if (owner != null)
        {
            bullet.name = owner.gameObject.name + "_Bullet";
        }

        Destroy(bullet, 3f);
    }
}