using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1.5f;
    public float jumpForce = 3.75f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim; // Added Animator reference
    private bool isGrounded;
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Automatically finds the Animator component

        if (groundCheckPoint == null)
        {
            Debug.LogError($"[MISSING ASSIGNMENT] '{gameObject.name}' needs a GroundCheckPoint assigned!", gameObject);
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        // Send movement value to the Animator to trigger the foot shuffle
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(horizontalInput));
        }

        // Jump (Space or Up Arrow)
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Drop Down (Down Arrow)
        if (Input.GetKeyDown(KeyCode.DownArrow) && isGrounded)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheckPoint.position, checkRadius, groundLayer);
            
            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)) 
                    continue;

                Collider2D platformCollider = hit.GetComponent<Collider2D>();
                Collider2D playerCollider = GetComponent<Collider2D>();

                if (platformCollider != null && playerCollider != null)
                {
                    StartCoroutine(TemporaryDrop(platformCollider, playerCollider));
                    break;
                }
            }
        }

        FlipSprite();
    }

    private System.Collections.IEnumerator TemporaryDrop(Collider2D platformCollider, Collider2D playerCollider)
    {
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.25f);
        if (platformCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        if (groundCheckPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheckPoint.position, checkRadius, groundLayer);
            bool foundGround = false;

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)) 
                    continue;

                foundGround = true; 
                break;
            }

            isGrounded = foundGround;
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

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
        }
    }
}