using UnityEngine;
using TMPro; // Required for TextMeshPro

public class DeliveryZone : MonoBehaviour
{
    [Header("UI Feedback")]
    [SerializeField] private GameObject interactPrompt; // Use GameObject to easily show/hide

    private bool playerIsInRange = false;
    private PlayerMovement playerInRange; // --- NEW --- Store a reference to the player

    void Start()
    {
        // Ensure the prompt is hidden when the game starts
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerIsInRange && InputManager.InteractWasPressed)
        {
            if (GameManager.Instance != null)
            {
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(false);
                }
                // --- NEW --- Ensure the player is no longer considered in the zone
                if (playerInRange != null)
                {
                    playerInRange.IsInInteractZone = false;
                }
                GameManager.Instance.StartTypingPhase();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            playerIsInRange = true;
            Debug.Log("Player entered delivery zone.");
            // Show the prompt
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            playerIsInRange = false;
            Debug.Log("Player exited delivery zone.");
            // Hide the prompt
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }
}
