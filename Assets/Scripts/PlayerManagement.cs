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
        isVictory = false;
        Time.timeScale = 1; // Ensure the game starts unpaused
    }

    void Update()
    {
        if (isGameOver)
        {
            HandleGameOver();
        }

        if (isVictory)
        {
            HandleVictory();
        }
    }

    private void HandleGameOver()
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    private void HandleVictory()
    {
        Time.timeScale = 0;
        victoryScreen.SetActive(true);
    }

    public void ReplayLevel()
    {
        ResetGameState();
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
        ResetGameState();
        SceneManager.LoadScene(28); // Replace with your actual main menu scene index
    }

    public void NextLevel()
    {
        ResetGameState();
        SceneManager.LoadScene(0); // Replace with your actual next level scene index
    }

    private void ResetGameState()
    {
        Time.timeScale = 1; // Reset time scale
        isGameOver = false;
        isVictory = false;

        // Deactivate all screens
        gameOverScreen.SetActive(false);
        pauseMenuScreen.SetActive(false);
        victoryScreen.SetActive(false);
    }
}
