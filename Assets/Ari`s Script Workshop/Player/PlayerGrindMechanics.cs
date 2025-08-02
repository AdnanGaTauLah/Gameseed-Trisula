using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrindMechanics : MonoBehaviour
{
    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private PlayerMovementStats _stats;
    private AudioSource _playerAudioSource; // --- AUDIO --- Reference to the player's main audio source

    private bool _isGrinding = false;
    private float _originalGravityScale;

    public bool IsGrinding => _isGrinding;

    void Awake()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _playerMovement = GetComponentInParent<PlayerMovement>();
        _stats = _playerMovement.MoveStats;
        _playerAudioSource = GetComponentInParent<AudioSource>(); // --- AUDIO --- Get component from parent
        _originalGravityScale = _rb.gravityScale;
    }

    void Update()
    {
        if (_isGrinding && InputManager.JumpWasPressed)
        {
            StopGrind();
            _rb.velocity = new Vector2(_rb.velocity.x, 0);
            _rb.AddForce(Vector2.up * _stats.GrindJumpForce, ForceMode2D.Impulse);

            // --- AUDIO CALL (ONE-SHOT) ---
            AudioManager.Instance.PlaySound("Player_GrindJump", transform.position);
            // -----------------------------
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isGrinding && _rb.velocity.y < 0 && IsInLayerMask(other.gameObject.layer, _stats.GrindableLayer))
        {
            StartGrind();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isGrinding && IsInLayerMask(other.gameObject.layer, _stats.GrindableLayer))
        {
            StopGrind();
        }
    }

    private void StartGrind()
    {
        _isGrinding = true;
        _playerMovement.ResetJump();
        _rb.gravityScale = 0;
        float grindDirection = _playerMovement._isFacingRight ? 1f : -1f;
        _rb.velocity = new Vector2(_stats.GrindSpeed * grindDirection, 0);

        // --- AUDIO CALL (START LOOP) ---
        AudioManager.Instance.StartLoopingSound(_playerAudioSource, "Player_Grind_Loop");
        // -------------------------------
    }

    private void StopGrind()
    {
        _isGrinding = false;
        _rb.gravityScale = _originalGravityScale;

        // --- AUDIO CALL (STOP LOOP) ---
        AudioManager.Instance.StopLoopingSound(_playerAudioSource);
        // ------------------------------
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) > 0;
    }
}
