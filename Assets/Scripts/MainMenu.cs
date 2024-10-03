using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject creditsMenu;
    void Start()
    {
        MainMenuButton();
    }

    public void PlayNowButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene1");
    }

    public void CreditsButton()
    {
        mainMenu.SetActive(false);
        creditsMenu.SetActive(true);
    }

    public void MainMenuButton()
    {
        mainMenu.SetActive(true);
        creditsMenu.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
