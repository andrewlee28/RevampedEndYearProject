using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Weapon Visuals")]
    public SpriteRenderer gunSpriteRenderer; // Drag your 'Gun_Visual' object here

    [Header("UI Visuals")]
    private LifeDisplay lifeDisplayUI;        // Automatically finds your UI Manager

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

    [Header("Player Status")]
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

        // Automatically find the life display UI in the scene upon falling from the sky
        lifeDisplayUI = GameObject.FindAnyObjectByType<LifeDisplay>();
        if (lifeDisplayUI != null)
        {
            lifeDisplayUI.UpdateLivesDisplay(lives);
        }

        if (currentGun == null)
        {
            Debug.LogError(gameObject.name + " has no gun assigned!");
            return;
        }
        currentAmmo = currentGun.maxAmmo;
        jumpsLeft = maxJumps;

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

        if (fireCooldownTimer > 0f)
        {
            fireCooldownTimer -= Time.deltaTime;
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

        if (Input.GetKeyDown(shootKey) && !isReloading && fireCooldownTimer <= 0f)
        {
            if (currentAmmo > 0) Shoot();
            else StartReload();
        }

        // 4. Weapon Switching Input
        if (Input.GetKeyDown(switchWeaponKey) && !isReloading)
        {
            SwitchWeapon();
        }

        // 5. Bomb Drop Input
        if (Input.GetKeyDown(bombThrowKey) && !isMovementLocked && !isBombCooldown)
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
                anim.Play("Jump", 0, 0f);
            }
        }

        // Structural logic gap elimination
        if (anim != null)
        {
            if (isGrounded)
            {
                anim.SetBool("isJumping", false);
                anim.SetBool("isFalling", false);
            }
            else
            {
                bool rising = rb.linearVelocity.y > 0.01f;
                anim.SetBool("isJumping", rising);
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
                isGrounded = false;

                if (anim != null)
                {
                    anim.SetBool("isJumping", false);
                    anim.SetBool("isFalling", true);
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
        if (jetpack != null) jetpack.SetActive(jetpackTimer > 0);
        if (shield != null) shield.SetActive(shieldTimer > 0);
        
        FlipSprite();
    }

    void DropItem()
    {
        if (bombPrefab != null && firePoint != null && currentBombsLeft > 0)
        {
            currentBombsLeft--;
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
            }
        }
    }

    public void TakeBlastKnockback(Vector2 blastVelocity)
    {
        if (rb != null && shieldTimer <= 0)
        {
            isMovementLocked = true;
            lockTimer = 0.4f;
            float preservedYVelocity = rb.linearVelocity.y;
            rb.linearVelocity = new Vector2(blastVelocity.x, preservedYVelocity);
        }
        FlipSprite();
    }

    void InitializeWeapons()
    {
        for (int i = 0; i < loadout.Length; i++)
        {
            if (loadout[i] != null)
            {
                loadout[i].gameObject.SetActive(i == currentGunIndex);
            }
        }

        currentGun = loadout[currentGunIndex];
        currentAmmo = currentGun.maxAmmo;
        UpdateGunVisual();
    }

    void SwitchWeapon()
    {
        if (loadout == null || loadout.Length <= 1) return;

        loadout[currentGunIndex].gameObject.SetActive(false);
        currentGunIndex = (currentGunIndex + 1) % loadout.Length;
        loadout[currentGunIndex].gameObject.SetActive(true);

        currentGun = loadout[currentGunIndex];
        currentAmmo = currentGun.maxAmmo;
        UpdateGunVisual();

        Debug.Log($"{gameObject.name} switched to {currentGun.gameObject.name}!");
    }

    public void EquipRandomWeapon()
    {
        if (loadout == null || loadout.Length == 0) return;

        loadout[currentGunIndex].gameObject.SetActive(false);
        currentGunIndex = Random.Range(0, loadout.Length);
        loadout[currentGunIndex].gameObject.SetActive(true);

        currentGun = loadout[currentGunIndex];
        currentAmmo = currentGun.maxAmmo;
        isReloading = false; 

        UpdateGunVisual();
        Debug.Log($"{gameObject.name} pulled a mystery weapon: {currentGun.gameObject.name}!");
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = currentGun.reloadDuration;
    }

    void RespawnPlayer()
    {
        lives--;
        
        // Update visual UI hearts instantly when a life drops
        if (lifeDisplayUI != null)
        {
            lifeDisplayUI.UpdateLivesDisplay(lives);
        }

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
        jumpsLeft = maxJumps;
        currentBombsLeft = maxBombs;

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

            currentGun.Fire(bulletPrefab, firePoint, shootingDirection);
            fireCooldownTimer = currentGun.fireRate;
            
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
        if (collision.gameObject.CompareTag("Ground") || collision.collider.GetComponent<PlatformEffector2D>() != null)
        {
            if (rb != null && rb.linearVelocity.y > 0.1f) return;
            foreach (ContactPoint2D contact in collision.contacts)
            {
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
        {
            isGrounded = false;
            if (collision.collider == currentPlatform)
            {
                currentPlatform = null;
            }
            if (jumpsLeft == maxJumps)
            {
                jumpsLeft = maxJumps - 1;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (collision.name.StartsWith(this.gameObject.name)) return;

            R_Body_Check(collision);
        }
    }

    private void R_Body_Check(Collider2D collision)
    {
        Rigidbody2D bulletRb = collision.GetComponent<Rigidbody2D>();
        if (bulletRb != null && rb != null && shieldTimer <= 0)
        {
            float pushDirection = Mathf.Sign(bulletRb.linearVelocity.x);
            Bullet bulletScript = collision.GetComponent<Bullet>();

            if (bulletScript != null)
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

                float preservedYVelocity = rb.linearVelocity.y;
                rb.linearVelocity = new Vector2(pushDirection * finalKnockback, preservedYVelocity);
            }
            Destroy(collision.gameObject);
        }
    }

    private System.Collections.IEnumerator TemporaryDrop(Collider2D platformCollider, Collider2D playerCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        isGrounded = false;

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

    void UpdateGunVisual()
    {
        if (gunSpriteRenderer != null && currentGun != null)
        {
            gunSpriteRenderer.sprite = currentGun.gunSprite;
        }
    }
}