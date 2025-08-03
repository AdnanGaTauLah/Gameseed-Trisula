using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))] // Ensure the player has an AudioSource for looping sounds
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    private Rigidbody2D _rb;
    private PlayerWallMechanics _wallMechanics;
    private PlayerGrindMechanics _grindMechanics;
    private AudioSource _audioSource; // --- AUDIO --- Reference for looping sounds

    private GameObject _lastWallJumpedFrom;
    private bool _isStunned = false;

    private Vector2 _moveVelocity;
    public bool _isFacingRight = true;

    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    public bool _isGrounded;
    private bool _wasGroundedLastFrame; // --- AUDIO --- To detect landing
    private bool _bumpedHead;

    public float VerticalVelocity { get; set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    public int _numberofJumpUsed;

    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;

    private float _coyoteTimer;
    public bool IsFrozen { set { _isStunned = value; } }
    public bool IsInInteractZone { get; set; } = false;


    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        _grindMechanics = GetComponentInChildren<PlayerGrindMechanics>();
        _audioSource = GetComponent<AudioSource>(); // --- AUDIO --- Get the component
        // Add this line inside the Awake() function of PlayerMovement.cs
        // GameManager.Instance.RegisterPlayer(this.GetComponent<PlayerController>());
        // // Or if PlayerMovement and PlayerController are the same script:
        GameManager.Instance.RegisterPlayer(this);
    }

    private void Update()
    {
        if (_isStunned) return;

        CountTimers();
        JumpChecks();

        if ((_wallMechanics != null && _wallMechanics.IsWallActionActive) || (_grindMechanics != null && _grindMechanics.IsGrinding))
        {
            return;
        }
    }
    private void FixedUpdate()
    {
        if (_isStunned) return;

        CollisionCheck();
        Jump();

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }

        // --- AUDIO --- Handle landing sound
        if (_isGrounded && !_wasGroundedLastFrame)
        {
            AudioManager.Instance.PlaySound("Player_Land", transform.position);
        }
        _wasGroundedLastFrame = _isGrounded;
        // ---------------

        if ((_wallMechanics != null && _wallMechanics.IsWallActionActive) || (_grindMechanics != null && _grindMechanics.IsGrinding))
        {
            return;
        }
    }
    public void PerformWallJump(float jumpDirection)
    {
        if (!_isJumping) { _isJumping = true; }
        _numberofJumpUsed = MoveStats.NumberofJumpsAllowed;
        _rb.velocity = Vector2.zero;
        VerticalVelocity = 0;
        Vector2 force = new Vector2(MoveStats.WallJumpForce.x * jumpDirection, MoveStats.WallJumpForce.y);
        _rb.AddForce(force, ForceMode2D.Impulse);
        if (jumpDirection > 0) { Turn(true); } else { Turn(false); }

        // --- AUDIO CALL ---
        AudioManager.Instance.PlaySound("Player_WallJump", transform.position);
        // ------------------
    }

    public void ResetJump() { _numberofJumpUsed = 0; }
    public void SetLastWall(GameObject wall) { _lastWallJumpedFrom = wall; }
    public bool IsNewWall(GameObject wall) { return _lastWallJumpedFrom != wall; }

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);
            Vector2 targetVelocity;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            Vector2 targetVelocity = Vector2.zero;
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);
        }
        _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0) { Turn(false); }
        else if (!_isFacingRight && moveInput.x > 0) { Turn(true); }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            _isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void JumpChecks()
    {
        // --- NEW CONDITION ---
        // If we are in an interact zone, do not process any jump checks.
        if (IsInInteractZone) return;
        // ---------------------

        if (_wallMechanics != null && _wallMechanics.IsWallActionActive) { return; }
        if (InputManager.JumpWasPressed) { _jumpBufferTimer = MoveStats.JumpBufferTime; _jumpReleaseDuringBuffer = false; }
        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f) { _jumpReleaseDuringBuffer = true; }
            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold) { _isPastApexThreshold = false; _isFastFalling = true; _fastFallTime = MoveStats.TimeForUpwardsCancel; VerticalVelocity = 0f; }
                else { _isFastFalling = true; _fastFallReleaseSpeed = VerticalVelocity; }
            }
        }
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);
            if (_jumpReleaseDuringBuffer) { _isFastFalling = true; _fastFallReleaseSpeed = VerticalVelocity; }
        }
        else if (_jumpBufferTimer > 0f && _isJumping && _numberofJumpUsed < MoveStats.NumberofJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        else if (_jumpBufferTimer > 0f && _isFalling && _numberofJumpUsed < MoveStats.NumberofJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false; _isFalling = false; _isFastFalling = false; _fastFallTime = 0f; _isPastApexThreshold = false; _numberofJumpUsed = 0; _lastWallJumpedFrom = null;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int jumpUsed)
    {
        if (!_isJumping) { _isJumping = true; }
        _jumpBufferTimer = 0f;
        _numberofJumpUsed += jumpUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;

        // --- AUDIO CALL ---
        AudioManager.Instance.PlaySound("Player_Jump", transform.position);
        // ------------------
    }

    private void Jump()
    {
        if (_isJumping)
        {
            if (_bumpedHead) { _isFastFalling = true; }
            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);
                if (_apexPoint > MoveStats.ApexTreshold)
                {
                    if (!_isPastApexThreshold) { _isPastApexThreshold = true; _timePastApexThreshold = 0f; }
                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime) { VerticalVelocity = 0f; }
                        else { VerticalVelocity = -0.01f; }
                    }
                }
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold) { _isPastApexThreshold = false; }
                }
            }
            else
            {
                if (!_isFalling) { _isFalling = true; }
                if (!_isFastFalling) { VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime; }
            }
        }
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel) { VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime; }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel) { VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel)); }
            _fastFallTime += Time.fixedDeltaTime;
        }
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling) { _isFalling = true; }
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }

    public void ApplyKnockbackAndStun(Vector2 knockbackDirection)
    {
        if (!_isStunned)
        {
            StartCoroutine(KnockbackAndStunCoroutine(knockbackDirection));
        }
    }

    private IEnumerator KnockbackAndStunCoroutine(Vector2 knockbackDirection)
    {
        _isStunned = true;
        _rb.velocity = Vector2.zero;
        VerticalVelocity = 0;
        _moveVelocity = Vector2.zero;
        Vector2 knockbackVelocity = new Vector2(knockbackDirection.x * MoveStats.KnockbackForce, MoveStats.KnockbackUpwardModifier * MoveStats.KnockbackForce);
        _rb.velocity = knockbackVelocity;

        // --- AUDIO CALL ---
        AudioManager.Instance.PlaySound("Player_Hurt", transform.position);
        // ------------------

        yield return new WaitForSeconds(MoveStats.StunDuration);
        _isStunned = false;
    }

    public void BounceOnTrampoline()
    {
        _isJumping = true; _isFalling = false; _isFastFalling = false; _fastFallTime = 0f; _isPastApexThreshold = false; _numberofJumpUsed = 0; _lastWallJumpedFrom = null;
        VerticalVelocity = MoveStats.TrampolineBounceForce;
        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);

        // --- AUDIO CALL ---
        AudioManager.Instance.PlaySound("Trampoline_Bounce", transform.position);
        // ------------------
    }

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);
        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);
        if (_groundHit.collider != null) { _isGrounded = true; } else { _isGrounded = false; }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);
        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null) { _bumpedHead = true; } else { _bumpedHead = false; }
    }

    private void CollisionCheck() { IsGrounded(); BumpedHead(); }

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;
        if (!_isGrounded) { _coyoteTimer -= Time.deltaTime; }
        else { _coyoteTimer -= MoveStats.JumpCoyoteTime; }
    }
}
