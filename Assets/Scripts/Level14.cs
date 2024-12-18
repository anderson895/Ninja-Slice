using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class Level14 : MonoBehaviour
{
    public static Level14 Instance;

    [Header("UI Elements")]
    public Text scoreText;
    public Text questionText;

    // Heart Image references
    public Image heart1;
    public Image heart2;
    public Image heart3;


    [Header("Sound Effects")]
    public AudioClip scoreSound;          // Sound for score increase
    public AudioClip gameOverSound;       // Sound for game over
    public AudioClip levelCompleteSound;  // Sound for level completion
    public AudioClip backgroundMusic;
    private AudioSource audioSource;


    [Header("Game Data")]
    private int playerScore = 0;
    private int currentQuestionIndex = 0;
    private int playerLives = 3; // Total hearts/lives

    private string[] questions = {
        "-3 × 176 = ?",
        "5 × -234 = ?",
        "-2 × 412 = ?",
        "7 × -188 = ?",
        "-8 × 125 = ?",
        "4 × -320 = ?",
        "-9 × 265 = ?",
        "6 × -450 = ?",
        "-6 × 198 = ?",
        "1 × -357 = ?"
    };

    private int[] answers = { -528, -1170, -824, -1316, -1000, -1280, -2385, -2700, -1188, -357 };

    private string filePath;
    private List<UserData> userList;

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

        filePath = Application.persistentDataPath + "/userdata.json";

        // Load existing user data if the file exists
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>();
        }

        audioSource = GetComponent<AudioSource>();
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

        // Play Background Music
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.1f;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Background music clip is not assigned in the Inspector.");
        }
    }

    public void AddScore(int amount)
    {
        if (audioSource != null && scoreSound != null)
        {
            Debug.Log("Playing slice sound.");
            audioSource.PlayOneShot(scoreSound);
        }
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

        // Play score sound
        if (audioSource != null && scoreSound != null)
        {
            Debug.Log("Playing slice sound.");
            audioSource.PlayOneShot(scoreSound);
        }

        Debug.Log($"Lives remaining: {playerLives}");
        UpdateUI();
    }

    private void GameOver()
    {
        PlayerManagement.isGameOver = true;
        Debug.Log("Game Over!");
        questionText.text = "Game Over!";

        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
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
            if (audioSource != null && levelCompleteSound != null)
            {
                audioSource.PlayOneShot(levelCompleteSound);
            }
            // Call UpdateUserLevel with the completed level (e.g., 14)
            UpdateUserLevel(15);

            // Save updated user data
            SaveUserData();
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

    private void UpdateUserLevel(int completedLevel)
    {
        // Get user ID from PlayerPrefs
        int userId = PlayerPrefs.GetInt("LoggedInUserId");

        // Find the user by ID
        UserData foundUser = userList.Find(user => user.id == userId);

        if (foundUser != null)
        {
            // Only update if the completed level is higher than the current level
            if (completedLevel > foundUser.currentLevel)
            {
                foundUser.currentLevel = completedLevel;
                Debug.Log($"User {foundUser.username} level updated to {foundUser.currentLevel}");
            }
            else
            {
                Debug.Log($"Completed level ({completedLevel}) is not higher than current level ({foundUser.currentLevel}). No update made.");
            }
        }
        else
        {
            Debug.LogError("User not found!");
        }
    }

    private void SaveUserData()
    {
        // Save updated user data back to JSON
        string json = JsonUtility.ToJson(new UserDataList { users = userList });
        File.WriteAllText(filePath, json);
        Debug.Log("User data saved to file.");
    }

    [System.Serializable]
    public class UserDataList
    {
        public List<UserData> users;
    }

    [System.Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public int age;
        public int currentLevel;
    }
}
