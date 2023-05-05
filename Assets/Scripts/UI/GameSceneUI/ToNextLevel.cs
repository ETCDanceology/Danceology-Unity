/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;
using UnityEngine.UI;

public class ToNextLevel : BaseUI
{
    public Button nextLevelButton;

    /// <summary>
    /// Display UI
    /// </summary>
    public void ShowUp()
    {
        Show();
        nextLevelButton.interactable = GameManager.instance.levelType < LevelType.UnguidedFast;
    }

    /// <summary>
    /// Hide UI
    /// </summary>
    public void HideIn()
    {
        Hide();
    }

    /// <summary>
    /// Called to go to next level
    /// </summary>
    public void ClickToNextLevel()
    {
        if (GameManager.instance.levelType < LevelType.UnguidedFast)
        {
            GameManager.instance.levelType += 1;
        }
        
        GameManager.instance.SetState(GameState.Loading);
    }

    /// <summary>
    /// Called to restart the current level
    /// </summary>
    public void ClickToCurrentLevel()
    {
        GameManager.instance.SetState(GameState.Loading);
    }

    /// <summary>
    /// Called to return to the main menu
    /// </summary>
    public void ClickToMenu()
    {
        GameManager.instance.ExitToMainMenu();
    }

    /// <summary>
    /// Called to quit the application
    /// </summary>
    public void ClickToQuit()
    {
        Application.Quit();
    }
}
