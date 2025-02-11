using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class Level9 : MonoBehaviour
{
    public static Level9 Instance;

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
    "-5 - 7 - (-2) = ?",
    "10 - (-6) - 4 = ?",
    "-20 - 3 - (-9) =  ?",
    "15 - (-8) - 5 = ?",
    "-12 - (-4) - (-3) = ?",
    "25 - 6 - (-7) = ?",
    "-30 - (-10) - 12 =  ?",
    "40 - (-9) - 6 = ?",
    "-50 - (-15) - (-5) = ?",
    "35 - 8 - (-11) = ?"
};

    private int[] answers = { -10, 12, -14, 18, -5, 26, -32, 43, -30, 38 };

    private string filePath;
    private string attemptsFilePath;
    private List<UserData> userList;
    private List<AttemptData> attemptList; // List to store attempt data

    [Header("Sound Effects")]
    public AudioClip sliceSound;
    public AudioClip gameOverSound;
    public AudioClip levelCompleteSound;
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

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
        attemptsFilePath = Application.persistentDataPath + "/attempts.json";

        // Load existing user data if the file exists
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>(); // Initialize empty list if file does not exist
        }

        // Load attempt data from the separate file
        if (File.Exists(attemptsFilePath))
        {
            string attemptsJson = File.ReadAllText(attemptsFilePath);
            attemptList = JsonUtility.FromJson<AttemptDataList>(attemptsJson).attempts;
            Debug.Log("Loaded " + attemptList.Count + " attempts from attempts.json.");
        }
        else
        {
            attemptList = new List<AttemptData>(); // Initialize empty list if file does not exist
        }

        audioSource = GetComponent<AudioSource>(); // Initialize AudioSource component
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
        playerScore += amount;
        Debug.Log($"Score updated: {playerScore}");

        // Play slice sound
        if (audioSource != null && sliceSound != null)
        {
            Debug.Log("Playing slice sound.");
            audioSource.PlayOneShot(sliceSound);
        }
        else
        {
            Debug.LogError("AudioSource or sliceSound is missing.");
        }

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

        // Play slice sound
        if (audioSource != null && sliceSound != null)
        {
            Debug.Log("Playing slice sound.");
            audioSource.PlayOneShot(sliceSound);
        }
        else
        {
            Debug.LogError("AudioSource or sliceSound is missing.");
        }

        Debug.Log($"Lives remaining: {playerLives}");
        UpdateUI();
    }

    private void GameOver()
    {
        PlayerManagement.isGameOver = true;
        Debug.Log("Game Over!");
        questionText.text = "Game Over!";

        // Get the user ID from PlayerPrefs (assuming user is logged in)
        int userId = PlayerPrefs.GetInt("LoggedInUserId");

        // Record the attempt for this user and level
        AddAttempt(userId, 10, "gameover");

        // Save attempt data
        SaveAttemptsData();

        // Play game over sound
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

        /* if (currentQuestionIndex < questions.Length)
         {
             UpdateUI();
         }
         else
         {*/
        PlayerManagement.isVictory = true;
        questionText.text = "Level Complete!";
        Debug.Log("All questions answered. Level complete!");

        // Play level complete sound
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }

        // Pass the completed level to the update method
        UpdateUserLevel(10); // Adjust to match the actual level number

        // Save the updated user data back to the file
        SaveUserData();
        SaveAttemptsData();
        //}
    }

    private void UpdateUserLevel(int completedLevel)
    {
        // Get user ID from PlayerPrefs
        int userId = PlayerPrefs.GetInt("LoggedInUserId");

        // Find the user by ID
        UserData foundUser = userList.Find(user => user.id == userId);

        if (foundUser != null)
        {
            // Only update the level if the completed level is higher than the current level
            if (completedLevel > foundUser.currentLevel)
            {
                foundUser.currentLevel = completedLevel;
                Debug.Log($"User {foundUser.username} level updated to {foundUser.currentLevel}");
            }
            else
            {
                Debug.Log($"Completed level ({completedLevel}) is not higher than current level ({foundUser.currentLevel}). No update made.");
            }
            // Track attempts
            AddAttempt(userId, completedLevel, "victory");
        }
        else
        {
            Debug.LogError("User not found!");
        }
    }

    private void AddAttempt(int userId, int level, string status)
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

        // If the attempt data exists for this user and level, update the status
        if (existingAttempt != null)
        {
            // Update the appropriate attempt status
            if (status == "victory")
            {
                existingAttempt.victory_attempts++;
            }
            else if (status == "gameover")
            {
                existingAttempt.gameover_attempts++;
            }

            Debug.Log($"Attempt updated: User {userId}, Level {level}, Victory Attempts: {existingAttempt.victory_attempts}, Game Over Attempts: {existingAttempt.gameover_attempts}");
        }
        else
        {
            // If no previous attempts exist, create a new entry
            existingAttempt = new AttemptData
            {
                attempt_id = attemptList.Count + 1,
                level = level,
                user_id = userId,
                victory_attempts = (status == "victory") ? 1 : 0,
                gameover_attempts = (status == "gameover") ? 1 : 0
            };

            attemptList.Add(existingAttempt); // Add the new attempt
            Debug.Log($"New attempt added: User {userId}, Level {level}, Victory Attempts: {existingAttempt.victory_attempts}, Game Over Attempts: {existingAttempt.gameover_attempts}");
        }
    }

    private void SaveUserData()
    {
        // Convert the list of users to JSON format
        string json = JsonUtility.ToJson(new UserDataList { users = userList });

        // Save the updated user data to the file
        File.WriteAllText(filePath, json);
        Debug.Log("User data saved to file.");
    }

    private void SaveAttemptsData()
    {
        // Convert the list of attempts to JSON format
        string attemptsJson = JsonUtility.ToJson(new AttemptDataList { attempts = attemptList });

        // Save the attempt data to the file
        File.WriteAllText(attemptsFilePath, attemptsJson);
        Debug.Log("Attempts data saved to file.");
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

    // Wrapper class for JSON serialization of a list of users
    [System.Serializable]
    public class UserDataList
    {
        public List<UserData> users;
    }

    // Wrapper class for JSON serialization of a list of attempts
    [System.Serializable]
    public class AttemptDataList
    {
        public List<AttemptData> attempts;
    }

    [System.Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public int currentLevel;
    }

    [System.Serializable]
    public class AttemptData
    {
        public int attempt_id;
        public int level;
        public int user_id;
        public int victory_attempts;
        public int gameover_attempts;
    }
}
