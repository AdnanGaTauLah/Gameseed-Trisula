using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrindMechanics : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private PlayerMovementStats _stats;

    private bool _isGrinding = false;
    private float _originalGravityScale;

    // Properti publik untuk dibaca skrip lain
    public bool IsGrinding => _isGrinding;

    void Awake()
    {
        // Karena skrip ini ada di child, kita ambil komponen dari parent
        _rb = GetComponentInParent<Rigidbody2D>();
        _playerMovement = GetComponentInParent<PlayerMovement>();
        _stats = _playerMovement.MoveStats;
        
        // Simpan gravitasi asli untuk dikembalikan nanti
        _originalGravityScale = _rb.gravityScale;
    }

    void Update()
    {
        // Izinkan pemain melompat dari rel kapan saja
        if (_isGrinding && InputManager.JumpWasPressed)
        {
            StopGrind();
            
            // Berikan gaya lompat ke atas
            _rb.velocity = new Vector2(_rb.velocity.x, 0); // Nol-kan kecepatan vertikal dulu
            _rb.AddForce(Vector2.up * _stats.GrindJumpForce, ForceMode2D.Impulse);
        }
    }

    // Metode ini berjalan otomatis saat trigger collider kita menyentuh collider lain
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kondisi untuk memulai grind:
        // 1. Belum sedang grinding
        // 2. Player sedang jatuh (velocity Y negatif)
        // 3. Objek yang disentuh ada di layer "Grindable"
        if (!_isGrinding && _rb.velocity.y < 0 && IsInLayerMask(other.gameObject.layer, _stats.GrindableLayer))
        {
            StartGrind();
        }
    }
    
    // Metode ini berjalan saat trigger collider kita berhenti menyentuh collider lain
    private void OnTriggerExit2D(Collider2D other)
    {
        // Jika sedang grinding dan keluar dari rel, hentikan grind
        if (_isGrinding && IsInLayerMask(other.gameObject.layer, _stats.GrindableLayer))
        {
            StopGrind();
        }
    }

    private void StartGrind()
    {
        _isGrinding = true;
        _playerMovement.ResetJump(); // Reset jatah lompat
        
        // Matikan gravitasi agar player tidak jatuh dari rel
        _rb.gravityScale = 0;
        
        // Atur kecepatan horizontal sesuai arah hadap player
        float grindDirection = _playerMovement._isFacingRight ? 1f : -1f;
        _rb.velocity = new Vector2(_stats.GrindSpeed * grindDirection, 0);
    }
    
    private void StopGrind()
    {
        _isGrinding = false;
        // Kembalikan gravitasi ke normal
        _rb.gravityScale = _originalGravityScale;
    }

    // Fungsi bantuan untuk mengecek layer
    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) > 0;
    }
}
