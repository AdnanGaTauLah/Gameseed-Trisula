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
    private GameObject _currentWallObject;
    

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
        RaycastHit2D hit = Physics2D.BoxCast(_wallCheckPoint.position, _wallCheckSize,
                0f, new Vector2(castDirection, 0f), 0.1f, _stats.WallLayer
            );
        if (hit.collider != null)
        {
            _isTouchingWall = true;
            _currentWallObject = hit.collider.gameObject; // Simpan GameObject-nya
        }
        else
        {
            _isTouchingWall = false;
            _currentWallObject = null;
        }
    }

    private void HandleWallSlide()
    { 
        float moveInputDirection = InputManager.Movement.x;
    float playerFacingDirection = _playerMovement._isFacingRight ? 1f : -1f;

    // Kondisi 1: Apakah secara fisik memungkinkan untuk berada di dinding?
    bool canBeOnWall = _isTouchingWall && !_playerMovement._isGrounded;
    // Kondisi 2: Apakah pemain secara sengaja menekan tombol ke arah dinding?
    bool isPushingAgainstWall = moveInputDirection * playerFacingDirection > 0; // Cek jika arah input sama dengan arah hadap
    // Pemain hanya akan masuk ke status wall slide jika KEDUA kondisi terpenuhi
    if (canBeOnWall && isPushingAgainstWall)
    {
        // Jika ini adalah frame PERTAMA saat wall slide dimulai...
        if (!_isWallSliding)
        {
            _isWallSliding = true;
            if (_playerMovement.IsNewWall(_currentWallObject))
            {
                _playerMovement.ResetJump();
            }
            _wallStickTimer = _stats.WallStickTime;
        }

        // Logika untuk menempel sesaat (wall stick)
        if (_wallStickTimer > 0)
        {
            _rb.velocity = new Vector2(0, 0);
            _wallStickTimer -= Time.deltaTime;
        }
        else // Setelah "lengket", baru mulai merosot
        {
            _rb.velocity = new Vector2(0f, -_stats.WallSlideSpeed);
        }
    }
    else // Jika pemain tidak lagi di dinding ATAU melepaskan tombol arah
    {
        // Jika sebelumnya sedang sliding, reset timer agar siap untuk slide berikutnya
        if (_isWallSliding)
        {
            _wallStickTimer = 0; 
        }
        _isWallSliding = false;
    }
    }

    private void HandleWallJump()
    {
        if (_isWallSliding && InputManager.JumpWasPressed)
        {
            float jumpDirection = _playerMovement._isFacingRight ? -1f : 1f;

            _playerMovement.PerformWallJump(jumpDirection);
            // --- CATAT DINDING INI KE MEMORI ---
            _playerMovement.SetLastWall(_currentWallObject);

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
