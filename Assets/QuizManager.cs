using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to import TextMeshPro
using System.Collections.Generic;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button answerButtonA;
    public Button answerButtonB;

    private System.Action onCorrectCallback; // Action to run when quiz is solved correctly


    [Header("Tapper Targets (auto-filled)")]
    public List<Tapper> tapperTargets = new List<Tapper>();

    private void OnValidate()
    {
        // Auto-populate the list with all Tapper instances in the scene
        tapperTargets.Clear();
        tapperTargets.AddRange(FindObjectsOfType<Tapper>());
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure quiz panel starts hidden
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
            Debug.Log("QuizManager: Quiz Panel set to hidden on Awake.");
        }
        else
        {
            Debug.LogError("QuizManager: Quiz Panel is not assigned in Inspector! Quiz will not function.");
        }
    }

    public void ShowQuiz(string question, string answerA, string answerB, int correctOption, System.Action onCorrect)
    {
        if (questionText == null || answerButtonA == null || answerButtonB == null || quizPanel == null)
        {
            Debug.LogError("QuizManager: One or more UI elements are not assigned in Inspector. Quiz cannot be shown.");
            return;
        }

        questionText.text = question;
        // Ensure TextMeshProUGUI components are children of buttons, or assigned directly
        answerButtonA.GetComponentInChildren<TextMeshProUGUI>().text = answerA;
        answerButtonB.GetComponentInChildren<TextMeshProUGUI>().text = answerB;

        onCorrectCallback = onCorrect;

        // Clear old listeners to prevent multiple calls
        answerButtonA.onClick.RemoveAllListeners();
        answerButtonB.onClick.RemoveAllListeners();
        Debug.Log("QuizManager: Cleared old button listeners.");

        // Set new listeners based on the correct option
        if (correctOption == 1) // Option A is correct
        {
            answerButtonA.onClick.AddListener(CorrectAnswer);
            answerButtonB.onClick.AddListener(WrongAnswer);
            Debug.Log("QuizManager: Correct answer is A.");
        }
        else if (correctOption == 2) // Option B is correct
        {
            answerButtonA.onClick.AddListener(WrongAnswer);
            answerButtonB.onClick.AddListener(CorrectAnswer);
            Debug.Log("QuizManager: Correct answer is B.");
        }
        else
        {
            Debug.LogError($"QuizManager: Invalid correctOption ({correctOption}). Must be 1 or 2. Defaulting to both wrong.");
            answerButtonA.onClick.AddListener(WrongAnswer);
            answerButtonB.onClick.AddListener(WrongAnswer);
        }

        quizPanel.SetActive(true); // Show the quiz panel
        Debug.Log("QuizManager: Quiz Panel set to active.");
    }

    private void CorrectAnswer()
    {
        Debug.Log("QuizManager: Correct Answer selected!");
        if (quizPanel != null)
        {
            quizPanel.SetActive(false); // Hide the quiz panel
            Debug.Log("QuizManager: Quiz Panel hidden.");
        }
        onCorrectCallback?.Invoke(); // Invoke the callback
        onCorrectCallback = null; // Clear callback to prevent accidental re-invocation
    }

    private void WrongAnswer()
    {
        Debug.Log("QuizManager: Wrong answer. Try again.");
        // Optional: show "try again" message, shake panel, change button colors, etc.
        // The quiz remains open until the correct answer is selected.
    }
}