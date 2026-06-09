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
    public Vector2 throwVelocity = new Vector2(1.5f, 1.0f); // Drops it slightly forward and up out of your feet

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

    // Number of lives
    public int lives = 3;

    void Start()
    {
        // 1. Get your components right away
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 2. Check for the gun error safely
        if (currentGun == null)
        {
            Debug.LogError(gameObject.name + " has no gun assigned!");
            return; 
        }

        // 3. Initialize variables OUTSIDE the if-block so they actually execute
        currentAmmo = currentGun.maxAmmo;
        jumpsLeft = maxJumps; 
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

        // 3. Horizontal Inputs
        horizontalInput = 0f;
        if (!isMovementLocked)
        {
            if (Input.GetKey(moveLeftKey)) horizontalInput = -1f;
            else if (Input.GetKey(moveRightKey)) horizontalInput = 1f;
        }
        bool isRunning = Mathf.Abs(horizontalInput) > 0.01f;
        anim.SetBool("isRunning", isRunning);

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        if (transform.position.y < fallThreshold)
        {
            RespawnPlayer();
        }

        // 4. Shooting Input
        if (Input.GetKeyDown(shootKey)) 
        {
            if (!isReloading)
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

        // 5. BOMB DROP INPUT
        if (Input.GetKeyDown(bombThrowKey) && !isMovementLocked)
        {
            DropItem();
        }

        // 6. Double Jump Input
        if (Input.GetKeyDown(jumpKey) && jumpsLeft > 0)
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

        // Keep updating the live state so it transitions back to landing smoothly
        if (anim != null)
        {
            bool isJumping = rb.linearVelocity.y > 0.1f || !isGrounded;
            anim.SetBool("isJumping", isJumping);
        }

        // 7. Platform Drop Input
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

        FlipSprite();
    }

    void DropItem()
    {
        if (bombPrefab != null && firePoint != null)
        {
            float facingDirection = Mathf.Sign(transform.localScale.x);

            GameObject newBomb = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);
            
            Bomb bombScript = newBomb.GetComponent<Bomb>();
            if (bombScript != null) bombScript.Initialize(gameObject.name);

            Rigidbody2D bombRb = newBomb.GetComponent<Rigidbody2D>();
            if (bombRb != null)
            {
                bombRb.linearVelocity = new Vector2(facingDirection * throwVelocity.x, throwVelocity.y);
            }
        }
    }

    // --- RECOIL RECEIVER (kept for bullet impacts) ---
    public void TakeBlastKnockback(Vector2 blastVelocity)
    {
        if (rb != null)
        {
            isMovementLocked = true;
            lockTimer = 0.4f; 
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = blastVelocity; 
        }
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
        currentAmmo = maxAmmo; 
        jumpsLeft = maxJumps; 
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
                rb.linearVelocity = new Vector2(
                -shootingDirection * currentGun.recoilForce,
                rb.linearVelocity.y
                );
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
                jumpsLeft = maxJumps; 
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        if (collision.collider == currentPlatform) currentPlatform = null;

        if (jumpsLeft == maxJumps) jumpsLeft = maxJumps - 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (collision.name.StartsWith(this.gameObject.name)) return; 

            Rigidbody2D bulletRb = collision.GetComponent<Rigidbody2D>();
            if (bulletRb != null && rb != null)
            {
                float pushDirection = Mathf.Sign(bulletRb.linearVelocity.x);
                isMovementLocked = true;
                lockTimer = 0.2f; 
                rb.linearVelocity = Vector2.zero;
                rb.linearVelocity = new Vector2(pushDirection * knockbackForce, rb.linearVelocity.y);
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