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
using UnityEngine.UI;

public class UIUtils : CSingletonMono<UIUtils>
{
    private Dictionary<string, GameObject> ui_dictionary = new Dictionary<string, GameObject>();

    #region Hide and Show UI
    /// <summary>
    /// Hide UI with Canvas Group
    /// </summary>
    /// <param name="go"></param>
    public void HideUI(GameObject go)
    {
        go.TryGetComponent(out CanvasGroup c);
        if (c)
        {
            c.alpha = 0;
            c.interactable = false;
            c.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("object dont have canvas Group component : " + go.name);
        }
    }

    /// <summary>
    /// Hide UI with Canvas Group
    /// </summary>
    /// <param name="go"></param>
    public void HideUI(Image go)
    {
        go.TryGetComponent(out CanvasGroup c);
        if (c)
        {
            c.alpha = 0;
            c.interactable = false;
            c.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("object dont have canvas Group component : " + go.name);
        }
    }


    /// <summary>
    /// Show UI with Canvas Group
    /// </summary>
    /// <param name="go"></param>
    /// <param name="interactable">Set whether this GameObject is interactable</param>
    public void ShowUI(GameObject go, bool interactable = true)
    {
        go.TryGetComponent(out CanvasGroup c);
        if (c)
        {
            c.alpha = 1;
            c.interactable = interactable;
            c.blocksRaycasts = interactable;
        }
        else
        {
            Debug.LogError("object dont have canvas Group component : " + go.name);
        }
    }

    /// <summary>
    /// Show UI with Canvas Group
    /// </summary>
    /// <param name="go"></param>
    /// <param name="interactable">Set whether this GameObject is interactable</param>
    public void ShowUI(Image go, bool interactable = true)
    {
        go.TryGetComponent(out CanvasGroup c);
        if (c)
        {
            c.alpha = 1;
            c.interactable = interactable;
            c.blocksRaycasts = interactable;
        }
        else
        {
            Debug.LogError("object dont have canvas Group component : " + go.name);
        }
    }
    #endregion
}
