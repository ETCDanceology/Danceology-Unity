/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Scriptable object class for Feedback that is provided to the user when pose matching
/// </summary>
[CreateAssetMenu(fileName = "FeedBackUI", menuName = "ScriptableObjects/FeedBackUI", order = 1)]
public class FeedBackUIData : ScriptableObject
{
    public List<FeedbackUIInfo> feedbackUIs = new List<FeedbackUIInfo>();
}

/// <summary>
/// Struct containing all information related to feedback presented to user
/// </summary>
[System.Serializable]
public struct FeedbackUIInfo
{
    /// <summary>
    /// Exact text provided to user, i.e. "Good", "Perfect"
    /// </summary>
    public string word; 

    /// <summary>
    /// SFX to play when feedback is displayed
    /// </summary>
    public SFX SFX;

    /// <summary>
    /// Font asset used to display feedback text
    /// </summary>
    public TMP_FontAsset fontMaterial;
}
