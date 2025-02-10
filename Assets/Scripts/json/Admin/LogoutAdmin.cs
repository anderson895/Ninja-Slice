using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // For Button
public class LogoutAdmin : MonoBehaviour
{

    public Button btnLogout; // Reference to the Logout button
    // Start is called before the first frame update
    void Start()
    {
        // Add listener to the Logout button
        btnLogout.onClick.AddListener(Logout);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Logout()
    {
        // Clear the logged-in user ID from PlayerPrefs (destroy session)
        PlayerPrefs.DeleteKey("LoggedInUserId");

        // Optionally, save PlayerPrefs changes immediately (though it's auto-saved)
        PlayerPrefs.Save();

        Debug.Log("Logged out successfully!");

        // Redirect to the Login scene
        SceneManager.LoadScene("login_teacher");
    }
}