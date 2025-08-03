using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object the camera will follow.")]
    public Transform playerTarget;

    [Header("Boundary")]
    [Tooltip("The BoxCollider2D that defines the world boundaries.")]
    public BoxCollider2D cameraBounds;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera catches up to the target. Lower values are slower/smoother.")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    private float initialZ;
    private Camera mainCamera;
    private float cameraHeight;
    private float cameraWidth;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (playerTarget == null)
        {
            Debug.LogError("CameraController: Player Target is not assigned!");
        }
        if (cameraBounds == null)
        {
            Debug.LogError("CameraController: Camera Bounds are not assigned!");
        }
        initialZ = transform.position.z;
    }
    
    void LateUpdate()
    {
        if (playerTarget == null || cameraBounds == null) return;
        Vector3 desiredPosition = new Vector3(playerTarget.position.x, playerTarget.position.y, initialZ);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // Hitung setengah tinggi dan lebar kamera dalam satuan dunia (world units)
        cameraHeight = mainCamera.orthographicSize;
        cameraWidth = cameraHeight * mainCamera.aspect;
        
        // Hitung batas min/max posisi TENGAH kamera
        float minX = cameraBounds.bounds.min.x + cameraWidth;
        float maxX = cameraBounds.bounds.max.x - cameraWidth;
        float minY = cameraBounds.bounds.min.y + cameraHeight;
        float maxY = cameraBounds.bounds.max.y - cameraHeight;
        
        Vector3 clampedPosition = new Vector3(
            Mathf.Clamp(smoothedPosition.x, minX, maxX),
            Mathf.Clamp(smoothedPosition.y, minY, maxY),
            initialZ
        );
        
        transform.position = clampedPosition;
    }
}
