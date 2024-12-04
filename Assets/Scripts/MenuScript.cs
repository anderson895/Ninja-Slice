using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;  // Import TextMeshPro namespace
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class MenuScript : MonoBehaviour
{
    public Button[] levelButtons; // Add buttons for levels 1, 2, 3, etc.
    public TextMeshProUGUI levelText;  // Reference to the TextMeshPro component that will display the current level
    public TextMeshProUGUI alertText;  // Reference to the TextMeshPro component that will display the alert message
    private List<UserData> userList; // List to store users
    private string filePath;

    void Start()
    {
        filePath = Application.persistentDataPath + "/userdata.json";

        userList = LoadUserData();

        int loggedInUserId = PlayerPrefs.GetInt("LoggedInUserId", -1);

        UserData loggedInUser = userList.Find(user => user.id == loggedInUserId);

        if (loggedInUser != null)
        {
            int currentLevel = loggedInUser.currentLevel;

            levelText.text = "Current Level: " + currentLevel;

            for (int i = 0; i < levelButtons.Length; i++)
            {
                int levelIndex = i + 1; // Levels start from 1
                if (levelIndex > currentLevel)
                {
                    levelButtons[i].interactable = false; // Disable button for higher levels
                }
            }
        }
        else
        {
            Debug.LogError("Logged-in user not found in user data.");
        }
    }

    public void LoadLevel(int index)
    {
        // Get the logged-in user's current level from the list
        int loggedInUserId = PlayerPrefs.GetInt("LoggedInUserId", -1);
        UserData loggedInUser = userList.Find(user => user.id == loggedInUserId);

        if (loggedInUser != null)
        {
            int currentLevel = loggedInUser.currentLevel;

            // Check if the selected level is higher than the current level
            if (index - 1 > currentLevel)
            {
                // Show alert message
                ShowAlert("Cannot load level higher than current level.");

                Debug.Log("Cannot load level higher than current level. The current level is " + currentLevel);
                return; // Prevent loading the level if it's higher than the current level
            }

            // Load the selected level if it's allowed
            SceneManager.LoadScene(index);
        }
        else
        {
            Debug.LogError("Logged-in user not found in user data.");
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Function to load user data from the JSON file
    private List<UserData> LoadUserData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            UserDataList userDataList = JsonUtility.FromJson<UserDataList>(json);
            return userDataList.users;
        }
        else
        {
            Debug.LogError("User data file not found!");
            return new List<UserData>(); // Return empty list if file not found
        }
    }

    private void ShowAlert(string message)
{
    alertText.text = message;  // Set the alert message
    alertText.color = Color.red;  // Set the color of the alert text to red
    alertText.gameObject.SetActive(true);  // Show the alert

    // Start the coroutine to hide the alert after 1 second
    StartCoroutine(HideAlert());
}


    // Coroutine to hide the alert after 1 second
    private IEnumerator HideAlert()
    {
        yield return new WaitForSeconds(1f);  // Wait for 1 second
        alertText.gameObject.SetActive(false);  // Hide the alert
    }

    // Wrapper class for JSON serialization of a list of users
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
        public int currentLevel; // Add currentLevel to UserData class
    }
}
