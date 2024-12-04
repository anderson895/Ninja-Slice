using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public Text loadingText;       // Reference to the Text component for loading message
    public Slider progressBar;    // Reference to the Progress Bar (Slider)
    public float loadingTime = 4f; // Time in seconds for the loading process (changed to 4 seconds)

    private void Start()
    {
        // Start loading the next scene
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        // Start the loading process for the scene 
        AsyncOperation operation = SceneManager.LoadSceneAsync("Landing");

        // Prevent the scene from activating immediately
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;  // Time elapsed since loading started

        // While the scene is still loading
        while (!operation.isDone)
        {
            // Gradually increase the progress over time (simulate loading over the specified loadingTime)
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / loadingTime);

            // Update progress bar and text
            progressBar.value = progress;
            loadingText.text = "Loading... " + (progress * 100f).ToString("F0") + "%";

            // If the scene is almost loaded (progress reaches 90%), show the final message
            if (progress >= 1f)
            {
                loadingText.text = "Press any key to continue...";

                // Wait for any key press before activating the scene
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }
}
