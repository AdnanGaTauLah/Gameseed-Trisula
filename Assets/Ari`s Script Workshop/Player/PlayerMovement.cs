using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;
    
    private Rigidbody2D _rb;
    private PlayerWallMechanics _wallMechanics;
    private PlayerGrindMechanics _grindMechanics;
    private GameObject _lastWallJumpedFrom;
    private bool _isStunned = false;
    
    
    //movement vars
    private Vector2 _moveVelocity;
    public bool _isFacingRight = true;
    
    //collision check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    public bool _isGrounded;
    private bool _bumpedHead;
    
    //Jump vars
    public float VerticalVelocity { get; set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    public int _numberofJumpUsed;
    
    //Apex Vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;
    
    //Jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleaseDuringBuffer;
    
    //Coyote Time
    private float _coyoteTimer;
    
    
    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
        _grindMechanics = GetComponentInChildren<PlayerGrindMechanics>();
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
        
        if ((_wallMechanics != null && _wallMechanics.IsWallActionActive) || (_grindMechanics != null && _grindMechanics.IsGrinding))
        {
            return;
        }
    }
    public void PerformWallJump(float jumpDirection)
    {
        // 1. Set status player menjadi sedang melompat
        if (!_isJumping)
        {
            _isJumping = true;
        }
    
        // 2. Langsung habiskan semua jatah lompatan di udara
        _numberofJumpUsed = MoveStats.NumberofJumpsAllowed;

        // 3. Hentikan semua kecepatan saat ini untuk lompatan yang konsisten
        _rb.velocity = Vector2.zero;
        VerticalVelocity = 0;

        // 4. Terapkan gaya lompatan
        Vector2 force = new Vector2(MoveStats.WallJumpForce.x * jumpDirection, MoveStats.WallJumpForce.y);
        _rb.AddForce(force, ForceMode2D.Impulse);
    
        // 5. Pastikan player berbalik arah setelah melompat dari dinding
        if (jumpDirection > 0)
        {
            Turn(true); // Berbalik ke kanan
        }
        else
        {
            Turn(false); // Berbalik ke kiri
        }
        
    }
    
    public void ResetJump()
    {
        _numberofJumpUsed = 0;
    }
    
    public void SetLastWall(GameObject wall)
    {
        _lastWallJumpedFrom = wall;
    }

    // Metode ini untuk bertanya apakah dinding yang disentuh sekarang adalah dinding baru
    public bool IsNewWall(GameObject wall)
    {
        return _lastWallJumpedFrom != wall;
    }
    
    
    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        #region Dump Code Move
        /*
        if (moveInput != Vector2.zero)
        {
            TurnCheck(moveInput);
            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            }
            else targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x , _rb.velocity.y);
        }*/
        // Jika ada input dari pemain (tombol gerak ditekan)
        #endregion
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
            // Akselerasi menuju kecepatan target
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        // PERBAIKAN: Jika TIDAK ada input (tombol gerak dilepas)
        else
        {
            // Deselerasi (pengereman) menuju kecepatan nol
            Vector2 targetVelocity = Vector2.zero;
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, deceleration * Time.fixedDeltaTime);
        }
        // Terapkan kecepatan kalkulasi ke Rigidbody
        _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);
        
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
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

    #endregion
    
    #region Jump

    private void JumpChecks()
    {
        if (_wallMechanics != null && _wallMechanics.IsWallActionActive)
        {
            return; 
        }
        
        if (InputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleaseDuringBuffer = false;
        }

        if (InputManager.JumpWasReleased)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleaseDuringBuffer =  true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        
        //Initiate Jump With Jump Buffering And Coyote Time
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);
            if (_jumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        
        //Double Jump
        else if (_jumpBufferTimer > 0f && _isJumping && _numberofJumpUsed < MoveStats.NumberofJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }
        
        //handle Air Jump After Coyote Time has lapsed (take off an extrea jump so don`t get a bonus jump
        else if (_jumpBufferTimer > 0f && _isFalling && _numberofJumpUsed < MoveStats.NumberofJumpsAllowed - 1)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }
        
        //Landed
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberofJumpUsed = 0;
            _lastWallJumpedFrom = null;
            
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int jumpUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberofJumpUsed += jumpUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        #region Dump Code
       /* 
        //Apply Gravity While Jumping
        if (_isJumping)
        {
            //Check for head bump
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }
            //Gravity On Ascending
            if (VerticalVelocity >= 0f)
            {
                //Apex Controls
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexTreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold = Time.fixedDeltaTime;
                        if (_timePastApexThreshold < MoveStats.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                //Gravity On Descending But Not Past Apex Treshold
                else
                {
                    VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            //Gravity on Descending
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            
            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }
        
        //Jump Cut
        if (_isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f,
                    (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }
            
            _fastFallTime += Time.fixedDeltaTime;
        }
        
        //Normal Gravity While Playing
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }
        
        //Clamp Fall Speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
        */
       // Apply Gravity While Jumping
       
       #endregion
    if (_isJumping)
    {
        // Periksa jika kepala terbentur
        if (_bumpedHead)
        {
            _isFastFalling = true;
        }

        // --- LOGIKA BARU UNTUK GRAVITASI ---

        // 1. Logika saat NAIK (Ascending)
        if (VerticalVelocity >= 0f)
        {
            // Kontrol di puncak lompatan (Apex Controls)
            _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

            if (_apexPoint > MoveStats.ApexTreshold)
            {
                if (!_isPastApexThreshold)
                {
                    _isPastApexThreshold = true;
                    _timePastApexThreshold = 0f;
                }

                if (_isPastApexThreshold)
                {
                    _timePastApexThreshold += Time.fixedDeltaTime;
                    if (_timePastApexThreshold < MoveStats.ApexHangTime)
                    {
                        VerticalVelocity = 0f; // Melayang sesaat di puncak
                    }
                    else
                    {
                        VerticalVelocity = -0.01f; // Mulai turun perlahan
                    }
                }
            }
            // Gravitasi normal saat naik
            else
            {
                VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                }
            }
        }
        // 2. Logika saat TURUN (Descending)
        else
        {
            // Jika sedang turun, pastikan status _isFalling aktif
            if (!_isFalling)
            {
                _isFalling = true;
            }

            // Terapkan gravitasi jatuh (jika tidak sedang fast fall)
            if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
        }
    }

    // Jump Cut (logika ini tetap sama dan sudah benar posisinya)
    if (_isFastFalling)
    {
        if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
        {
            VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
        }
        else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
        {
            VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f,
                (_fastFallTime / MoveStats.TimeForUpwardsCancel));
        }

        _fastFallTime += Time.fixedDeltaTime;
    }

    // Normal Gravity While not jumping (saat jatuh dari pijakan)
    if (!_isGrounded && !_isJumping)
    {
        if (!_isFalling)
        {
            _isFalling = true;
        }

        VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
    }

    // Clamp Fall Speed
    VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
    _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }
    
    #endregion

    #region Stun and Knockback

    public void ApplyKnockbackAndStun(Vector2 knockbackDirection)
    {
        if (!_isStunned) // Pastikan tidak memanggil knockback berkali-kali
        {
            StartCoroutine(KnockbackAndStunCoroutine(knockbackDirection));
        }
    }

    private IEnumerator KnockbackAndStunCoroutine(Vector2 knockbackDirection)
    {
        // 1. Masuk ke mode stun
        _isStunned = true;

        // 2. Hentikan total semua kecepatan awal pemain agar knockback konsisten
        _rb.velocity = Vector2.zero;
        VerticalVelocity = 0;
        _moveVelocity = Vector2.zero;

        // 3. ATUR KECEPATAN KNOCKBACK SECARA LANGSUNG (LEBIH STABIL)
        Vector2 knockbackVelocity = new Vector2(knockbackDirection.x * MoveStats.KnockbackForce, MoveStats.KnockbackUpwardModifier * MoveStats.KnockbackForce);
        _rb.velocity = knockbackVelocity;

        // Di sini bisa memutar animasi "stun" dan sound effect
        // Contoh: animator.SetTrigger("Stunned");
        // Contoh: audioSource.PlayOneShot(stunSound);

        // 4. Tunggu selama durasi stun
        yield return new WaitForSeconds(MoveStats.StunDuration);

        // 5. Keluar dari mode stun
        _isStunned = false;
    }

    #endregion

    #region Trampoline

    public void BounceOnTrampoline()
    {
        // 1. Reset status lompat dan jatuh
        _isJumping = true;
        _isFalling = false;
        _isFastFalling = false;
        _fastFallTime = 0f;
        _isPastApexThreshold = false;
        _numberofJumpUsed = 0; // Reset jatah lompat!
        _lastWallJumpedFrom = null;

        // Di sini kamu bisa putar animasi & sound effect pantulan

        // 2. Berikan kecepatan vertikal yang kuat
        VerticalVelocity = MoveStats.TrampolineBounceForce;
        _rb.velocity = new Vector2(_rb.velocity.x, VerticalVelocity);
    }

    #endregion
    
    #region  Collision Check

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength,
            MoveStats.GroundLayer);
        if (_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else 
        {
            _isGrounded = false;
        }
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);
        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength,
            MoveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else
        {
            _bumpedHead = false;
        }

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }
            
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x /2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(
                new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth,
                    boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth,
                rayColor);
        }

        #endregion
    }

    private void CollisionCheck()
    {
        IsGrounded();
        BumpedHead();
    }
    

    #endregion
    
    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer -= MoveStats.JumpCoyoteTime;
        }
    }

    #endregion
}
