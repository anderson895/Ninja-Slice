using UnityEngine;
using TMPro;
using UnityEngine.UI; // For Button
using UnityEngine.SceneManagement; // For scene management
using System.IO;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
    public TMP_Text playerNameText; // TextMeshPro to display player name
    public Button btnLogout; // Reference to the Logout button

    private string filePath;
    private List<UserData> userList; // List to store users

    void Start()
    {
        // Set file path for userdata.json
        filePath = Application.persistentDataPath + "/userdata.json";

        // Load user data from JSON if it exists
        LoadUserData();

        // Check if user is logged in
        int userId = PlayerPrefs.GetInt("LoggedInUserId", -1); // Default to -1 if not found

        if (userId != -1)
        {
            // Find the logged-in user and display their name
            UserData loggedInUser = userList.Find(user => user.id == userId);
            if (loggedInUser != null)
            {
                playerNameText.text = "Welcome, " + loggedInUser.username;
            }
            else
            {
                playerNameText.text = "User not found!";
            }
        }
        else
        {
            playerNameText.text = "No user logged in.";
        }

        // Add listener to the Logout button
        btnLogout.onClick.AddListener(Logout);
    }

    // Load user data from JSON file
    void LoadUserData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>(); // Initialize empty list if file doesn't exist
            Debug.LogWarning("No user data file found!");
        }
    }

    // Logout function to clear session and redirect to Login scene
    void Logout()
    {
        // Clear the logged-in user ID from PlayerPrefs (destroy session)
        PlayerPrefs.DeleteKey("LoggedInUserId");

        // Optionally, save PlayerPrefs changes immediately (though it's auto-saved)
        PlayerPrefs.Save();

        Debug.Log("Logged out successfully!");

        // Redirect to the Login scene
        SceneManager.LoadScene("Login");
    }

    // Wrapper class for JSON serialization of a list of users
    [System.Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public int age;
    }

    [System.Serializable]
    public class UserDataList
    {
        public List<UserData> users;
    }
}
