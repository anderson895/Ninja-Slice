using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneOnClick : MonoBehaviour
{
    // This public string holds the name of the scene to load
    public string sceneName;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Button component and add a listener to the OnClick event
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnButtonClick);
        }
    }

    // This method will be called when the button is clicked
    void OnButtonClick()
    {
        // Load the scene using the name provided in the inspector
        SceneManager.LoadScene(sceneName);
    }
}
