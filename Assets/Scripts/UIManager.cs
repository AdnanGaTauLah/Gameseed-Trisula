using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;

    void Update()
    {
        // Check if the GameManager instance exists before trying to access it
        if (GameManager.Instance != null)
        {
            // Get the time remaining from the GameManager's public property
            float time = GameManager.Instance.timeRemaining;

            // Ensure time doesn't display as negative
            if (time < 0)
            {
                time = 0;
            }

            // Format the time into minutes and seconds for display
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            // Update the text field. The "D2" format ensures two digits (e.g., 09)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
