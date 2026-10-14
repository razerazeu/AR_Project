using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Linq;

public class Tapper : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button answerButtonA;
    public Button answerButtonB;

    [Header("Clue UI")]
    public GameObject cluePanel;
    public TextMeshProUGUI clueText;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;
    public GameObject scorePanel;

    [Header("Winning Screen")]
    public GameObject winningPanel;
    public TextMeshProUGUI winningText;
    public Button quitButton;


    [Header("Prefabs to Manage")]
    public List<GameObject> interactivePrefabs = new List<GameObject>();

    // Static list to keep track of all Tapper instances
    public static List<Tapper> AllTappers = new List<Tapper>();

    // Score tracking
    private int artifactsCollected = 0;
    private int totalArtifacts = 0;

    private GameObject currentActivePrefab;
    private InputAction tapAction;
    private Camera mainCamera;
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            Debug.LogWarning("Tapper: AudioManager.Instance not found. Looking for AudioManager in scene...");
            audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogError("Tapper: No AudioManager found in scene! Sound effects won't play.");
            }
        }

        // Register this Tapper instance
        if (!AllTappers.Contains(this))
            AllTappers.Add(this);

        // Ensure quiz panel starts hidden
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
            Debug.Log("Tapper: Quiz Panel set to hidden on Awake.");
        }
        else
        {
            Debug.LogError("Tapper: Quiz Panel is not assigned in Inspector! Quiz will not function.");
        }

        // Cache the main camera reference
        mainCamera = Camera.main;

        // Enable enhanced touch support for better touch handling
        EnhancedTouchSupport.Enable();

        // Set up tap action
        tapAction = new InputAction(binding: "<Mouse>/leftButton");
        tapAction.performed += ctx => OnTap(Pointer.current.position.ReadValue());
        tapAction.Enable();

        // Process prefabs and check for components
        ProcessPrefabs();

        // Count total artifacts
        totalArtifacts = interactivePrefabs.Count;

        // Initialize score display
        UpdateScoreDisplay();

        // Add delay before showing clue to ensure everything is initialized
        Invoke("ShowFirstClue", 0.5f);
    }

    private void ProcessPrefabs()
    {
        if (interactivePrefabs.Count == 0)
        {
            Debug.LogError("Tapper: No prefabs added to the interactivePrefabs list!");
            return;
        }

        // Ensure all prefabs have colliders and PrefabQuizData component
        foreach (GameObject prefab in interactivePrefabs)
        {
            if (prefab != null)
            {
                // Add collider if missing
                if (prefab.GetComponent<Collider>() == null)
                {
                    Debug.LogWarning($"Tapper: Prefab {prefab.name} has no collider. Adding BoxCollider.");
                    prefab.AddComponent<BoxCollider>();
                }

                // Ensure it has quiz data
                if (prefab.GetComponent<PrefabQuizData>() == null)
                {
                    Debug.LogWarning($"Tapper: Prefab {prefab.name} has no PrefabQuizData. Adding component.");
                    prefab.AddComponent<PrefabQuizData>();
                }
            }
            else
            {
                Debug.LogError("Tapper: Found null prefab in interactivePrefabs list!");
            }
        }
    }

    // Update the score display in the UI
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Artifacts: {artifactsCollected}/{totalArtifacts}";

            // Make sure the score panel is visible
            if (scorePanel != null && !scorePanel.activeSelf)
            {
                scorePanel.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Tapper: scoreText is not assigned. Cannot update score display.");
        }
    }

    private void ShowFirstClue()
    {
        Debug.Log("Tapper: Attempting to show first clue...");
        ShowCurrentClue();
    }

    private void OnEnable()
    {
        // Subscribe to touch events
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
    }

    private void OnDisable()
    {
        // Unsubscribe from touch events
        UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;

        // Disable the tap action
        if (tapAction != null)
        {
            tapAction.Disable();
        }
    }

    private void OnDestroy()
    {
        // Unregister this Tapper instance
        AllTappers.Remove(this);

        // Dispose the input action
        if (tapAction != null)
        {
            tapAction.Dispose();
        }
    }

    private void ShowCurrentClue()
    {
        if (cluePanel == null || clueText == null)
        {
            Debug.LogError("Tapper: Clue Panel or Clue Text is not assigned in Inspector!");
            return;
        }

        // First, sort prefabs by clue order
        var sortedPrefabs = interactivePrefabs
            .Where(p => p != null && p.GetComponent<PrefabQuizData>() != null)
            .Select(p => p.GetComponent<PrefabQuizData>())
            .OrderBy(p => p.clueOrder)
            .ToList();

        if (sortedPrefabs.Count == 0)
        {
            Debug.LogError("Tapper: No valid prefabs with PrefabQuizData found!");
            return;
        }

        // Find the first one that hasn't been found
        PrefabQuizData currentPrefabData = null;
        string currentClueString = "";

        foreach (var prefabData in sortedPrefabs)
        {
            if (!prefabData.isFound)
            {
                currentPrefabData = prefabData;
                currentClueString = prefabData.clueText;
                Debug.Log($"Tapper: Selected prefab {prefabData.name} with clue: {currentClueString}");
                break;
            }
        }

        // Show clue if we found one
        if (currentPrefabData != null && !string.IsNullOrEmpty(currentClueString))
        {
            clueText.text = "Clue: "+currentClueString;
            cluePanel.SetActive(true);
            Debug.Log($"Tapper: Showing clue: {currentClueString}");
        }
        else
        {
            // No more clues to show, all prefabs found
            cluePanel.SetActive(false);
            Debug.Log("Tapper: All prefabs found or no clues defined! No more clues.");

            // If all artifacts collected, show winning screen
            if (artifactsCollected >= totalArtifacts && totalArtifacts > 0)
            {
                Debug.Log("Tapper: All artifacts collected! Showing winning screen.");
                ShowWinningScreen();
            }
        }
    }

    private void ShowWinningScreen()
    {
        // Hide other UI panels that might be visible
        if (cluePanel != null) cluePanel.SetActive(false);
        if (quizPanel != null) quizPanel.SetActive(false);

        // Show winning panel if it exists
        if (winningPanel != null)
        {
            // Set winning text if available
            if (winningText != null)
            {
                winningText.text = $"Congratulations!\nYou collected all {artifactsCollected} artifacts!";
            }

            // Setup quit button
            if (quitButton != null)
            {
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(QuitGame);
            }

            // Play winning music
            if (audioManager != null)
            {
                // If you've added a dedicated winning music method to AudioManager
                if (audioManager.GetType().GetMethod("PlayWinningMusic") != null)
                {
                    // Use reflection to safely call the method if it exists
                    audioManager.GetType().GetMethod("PlayWinningMusic").Invoke(audioManager, null);
                }
                else
                {
                    // Fallback: Use existing audio method
                    audioManager.PlayGameOverSound(); // Or another appropriate sound
                }
            }

            // Show the winning panel
            winningPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Tapper: Winning panel not assigned! Cannot show winning screen.");
        }
    }

    private void QuitGame()
    {
        Debug.Log("Tapper: Quit button clicked. Exiting game.");
        // Implement your quit logic here, e.g., Application.Quit() for standalone builds
        Application.Quit();
        // If running in the editor, stop playing
         #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }



    private void OnFingerDown(Finger finger)
    {
        // Handle touch input
        OnTap(finger.currentTouch.screenPosition);
    }

    private void OnTap(Vector2 screenPosition)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("Tapper: No MainCamera found in scene.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log($"Tapper: Raycast hit {hit.transform.name}");

            // Check if the hit object is one of our interactive prefabs
            foreach (GameObject prefab in interactivePrefabs)
            {
                if (prefab != null && hit.transform.gameObject == prefab)
                {
                    Debug.Log($"Tapper: Interactive prefab {prefab.name} clicked.");
                    currentActivePrefab = prefab;

                    // Get quiz data from the prefab and show the quiz
                    PrefabQuizData quizData = prefab.GetComponent<PrefabQuizData>();
                    if (quizData != null)
                    {
                        ShowQuiz(quizData);
                    }
                    else
                    {
                        Debug.LogError($"Tapper: Prefab {prefab.name} has no PrefabQuizData component!");
                    }
                    return;
                }
            }
        }
    }

    private void ShowQuiz(PrefabQuizData quizData)
    {
        if (questionText == null || answerButtonA == null || answerButtonB == null || quizPanel == null)
        {
            Debug.LogError("Tapper: One or more UI elements are not assigned in Inspector. Quiz cannot be shown.");
            return;
        }

        // Update UI with quiz data
        questionText.text = quizData.question;
        answerButtonA.GetComponentInChildren<TextMeshProUGUI>().text = quizData.answerA;
        answerButtonB.GetComponentInChildren<TextMeshProUGUI>().text = quizData.answerB;

        // Clear old listeners to prevent multiple calls
        answerButtonA.onClick.RemoveAllListeners();
        answerButtonB.onClick.RemoveAllListeners();

        // Set new listeners based on correct option
        if (quizData.correctOption == 1) // Option A is correct
        {
            answerButtonA.onClick.AddListener(() => CorrectAnswer(quizData));
            answerButtonB.onClick.AddListener(WrongAnswer);
        }
        else if (quizData.correctOption == 2) // Option B is correct
        {
            answerButtonA.onClick.AddListener(WrongAnswer);
            answerButtonB.onClick.AddListener(() => CorrectAnswer(quizData));
        }
        else
        {
            Debug.LogError($"Tapper: Invalid correctOption ({quizData.correctOption}). Must be 1 or 2.");
            answerButtonA.onClick.AddListener(WrongAnswer);
            answerButtonB.onClick.AddListener(WrongAnswer);
        }

        // Show the quiz panel
        quizPanel.SetActive(true);
        Debug.Log("Tapper: Quiz Panel activated.");
    }

    private void CorrectAnswer(PrefabQuizData quizData)
    {
        Debug.Log("Tapper: Correct Answer selected!");
        quizPanel.SetActive(false);

        // Play correct answer sound
        if (audioManager != null)
        {
            Debug.Log("Tapper: Calling PlayCorrectAnswerSound...");
            audioManager.PlayCorrectAnswerSound(); // This should play the treasure sound
        }
        else
        {
            Debug.LogError("Tapper: audioManager is null! Cannot play correct answer sound.");
        }

        // Mark this prefab as found
        quizData.isFound = true;
        Debug.Log($"Tapper: Marked prefab {quizData.gameObject.name} as found.");

        // Increment artifact count and update display
        artifactsCollected++;
        UpdateScoreDisplay();
        Debug.Log($"Tapper: Artifact collected! Total: {artifactsCollected}/{totalArtifacts}");

        // Destroy or disable the model
        if (currentActivePrefab != null)
        {
            // Option 1: Destroy the object completely
            Destroy(currentActivePrefab);
            Debug.Log($"Tapper: Removed prefab {currentActivePrefab.name} from the scene.");
        }

        // Show next clue
        ShowCurrentClue();

        // Call event for any additional effects
        OnCorrectAnswered();
    }

    private void WrongAnswer()
    {
        Debug.Log("Tapper: Wrong answer. Try again.");

        // Play wrong answer sound if available
        if (audioManager != null)
        {
            audioManager.PlayWrongAnswerSound();
        }

        // Optional: Add visual feedback, shake panel, etc.
        // Quiz remains open until correct answer is provided
    }

    private void OnCorrectAnswered()
    {
        if (currentActivePrefab != null)
        {
            Debug.Log($"{currentActivePrefab.name}: Correct answer selected!");
            // Optional: Add effects, animations, etc.
            // currentActivePrefab.SetActive(false); // Uncomment to hide the prefab after correct answer
        }
    }
}

