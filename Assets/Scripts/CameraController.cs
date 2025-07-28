using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object the camera will follow.")]
    public Transform playerTarget;

    // Store the initial z-position of the camera
    private float initialZ;

    void Start()
    {
        if (playerTarget == null)
        {
            Debug.LogError("CameraController: Player Target is not assigned!");
        }
        initialZ = transform.position.z;
    }

    // LateUpdate is called after all Update functions have been called.
    // This is the best place to move a camera that follows a target,
    // as it ensures the target has already finished its movement for the frame.
    void LateUpdate()
    {
        if (playerTarget != null)
        {
            // Set the camera's position to the player's position,
            // but maintain the camera's original z-depth.
            transform.position = new Vector3(playerTarget.position.x, playerTarget.position.y, initialZ);
        }
    }
}
