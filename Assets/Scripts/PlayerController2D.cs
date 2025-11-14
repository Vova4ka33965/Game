using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController2D : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 4f;
    public float jumpForce = 8f;
    public int maxHealth = 3;
    public float invincibilityTime = 1f;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("References")]
    public Transform groundCheck;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // States
    private bool isGrounded, isFacingRight = true, isDead = false, isInvincible = false;
    private int currentHealth;
    private float invincibilityTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        CreateGroundCheckIfNeeded();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        UpdateInvincibility();
        CheckGrounded();
        HandleMovement();
        HandleJump();
        UpdateAnimations();
        FixJumpAnimation();
    }

    void UpdateInvincibility()
    {
        if (!isInvincible) return;

        invincibilityTimer -= Time.deltaTime;
        if (invincibilityTimer <= 0f)
        {
            isInvincible = false;
            spriteRenderer.color = Color.white;
        }
    }

    void CreateGroundCheckIfNeeded()
    {
        if (groundCheck != null) return;

        GameObject groundCheckObj = new GameObject("GroundCheck");
        groundCheckObj.transform.SetParent(transform);
        groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
        groundCheck = groundCheckObj.transform;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

        // Flip character
        if ((horizontalInput > 0.1f && !isFacingRight) || (horizontalInput < -0.1f && isFacingRight))
        {
            Flip();
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }

    void UpdateAnimations()
    {
        if (isDead) return;

        bool isMoving = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) ||
                       Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        animator.SetFloat("Speed", isMoving ? 1f : 0f);
        animator.SetBool("IsGrounded", isGrounded);
    }

    void FixJumpAnimation()
    {
        if (isGrounded && animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
        {
            animator.Play("Idle");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Water") && IsSufficientlyInWater(collision))
        {
            TakeDamage(maxHealth);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Water") && IsSufficientlyInWater(collision.collider))
        {
            TakeDamage(maxHealth);
        }
    }

    bool IsSufficientlyInWater(Collider2D waterCollider)
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return false;

        Bounds waterBounds = waterCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        // Check if bottom part of player is in water
        float checkHeight = playerBounds.size.y * 0.3f;
        Vector2 checkCenter = new Vector2(playerBounds.center.x, playerBounds.min.y + checkHeight * 0.5f);
        Vector2 checkSize = new Vector2(playerBounds.size.x * 0.7f, checkHeight);

        return waterBounds.Intersects(new Bounds(checkCenter, checkSize));
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        animator.SetTrigger("TakeHit");

        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        spriteRenderer.color = Color.red;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Knockback effect
            rb.linearVelocity = new Vector2(-transform.localScale.x * 3f, 5f);
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("Death");
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(FadeOut());
        Invoke(nameof(RestartLevel), 2f);
    }

    IEnumerator FadeOut()
    {
        float fadeTime = 1.5f;
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Public methods for external access
    public bool IsGrounded() => isGrounded;
    public void Heal(int healAmount) => currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
    public void Kill() => TakeDamage(maxHealth);

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}