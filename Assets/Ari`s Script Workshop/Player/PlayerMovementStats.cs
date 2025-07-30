using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu (menuName = "PlayerMovementStats")]
public class PlayerMovementStats : ScriptableObject
{

    #region Move Mechanics Header
    
    
    [Header("Walk")] 
    [Range(1f, 40f)] public float MaxWalkSpeed = 40.5f;
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;
    
    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 100f;
    
    #endregion
    
    #region Collision Check Header
    
    [Header("Grounded/Collision Checks")] 
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;
    
    #endregion

    #region  Jump Area Header
    
    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int NumberofJumpsAllowed = 2;

    [Header("Jump Cut")] 
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexTreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    [Header("Jump Buffer")] [Range(0f, 1f)]
    public float JumpBufferTime = 0.125f;
    
    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    [Header("Debug")]
    public bool DebugShowIsGrounded;
    public bool DebugShowHeadBumpBox;

    [Header("Jump Visualization Tool")] 
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    #endregion 

    #region Wall Mechanics Header
    
    [Header("Wall Mechanics")]
    public LayerMask WallLayer;
    public float WallStickTime = 0.15f;
    public float WallSlideSpeed = 2f;
    public Vector2 WallJumpForce = new Vector2(25f, 20f);
    [Range(0f, 1f)] public float WallJumpInputFreezeTime = 0.1f;
    
    #endregion

    #region Grind Mechanics Header

    [Header("Grind Mechanics")]
    public LayerMask GrindableLayer;
    [Range(5f, 100f)] public float GrindSpeed = 30f;
    [Range(5f, 50f)] public float GrindJumpForce = 20f;
    
    #endregion
    
    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }
    
    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }
}
