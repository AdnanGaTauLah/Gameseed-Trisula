using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float maxHorizontalVelocity = 6f;
    [SerializeField] private float stopDamping = 0.9f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    private bool isGrounded;

    [Header("Feel & Polish")]
    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    // --- LEVEL 2: WALL INTERACTION ---
    [Header("Wall Interaction")]
    [SerializeField] private LayerMask wallLayer; // Set this to your "Ground" layer or a new "Wall" layer
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallSlideSpeed = 2f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 15f);
    private bool isWallSliding;
    private int wallDirection = 1; // 1 for right, -1 for left

    // --- LEVEL 2: DOUBLE JUMP ---
    private bool canDoubleJump = false;
    // --------------------------------

    private Vector2 moveInput;
    private bool controlsEnabled = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!controlsEnabled) return;

        // Grounded state updates
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true; // Reset double jump on ground
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        jumpBufferCounter -= Time.deltaTime;

        // --- LEVEL 2: WALL SLIDE LOGIC ---
        CheckWallSlide();
        // ---------------------------------
    }

    void FixedUpdate()
    {
        if (!controlsEnabled) return;

        CheckIfGrounded();

        // --- LEVEL 2: OVERRIDE MOVEMENT IF WALL SLIDING ---
        if (isWallSliding)
        {
            HandleWallSlideMovement();
        }
        else
        {
            HandleHorizontalMovement();
        }
        // -------------------------------------------------

        HandleGravity();
    }

    // ... (SetControlsEnabled, CheckIfGrounded, HandleHorizontalMovement, HandleGravity are the same) ...
    public void SetControlsEnabled(bool isEnabled)
    {
        controlsEnabled = isEnabled;
        if (!isEnabled)
        {
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
        }
    }
    private void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }
    private void HandleHorizontalMovement()
    {
        float horizontalForce = moveInput.x * moveSpeed;
        rb.AddForce(new Vector2(horizontalForce, 0f), ForceMode2D.Force);
        Vector2 currentVelocity = rb.velocity;
        if (Mathf.Abs(currentVelocity.x) > maxHorizontalVelocity) { rb.velocity = new Vector2(Mathf.Sign(currentVelocity.x) * maxHorizontalVelocity, currentVelocity.y); }
        if (Mathf.Abs(moveInput.x) < 0.1f && isGrounded)
        {
            if (Mathf.Abs(rb.velocity.x) < 0.1f) { rb.velocity = new Vector2(0f, rb.velocity.y); }
            else { rb.velocity = new Vector2(rb.velocity.x * stopDamping, rb.velocity.y); }
        }
    }
    private void HandleGravity()
    {
        if (rb.velocity.y < 0 && !isWallSliding) // Don't apply extra gravity when wall sliding
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // --- LEVEL 2: NEW METHODS ---
    private void CheckWallSlide()
    {
        // Check for a wall to the right
        RaycastHit2D wallHitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        // Check for a wall to the left
        RaycastHit2D wallHitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);

        if ((wallHitRight || wallHitLeft) && !isGrounded && rb.velocity.y < 0)
        {
            isWallSliding = true;
            wallDirection = wallHitRight ? 1 : -1; // Set wall direction
            canDoubleJump = true; // Reset double jump when touching a wall
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void HandleWallSlideMovement()
    {
        // Clamp vertical velocity to the wall slide speed
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -wallSlideSpeed));
    }

    private void HandleWallJump()
    {
        isWallSliding = false; // Break free from the wall
        Vector2 force = new Vector2(wallJumpForce.x * -wallDirection, wallJumpForce.y);
        rb.velocity = Vector2.zero; // Reset velocity for consistent jump
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    // ----------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) { moveInput = Vector2.zero; return; }
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!controlsEnabled) return;

        if (context.started) { jumpBufferCounter = jumpBufferTime; }
        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }

        // --- LEVEL 2: JUMP LOGIC REFACTOR ---
        // Prioritize Wall Jump over other jumps.
        if (jumpBufferCounter > 0f && isWallSliding)
        {
            HandleWallJump();
            jumpBufferCounter = 0f;
        }
        // Then check for a regular jump
        else if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
        // Then check for a double jump
        else if (jumpBufferCounter > 0f && canDoubleJump && !isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            canDoubleJump = false;
            jumpBufferCounter = 0f;
        }
        // ------------------------------------
    }

    private void OnDrawGizmos()
    {
        // ... (ground check gizmo is the same) ...
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + Vector3.down * groundCheckDistance, boxCollider.bounds.size);

        // --- LEVEL 2: GIZMO FOR WALL CHECK ---
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckDistance);
        // -------------------------------------
    }
}
