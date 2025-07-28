using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypingManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI targetSentenceText;
    [SerializeField] private TextMeshProUGUI playerInputText;

    [Header("Sentences")]
    [SerializeField] private string[] sentencePool; // A list of possible sentences

    private string currentTargetSentence;
    private string currentPlayerInput = "";

    void Update()
    {
        // Only listen for input if this component's GameObject is active.
        if (gameObject.activeInHierarchy)
        {
            // This is a simple way to get typed characters.
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // Handle backspace
                {
                    if (currentPlayerInput.Length > 0)
                    {
                        currentPlayerInput = currentPlayerInput.Substring(0, currentPlayerInput.Length - 1);
                    }
                }
                else if ((c == '\n') || (c == '\r')) // Handle enter/return
                {
                    CheckIfSentenceIsComplete();
                }
                else
                {
                    currentPlayerInput += c;
                }
            }

            // Update the display text
            playerInputText.text = currentPlayerInput;
        }
    }

    public void StartChallenge()
    {
        // Reset everything for a new challenge
        gameObject.SetActive(true);
        currentPlayerInput = "";
        playerInputText.text = "|"; // Show a simple cursor

        // Pick a random sentence from our pool
        if (sentencePool.Length > 0)
        {
            currentTargetSentence = sentencePool[Random.Range(0, sentencePool.Length)];
            targetSentenceText.text = currentTargetSentence;
        }
        else
        {
            Debug.LogError("TypingManager: Sentence Pool is empty!");
        }
    }

    private void CheckIfSentenceIsComplete()
    {
        if (currentPlayerInput.Equals(currentTargetSentence))
        {
            Debug.Log("Typing Correct!");
            gameObject.SetActive(false); // Hide the typing panel
            GameManager.Instance.FinishGame(); // Tell the GameManager we're done
        }
        else
        {
            Debug.Log("Typing Incorrect! Try again.");
            // We could add an error sound or a screen shake here.
        }
    }
}
