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
    public float knockbackForce = 7f;
    public float recoilForce = 4f;
    public int maxAmmo = 15;
    public float reloadDuration = 2f;

    [Header("Bomb Drop Settings")]
    public KeyCode bombThrowKey = KeyCode.RightBracket;
    public GameObject bombPrefab;
    public Vector2 throwVelocity = new Vector2(1.5f, 1.0f);
    public int maxBombs = 3;
    public float bombCooldownDuration = 5f;

    [Header("Weapon Switching")]
    public KeyCode switchWeaponKey = KeyCode.Q; // Key to cycle weapons
    public Gun[] loadout;                      // Array to hold all available child guns
    private int currentGunIndex = 0;           // Tracks which gun in the array is active

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = false;
    private float horizontalInput;

    // --- WEAPONS, JUMPS & LOCKOUT STATES ---

    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private bool isMovementLocked = false;
    private float lockTimer = 0f;

    private Collider2D currentPlatform;
    private int jumpsLeft;
    private int maxJumps = 2;

    // --- BOMB TRACKING STATES ---
    private int currentBombsLeft;
    private bool isBombCooldown = false;
    private float bombCooldownTimer = 0f;

    public int lives = 3;
    public float jetpackTimer = 0f;
    public GameObject jetpack;
    public float shieldTimer = 0f;
    public GameObject shield;
    private float fireCooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (currentGun == null)
        {
            Debug.LogError(gameObject.name + " has no gun assigned!");
            return;
        }
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentAmmo = maxAmmo;
        jumpsLeft = maxJumps;

        // ADDED/MOVED HERE: These now properly run every time the game starts
        currentAmmo = currentGun.maxAmmo;
        // Safety check to ensure weapons are linked in the inspector loadout array
        if (loadout == null || loadout.Length == 0)
        {
            Debug.LogError(gameObject.name + " has no weapons assigned in their loadout array!");
            return;
        }

        // Setup weapons and equip the starting one
        InitializeWeapons();
        currentBombsLeft = maxBombs; 
    }

    void Update()
    {
        // 1. Reload Timer
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                currentAmmo = currentGun.maxAmmo;
                isReloading = false;
            }
        }

        // 2. Lockout Timer
        if (isMovementLocked)
        {
            lockTimer -= Time.deltaTime;
            if (lockTimer <= 0f)
            {
                isMovementLocked = false;
            }
        }

        // 3. Bomb Cooldown Timer
        if (isBombCooldown)
        {
            bombCooldownTimer -= Time.deltaTime;
            if (bombCooldownTimer <= 0f)
            {
                currentBombsLeft = maxBombs;
                isBombCooldown = false;
                Debug.Log(gameObject.name + " Bombs Refilled!");
            }
        }

        // 4. Horizontal Inputs
        horizontalInput = 0f;
        if (!isMovementLocked)
        {
            if (Input.GetKey(moveLeftKey)) horizontalInput = -1f;
            else if (Input.GetKey(moveRightKey)) horizontalInput = 1f;
        }
        bool isRunning = Mathf.Abs(horizontalInput) > 0.01f;
        anim.SetBool("isRunning", isRunning);
        
        if (transform.position.y < fallThreshold)
        {
            RespawnPlayer();
        }

        // 4. Shooting Input
        if (Input.GetKeyDown(shootKey))
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

        // 6. Bomb Drop Input
        if (Input.GetKeyDown(bombThrowKey) && !isMovementLocked && !isBombCooldown)
            // Weapon Switching Input
            if (Input.GetKeyDown(switchWeaponKey) && !isReloading)
            {
                SwitchWeapon();
            }

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            DropItem();
        }

        // 7. Jump Input
        if (Input.GetKeyDown(jumpKey) && (jumpsLeft > 0 || jetpackTimer > 0))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
            isGrounded = false;

            // Force the animation to trigger IMMEDIATELY
            if (anim != null)
            {
                anim.SetBool("isJumping", true);

                // Forces the animation state to snap-play from frame 0 instantly.
                // This overrides delays and forces double jumps to restart the animation!
                anim.Play("Jump", 0, 0f);
            }
        }

        // FIX: Structural logic gap elimination
        if (anim != null)
        {
            if (isGrounded)
            {
                // Grounded means both are always false
                anim.SetBool("isJumping", false);
                anim.SetBool("isFalling", false);
            }
            else
            {
                // IF WE ARE AIRBORNE:
                // If velocity is upward, we are strictly jumping.
                bool rising = rb.linearVelocity.y > 0.01f;
                
                anim.SetBool("isJumping", rising);
                
                // If we are NOT rising, we MUST be falling. No gaps allowed!
                anim.SetBool("isFalling", !rising);
            }
        }

        // 8. Platform Descend Input
        if (Input.GetKeyDown(dropKey) && isGrounded && currentPlatform != null)
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                StartCoroutine(TemporaryDrop(currentPlatform, playerCollider));

                // Force the player out of the grounded state instantly
                isGrounded = false;

                // Force the falling animation to trigger IMMEDIATELY
                if (anim != null)
                {
                    anim.SetBool("isGrounded", false); // If you use an isGrounded bool in Animator
                    anim.SetBool("isJumping", false);
                    anim.SetBool("isFalling", true);

                    // Forces the fall animation state to play from frame 0 instantly.
                    anim.Play("Fall", 0, 0f);
                }
            }
        }

        // 8. Deprecate powerup timers (if > 0)
        if (jetpackTimer > 0)
            jetpackTimer -= Time.deltaTime;
        else
            jetpackTimer = 0;
        if (shieldTimer > 0)
            shieldTimer -= Time.deltaTime;
        else
            shieldTimer = 0;

        // 9. Child prefabs
        jetpack.SetActive(jetpackTimer > 0);
        shield.SetActive(shieldTimer > 0);
        FlipSprite();
    }

    void DropItem()
    {
        Debug.Log("DropItem called");
        if (bombPrefab != null && firePoint != null && currentBombsLeft > 0)
        {
            currentBombsLeft--;
            Debug.Log("Bomb thrown by " + gameObject.name);
            float facingDirection = Mathf.Sign(transform.localScale.x);

            GameObject newBomb = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);

            Bomb bombScript = newBomb.GetComponent<Bomb>();
            if (bombScript != null) bombScript.Initialize(gameObject.name);

            Rigidbody2D bombRb = newBomb.GetComponent<Rigidbody2D>();
            if (bombRb != null)
            {
                bombRb.linearVelocity = new Vector2(facingDirection * throwVelocity.x, throwVelocity.y);
            }

            if (currentBombsLeft <= 0)
            {
                isBombCooldown = true;
                bombCooldownTimer = bombCooldownDuration;
                Debug.Log(gameObject.name + " out of bombs! Cooldown activated.");
            }
        }
    }

    public void TakeBlastKnockback(Vector2 blastVelocity)
    {
        if (rb != null && shieldTimer <= 0)
        {
            isMovementLocked = true;
            lockTimer = 0.4f;
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = blastVelocity;
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
        jetpackTimer = 0;
        shieldTimer = 0;

        if (lives <= 0)
        {
            Debug.Log(gameObject.name + " is out of lives!");
            gameObject.SetActive(false);
            return;
        }

        transform.position = spawnPosition;
        isMovementLocked = false;
        isReloading = false;
        isBombCooldown = false;
        currentAmmo = maxAmmo;
        jumpsLeft = maxJumps;
        currentBombsLeft = maxBombs;

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
            float shootingDirection = Mathf.Sign(transform.localScale.x);

            currentGun.Fire(
                bulletPrefab,
                firePoint,
                shootingDirection
            );
            /*
                        if (bulletRb != null)
                        {
                            bulletRb.linearVelocity = new Vector2(shootingDirection * bulletSpeed, 0f);
            fireCooldownTimer = currentGun.fireRate;
            float shootingDirection = Mathf.Sign(transform.localScale.x);

            currentGun.Fire(bulletPrefab, firePoint, shootingDirection);

                            Vector3 bulletScale = newBullet.transform.localScale;
                            bulletScale.x = Mathf.Abs(bulletScale.x) * shootingDirection;
                            newBullet.transform.localScale = bulletScale;
                        }
            */
            // Recoil only if standing completely still
            if (rb != null && horizontalInput == 0f)
            {
                isMovementLocked = true;
                lockTimer = 0.05f;
                lockTimer = 0.05f;
                rb.linearVelocity = new Vector2(-shootingDirection * currentGun.recoilForce, rb.linearVelocity.y);
            }

            if (currentAmmo <= 0)
            {
                StartReload();
            }
        }
    }

    // --- ACCURATE ENVIRONMENT PHYSICS ENGINE MATRIX ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.collider.GetComponent<PlatformEffector2D>() != null)
        {
            currentPlatform = collision.collider;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.collider.GetComponent<PlatformEffector2D>() != null)
        {
            if (rb != null && rb.linearVelocity.y > 0.1f) return;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Verify the landing normal vector direction is facing upward
                if (contact.normal.y > 0.6f)
                {
                    isGrounded = true;
                    if (!isMovementLocked)
                    {
                        jumpsLeft = maxJumps;
                    }
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.collider.GetComponent<PlatformEffector2D>() != null)
            isGrounded = false;
        if (collision.collider == currentPlatform)
        {
            isGrounded = false;
            if (collision.collider == currentPlatform)
            {
                currentPlatform = null;
            }
            if (jumpsLeft == maxJumps)
            {
                jumpsLeft = maxJumps - 1; // Fall off edge gracefully leaves 1 jump remaining
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (collision.name.StartsWith(this.gameObject.name)) return;

            Rigidbody2D bulletRb = collision.GetComponent<Rigidbody2D>();
            if (bulletRb != null && rb != null && shieldTimer <= 0)
            {
                float pushDirection = Mathf.Sign(bulletRb.linearVelocity.x);
                isMovementLocked = true;
                lockTimer = 0.2f;
                Bullet bulletScript = collision.GetComponent<Bullet>();

                if (bulletRb != null && rb != null && bulletScript != null)
                {
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
    }
    private System.Collections.IEnumerator TemporaryDrop(Collider2D platformCollider, Collider2D playerCollider)
    {
        // Turn off collisions to fall through cleanly
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        isGrounded = false;

        yield return new WaitForSeconds(0.35f);

        // Safely re-engage collisions so you land on the next floor
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