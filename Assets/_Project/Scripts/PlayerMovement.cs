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

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded = false;
    private float horizontalInput;
    
    // --- AMMO & LOCKOUT SYSTEM ---
    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private bool isMovementLocked = false;
    private float lockTimer = 0f;

    // Track the current platform we are standing on for dropping
    private Collider2D currentPlatform;

    // Number of lives
    public int lives = 3;

    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponent<Animator>();

    if (currentGun == null)
    {
        Debug.LogError(gameObject.name + " has no gun assigned!");
        return;
    }

    currentAmmo = currentGun.maxAmmo;
}

    void Update()
    {
        //Debug.Log(gameObject.name + " locked: " + isMovementLocked);
        // 1. Handle reload timer countdown
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

        // 2. Handle the movement lock timer (For Recoil/Knockback)
        if (isMovementLocked)
        {
            lockTimer -= Time.deltaTime;
            if (lockTimer <= 0f)
            {
                isMovementLocked = false; 
            }
        }

        // 3. Horizontal Input calculation
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

        // 5. Jumping Input
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false; 
        }

        // 6. Platform Drop Input (FIXED SYSTEM)
        if (Input.GetKeyDown(dropKey) && isGrounded && currentPlatform != null)
        {
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                StartCoroutine(TemporaryDrop(currentPlatform, playerCollider));
            }
        }

        FlipSprite();
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

    // --- COLLISION TRACKING FOR GROUND AND PLATFORMS ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Cache the platform we just landed on
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
        
        // Clear the platform reference when we leave it
        if (collision.collider == currentPlatform)
        {
            currentPlatform = null;
        }
    }

    // --- ENEMY BULLET KNOCKBACK DETECTOR ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (collision.name.StartsWith(this.gameObject.name))
            {
                return; 
            }

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
        // Ignore collisions to fall through
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.35f); // Give enough time to clear the platform depth
        
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