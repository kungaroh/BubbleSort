using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonScript : MonoBehaviour
{
    Scene currentScene;
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                Resume();
            }
            else
            { 
                Pause();
            }
        }
    }


    public void ExitGame()
    {
        Application.Quit();
    }
    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }
    public void Reload()
    {
        if (!gameIsPaused)
        {
            currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("menu");
        if(gameIsPaused)
        {
            gameIsPaused = false;
            Time.timeScale = 1f;
        }
    }
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void NextLvl()
    {
        Debug.Log("Loading Next Level");
        gameIsPaused = false;
        StartCoroutine(MovementScript.LoadNextLevel());
    }
}
