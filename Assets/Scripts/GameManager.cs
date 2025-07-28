using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Typing, Finished }
    [Header("Game State")]
    public GameState currentState;

    [Header("Master Timer")]
    [SerializeField] private float levelTime = 90f;
    public float timeRemaining { get; private set; }

    // --- NEW REFERENCES ---
    [Header("System References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TypingManager typingManager;
    // ----------------------

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        StartNewGame();
    }

    void Update()
    {
        if (currentState == GameState.Playing)
        {
            if (timeRemaining > 0) { timeRemaining -= Time.deltaTime; }
            else
            {
                timeRemaining = 0;
                Debug.Log("GAME OVER - Ran out of time!");
                currentState = GameState.Finished;
            }
        }
    }

    public void StartNewGame()
    {
        timeRemaining = levelTime;
        currentState = GameState.Playing;

        // Ensure player controls are on and typing panel is off
        playerController.SetControlsEnabled(true);
        typingManager.gameObject.SetActive(false);

        Debug.Log("New game started! State: Playing");
    }

    public void StartTypingPhase()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Typing;
            Debug.Log("Delivery reached! State: Typing");

            // --- ORCHESTRATION ---
            playerController.SetControlsEnabled(false); // Stop the player
            typingManager.StartChallenge();             // Start the typing game
            // ---------------------
        }
    }

    public void FinishGame()
    {
        if (currentState == GameState.Typing)
        {
            currentState = GameState.Finished;
            Debug.Log("Typing complete! State: Finished. Time Remaining: " + timeRemaining);
            // We will add code here later to calculate and display stars.
        }
    }
}
