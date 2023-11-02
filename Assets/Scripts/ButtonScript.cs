using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    public void exitGame()
    {
        Application.Quit();
    }
    public void startGame()
    {
        SceneManager.LoadScene("MainScene");
    }
    public void reload()
    {
        SceneManager.LoadScene("MainScene");
    }
    public void mainMenu()
    {
        SceneManager.LoadScene("menu");
    }
}
