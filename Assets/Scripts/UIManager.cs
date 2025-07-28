using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;

    // --- NEW ---
    [Header("Results Panel")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI starRatingText;
    // -----------

    void Start()
    {
        // Make sure the results panel is hidden when the game starts
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Playing)
        {
            float time = GameManager.Instance.timeRemaining;
            if (time < 0) time = 0;

            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // --- NEW ---
    // This public method will be called by the GameManager to show the final score.
    public void ShowResults(int starRating)
    {
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);

            // Convert the integer rating to a string of stars
            string stars = "";
            for (int i = 0; i < 3; i++)
            {
                if (i < starRating)
                {
                    stars += "★"; // A filled star
                }
                else
                {
                    stars += "☆"; // An empty star
                }
            }
            starRatingText.text = stars;
        }
    }
    // -----------------
}
