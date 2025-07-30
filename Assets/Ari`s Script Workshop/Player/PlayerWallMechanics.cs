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

    // Properti ini akan dibaca oleh skrip lain untuk tahu apakah script ini sedang aktif
    public bool IsWallActionActive => _isWallSliding || _wallJumpFreezeTimer > 0;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerMovement = GetComponent<PlayerMovement>();
        _stats = _playerMovement.MoveStats;
    }

    void Update()
    {
        // Hitung mundur timer "freeze" setelah wall jump
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
        // Gunakan arah hadap player dari PlayerMovement untuk menentukan arah deteksi
        float castDirection = _playerMovement._isFacingRight ? 1f : -1f;
        
        // Cek dinding menggunakan BoxCast
        _isTouchingWall = Physics2D.BoxCast(
            _wallCheckPoint.position, 
            _wallCheckSize, 
            0f, 
            new Vector2(castDirection, 0f), 
            0.1f, // Jarak kecil untuk deteksi
            _stats.WallLayer // Kita asumsikan dinding ada di layer yang sama dengan tanah
            );
    }

    private void HandleWallSlide()
    {
        // Kondisi untuk wall slide: menyentuh dinding, tidak di tanah, dan sedang jatuh
        bool canWallSlide = _isTouchingWall && !_playerMovement._isGrounded && _rb.velocity.y < 0;

        if (canWallSlide)
        {
            _isWallSliding = true;
            
            // Batasi kecepatan jatuh sesuai kecepatan wall slide
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -_stats.WallSlideSpeed));
            
            // Reset jumlah lompatan saat pertama kali menempel ke dinding
            _playerMovement.ResetJump();
        }
        else
        {
            _isWallSliding = false;
        }
    }

    private void HandleWallJump()
    {
        if (_isWallSliding && InputManager.JumpWasPressed)
        {
            // Tentukan arah lompatan menjauhi dinding
            float jumpDirection = _playerMovement._isFacingRight ? -1f : 1f;
            // Hentikan kecepatan saat ini untuk lompatan yang konsisten
            _rb.velocity = Vector2.zero;
            _playerMovement.VerticalVelocity = 0;
            // Panggil metode untuk menghabiskan semua jatah lompat udara
            _playerMovement.ConsumeAllAirJumps();
            // Terapkan gaya lompatan
            _rb.AddForce(new Vector2(_stats.WallJumpForce.x * jumpDirection, _stats.WallJumpForce.y), ForceMode2D.Impulse);
            // "Bekukan" input player sementara agar tidak langsung menempel kembali
            _wallJumpFreezeTimer = _stats.WallJumpInputFreezeTime;
            // Hentikan status wall slide
            _isWallSliding = false;
        }
    }

    // Untuk visualisasi di editor, sangat membantu saat debugging
    private void OnDrawGizmosSelected()
    {
        if (_wallCheckPoint == null) return;
        
        Gizmos.color = Color.yellow;
        float castDirection = _playerMovement != null && _playerMovement._isFacingRight ? 1f : -1f;
        
        Vector3 checkCenter = _wallCheckPoint.position + new Vector3(0.1f * castDirection, 0);
        Gizmos.DrawWireCube(checkCenter, _wallCheckSize);
    }
}
