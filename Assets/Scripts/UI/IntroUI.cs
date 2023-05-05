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
using UnityEngine.Video;

/// <summary>
/// UI class used exclusively for the introduction level
/// </summary>
public class IntroUI : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject pauseUI;

    void Start()
    {
        videoPlayer.loopPointReached += EndReached;
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseVideo();
        }
    }

    /// <summary>
    /// Callback once the intro video has finished playing
    /// </summary>
    void EndReached(VideoPlayer vp)
    {
        BackToMenu();
    }

    /// <summary>
    /// Pause the intro video
    /// </summary>
    public void PauseVideo()
    {
        videoPlayer.Pause();
        UIUtils.instance.ShowUI(pauseUI);
    }

    /// <summary>
    /// Resume the intro video
    /// </summary>
    public void ResumeVideo()
    {
        videoPlayer.Play();
        UIUtils.instance.HideUI(pauseUI);
    }

    /// <summary>
    /// Restart the intro video
    /// </summary>
    public void RestartVideo()
    {
        videoPlayer.time = 0;
        videoPlayer.Play();
        UIUtils.instance.HideUI(pauseUI);
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    public void BackToMenu()
    {
        GameManager.instance.ExitToMainMenu();
    }
}
