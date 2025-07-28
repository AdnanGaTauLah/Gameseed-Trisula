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

    private Vector2 moveInput;

    // --- NEW ---
    private bool controlsEnabled = true;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!controlsEnabled) return; // If controls are disabled, do nothing in Update.

        if (isGrounded) { coyoteTimeCounter = coyoteTime; }
        else { coyoteTimeCounter -= Time.deltaTime; }
        jumpBufferCounter -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!controlsEnabled) return; // If controls are disabled, do nothing in FixedUpdate.

        CheckIfGrounded();
        HandleHorizontalMovement();
        HandleGravity();
    }

    // --- NEW PUBLIC METHOD ---
    public void SetControlsEnabled(bool isEnabled)
    {
        controlsEnabled = isEnabled;
        if (!isEnabled)
        {
            // Immediately stop all movement when disabled
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
        }
    }

    private void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.bounds.size,
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        isGrounded = hit.collider != null;
    }

    private void HandleHorizontalMovement()
    {
        float horizontalForce = moveInput.x * moveSpeed;
        rb.AddForce(new Vector2(horizontalForce, 0f), ForceMode2D.Force);

        Vector2 currentVelocity = rb.velocity;
        if (Mathf.Abs(currentVelocity.x) > maxHorizontalVelocity)
        {
            rb.velocity = new Vector2(Mathf.Sign(currentVelocity.x) * maxHorizontalVelocity, currentVelocity.y);
        }

        if (Mathf.Abs(moveInput.x) < 0.1f && isGrounded)
        {
            if (Mathf.Abs(rb.velocity.x) < 0.1f)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x * stopDamping, rb.velocity.y);
            }
        }
    }

    private void HandleGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

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

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + Vector3.down * groundCheckDistance, boxCollider.bounds.size);
    }

}
