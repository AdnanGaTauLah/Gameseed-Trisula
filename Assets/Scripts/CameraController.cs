using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object the camera will follow.")]
    public Transform playerTarget;

    // --- LEVEL 1: SMOOTH DAMPING ---
    [Header("Smoothing")]
    [Tooltip("How quickly the camera catches up to the target. Lower values are slower/smoother.")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;
    // -------------------------------

    private float initialZ;

    void Start()
    {
        if (playerTarget == null)
        {
            Debug.LogError("CameraController: Player Target is not assigned!");
        }
        initialZ = transform.position.z;
    }

    void LateUpdate() // Changed from LateUpdate to FixedUpdate for smoother physics-based tracking
    {
        if (playerTarget != null)
        {
            // Define the desired position for the camera
            Vector3 desiredPosition = new Vector3(playerTarget.position.x, playerTarget.position.y, initialZ);

            // --- LEVEL 1: SMOOTH DAMPING ---
            // Instead of instantly moving, we smoothly interpolate (Lerp) to the desired position.
            // This creates a fluid, stabilized camera motion.
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

}
