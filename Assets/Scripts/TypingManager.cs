using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TypingManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI targetSentenceText;
    [SerializeField] private TextMeshProUGUI playerInputText;
    [SerializeField] private TextMeshProUGUI feedbackText; // For "Salah!" pop-up

    [Header("Sentences")]
    [SerializeField] private string[] sentencePool;

    [Header("Feedback & VFX")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private float feedbackDisplayDuration = 1.0f;

    private AudioSource audioSource;
    private AudioClip correctKeySound;
    private AudioClip errorKeySound;
    private AudioClip successSound;

    private string currentTargetSentence;
    private string currentPlayerInput = "";

    private Coroutine feedbackCoroutine;

    // --- BUG FIX ---
    // This flag prevents input from being processed on the same frame the manager is activated.
    private bool acceptInput = false;
    // ---------------

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        correctKeySound = CreateTone(880, 0.05f);
        errorKeySound = CreateTone(165, 0.2f);
        successSound = CreateTone(1046, 0.3f);

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            // --- BUG FIX ---
            // On the first frame this object is active, we set the flag to true
            // but skip the rest of the Update loop.
            if (!acceptInput)
            {
                acceptInput = true;
                return;
            }
            // ---------------

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
                        audioSource.PlayOneShot(correctKeySound, 0.7f);

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

        // --- BUG FIX ---
        // Reset the flag every time a new challenge starts.
        acceptInput = false;
        // ---------------

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
        audioSource.PlayOneShot(errorKeySound);

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
        audioSource.PlayOneShot(successSound);

        if (successParticles != null)
        {
            successParticles.Play();
        }

        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
        GameManager.Instance.FinishGame();
    }

    private AudioClip CreateTone(int frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * t) * (1 - t / duration);
        }
        AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
