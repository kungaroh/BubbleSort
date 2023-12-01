using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    string currentScene;
    public void awake()
    {
       currentScene = SceneManager.GetActiveScene().ToString();
    }
    
    public void exitGame()
    {
        Application.Quit();
    }
    public void startGame()
    {
        SceneManager.LoadScene("Level 1");
    }
    public void reload()
    {
        SceneManager.LoadScene(currentScene);
    }
    public void mainMenu()
    {
        SceneManager.LoadScene("menu");
    }
}
