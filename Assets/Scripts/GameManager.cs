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
    // --- CHANGE: Updated to reference the new PlayerMovement script ---
    private PlayerMovement playerMovement;
    [SerializeField] private TypingManager typingManager;
    [SerializeField] private UIManager uiManager;

    [Header("Scoring")]
    [Tooltip("Time remaining required for a 3-star rating.")]
    [SerializeField] private float threeStarThreshold = 60f;
    [Tooltip("Time remaining required for a 2-star rating.")]
    [SerializeField] private float twoStarThreshold = 30f;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        ResetGameState();
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
                uiManager.ShowResults(0);
            }
        }
    }

    /// <summary>
    /// Called by the PlayerMovement script on its Awake() to register itself.
    /// </summary>
    public void RegisterPlayer(PlayerMovement newPlayer)
    {
        playerMovement = newPlayer;
        Debug.Log("Player has been registered with the GameManager.");
    }

    private void ResetGameState()
    {
        timeRemaining = levelTime;
        currentState = GameState.Playing;

        if (playerMovement != null)
        {
            // Use the freezing mechanism from PlayerMovement
            playerMovement.IsFrozen = false;
        }

        typingManager.gameObject.SetActive(false);
        Debug.Log("Game state has been reset.");
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting level...");
        LevelManager.Instance.ReloadCurrentLevel();
        ResetGameState();
    }

    public void GoToNextLevel()
    {
        Debug.Log("Loading next level...");
        LevelManager.Instance.LoadNextLevel();
        ResetGameState();
    }

    public void StartTypingPhase()
    {
        if (playerMovement == null)
        {
            Debug.LogError("Cannot start typing phase: PlayerMovement is not registered!");
            return;
        }

        if (currentState == GameState.Playing)
        {
            currentState = GameState.Typing;
            Debug.Log("Delivery reached! State: Typing");

            // Use the freezing mechanism from PlayerMovement
            playerMovement.IsFrozen = true;
            typingManager.StartChallenge();
        }
    }

    public void FinishGame()
    {
        if (currentState == GameState.Typing)
        {
            currentState = GameState.Finished;
            Debug.Log("Typing complete! State: Finished. Time Remaining: " + timeRemaining);

            int stars = 0;
            if (timeRemaining >= threeStarThreshold) { stars = 3; }
            else if (timeRemaining >= twoStarThreshold) { stars = 2; }
            else if (timeRemaining > 0) { stars = 1; }

            uiManager.ShowResults(stars);
        }
    }
}
