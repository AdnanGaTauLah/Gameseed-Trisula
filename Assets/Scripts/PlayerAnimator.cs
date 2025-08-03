using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerWallMechanics playerWallMechanics;
    [SerializeField] private PlayerGrindMechanics playerGrindMechanics;

    private Animator animator;
    private Rigidbody2D rb;

    private bool wasGroundedLastFrame;
    private bool wasStunnedLastFrame;

    private readonly int isGroundedHash = Animator.StringToHash("isGrounded");
    private readonly int xVelocityHash = Animator.StringToHash("xVelocity");
    private readonly int yVelocityHash = Animator.StringToHash("yVelocity");
    private readonly int isWallSlidingHash = Animator.StringToHash("isWallSliding");
    private readonly int isGrindingHash = Animator.StringToHash("isGrinding");
    private readonly int hurtTriggerHash = Animator.StringToHash("HurtTrigger");
    private readonly int jumpTriggerHash = Animator.StringToHash("JumpTrigger");
    private readonly int doubleJumpTriggerHash = Animator.StringToHash("DoubleJumpTrigger");
    private readonly int landTriggerHash = Animator.StringToHash("LandTrigger");

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Read states from other scripts
        float absoluteXVelocity = Mathf.Abs(rb.velocity.x);
        float yVelocity = rb.velocity.y;
        bool isGrounded = playerMovement._isGrounded;
        bool isWallSliding = playerWallMechanics.IsWallActionActive;
        bool isGrinding = playerGrindMechanics.IsGrinding;
        // This requires a public property `public bool IsStunned => _isStunned;` in PlayerMovement.cs
        // bool isStunned = playerMovement.IsStunned; 

        // Set the continuous parameters
        animator.SetBool(isGroundedHash, isGrounded);
        animator.SetFloat(xVelocityHash, absoluteXVelocity);
        animator.SetFloat(yVelocityHash, yVelocity);
        animator.SetBool(isWallSlidingHash, isWallSliding);
        animator.SetBool(isGrindingHash, isGrinding);

        // Handle Triggers
        HandleJumpTriggers();
        HandleLandTrigger(isGrounded);
        // HandleHurtTrigger(isStunned);

        // Update state trackers for the next frame
        wasGroundedLastFrame = isGrounded;
        // wasStunnedLastFrame = isStunned;
    }

    private void HandleJumpTriggers()
    {
        // --- NEW CONDITION ---
        // If we are in an interact zone, do not trigger any jump animations.
        if (playerMovement.IsInInteractZone) return;
        // ---------------------

        if (InputManager.JumpWasPressed)
        {
            if (playerMovement._numberofJumpUsed > 0 && !playerMovement._isGrounded && !playerWallMechanics.IsWallActionActive)
            {
                animator.SetTrigger(doubleJumpTriggerHash);
            }
            else
            {
                animator.SetTrigger(jumpTriggerHash);
            }
        }
    }

    // --- THIS IS THE BUG LOCATION ---
    // This function determines when to fire the LandTrigger.
    private void HandleLandTrigger(bool isGrounded)
    {
        // If we weren't on the ground last frame, but we are now, we have landed.
        if (!wasGroundedLastFrame && isGrounded)
        {
            animator.SetTrigger(landTriggerHash);
        }
    }

    private void HandleHurtTrigger(bool isStunned)
    {
        // If we weren't stunned last frame, but we are now, we just got hit.
        if (!wasStunnedLastFrame && isStunned)
        {
            animator.SetTrigger(hurtTriggerHash);
        }
    }
}
