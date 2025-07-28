using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeliveryZone : MonoBehaviour
{
    private bool playerIsInRange = false;

    // This function is called by the PlayerInput component when the "Interact" action is performed.
    public void OnInteract(InputAction.CallbackContext context)
    {
        // We only care about the moment the button is pressed down.
        if (context.performed && playerIsInRange)
        {
            Debug.Log("Interact key pressed while in range!");

            // Tell the GameManager to switch states.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartTypingPhase();
            }
        }
    }

    // When the player enters the trigger collider...
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered is the player (by checking for the PlayerController script)
        if (other.GetComponent<PlayerController>() != null)
        {
            playerIsInRange = true;
            Debug.Log("Player entered delivery zone.");
            // We could also show a UI prompt here like "[E] to Deliver"
        }
    }

    // When the player exits the trigger collider...
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerIsInRange = false;
            Debug.Log("Player exited delivery zone.");
            // We would hide the UI prompt here.
        }
    }
}
