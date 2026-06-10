using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Keys")]
    public KeyCode moveLeftKey = KeyCode.LeftArrow;
    public KeyCode moveRightKey = KeyCode.RightArrow;
    public KeyCode jumpKey = KeyCode.UpArrow;
    public KeyCode dropKey = KeyCode.DownArrow;

    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float jumpForce = 3.75f;

    [Header("Respawn Settings")]
    public float fallThreshold = -5f;  
    public Vector3 spawnPosition = new Vector3(0.34f, 2.5f, 0f); 

    [Header("Shooting & Ammo Settings")]
    public KeyCode shootKey = KeyCode.LeftBracket; 
    public GameObject bulletPrefab;    
    public Transform firePoint;
    public Gun currentGun;        
    public float bulletSpeed = 12f;    
    public float recoilForce = 4f; 
    public int maxAmmo = 15;
    public float reloadDuration = 2f;

    [Header("Weapon Switching")]
    public KeyCode switchWeaponKey = KeyCode.Q; // Key to cycle weapons
    public Gun[] loadout;                      // Array to hold all available child guns
    private int currentGunIndex = 0;           // Tracks which gun in the array is active

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = false;
    private float horizontalInput;
    
    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private bool isMovementLocked = false;
    private float lockTimer = 0f;

    private Collider2D currentPlatform;

    public int lives = 3;
    private float fireCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Safety check to ensure weapons are linked in the inspector loadout array
        if (loadout == null || loadout.Length == 0)
        {
            Debug.LogError(gameObject.name + " has no weapons assigned in their loadout array!");
            return;
        }

        // Setup weapons and equip the starting one
        InitializeWeapons();
    }

    void Update()
    {
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                currentAmmo = currentGun.maxAmmo;
                isReloading = false;
                Debug.Log($"{gameObject.name} reloaded!");
            }
        }

        if (isMovementLocked)
        {
            lockTimer -= Time.deltaTime;
            if (lockTimer <= 0f)
            {
                isMovementLocked = false; 
            }
        }

        horizontalInput = 0f;
        if (!isMovementLocked)
        {
            if (Input.GetKey(moveLeftKey)) horizontalInput = -1f;
            else if (Input.GetKey(moveRightKey)) horizontalInput = 1f;
        }

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        if (transform.position.y < fallThreshold)
        {
            RespawnPlayer();
        }

        // Shooting Input
        if (Input.GetKeyDown(shootKey)) 
        {
            if (!isReloading && fireCooldownTimer <= 0f) 
            {
                if (currentAmmo > 0)
                {
                    Shoot();
                }
                else
                {
                    StartReload();
                }
            }
        }

        // Weapon Switching Input
        if (Input.GetKeyDown(switchWeaponKey) && !isReloading)
        {
            SwitchWeapon();
        }

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; 
        }

        if (Input.GetKeyDown(dropKey) && isGrounded && currentPlatform != null)
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                StartCoroutine(TemporaryDrop(currentPlatform, playerCollider));
            }
        }

        // Handle the fire rate cooldown timer
        if (fireCooldownTimer > 0f)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        FlipSprite();
    }

    void InitializeWeapons()
    {
        for (int i = 0; i < loadout.Length; i++)
        {
            if (loadout[i] != null)
            {
                // Deactivate every weapon except the first one (index 0)
                loadout[i].gameObject.SetActive(i == currentGunIndex);
            }
        }

        // Point currentGun to the active weapon slot
        currentGun = loadout[currentGunIndex];
        currentAmmo = currentGun.maxAmmo;
    }

    void SwitchWeapon()
    {
        if (loadout == null || loadout.Length <= 1) return;

        // Turn off the gun we are holding right now
        loadout[currentGunIndex].gameObject.SetActive(false);

        // Advance to the next gun slot (loops back to 0 if it goes over the total length)
        currentGunIndex = (currentGunIndex + 1) % loadout.Length;

        // Turn on the new gun child object
        loadout[currentGunIndex].gameObject.SetActive(true);

        // Update the script references to match the newly equipped gun
        currentGun = loadout[currentGunIndex];
        currentAmmo = currentGun.maxAmmo; 

        Debug.Log($"{gameObject.name} switched to {currentGun.gameObject.name}!");
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = currentGun.reloadDuration;
    }

    void RespawnPlayer()
    {
        lives--;
        Debug.Log(gameObject.name + " has " + lives + " lives remaining");

        if (lives <= 0)
        {
            Debug.Log(gameObject.name + " is out of lives!");
            gameObject.SetActive(false);
            return;
        }

        transform.position = spawnPosition;
        isMovementLocked = false; 
        isReloading = false;
        
        // Safety check to reset current weapon stats upon respawning
        if (currentGun != null)
        {
            currentAmmo = currentGun.maxAmmo; 
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            currentAmmo--;
            fireCooldownTimer = currentGun.fireRate;
            float shootingDirection = Mathf.Sign(transform.localScale.x);

            currentGun.Fire(bulletPrefab, firePoint, shootingDirection);

            if (rb != null && horizontalInput == 0f)
            {
                isMovementLocked = true;
                lockTimer = 0.05f; 
                rb.linearVelocity = new Vector2(-shootingDirection * currentGun.recoilForce, rb.linearVelocity.y);
            }

            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.collider.GetComponent<PlatformEffector2D>() != null)
        {
            currentPlatform = collision.collider;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.6f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        if (collision.collider == currentPlatform)
        {
            currentPlatform = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (collision.name.StartsWith(this.gameObject.name))
            {
                return; 
            }

            Rigidbody2D bulletRb = collision.GetComponent<Rigidbody2D>();
            Bullet bulletScript = collision.GetComponent<Bullet>();

            if (bulletRb != null && rb != null && bulletScript != null)
            {
                float pushDirection = Mathf.Sign(bulletRb.linearVelocity.x);
                float distanceTravelled = Vector2.Distance(bulletScript.startPosition, collision.transform.position);

                isMovementLocked = true;
                lockTimer = 0.2f;

                float finalKnockback = bulletScript.knockbackForce;

                if (bulletScript.distanceBasedKnockback)
                {
                    float distancePercentage = Mathf.Clamp01(distanceTravelled / bulletScript.maxKnockbackRange);
                    float knockbackMultiplier = Mathf.Lerp(bulletScript.maxKnockbackMultiplier, 1f, distancePercentage);
                    finalKnockback *= knockbackMultiplier;
                }

                rb.linearVelocity = Vector2.zero;
                rb.linearVelocity = new Vector2(pushDirection * finalKnockback, rb.linearVelocity.y);
                
                Debug.Log($"Hit by {collision.name}. Distance: {distanceTravelled}. Final Knockback: {finalKnockback}");
            }

            Destroy(collision.gameObject);
        }
    }

    private System.Collections.IEnumerator TemporaryDrop(Collider2D platformCollider, Collider2D playerCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.35f);
        if (platformCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    void FixedUpdate()
    {
        if (!isMovementLocked)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    void FlipSprite()
    {
        if (horizontalInput > 0.01f) 
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}