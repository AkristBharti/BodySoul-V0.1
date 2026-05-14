using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;     // Speed for left/right movement
    public float jumpHeight = 3f;   // How high the player jumps

    private Rigidbody2D rb;
    Animator animator;
    private float horizontalInput;   // Stores the left/right key presses
    public bool isGrounded = false; // Tracks if we are on the ground

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1. Listen for Left/Right arrows or A/D keys
        // Returns -1 for left, 1 for right, and 0 for nothing
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (animator != null)
        {
            animator.SetBool("isWalking", horizontalInput != 0);
            animator.SetBool("isJumping", rb.velocity.y > 0.1f);
        }

        // 2. Listen for Jump (Spacebar) ONLY if we are on the ground
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true)
        {
           
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        
        // 3. Apply the left/right movement to the Rigidbody
        // We keep rb.velocity.y so falling and jumping aren't interrupted
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    // --- Ground Detection ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") )
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") )
        {
            isGrounded = false;
        }
    }
}