using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float bulletSpeed = 12f;
    public int maxAmmo = 15;
    public float reloadDuration = 2f;
    public float recoilForce = 4f;

    // public virtual void Fire(
    //     GameObject bulletPrefab,
    //     Transform firePoint,
    //     float direction
    // )
    // {
    //     GameObject bullet = Instantiate(
    //         bulletPrefab,
    //         firePoint.position,
    //         firePoint.rotation
    //     );

    //     Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

    //     if (bulletRb != null)
    //     {
    //         bulletRb.linearVelocity =
    //             new Vector2(direction * bulletSpeed, 0f);
    //     }

    //     Destroy(bullet, 3f);
    // }
    public virtual void Fire(
    GameObject bulletPrefab,
    Transform firePoint,
    float direction
)
{
    GameObject bullet = Instantiate(
        bulletPrefab,
        firePoint.position,
        firePoint.rotation
    );

    Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

    if (bulletRb != null)
    {
        bulletRb.linearVelocity =
            new Vector2(direction * bulletSpeed, 0f);
    }

    // Flip bullet sprite
    Vector3 bulletScale = bullet.transform.localScale;
    bulletScale.x = Mathf.Abs(bulletScale.x) * direction;
    bullet.transform.localScale = bulletScale;

    // IMPORTANT: mark who fired it
    PlayerMovement owner =
        firePoint.GetComponentInParent<PlayerMovement>();

    if (owner != null)
    {
        bullet.name = owner.gameObject.name + "_Bullet";
    }

    Destroy(bullet, 3f);
}
}