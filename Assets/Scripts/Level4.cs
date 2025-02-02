using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class Level4 : MonoBehaviour
{
    public static Level4 Instance;

    [Header("UI Elements")]
    public Text scoreText;
    public Text questionText;

    // Heart Image references
    public Image heart1;
    public Image heart2;
    public Image heart3;

    [Header("Sound Effects")]
    public AudioClip scoreSound;       // Sound for score increase
    public AudioClip gameOverSound;    // Sound for game over
    public AudioClip levelCompleteSound; // Sound for level completion
    public AudioClip backgroundMusic;
    private AudioSource audioSource;   // AudioSource to play sounds

    [Header("Game Data")]
    private int playerScore = 0;
    private int currentQuestionIndex = 0;
    private int playerLives = 3; // Total hearts/lives

    private string[] questions = {
        "-2 + 5 + -3 = ?",
        "7 + -4 + 3 = ?",
        "-10 + 8 + -1 = ?",
        "12 + -5 + -7 = ?",
        "-8 + 6 + 4 = ?",
        "9 + -3 + -5 = ?",
        "-6 + 11 + -2 = ?",
        "4 + -7 + 5 = ?",
        "-9 + 12 + -6 = ?",
        "10 + -2 + -1 = ?"
    };

    private int[] answers = { 0, 6, -3, 0, 2, 1, 3, 2, -3, 7 };

    private string filePath;
    private string attemptsFilePath;
    private List<UserData> userList;
    private List<AttemptData> attemptList;

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

        filePath = Application.persistentDataPath + "/userdata.json";
        attemptsFilePath = Application.persistentDataPath + "/attempts.json";

        // Load existing user data
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

        // Load existing attempt data
        if (File.Exists(attemptsFilePath))
        {
            string attemptsJson = File.ReadAllText(attemptsFilePath);
            attemptList = JsonUtility.FromJson<AttemptDataList>(attemptsJson).attempts;
            Debug.Log("Loaded " + attemptList.Count + " attempts from attempts.json.");
        }
        else
        {
            attemptList = new List<AttemptData>(); // Initialize empty list
        }

        audioSource = GetComponent<AudioSource>(); // Initialize AudioSource
    }

    private void Start()
    {
        if (scoreText == null || questionText == null || heart1 == null || heart2 == null || heart3 == null)
        {
            Debug.LogError("UI references are not assigned in the Inspector!");
            return;
        }

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

        // Play score sound
        if (audioSource != null && scoreSound != null)
        {
            audioSource.PlayOneShot(scoreSound);
        }

        UpdateUI();
    }

    public void LoseHeart()
    {
        playerLives--;
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

        // Record attempt
        int userId = PlayerPrefs.GetInt("LoggedInUserId");
        AddAttempt(userId, 4); // Level 4

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
        return -1;
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

            // Update User Level
            UpdateUserLevel(5);

            // Record attempt
            int userId = PlayerPrefs.GetInt("LoggedInUserId");
            AddAttempt(userId, 5); // Level 5

            // Save user data and attempt data
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
        int userId = PlayerPrefs.GetInt("LoggedInUserId");
        UserData foundUser = userList.Find(user => user.id == userId);

        if (foundUser != null)
        {
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
        if (attemptList == null)
        {
            attemptList = new List<AttemptData>();
        }

        level = Mathf.Max(0, level - 1); // Ensure the level doesn't go below 0
        AttemptData existingAttempt = attemptList.Find(attempt => attempt.user_id == userId && attempt.level == level);

        int attemptCount = 1;

        if (existingAttempt != null)
        {
            attemptCount = existingAttempt.attempt + 1;
            existingAttempt.attempt = attemptCount;
            Debug.Log($"Attempt updated: User {userId}, Level {level}, Attempt #{attemptCount}");
        }
        else
        {
            existingAttempt = new AttemptData
            {
                attempt_id = attemptList.Count + 1,
                level = level,
                user_id = userId,
                attempt = attemptCount
            };

            attemptList.Add(existingAttempt);
            Debug.Log($"New attempt added: User {userId}, Level {level}, Attempt #{attemptCount}");
        }
    }

    private void SaveUserData()
    {
        string json = JsonUtility.ToJson(new UserDataList { users = userList });
        File.WriteAllText(filePath, json);
        Debug.Log("User data saved to file.");
    }

    private void SaveAttemptsData()
    {
        string attemptsJson = JsonUtility.ToJson(new AttemptDataList { attempts = attemptList });
        File.WriteAllText(attemptsFilePath, attemptsJson);
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
