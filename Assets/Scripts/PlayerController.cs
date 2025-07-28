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
    [SerializeField] private float stopDamping = 0.9f; // Damping factor for stopping

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    private bool isGrounded;

    // Input storage
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        CheckIfGrounded();
        HandleHorizontalMovement();
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

    public void OnMove(InputAction.CallbackContext context)
    {
        // --- MOVE DEBUGGING ---
        Debug.Log($"'OnMove' event fired! Value: {context.ReadValue<Vector2>()}");
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"'OnJump' event fired! Is the player grounded? -> {isGrounded}");
        }

        if (context.performed && isGrounded)
        {
            Debug.Log("Conditions met! Applying jump force.");
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.bounds.center + Vector3.down * groundCheckDistance, boxCollider.bounds.size);
    }
}
