using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI;
using UnityEngine.SceneManagement; // For scene management
using System.IO;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class UserData
{
    public int id;
    public string username;
    public int age;
    public int currentLevel;
}

public class SaveUserData : MonoBehaviour
{
    public TMP_InputField register_usernameText;
    public TMP_InputField register_ageText;
    public Button btnRegister;
    public TMP_Text successMessage; // Reference to the TextMeshPro Text for the success message
    public Slider loadingSlider; // Reference to the loading slider

    private string filePath;
    private List<UserData> userList; // List to store users

    void Start()
    {
        filePath = Application.persistentDataPath + "/userdata.json";
        btnRegister.onClick.AddListener(SaveToJson);
        successMessage.gameObject.SetActive(false); // Hide the success message initially
        loadingSlider.gameObject.SetActive(false); // Hide the loading slider initially

        // Load existing user data if the file exists
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            userList = JsonUtility.FromJson<UserDataList>(json).users;

            // Debugging: Check the number of users loaded
            Debug.Log("Loaded " + userList.Count + " users from JSON.");
        }
        else
        {
            userList = new List<UserData>(); // Initialize empty list if file does not exist
        }
    }

    void SaveToJson()
    {
        string username = register_usernameText.text;
        int age;

        // Validate username input
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username is empty!");  // Changed to LogWarning to prevent error
            successMessage.text = "Username cannot be empty!";
            successMessage.color = Color.red; // Set the color to red for error
            successMessage.gameObject.SetActive(true);
            Invoke(nameof(HideSuccessMessage), 3f);
            return; // This stops the process to prevent saving, but keeps the program running
        }

        // Validate age input
        if (!int.TryParse(register_ageText.text, out age))
        {
            Debug.LogWarning("Age is not a valid number!");  // Changed to LogWarning to prevent error
            successMessage.text = "Age must be a valid number!";
            successMessage.color = Color.red; // Set the color to red for error
            successMessage.gameObject.SetActive(true);
            Invoke(nameof(HideSuccessMessage), 3f);
            return; // Stops the process but keeps the program running
        }

        // Age validation: Ensure the age is positive and reasonable
        if (age <= 0)
        {
            Debug.LogWarning("Age must be greater than zero!");  // Changed to LogWarning to prevent error
            successMessage.text = "Age must be greater than zero!";
            successMessage.color = Color.red; // Set the color to red for error
            successMessage.gameObject.SetActive(true);
            Invoke(nameof(HideSuccessMessage), 3f);
            return; // Stops the process but keeps the program running
        }

        // Check if the username already exists
        bool usernameExists = false;
        foreach (var user in userList)
        {
            Debug.Log("Existing user: " + user.username);  // Debugging log
            if (user.username == username)
            {
                Debug.LogWarning("Username already exists!");  // Changed to LogWarning to prevent error
                successMessage.text = "Username already exists!";
                successMessage.color = Color.red; // Set the color to red for error
                successMessage.gameObject.SetActive(true);
                Invoke(nameof(HideSuccessMessage), 3f);
                usernameExists = true;
                break; // Exit the loop if username is found
            }
        }

        if (usernameExists)
        {
            return; // Stop further processing if username already exists
        }

        // Generate a new user ID by auto-incrementing the highest existing ID + 1
        int newUserId = userList.Count == 0 ? 1 : userList[userList.Count - 1].id + 1;

        // Create new user data
        UserData newUserData = new UserData
        {
            id = newUserId,
            username = username,
            age = age,
            currentLevel = 1
        };

        // Add the new user data to the list
        userList.Add(newUserData);

        // Create a wrapper object for the user list
        UserDataList wrapper = new UserDataList { users = userList };

        // Serialize the list of users to JSON
        string json = JsonUtility.ToJson(wrapper, true);

        // Write the updated user list to the JSON file
        File.WriteAllText(filePath, json);

        Debug.Log("Data saved successfully!");

        // Show the success message
        successMessage.text = "Account created successfully!";
        successMessage.color = Color.green; // Set the color to green for success
        successMessage.gameObject.SetActive(true);

        // Start the loading slider process
        StartCoroutine(ShowLoadingSliderAndRedirect());
    }

    void HideSuccessMessage()
    {
        successMessage.gameObject.SetActive(false); // Hide the success message after a few seconds
    }

    // Coroutine to show the loading slider and redirect to login scene
    IEnumerator ShowLoadingSliderAndRedirect()
    {
        loadingSlider.gameObject.SetActive(true); // Show the loading slider
        loadingSlider.value = 0; // Reset the slider value

        float duration = 2f; // Duration for the loading process (can be adjusted)
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            loadingSlider.value = Mathf.Lerp(0f, 1f, timeElapsed / duration); // Gradually fill the slider
            yield return null;
        }

        // Once the slider is filled, load the login scene
        SceneManager.LoadScene("Login"); // Replace "Login" with the actual name of your login scene
    }

    // Wrapper class for JSON serialization of a list of users
    [System.Serializable]
    public class UserDataList
    {
        public List<UserData> users;
    }
}
