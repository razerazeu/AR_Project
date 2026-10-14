using UnityEngine;

public class PrefabQuizData : MonoBehaviour
{
    [Header("Quiz Data")]
    [TextArea]
    public string question;
    public string answerA;
    public string answerB;
    [Tooltip("1 = A is correct, 2 = B is correct")]
    public int correctOption = 1;

    [Header("Clue Data")]
    [TextArea]
    public string clueText;
    public int clueOrder = 0; // 0 = first clue to show, 1 = second, etc.

    [HideInInspector]
    public bool isFound = false;
}

