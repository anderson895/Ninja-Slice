using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class Level18 : MonoBehaviour
{
    public static Level18 Instance;

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
        "45 ÷ −5 = ?",
        "−72 ÷ 8 = ?",
        "−81 ÷ 9 = ?",
        "64 ÷ −4 = ?",
        "−96 ÷ 12 = ?",
        "55 ÷ −11 = ?",
        "−84 ÷ 7=  ?",
        "36 ÷ −6 = ?",
        "−108 ÷ 9 = ?",
        "48 ÷ −8 = ?"
    };

    private int[] answers = { -9, -9, -9, -8, -8, -5, -12, -6, -19, -6 };

    private string userDataPath;
    private string attemptDataPath;
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

        userDataPath = Application.persistentDataPath + "/userdata.json";
        attemptDataPath = Application.persistentDataPath + "/attempts.json";

        // Load existing user data if the file exists
        if (File.Exists(userDataPath))
        {
            string json = File.ReadAllText(userDataPath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>();
        }

        // Load attempt data if the file exists
        if (File.Exists(attemptDataPath))
        {
            string attemptsJson = File.ReadAllText(attemptDataPath);
            attemptList = JsonUtility.FromJson<AttemptDataList>(attemptsJson).attempts;
            Debug.Log("Loaded " + attemptList.Count + " attempts from JSON.");
        }
        else
        {
            attemptList = new List<AttemptData>();
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

        // Record attempt when the game is over
        int userId = PlayerPrefs.GetInt("LoggedInUserId");
        AddAttempt(userId, 18); // Track the user's attempt for level 18

        // Save attempt data
        SaveAttemptsData();

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
            // Call UpdateUserLevel with the completed level (e.g., 18)
            UpdateUserLevel(19);

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

    private void AddAttempt(int userId, int level)
    {
        // Ensure attemptList is initialized
        if (attemptList == null)
        {
            attemptList = new List<AttemptData>();
        }

        // Find the attempt data for this user and level
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
        // Save updated user data back to JSON
        string json = JsonUtility.ToJson(new UserDataList { users = userList });
        File.WriteAllText(userDataPath, json);
        Debug.Log("User data saved to file.");
    }

    private void SaveAttemptsData()
    {
        // Save the attempt data to the file
        string attemptsJson = JsonUtility.ToJson(new AttemptDataList { attempts = attemptList });
        File.WriteAllText(attemptDataPath, attemptsJson);
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
