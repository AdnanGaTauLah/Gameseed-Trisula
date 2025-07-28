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

    [Header("System References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TypingManager typingManager;
    [SerializeField] private UIManager uiManager;

    [Header("Scoring")]
    [Tooltip("Time remaining required for a 3-star rating.")]
    [SerializeField] private float threeStarThreshold = 60f;
    [Tooltip("Time remaining required for a 2-star rating.")]
    [SerializeField] private float twoStarThreshold = 30f;
    // 1 star is anything above 0.

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
                // We could show a "Game Over" panel here
            }
        }
    }


    public void StartNewGame()
    {
        timeRemaining = levelTime;
        currentState = GameState.Playing;

        playerController.SetControlsEnabled(true);
        typingManager.gameObject.SetActive(false);
        // The UIManager already hides its results panel in its own Start() method.
    }

    public void StartTypingPhase()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Typing;
            Debug.Log("Delivery reached! State: Typing");

            playerController.SetControlsEnabled(false);
            typingManager.StartChallenge();
        }
    }

    public void FinishGame()
    {
        if (currentState == GameState.Typing)
        {
            currentState = GameState.Finished;
            Debug.Log("Typing complete! State: Finished. Time Remaining: " + timeRemaining);

            // --- SCORING LOGIC ---
            int stars = 0;
            if (timeRemaining >= threeStarThreshold)
            {
                stars = 3;
            }
            else if (timeRemaining >= twoStarThreshold)
            {
                stars = 2;
            }
            else if (timeRemaining > 0)
            {
                stars = 1;
            }
            // 0 stars if time ran out.

            uiManager.ShowResults(stars);
        }
    }
}
