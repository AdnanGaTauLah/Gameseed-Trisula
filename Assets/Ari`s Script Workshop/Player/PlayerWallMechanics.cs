using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallMechanics : MonoBehaviour
{
   private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private PlayerMovementStats _stats;

    [Header("Wall Detection")]
    [SerializeField] private Transform _wallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);

    private bool _isTouchingWall;
    private bool _isWallSliding;
    private float _wallJumpFreezeTimer;
    private float _wallStickTimer;

    public bool IsWallActionActive => _isWallSliding || _wallJumpFreezeTimer > 0;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerMovement = GetComponent<PlayerMovement>();
        _stats = _playerMovement.MoveStats;
    }

    void Update()
    {
        if (_wallJumpFreezeTimer > 0)
        {
            _wallJumpFreezeTimer -= Time.deltaTime;
            return;
        }

        CheckIfTouchingWall();
        HandleWallSlide();
        HandleWallJump();
    }

    private void CheckIfTouchingWall()
    {
        float castDirection = _playerMovement._isFacingRight ? 1f : -1f;
        _isTouchingWall = Physics2D.BoxCast(
            _wallCheckPoint.position,
            _wallCheckSize,
            0f,
            new Vector2(castDirection, 0f),
            0.1f,
            _stats.WallLayer
        );
    }

    private void HandleWallSlide()
    {
        float moveInputDirection = InputManager.Movement.x;
        float playerFacingDirection = _playerMovement._isFacingRight ? 1f : -1f;

        // Kondisi utama untuk semua aksi dinding
        bool canBeOnWall = _isTouchingWall && !_playerMovement._isGrounded;

        if (canBeOnWall)
        {
            // Cek jika pemain menekan tombol ke arah dinding
            bool isPushingAgainstWall = moveInputDirection == playerFacingDirection;

            // Jika pertama kali menempel di dinding
            if (!_isWallSliding)
            {
                _isWallSliding = true;
                _playerMovement.ResetJump();
                // Mulai timer "lengket"
                _wallStickTimer = _stats.WallStickTime;
            }

            // Jika sedang menempel dan timer masih berjalan
            if (_wallStickTimer > 0)
            {
                // Tahan player agar tidak jatuh (efek lengket)
                _rb.velocity = new Vector2(0, 0);

                // Kurangi timer hanya jika pemain menekan ke arah dinding
                if (isPushingAgainstWall)
                {
                    _wallStickTimer -= Time.deltaTime;
                }
            }
            else // Setelah timer habis, baru mulai merosot
            {
                // Pemain hanya akan merosot jika masih menekan tombol ke arah dinding
                if (isPushingAgainstWall)
                {
                    _rb.velocity = new Vector2(0f, -_stats.WallSlideSpeed);
                }
                else // Jika tombol dilepas, pemain akan jatuh bebas
                {
                    _isWallSliding = false;
                }
            }
        }
        else // Jika tidak lagi menyentuh dinding
        {
            _isWallSliding = false;
        }
    }

    private void HandleWallJump()
    {
        if (_isWallSliding && InputManager.JumpWasPressed)
        {
            float jumpDirection = _playerMovement._isFacingRight ? -1f : 1f;

            // --- CUKUP PANGGIL METODE DARI PLAYER MOVEMENT ---
            _playerMovement.PerformWallJump(jumpDirection);
            // ------------------------------------------------

            _wallJumpFreezeTimer = _stats.WallJumpInputFreezeTime;
            _isWallSliding = false; // Langsung hentikan status slide
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_wallCheckPoint == null) return;
        Gizmos.color = Color.yellow;
        // Gunakan property publik yang aman
        float castDirection = _playerMovement != null && _playerMovement._isFacingRight ? 1f : -1f;
        Vector3 checkCenter = _wallCheckPoint.position + new Vector3(0.1f * castDirection, 0);
        Gizmos.DrawWireCube(checkCenter, _wallCheckSize);
    }
}
