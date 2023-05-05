/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : CSingletonMono<MenuUI>
{
    public void Start()
    {   
        UIUtils.instance.HideUI(LevelInfoPages.instance.gameObject);
    }

    /// <summary>
    /// Open the level selection page
    /// </summary>
    public void StartGame()
    {       
        UIUtils.instance.ShowUI(LevelInfoPages.instance.gameObject);
        LevelInfoPages.instance.UpdateLevelINfo();
    }

    /// <summary>
    /// Quit application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
