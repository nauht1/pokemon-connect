using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayEnlessGameMode()
    {
        GameManager.Instance.SetGameMode(GameManager.GameMode.Endless);
        SceneManager.LoadSceneAsync(2);
    }

    public void PlayMultiplayerGameMode()
    {
        GameManager.Instance.SetGameMode(GameManager.GameMode.Multiplayer);
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
