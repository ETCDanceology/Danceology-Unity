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
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class LevelInfoPages : CSingletonMono<LevelInfoPages>
{
    public GameObject DoubleCheckCameraPanel;

    public TextMeshProUGUI infoText;
    public Image[] buttons;

    private LevelType levelType;

    /// <summary>
    /// Open the level choosen panel and hide the check camera panel
    /// </summary>
    public void UpdateLevelINfo()
    {
        UIUtils.instance.HideUI(DoubleCheckCameraPanel);
    }

    /// <summary>
    /// When clicking on a level to start, jump to a panel that confirms camera usage
    /// </summary>
    public void TryStartLevel(int levelNum)
    {
        levelType = (LevelType)levelNum;
        if (levelType == LevelType.Intro) 
        {
            StartWithCamera((int)PlayerInputDevice.NoCamera);
            return;
        }

        UIUtils.instance.ShowUI(DoubleCheckCameraPanel);
        InitDoubleCheckCameraPanel();
    }

    /// <summary>
    /// Do some initialization for check camera panel
    /// </summary>
    private void InitDoubleCheckCameraPanel()
    {
        // Make text transparent
        Color c = infoText.color;
        c.a = 0;
        infoText.color = c;

        // Make Camera btn also transparent and disabled
        foreach (Image img in buttons)
        {
            c = img.color;
            c.a = 0;
            img.color = c;
            img.raycastTarget = false;
        }

        DOTween.KillAll();
        StopAllCoroutines();
        StartCoroutine(CameraBtnShowUpAnim());
    }

    /// <summary>
    /// Trigger animation to fade in camera buttons
    /// </summary>
    private IEnumerator CameraBtnShowUpAnim()
    {
        Sequence s = DOTween.Sequence();
        s.Append(infoText.DOFade(1, 1f));
        s.Append(infoText.DOFade(1, 1f));
        s.Append(infoText.DOFade(0, 1f));
        yield return s.WaitForCompletion();
        Tweener t1 = buttons[0].DOFade(1, 1f);
        Tweener t2 = buttons[1].DOFade(1, 1f);
       
        foreach (Image img in buttons)
        {
            img.raycastTarget = true;
        }
    }

    /// <summary>
    /// Close the camera check panel
    /// </summary>
    public void CloseCheckCamera()
    {
        UIUtils.instance.HideUI(DoubleCheckCameraPanel);
    }

    /// <summary>
    /// Start level with camera option
    /// </summary>
    public void StartWithCamera(int use_camera)
    {
        GameManager.instance.playerInputDevice = (PlayerInputDevice)use_camera;
        GameManager.instance.levelType = levelType;
        GameManager.instance.SetState(GameState.Loading);
    }

    /// <summary>
    /// Return from camera check panel to level selection
    /// </summary>
    public void BackToLevelChosen()
    {
        UIUtils.instance.HideUI(gameObject);
        foreach (Image img in buttons)
        {
            img.gameObject.SetActive(true);
            Color c = img.color;
            c.a = 0;
            img.color = c;
        }
    }
}
