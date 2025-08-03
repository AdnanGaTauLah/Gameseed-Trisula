using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallMechanics : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private PlayerMovementStats _stats;
    private AudioSource _playerAudioSource; // --- AUDIO --- Reference to the player's main audio source

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
        _playerAudioSource = GetComponent<AudioSource>(); // --- AUDIO --- Get component
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
        RaycastHit2D hit = Physics2D.BoxCast(_wallCheckPoint.position, _wallCheckSize, 0f, new Vector2(castDirection, 0f), 0.2f, _stats.WallLayer);
        if (hit.collider != null)
        {
            _isTouchingWall = true;
            _currentWallObject = hit.collider.gameObject;
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

        bool canBeOnWall = _isTouchingWall && !_playerMovement._isGrounded;
        bool isPushingAgainstWall = moveInputDirection * playerFacingDirection > 0;

        if (canBeOnWall && isPushingAgainstWall)
        {
            if (!_isWallSliding)
            {
                _isWallSliding = true;
                if (_playerMovement.IsNewWall(_currentWallObject))
                {
                    _playerMovement.ResetJump();
                }
                _wallStickTimer = _stats.WallStickTime;

                // --- AUDIO CALL (START LOOP) ---
                AudioManager.Instance.StartLoopingSound(_playerAudioSource, "Player_WallSlide_Loop");
                // -------------------------------
            }

            if (_wallStickTimer > 0)
            {
                _rb.velocity = new Vector2(0, 0);
                _wallStickTimer -= Time.deltaTime;
            }
            else
            {
                _rb.velocity = new Vector2(0f, -_stats.WallSlideSpeed);
            }
        }
        else
        {
            if (_isWallSliding)
            {
                _wallStickTimer = 0;

                // --- AUDIO CALL (STOP LOOP) ---
                AudioManager.Instance.StopLoopingSound(_playerAudioSource);
                // ------------------------------
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
            _playerMovement.SetLastWall(_currentWallObject);
            _wallJumpFreezeTimer = _stats.WallJumpInputFreezeTime;
            _isWallSliding = false;

            // --- AUDIO CALL (STOP LOOP) ---
            // Ensure the wall slide sound stops immediately on jump
            AudioManager.Instance.StopLoopingSound(_playerAudioSource);
            // ------------------------------
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_wallCheckPoint == null) return;
        // Cek apakah sedang menyentuh dinding (berdasarkan hasil dari Update)
        if (_isTouchingWall)
        {
            // Jika ya, ubah warna jadi merah
            Gizmos.color = Color.red;
        }
        else
        {
            // Jika tidak, biarkan warna kuning
            Gizmos.color = Color.yellow;
        }
        // Logika untuk menggambar kotak tetap sama
        float castDirection = _playerMovement != null && _playerMovement._isFacingRight ? 1f : -1f;
        Vector3 checkCenter = _wallCheckPoint.position + new Vector3(0.1f * castDirection, 0);
        Gizmos.DrawWireCube(checkCenter, _wallCheckSize);
    }
}
