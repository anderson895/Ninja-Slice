using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Level8 : MonoBehaviour
{
    public static Level8 Instance;

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
        "14 - (-5) = ?",
        "-10 - 6 = ?",
        "20 - (-8) =  ?",
        "-15 - 4 = ?",
        "25 - (-10) = ?",
        "-30 - 12 = ?",
        "35 - (-7) =  ?",
        "-40 - 15 = ?",
        "50 - (-20) = ?",
        "-45 - 10 = ?"
    };

    private int[] answers = { 19, -16, 28, -19, 35, -42, 42, -55, 70, -55 };

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
