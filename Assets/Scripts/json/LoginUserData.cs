using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI; // For Button and Slider
using UnityEngine.SceneManagement; // For scene management
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class LoginUserData : MonoBehaviour
{
    public TMP_InputField LoginusernameMeshText;
    public Button btnLogin;
    public TMP_Text messageText; // Single TextMeshPro Text for both success and error messages
    public Slider loadingSlider; // Slider to show loading progress

    private string filePath;
    private List<UserData> userList; // List to store users

    void Start()
    {
        filePath = Application.persistentDataPath + "/userdata.json";
        btnLogin.onClick.AddListener(LoginUser);

        // Hide the message and slider initially
        messageText.gameObject.SetActive(false);
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

    void LoginUser()
    {
        string username = LoginusernameMeshText.text;

        // Validate username input
        if (string.IsNullOrEmpty(username))
        {
            ShowMessage("Username cannot be empty!", Color.red); // Show error in red
            return; // Stop further processing
        }

        // Check if the username exists
        UserData foundUser = null;
        foreach (var user in userList)
        {
            Debug.Log("Existing user: " + user.username);  // Debugging log
            if (user.username == username)
            {
                foundUser = user;
                break;
            }
        }

        // If user not found, show the error message
        if (foundUser == null)
        {
            ShowMessage("Username Not Found!", Color.red); // Show error in red
            return; // Stop further processing
        }

        // If user found, store user ID in "session" (PlayerPrefs for simplicity)
        PlayerPrefs.SetInt("LoggedInUserId", foundUser.id);
        PlayerPrefs.Save();  // Ensure the PlayerPrefs are saved

        // Show success message in green
        ShowMessage("Login Successful!", Color.green);

        // Start loading and redirect to Menu after a delay
        StartCoroutine(ShowLoadingAndRedirect());
    }

    void ShowMessage(string message, Color color)
    {
        messageText.text = message;
        messageText.color = color;
        messageText.gameObject.SetActive(true);

        // Start a coroutine to hide the message after 1 second
        StartCoroutine(HideMessageAfterDelay(2f));
    }

    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        messageText.gameObject.SetActive(false); // Hide the message
    }


    // Coroutine to simulate loading process
    IEnumerator ShowLoadingAndRedirect()
    {
        loadingSlider.gameObject.SetActive(true); // Show the loading slider

        // Start the slider animation (simulate a 2-second delay for login)
        float timeToLoad = 2f;
        float elapsedTime = 0f;
        while (elapsedTime < timeToLoad)
        {
            elapsedTime += Time.deltaTime;
            loadingSlider.value = Mathf.Clamp01(elapsedTime / timeToLoad); // Update slider progress
            yield return null; // Wait for the next frame
        }

        // After loading, redirect to the Menu scene
        RedirectToMenu();
    }

    void HideMessage()
    {
        messageText.gameObject.SetActive(false); // Hide the message after a few seconds
    }

    void RedirectToMenu()
    {
        // Load the Menu scene (make sure to add the scene to the build settings)
        SceneManager.LoadScene("Menu"); // Replace "Menu" with the actual name of your Menu scene
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
    }
}
