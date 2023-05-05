/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;

/// <summary>
/// General base class for UI elements that can be shown and hidden
/// </summary>
public class BaseUI : MonoBehaviour
{
    protected bool isVisible;

    /// <summary>
    /// Show UI
    /// </summary>
    public void Show()
    {
        UIUtils.instance.ShowUI(gameObject);
        isVisible = true;
    }

    /// <summary>
    /// Hide UI
    /// </summary>
    public void Hide()
    {
        UIUtils.instance.HideUI(gameObject);
        isVisible = false;
    }
}
