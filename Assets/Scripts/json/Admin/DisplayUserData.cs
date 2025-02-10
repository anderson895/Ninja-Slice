using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class DisplayUserData : MonoBehaviour
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
    private string selectedLevel;  // This should be class-level

    public Text selectedLevelText; // UI Text para sa selected level

    void Start()
    {
        // Load the JSON data
        string userJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/userdata.json");
        string attemptsJson = System.IO.File.ReadAllText(Application.persistentDataPath + "/attempts.json");

        // Get the level selected from PlayerPrefs
        selectedLevel = PlayerPrefs.GetString("ButtonClickedText", "Default Value");

        // Display selected level sa UI
        if (selectedLevelText != null)
        {
            selectedLevelText.text = "Level: " + selectedLevel;
        }

        // Parse JSON into objects
        userData = JsonUtility.FromJson<UserData>(userJson);
        attemptData = JsonUtility.FromJson<AttemptData>(attemptsJson);

        // Display user data based on the level
        DisplayUserAttempts();
    }


    void DisplayUserAttempts()
    {
        // Clear existing text in UI components
        rankText.text = "";
        nameText.text = "";
        attemptText.text = "";

        // Parse the selected level to integer
        int level = int.Parse(selectedLevel);

        // Debug log for the level selected
        Debug.Log("Selected Level: " + selectedLevel);

        // List to store users with their attempts for the selected level
        List<UserWithAttempts> usersWithAttempts = new List<UserWithAttempts>();

        // Calculate total attempts for each user based on selected level
        foreach (var user in userData.users)
        {
            int totalAttempts = 0;

            // Count attempts for each user that match the selected level
            foreach (var attempt in attemptData.attempts)
            {
                if (attempt.user_id == user.id && attempt.level == level)
                {
                    totalAttempts += attempt.attempt; // Sum attempts for the user
                }
            }

            // Only add the user if they have attempts greater than 0 for this level
            if (totalAttempts > 0)
            {
                usersWithAttempts.Add(new UserWithAttempts(user, totalAttempts));
            }
        }

        // Sort users by total attempts (ascending)
        usersWithAttempts.Sort((x, y) => x.totalAttempts.CompareTo(y.totalAttempts));

        // Log the users with attempts in array format to the console
        string userAttemptsArrayFormat = "[";
        foreach (var userWithAttempts in usersWithAttempts)
        {
            userAttemptsArrayFormat += $"{{Username: \"{userWithAttempts.user.username}\", TotalAttempts: {userWithAttempts.totalAttempts}}}, ";
        }
        userAttemptsArrayFormat = userAttemptsArrayFormat.TrimEnd(new char[] { ',', ' ' }) + "]";
        Debug.Log("Users with attempts: " + userAttemptsArrayFormat);

        // Display the sorted users for the selected level
        int rank = 1;
        foreach (var userWithAttempts in usersWithAttempts)
        {
            // Build the row as a string and append it to each component
            string rankStr = rank.ToString() + "\n";
            string nameStr = userWithAttempts.user.username + "\n";
            string attemptsStr = userWithAttempts.totalAttempts.ToString() + "\n";

            // Append the strings to the respective text components
            rankText.text += rankStr;
            nameText.text += nameStr;
            attemptText.text += attemptsStr;

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