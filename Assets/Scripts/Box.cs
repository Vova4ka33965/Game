using UnityEngine;
using System.Collections;

public class Box : MonoBehaviour
{
    [Header("Box Physics Settings")]
    [Tooltip("Force applied when pushing")]
    [Range(1, 20)] public float pushForce = 5f;
    [Tooltip("Mass of the box")]
    [Range(0.1f, 10f)] public float boxMass = 2f;
    [Tooltip("Max velocity to prevent sliding")]
    [Range(0.1f, 5f)] public float maxSlideSpeed = 2f;
    [Tooltip("How fast box stops when not pushed")]
    [Range(0.1f, 5f)] public float dragForce = 2f;

    [Header("Rotation Settings")]
    [Tooltip("Should box rotate on slopes?")]
    public bool allowRotation = true;
    [Tooltip("How quickly box rotates on slopes")]
    [Range(0.1f, 5f)] public float rotationSpeed = 1f;
    [Tooltip("Max rotation angle on slopes")]
    [Range(0, 45)] public float maxSlopeAngle = 30f;

    [Header("Box Visual")]
    public Sprite boxSprite;
    [Tooltip("Color tint for the box")]
    public Color boxColor = Color.white;

    [Header("Debug")]
    public bool showDebugInfo = false;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isBeingPushed = false;
    private GameObject playerPushing;
    private float timeSinceLastPush = 0f;
    private bool isOnSlope = false;
    private float currentSlopeAngle = 0f;

    void Awake()
    {
        SetupComponents();
        SetupPhysics();
        SetupVisuals();
    }

    void SetupComponents()
    {
        // Rigidbody
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Sprite Renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && boxSprite != null)
        {
            spriteRenderer.sprite = boxSprite;
        }

        // Collider
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.9f, 0.9f);
        }

        // Tag
        if (!gameObject.CompareTag("Box"))
        {
            gameObject.tag = "Box";
        }
    }

    void SetupPhysics()
    {
        // Rigidbody settings
        rb.mass = boxMass;
        rb.linearDamping = dragForce * 0.5f; // Linear drag
        rb.angularDamping = dragForce * 2f; // Higher angular drag for stability
        rb.gravityScale = 3f; // Higher gravity for better feel on slopes
        rb.constraints = allowRotation ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezeRotation;

        // Setup physics material for better slope handling
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            PhysicsMaterial2D material = new PhysicsMaterial2D();
            material.friction = 0.8f;
            material.bounciness = 0.1f;
            collider.sharedMaterial = material;
        }
    }

    void SetupVisuals()
    {
        if (spriteRenderer != null)
        {
            if (boxSprite != null)
            {
                spriteRenderer.sprite = boxSprite;
            }
            spriteRenderer.color = boxColor;
            spriteRenderer.sortingLayerName = "Objects";
        }
    }

    void Update()
    {
        // Update push timer
        if (isBeingPushed)
        {
            timeSinceLastPush = 0f;
        }
        else
        {
            timeSinceLastPush += Time.deltaTime;
        }

        // Apply additional drag when not pushed for a while
        if (timeSinceLastPush > 0.5f && rb.linearVelocity.magnitude > 0.1f)
        {
            rb.linearVelocity *= 0.95f; // Natural slowing
        }

        // Limit maximum slide speed on slopes
        if (rb.linearVelocity.magnitude > maxSlideSpeed && !isBeingPushed && isOnSlope)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSlideSpeed;
        }

        // Handle pushing
        if (isBeingPushed && playerPushing != null)
        {
            PushBox();
        }

        // Handle rotation on slopes
        if (allowRotation && isOnSlope)
        {
            UpdateRotationOnSlope();
        }
    }

    void FixedUpdate()
    {
        // Check if we're on a slope
        CheckSlope();
    }

    void CheckSlope()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f);
        if (hit.collider != null)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            if (angle > 5f && angle <= maxSlopeAngle)
            {
                isOnSlope = true;
                currentSlopeAngle = angle * Mathf.Sign(Vector3.Cross(Vector2.up, hit.normal).z);
            }
            else
            {
                isOnSlope = false;
                currentSlopeAngle = 0f;
            }
        }
        else
        {
            isOnSlope = false;
            currentSlopeAngle = 0f;
        }
    }

    void UpdateRotationOnSlope()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            // Calculate target rotation based on slope
            float targetZRotation = -currentSlopeAngle * Mathf.Sign(rb.linearVelocity.x) * 0.5f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);

            // Smoothly rotate to match slope when moving
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,
                Time.deltaTime * rotationSpeed);
        }
        else
        {
            // Return to upright when stationary
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity,
                Time.deltaTime * rotationSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.CompareTag("FirePlayer") ||
             collision.gameObject.CompareTag("WaterPlayer")) &&
            IsPlayerPushing(collision))
        {
            isBeingPushed = true;
            playerPushing = collision.gameObject;

            // Small visual feedback
            StartCoroutine(PushEffect());
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == playerPushing)
        {
            isBeingPushed = false;
            playerPushing = null;
        }
    }

    bool IsPlayerPushing(Collision2D collision)
    {
        if (collision.contacts.Length == 0) return false;

        Vector2 contactPoint = collision.contacts[0].point;
        Vector2 boxCenter = transform.position;

        float playerInput = 0f;

        // Try to get input from player controllers
        FirePlayerController firePlayer = collision.gameObject.GetComponent<FirePlayerController>();
        WaterPlayerController waterPlayer = collision.gameObject.GetComponent<WaterPlayerController>();

        if (firePlayer != null)
        {
            playerInput = firePlayer.GetHorizontalInput();
        }
        else if (waterPlayer != null)
        {
            playerInput = waterPlayer.GetHorizontalInput();
        }

        // Check if player is pushing in the right direction
        float pushDirection = Mathf.Sign(playerInput);
        float contactDirection = Mathf.Sign(contactPoint.x - boxCenter.x);

        return Mathf.Abs(playerInput) > 0.1f &&
               Mathf.Abs(pushDirection - contactDirection) < 0.1f;
    }

    void PushBox()
    {
        if (playerPushing == null) return;

        float playerInput = 0f;

        // Get player input
        FirePlayerController firePlayer = playerPushing.GetComponent<FirePlayerController>();
        WaterPlayerController waterPlayer = playerPushing.GetComponent<WaterPlayerController>();

        if (firePlayer != null)
        {
            playerInput = firePlayer.GetHorizontalInput();
        }
        else if (waterPlayer != null)
        {
            playerInput = waterPlayer.GetHorizontalInput();
        }

        if (Mathf.Abs(playerInput) > 0.1f)
        {
            // Calculate push direction based on current slope
            Vector2 pushDirection = new Vector2(playerInput, 0).normalized;

            // Adjust force for slopes
            float slopeMultiplier = isOnSlope ? (1f + Mathf.Abs(currentSlopeAngle) / 45f) : 1f;
            float forceMultiplier = pushForce * (1f + boxMass * 0.1f) * slopeMultiplier;

            rb.AddForce(pushDirection * forceMultiplier, ForceMode2D.Force);
        }
    }

    IEnumerator PushEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.Lerp(originalColor, Color.white, 0.2f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Show velocity
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position,
                transform.position + (Vector3)rb.linearVelocity.normalized * 0.5f);

            // Show slope info
            if (isOnSlope)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);

#if UNITY_EDITOR
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                    $"Slope: {currentSlopeAngle:F1}°", 
                    new GUIStyle { normal = { textColor = Color.green }, fontSize = 8 });
#endif
            }

            // Show mass and speed
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, 
                $"Mass: {boxMass}\nSpeed: {rb.linearVelocity.magnitude:F2}", 
                new GUIStyle { normal = { textColor = Color.red }, fontSize = 8 });
#endif
        }
    }

    // Public methods
    public void SetMass(float mass)
    {
        boxMass = Mathf.Clamp(mass, 0.1f, 10f);
        if (rb != null) rb.mass = boxMass;
    }

    public void SetPushForce(float force)
    {
        pushForce = Mathf.Clamp(force, 1, 20);
    }

    public void ToggleRotation(bool allow)
    {
        allowRotation = allow;
        if (rb != null)
        {
            rb.constraints = allow ? RigidbodyConstraints2D.None :
                RigidbodyConstraints2D.FreezeRotation;
        }
    }
}