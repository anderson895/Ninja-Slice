using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Leaderboard : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text leaderboardText1; // First leaderboard text component
    public TMP_Text leaderboardText2; // Second leaderboard text component
    public TMP_Text levelText;       // TextMeshPro component to display the current level

    private string attemptsFilePath;
    private string userFilePath;
    private List<AttemptData> attemptList;
    private List<UserData> userList;

    private void Start()
    {
        attemptsFilePath = Application.persistentDataPath + "/attempts.json";
        userFilePath = Application.persistentDataPath + "/userdata.json";

        // Load data
        LoadAttemptData();
        LoadUserData();

        // Display leaderboard dynamically based on level text
        DisplayLeaderboardFromLevelText();
    }

    private void LoadAttemptData()
    {
        if (File.Exists(attemptsFilePath))
        {
            string attemptsJson = File.ReadAllText(attemptsFilePath);
            attemptList = JsonUtility.FromJson<AttemptDataList>(attemptsJson).attempts;
            Debug.Log("Loaded " + attemptList.Count + " attempts from attempts.json.");
        }
        else
        {
            attemptList = new List<AttemptData>();
            Debug.LogWarning("attempts.json file not found. No attempt data loaded.");
        }
    }

    private void LoadUserData()
    {
        if (File.Exists(userFilePath))
        {
            string userJson = File.ReadAllText(userFilePath);
            userList = JsonUtility.FromJson<UserDataList>(userJson).users;
            Debug.Log("Loaded " + userList.Count + " users from userdata.json.");
        }
        else
        {
            userList = new List<UserData>();
            Debug.LogWarning("userdata.json file not found. No user data loaded.");
        }
    }

    public void ReloadData()
    {
        Debug.Log("Reloading leaderboard data...");
        LoadAttemptData(); // Reload the attempt data
        LoadUserData();    // Reload the user data
        DisplayLeaderboardFromLevelText(); // Update the leaderboard display
    }

    private void DisplayLeaderboardFromLevelText()
    {
        if (levelText != null)
        {
            string levelTextValue = levelText.text;
            int level = ExtractLevelNumber(levelTextValue);

            if (level != -1)
            {
                DisplayLeaderboard(level);
            }
            else
            {
                Debug.LogError("Could not extract level number from text.");
            }
        }
        else
        {
            Debug.LogError("Level text component not assigned.");
        }
    }

    private int ExtractLevelNumber(string levelTextValue)
    {
        string[] parts = levelTextValue.Split(' ');
        if (parts.Length == 2 && int.TryParse(parts[1], out int level))
        {
            return level;
        }
        return -1;
    }

    private void DisplayLeaderboard(int level)
    {
        var levelAttempts = attemptList.Where(attempt => attempt.level == level)
                                       .OrderBy(attempt => attempt.attempt)
                                       .Take(10)
                                       .ToList();

        if (levelText != null)
        {
            levelText.text = "Level " + level;
        }

        string leaderboard = "Top 10 Leaderboard (Level " + level + "):\n";
        int rank = 1;

        foreach (var attempt in levelAttempts)
        {
            string username = GetUsernameById(attempt.user_id);
            leaderboard += $"Rank {rank}: {username} - {attempt.attempt} attempts\n";
            rank++;
        }

        // Assign leaderboard text to both components
        if (leaderboardText1 != null)
        {
            leaderboardText1.text = leaderboard;
        }
        else
        {
            Debug.LogError("LeaderboardText1 component not assigned.");
        }

        if (leaderboardText2 != null)
        {
            leaderboardText2.text = leaderboard;
        }
        else
        {
            Debug.LogError("LeaderboardText2 component not assigned.");
        }
    }

    private string GetUsernameById(int userId)
    {
        UserData user = userList.FirstOrDefault(u => u.id == userId);
        return user != null ? user.username : "Unknown User";
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
