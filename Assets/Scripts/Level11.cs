using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level11 : MonoBehaviour
{
    public static Level11 Instance;

    [Header("UI Elements")]
    public Text scoreText;
    public Text questionText;

    // Heart Image references
    public Image heart1;
    public Image heart2;
    public Image heart3;

    [Header("Game Data")]
    private int playerScore = 0;
    private int currentQuestionIndex = 0;
    private int playerLives = 3; // Total hearts/lives

    private string[] questions = {
        "1 x 3 = ?",
        "4 x 7 = ?",
        "2 x 9 = ?",
        "6 x 5 = ?",
        "8 x 2 = ?",
        "3 x 6 = ?",
        "7 x 4 = ?",
        "9 x 1 = ?",
        "5 x 8 = ?",
        "7 x 3 = ?"
    };

    private int[] answers = { 3, 28, 18, 30, 16, 18, 28, 9, 40, 21 };

    private void Awake()
    {
        // Set up singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Validate UI references
        if (scoreText == null || questionText == null || heart1 == null || heart2 == null || heart3 == null)
        {
            Debug.LogError("UI references are not assigned in the Inspector!");
            return;
        }

        // Initialize the UI
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        playerScore += amount;
        Debug.Log($"Score updated: {playerScore}");
        UpdateUI();
    }

    public void LoseHeart()
    {
        playerLives--;

        // Hide a heart based on remaining lives
        switch (playerLives)
        {
            case 2:
                heart3.enabled = false;
                break;
            case 1:
                heart2.enabled = false;
                break;
            case 0:
                heart1.enabled = false;
                GameOver();
                break;
        }

        Debug.Log($"Lives remaining: {playerLives}");
        UpdateUI();
    }

    private void GameOver()
    {
        PlayerManagement.isGameOver = true;
        Debug.Log("Game Over!");
        questionText.text = "Game Over!";
        // Optionally disable fruit slicing or other gameplay elements
    }

    public int GetCurrentAnswer()
    {
        if (currentQuestionIndex < answers.Length)
        {
            return answers[currentQuestionIndex];
        }
        Debug.LogError("No valid answer available. Out of bounds!");
        return -1; // Return an invalid answer
    }

    public void DisplayNextQuestion()
    {
        currentQuestionIndex++;

        Debug.Log($"Question Index Updated: {currentQuestionIndex}");

        if (currentQuestionIndex < questions.Length)
        {
            UpdateUI();
        }
        else
        {
            PlayerManagement.isVictory = true;
            questionText.text = "Level Complete!";
            Debug.Log("All questions answered. Level complete!");
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {playerScore}";
        }

        if (questionText != null && currentQuestionIndex < questions.Length)
        {
            questionText.text = questions[currentQuestionIndex];
            Debug.Log($"Updated Question: {questions[currentQuestionIndex]}");
        }
        else
        {
            Debug.LogWarning("No more questions to display or QuestionText is null.");
        }
    }
}
