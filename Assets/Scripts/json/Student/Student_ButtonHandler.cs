using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Student_ButtonHandler : MonoBehaviour
{
    // Field para sa value na ipapasa
    public string StudentButtonValue;

    // Function para sa pag-click ng button
    public void OnButtonClick()
    {
        // I-store ang value ng button sa PlayerPrefs
        PlayerPrefs.SetString("ButtonClickedText", StudentButtonValue);

      
    }
}
