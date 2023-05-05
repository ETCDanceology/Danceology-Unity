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

/// <summary>
/// Scriptable object used for managing data for tutorial level flow
/// </summary>
[CreateAssetMenu(fileName = "TutorialLevelData", menuName = "ScriptableObjects/TutorialLevelData", order = 2)]
public class TutorialKeyFrameData : ScriptableObject
{
    /// <summary>
    /// Action list that needs to be filled by the designer to dictate the flow of the tutorial level
    /// </summary>
    public List<TutorialAction> actionList = new List<TutorialAction>();

    /// <summary>
    /// Helper list containing the combination of actions to run at the beginning of "Watch Me" phases
    /// </summary>
    public List<TutorialData> combinationWatchMes = new List<TutorialData>();

    /// <summary>
    /// Helper list containing the combination of actions to run at the beginning of "Follow Me" phases
    /// </summary>
    public List<TutorialData> combinationFollowMes = new List<TutorialData>();

    /// <summary>
    /// Resulting list of all keyframe-based tutorial data that is populated programmatically based on the above
    /// </summary>
    public List<TutorialData> keyframeTutorialData = new List<TutorialData>();

    /// <summary>
    /// Programmatically combine all the data inputted into the above fields within the Scriptable Object
    /// into the <code>keyframeTutorialData</code> list
    /// </summary>
    public void StackKeyframeTutorialData()
    {
        // Clear any existing data
        keyframeTutorialData.Clear();

        foreach (var ta in actionList)
        {
            foreach (var ad in ta.actionData)
            {
                List<TutorialData> combinationList;
                if (ad.keyFrameType == KeyFrameType.CombinationWatchMe)
                {
                    combinationList = combinationWatchMes;
                }
                else if (ad.keyFrameType == KeyFrameType.CombinationFollowMe)
                {
                    combinationList = combinationFollowMes;
                }
                else
                {
                    TutorialData td = new TutorialData(ad);
                    keyframeTutorialData.Add(td);
                    continue;
                }

                // Fill in combination data
                int baseFrameCount = ad.frameCount;
                foreach (var comboData in combinationList)
                {
                    TutorialData td = new TutorialData(comboData);
                    td.frameCount += baseFrameCount;
                    if (td.keyFrameType == KeyFrameType.JumpToFrame)
                    {
                        td.jumpBackFrame = ad.jumpBackFrame;
                        baseFrameCount = ad.jumpBackFrame;
                    }

                    keyframeTutorialData.Add(td);
                }
            }
        }   
    }
}

/// <summary>
/// Data class for any large action/phase within the tutorial level
/// </summary>
[System.Serializable]
public class TutorialAction
{
    /// <summary>
    /// Type of tutorial phase, watch or follow
    /// </summary>
    public TutorialType tutorialType = TutorialType.Watch;

    /// <summary>
    /// List of tutorial data for the given phase
    /// </summary>
    public List<TutorialData> actionData = new List<TutorialData>();
}

/// <summary>
/// Data class used in the tutorial level, containing all the data necessary for any frame count
/// within the level (UI, audio, text, etc)
/// </summary>
[System.Serializable]
public class TutorialData
{
    /// <summary>
    /// Type of event to trigger on the current frame
    /// </summary>
    public KeyFrameType keyFrameType;

    /// <summary>
    /// Frame count to trigger this event on
    /// </summary>
    public int frameCount;

    /// <summary>
    /// Text to show for <code>KeyFrameType.HintText1Show</code> and <code>KeyFrameType.HintText2Show</code>
    /// When changing FPS for <code>KeyFrameType.ChangeAnimationFPS</code>, this should contain the new FPS value
    /// </summary>
    public string HintText;

    /// <summary>
    /// Audio to play for <code>KeyFrameType.Audio</code> or to check against in <code>KeyFrameType.WatchStop</code>
    /// </summary>
    public AudioClip tutorialSound;

    /// <summary>
    /// Frame count to jump to on <code>KeyFrameType.JumpBackFrame</code>
    /// </summary>
    public int jumpBackFrame = -1;

    /// <summary>
    /// Number of seconds to wait on <code>KeyFrameType.DurationStop</code>
    /// </summary>
    public float stopDuration = 0;

    /// <summary>
    /// Line position information for displaying indicator lines on <code>KeyFrameType.ArrowHintShow</code>
    /// </summary>
    public IndicatorLine indicatorLine;

    /// <summary>
    /// Denoting what type of arrow to use on <code>KeyFrameType.ArrowHintShow</code>
    /// </summary>
    public ArrowType arrowType;

    public TutorialData() { }

    /// <summary>
    /// Constructor for a new instance of the tutorial data based on the values within an existing instance
    /// </summary>
    public TutorialData(TutorialData td)
    {
        keyFrameType = td.keyFrameType;
        frameCount = td.frameCount;
        HintText = td.HintText;
        tutorialSound = td.tutorialSound;
        jumpBackFrame = td.jumpBackFrame;
        stopDuration = td.stopDuration;
        indicatorLine = new IndicatorLine(td.indicatorLine);
        arrowType = td.arrowType;
    }
}

/// <summary>
/// Types of different events that can be triggered at a given key frame during the tutorial
/// Note that missing enum values are events that were deprecated by the final tutorial design
/// </summary>
public enum KeyFrameType : int
{
    ShowHintText = 0,                                   // Display large text on the screen, used for "Watch My Movement" and "Follow My Movement"
    HideHintText = 1,                                   // Hide large hint text
    PlayAudio = 4,                                      // Play guiding audio clip
    PlayIndicator = 6,                                  // Show an indicator arrow
    HideWebCam = 8,                                     // Hide webcam view
    ShowWebCam = 9,                                     // Show webcam view
    WaitUntilAudioFinish = 10,                          // Wait until the current audio finishes playing
    Waiting = 12,                                       // Buffer event used after DurationStop to wait until the duration finishes
    StopForDuration = 13,                               // Stop for an indicated duration
    FadeToBlack = 14,                                   // Fade screen to black
    FadeFromBlack = 15,                                 // Fade out of black
    JumpToFrame = 16,                                   // Jump to a given frame
    ContinuePlay = 17,                                  // Continue to play animation
    CombinationWatchMe = 18,                            // Insert combination list for "Watch Me"
    CombinationFollowMe = 19,                           // Insert combination list for "Follow Me"
    ChangeAnimationFPS = 21,                            // Change the animation FPS
    ShowMirrorHint = 22,                                // Show custom "mirror" helping UI
}

/// <summary>
/// Type of the current tutorial phase
/// </summary>
public enum TutorialType
{
    Watch,
    Follow,
}

/// <summary>
/// Type for the current indicator arrow
/// </summary>
public enum ArrowType
{
    Force,
    Movement,
}
