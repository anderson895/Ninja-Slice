using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI; // For Button and Slider
using UnityEngine.SceneManagement; // For scene management
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class LoginAdminData : MonoBehaviour
{
    public TMP_InputField LoginusernameMeshText;
    public TMP_InputField LoginPasswordMeshText; // Added for password input
    public Button btnLogin;
    public TMP_Text messageText;
    public Slider loadingSlider;

    private string filePath;
    private List<UserData> userList;

    void Start()
    {
        filePath = Application.persistentDataPath + "/admindata.json";
        btnLogin.onClick.AddListener(LoginUser);

        messageText.gameObject.SetActive(false);
        loadingSlider.gameObject.SetActive(false);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            UserDataList dataList = JsonUtility.FromJson<UserDataList>(json);
            userList = dataList != null ? dataList.users : new List<UserData>();
        }
        else
        {
            userList = new List<UserData>();
        }

        if (userList.Count == 0)
        {
            CreateDefaultAdminUser();
        }
    }

    void CreateDefaultAdminUser()
    {
        UserData admin = new UserData { id = 1, username = "admin", password = "admin" };
        userList.Add(admin);

        UserDataList dataList = new UserDataList { users = userList };
        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Default admin user created.");
    }

    void LoginUser()
    {
        string username = LoginusernameMeshText.text;
        string password = LoginPasswordMeshText.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Username and Password cannot be empty!", Color.red);
            return;
        }

        UserData foundUser = userList.Find(user => user.username == username && user.password == password);

        if (foundUser == null)
        {
            ShowMessage("Invalid Username or Password!", Color.red);
            return;
        }

        PlayerPrefs.SetInt("LoggedInUserId", foundUser.id);
        PlayerPrefs.Save();

        ShowMessage("Login Successful!", Color.green);
        StartCoroutine(ShowLoadingAndRedirect());
    }

    void ShowMessage(string message, Color color)
    {
        messageText.text = message;
        messageText.color = color;
        messageText.gameObject.SetActive(true);
        StartCoroutine(HideMessageAfterDelay(2f));
    }

    IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messageText.gameObject.SetActive(false);
    }

    IEnumerator ShowLoadingAndRedirect()
    {
        loadingSlider.gameObject.SetActive(true);
        float timeToLoad = 2f;
        float elapsedTime = 0f;
        while (elapsedTime < timeToLoad)
        {
            elapsedTime += Time.deltaTime;
            loadingSlider.value = Mathf.Clamp01(elapsedTime / timeToLoad);
            yield return null;
        }
        RedirectToMenu();
    }

    void RedirectToMenu()
    {
        SceneManager.LoadScene("Teacher_Menu");
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
        public string password; // Added for password handling
    }
}
