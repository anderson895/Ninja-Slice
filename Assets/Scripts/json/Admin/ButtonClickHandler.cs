using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonClickHandler : MonoBehaviour
{
    // Field para sa value na ipapasa
    public string buttonValue;

    // Function para sa pag-click ng button
    public void OnButtonClick()
    {
        // I-store ang value ng button sa PlayerPrefs
        PlayerPrefs.SetString("ButtonClickedText", buttonValue);

        // Mag-load ng scene
        SceneManager.LoadScene("admin_leader_board");
    }
}
