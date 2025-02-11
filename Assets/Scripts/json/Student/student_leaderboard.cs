using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class student_leaderboard : MonoBehaviour
{
    [System.Serializable]
    public class User
    {
        public int id;
        public string username;
        public int age;
    }

    [System.Serializable]
    public class Attempt
    {
        public int attempt_id;
        public int level;
        public int user_id;
        public int attempt;
        public int victory_attempts; // Add victory_attempts to the Attempt class
        public int gameover_attempts;
    }

    [System.Serializable]
    public class UserData
    {
        public List<User> users;
    }

    [System.Serializable]
    public class AttemptData
    {
        public List<Attempt> attempts;
    }

    public Text rankText;  // Text component to display ranks
    public Text nameText;  // Text component to display usernames
    public Text attemptText;  // Text component to display attempts
    public ScrollRect scrollView;  // Scroll View for overflow scroll

    private UserData userData;
    private AttemptData attemptData;

    public Text selectedLevelText; // UI Text para sa selected level

    private int selectedLevel;

    void Start()
    {
        // Load the JSON data
        string userJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/userdata.json");
        string attemptsJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/attempts.json");

        // Parse JSON into objects
        userData = JsonUtility.FromJson<UserData>(userJson);
        attemptData = JsonUtility.FromJson<AttemptData>(attemptsJson);

        // Get the level selected from selectedLevelText
        ParseSelectedLevel();

        // Display user data based on the level
        DisplayUserAttempts();
    }

    void ParseSelectedLevel()
    {
        // Extract the numeric part of the level from the selectedLevelText
        string levelText = selectedLevelText.text; // Example: "Level 1"
        string levelNumberStr = System.Text.RegularExpressions.Regex.Match(levelText, @"\d+").Value; // Extract numeric value

        if (int.TryParse(levelNumberStr, out int level))
        {
            selectedLevel = level;
        }
        else
        {
            Debug.LogError("Invalid level text format: " + levelText);
            selectedLevel = 0; // Default to 0 if parsing fails
        }
    }

    void DisplayUserAttempts()
    {
        // Clear existing text in UI components
        rankText.text = "";
        nameText.text = "";
        attemptText.text = "";

        // List to store users with their attempts for the selected level
        List<UserWithAttempts> usersWithAttempts = new List<UserWithAttempts>();

        // Calculate total attempts for each user based on selected level
        foreach (var user in userData.users)
        {
            int totalAttempts = 0;
            bool hasVictory = false;

            // Count victory and gameover attempts for each user that match the selected level
            foreach (var attempt in attemptData.attempts)
            {
                if (attempt.user_id == user.id && attempt.level == selectedLevel)
                {
                    if (attempt.victory_attempts > 0)
                    {
                        hasVictory = true;
                        totalAttempts += attempt.victory_attempts; // Add victory attempts
                    }

                    // Add gameover attempts regardless of the victory attempts
                    totalAttempts += attempt.gameover_attempts; // Add gameover attempts
                }
            }

            // Only add the user if they have at least one victory attempt for this level
            if (hasVictory)
            {
                usersWithAttempts.Add(new UserWithAttempts(user, totalAttempts));
            }
        }

        // Sort users by total attempts (ascending)
        usersWithAttempts.Sort((x, y) => x.totalAttempts.CompareTo(y.totalAttempts));

        // Display the sorted users for the selected level
        int rank = 1;
        foreach (var userWithAttempts in usersWithAttempts)
        {
            // Build the row as a string and append it to each component
            rankText.text += rank.ToString() + "\n";
            nameText.text += userWithAttempts.user.username + "\n";
            attemptText.text += userWithAttempts.totalAttempts.ToString() + "\n";

            // Increment rank
            rank++;
        }

        // Start looping the text
        StartCoroutine(LoopText());
    }

    // Helper class to store user and their attempts
    [System.Serializable]
    public class UserWithAttempts
    {
        public User user;
        public int totalAttempts;

        public UserWithAttempts(User user, int totalAttempts)
        {
            this.user = user;
            this.totalAttempts = totalAttempts;
        }
    }

    // Coroutine to loop the texts with overflow scroll
    IEnumerator LoopText()
    {
        while (true)
        {
            // Wait a moment before starting the loop
            yield return new WaitForSeconds(5f); // Set the time between loops

            // Scroll to the top and then to the bottom for looping effect
            scrollView.verticalNormalizedPosition = 1f; // Scroll to the top
            yield return new WaitForSeconds(1f); // Wait before scrolling down
            scrollView.verticalNormalizedPosition = 0f; // Scroll to the bottom
        }
    }
}
