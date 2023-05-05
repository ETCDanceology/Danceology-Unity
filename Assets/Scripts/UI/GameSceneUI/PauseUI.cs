/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : BaseUI
{
    private void Start()
    {
        Hide();
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void Update()
    {
        // Check current scene state, not applicable in anything other than play
        if (Input.GetKeyDown(KeyCode.Escape) && !isVisible && GameManager.instance.state == GameState.InLevel)
        {
            Pause();
        }
    }

    /// <summary>
    /// Pause the current game
    /// </summary>
    public void Pause()
    {
        Show();
        Time.timeScale = 0;
        BGMManager.instance.Pause();
        SFXManager.instance.PauseAll();
    }

    /// <summary>
    /// Resume the current game
    /// </summary>
    public void Continue()
    {
        Time.timeScale = 1;
        Hide();
        BGMManager.instance.Resume();
        SFXManager.instance.ResumeAll();
    }

    /// <summary>
    /// Restart the current level
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1;
        Hide();
        BGMManager.instance.StopBGM(false);
        BGMManager.instance.PlayBGM();
        GameManager.instance.SetState(GameState.Loading);

    }

    /// <summary>
    /// Return to menu screen
    /// </summary>
    public void BackToMenu()
    {
        Time.timeScale = 1;
        GameManager.instance.ExitToMainMenu();
    }

    /// <summary>
    /// Exit game
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
