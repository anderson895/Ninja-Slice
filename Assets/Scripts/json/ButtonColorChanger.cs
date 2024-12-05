using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ButtonColorChanger : MonoBehaviour
{
    public Button button;
    private List<UserData> userList; // List to store users
    private string filePath;

    // Color to change the button to when the level is not accessible
    public Color lockedColor = Color.red;  // Color for locked buttons
    public Color lockedGrayColor = Color.gray;  // Color for gray locked buttons
    public Color normalColor = Color.white;  // Normal color for unlocked buttons
   

    // Custom level index for each button (Assign values directly)
    public int levelIndex = 1;  // Assign the level for this button

    void Start()
    {
        if (button == null)
        {
            Debug.LogError("Button is not assigned!");
            return;
        }

        filePath = Application.persistentDataPath + "/userdata.json";

        // Load the user data from the JSON file
        userList = LoadUserData();  // Load the user data

        int loggedInUserId = PlayerPrefs.GetInt("LoggedInUserId", -1);
        UserData loggedInUser = userList.Find(user => user.id == loggedInUserId);

        if (loggedInUser != null)
        {
            int currentLevel = loggedInUser.currentLevel;

            // Set button colors based on the current level
            SetButtonColors(currentLevel);
        }
        else
        {
            Debug.LogError("Logged-in user not found in user data.");
        }
    }


    // Method to change the button color manually
    public void ChangeButtonColor(Color color)
    {
        if (button != null)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = color;
            colorBlock.highlightedColor = color;

            if (!button.interactable)
            {
                colorBlock.disabledColor = lockedGrayColor;
            }

            button.colors = colorBlock;
        }
        else
        {
            Debug.LogError("Button reference is null in ChangeButtonColor!");
        }
    }


    // Function to set button colors based on the current level
    private void SetButtonColors(int currentLevel)
    {
        Debug.Log("Current Level: " + currentLevel);
        Debug.Log("Button Interactable: " + button.interactable);
        Debug.Log("Level Index: " + levelIndex);

        if (levelIndex > currentLevel)
        {
            button.interactable = false;
            ChangeButtonColor(lockedGrayColor);
        }
        else
        {
            button.interactable = true;
            ChangeButtonColor(normalColor);
        }

        Canvas.ForceUpdateCanvases();
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
