using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticBackground : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraToFollow;

    // We store the initial offset so the background doesn't snap to the camera's center.
    private Vector3 offset;
    private float initialZ;

    void Start()
    {
        if (cameraToFollow == null)
        {
            cameraToFollow = Camera.main.transform;
        }

        // Calculate the initial difference in position between the background and camera.
        offset = transform.position - cameraToFollow.position;
        initialZ = transform.position.z; // Keep the background's original Z depth.
    }

    void LateUpdate()
    {
        // Set our position to the camera's position plus the initial offset.
        // This keeps the background perfectly locked relative to the camera.
        Vector3 targetPos = cameraToFollow.position + offset;
        transform.position = new Vector3(targetPos.x, targetPos.y, initialZ);
    }
}
