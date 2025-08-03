using UnityEngine;
using TMPro; // Required for TextMeshPro

public class DeliveryZone : MonoBehaviour
{
    [Header("UI Feedback")]
    [SerializeField] private GameObject interactPrompt; // The pop-up UI element

    private bool playerIsInRange = false;

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
            Debug.Log("Interact key pressed while in range!");

            if (GameManager.Instance != null)
            {
                // Hide the prompt immediately when interaction is successful
                if (interactPrompt != null)
                {
                    interactPrompt.SetActive(false);
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

            // Show the pop-up
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

            // Hide the pop-up
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }
}
