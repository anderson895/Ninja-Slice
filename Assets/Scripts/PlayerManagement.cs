using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PlayerManagement : MonoBehaviour
{

    public static bool isGameOver;
    public static bool isVictory;
    public GameObject gameOverScreen;
    public GameObject pauseMenuScreen;
    public GameObject victoryScreen;

    public void Awake()
    {
        isGameOver = false;
    }

    void Update()
    {
        if (isGameOver)
        {
            Time.timeScale = 0;
            gameOverScreen.SetActive(true);
        }

         if (isVictory)
        {
            Time.timeScale = 0;
            victoryScreen.SetActive(true);
        }
    }

    public void ReplayLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenuScreen.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenuScreen.SetActive(false);
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(0);
    }

}
