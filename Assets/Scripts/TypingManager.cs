using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TypingManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI targetSentenceText;
    [SerializeField] private TextMeshProUGUI playerInputText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Sentences")]
    [SerializeField] private string[] sentencePool;

    [Header("Feedback & VFX")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private float feedbackDisplayDuration = 1.0f;

    private string currentTargetSentence;
    private string currentPlayerInput = "";
    private Coroutine feedbackCoroutine;
    private bool acceptInput = false;

    void Awake()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            if (!acceptInput)
            {
                acceptInput = true;
                return;
            }
            HandleHardStopInput();
        }
    }

    private void HandleHardStopInput()
    {
        if (!Input.anyKeyDown) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (currentPlayerInput.Length > 0)
                {
                    currentPlayerInput = currentPlayerInput.Substring(0, currentPlayerInput.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r'))
            {
                CheckIfSentenceIsComplete();
            }
            else
            {
                if (currentPlayerInput.Length < currentTargetSentence.Length)
                {
                    char expectedChar = currentTargetSentence[currentPlayerInput.Length];
                    if (c == expectedChar)
                    {
                        currentPlayerInput += c;
                        // --- AUDIO CALL ---
                        AudioManager.Instance.PlaySound("Typing_Key_Correct", transform.position);

                        if (feedbackCoroutine != null)
                        {
                            StopCoroutine(feedbackCoroutine);
                            feedbackCoroutine = null;
                            feedbackText.text = "";
                        }
                    }
                    else
                    {
                        TriggerErrorFeedback();
                    }
                }
            }
        }
        playerInputText.text = currentPlayerInput;
    }

    public void StartChallenge()
    {
        gameObject.SetActive(true);
        currentPlayerInput = "";
        playerInputText.text = "";
        acceptInput = false;

        if (feedbackText != null) feedbackText.text = "";

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
        if (currentPlayerInput.Length == currentTargetSentence.Length)
        {
            StartCoroutine(SuccessSequence());
        }
        else
        {
            TriggerErrorFeedback();
        }
    }

    private void TriggerErrorFeedback()
    {
        // --- AUDIO CALL ---
        AudioManager.Instance.PlaySound("Typing_Key_Error", transform.position);

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(ShowFeedbackPopup("Salah!"));
    }

    private IEnumerator ShowFeedbackPopup(string message)
    {
        if (feedbackText == null) yield break;
        feedbackText.text = message;
        feedbackText.alpha = 1f;
        yield return new WaitForSeconds(feedbackDisplayDuration);
        feedbackText.text = "";
        feedbackCoroutine = null;
    }

    private IEnumerator SuccessSequence()
    {
        Debug.Log("Typing Correct!");
        // --- AUDIO CALL ---
        AudioManager.Instance.PlaySound("Typing_Success", transform.position);

        if (successParticles != null)
        {
            successParticles.Play();
        }
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
        GameManager.Instance.FinishGame();
    }

}
