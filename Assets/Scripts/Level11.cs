using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

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

    private string userFilePath;
    private string attemptFilePath;
    private List<UserData> userList;
    private List<AttemptData> attemptList;

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

        userFilePath = Application.persistentDataPath + "/userdata.json";
        attemptFilePath = Application.persistentDataPath + "/attempts.json";

        // Load existing user data if the file exists
        if (File.Exists(userFilePath))
        {
            string json = File.ReadAllText(userFilePath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>();
        }

        // Load attempt data from the separate file
        if (File.Exists(attemptFilePath))
        {
            string attemptsJson = File.ReadAllText(attemptFilePath);
            attemptList = JsonUtility.FromJson<AttemptDataList>(attemptsJson).attempts;
            Debug.Log("Loaded " + attemptList.Count + " attempts from attempts.json.");
        }
        else
        {
            attemptList = new List<AttemptData>(); // Initialize empty list if file does not exist
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
            Debug.Log("Playing score sound.");
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
            Debug.Log("Playing score sound.");
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
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        // Record attempt for this user and level
        int userId = PlayerPrefs.GetInt("LoggedInUserId");
        AddAttempt(userId, 11); // Level 11

        // Save attempt data
        SaveAttemptsData();
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
            // Call UpdateUserLevel with the completed level (e.g., 11)
            UpdateUserLevel(12);

            // Record attempt for this user and level
            int userId = PlayerPrefs.GetInt("LoggedInUserId");
            AddAttempt(userId, 12); // Level 12

            // Save updated user data and attempt data
            SaveUserData();
            SaveAttemptsData();
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

    private void AddAttempt(int userId, int level)
    {
        // Ensure attemptList is initialized
        if (attemptList == null)
        {
            attemptList = new List<AttemptData>();
        }

        // Decrease the level by 1
        level = Mathf.Max(0, level - 1); // Ensure the level doesn't go below 0

        // Find the attempt data for the specific user and level
        AttemptData existingAttempt = attemptList.Find(attempt => attempt.user_id == userId && attempt.level == level);

        int attemptCount = 1;

        // If the attempt data exists for this user and level, increment the attempt count
        if (existingAttempt != null)
        {
            attemptCount = existingAttempt.attempt + 1; // Increment the attempt count
            existingAttempt.attempt = attemptCount; // Update the attempt count for the existing attempt data
            Debug.Log($"Attempt updated: User {userId}, Level {level}, Attempt #{attemptCount}");
        }
        else
        {
            // If no previous attempts exist, create a new entry
            existingAttempt = new AttemptData
            {
                attempt_id = attemptList.Count + 1,
                level = level,
                user_id = userId,
                attempt = attemptCount
            };

            attemptList.Add(existingAttempt); // Add the new attempt
            Debug.Log($"New attempt added: User {userId}, Level {level}, Attempt #{attemptCount}");
        }
    }

    private void SaveUserData()
    {
        string json = JsonUtility.ToJson(new UserDataList { users = userList });
        File.WriteAllText(userFilePath, json);
        Debug.Log("User data saved to file.");
    }

    private void SaveAttemptsData()
    {
        // Convert the list of attempts to JSON format
        string attemptsJson = JsonUtility.ToJson(new AttemptDataList { attempts = attemptList });

        // Save the attempt data to the file
        File.WriteAllText(attemptFilePath, attemptsJson);
        Debug.Log("Attempts data saved to file.");
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

    [System.Serializable]
    public class AttemptDataList
    {
        public List<AttemptData> attempts;
    }

    [System.Serializable]
    public class AttemptData
    {
        public int attempt_id;
        public int level;
        public int user_id;
        public int attempt;
    }
}
