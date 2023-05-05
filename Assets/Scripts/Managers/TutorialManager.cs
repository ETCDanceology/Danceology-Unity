/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manager class for all tutorial-related behaviour
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private AnimModelMovement modelController;      // Reference to instance of AnimModelMovement within the current scene

    private void Start()
    {
        modelController = FindObjectOfType<AnimModelMovement>();
    }

    private void OnEnable()
    {
        EventBus.AddListener<TutorialData>(EventTypes.ReachedTutorialKey, OnTutorialKeyFrame);
    }

    private void OnDisable()
    {
        EventBus.RemoveListener<TutorialData>(EventTypes.ReachedTutorialKey, OnTutorialKeyFrame);
    }

    /// <summary>
    /// Given an input tutorial data, switches on the given event type and performs action accordingly
    /// Detailed descriptions for each action are provided in the <code>TutorialData</code> class and
    /// <code>KeyFrameType</code> enum
    /// </summary>
    private void OnTutorialKeyFrame(TutorialData td)
    {
        if (modelController == null)
        {
            modelController = FindObjectOfType<AnimModelMovement>();
        }

        switch (td.keyFrameType)
        {
            case KeyFrameType.HideWebCam:
                HideWebCamera();
                break;
            case KeyFrameType.ShowWebCam:
                ShowWebCamera();
                break;
            case KeyFrameType.ShowHintText:
                ShowHintText(td);
                break;
            case KeyFrameType.HideHintText:
                HideHintText();
                break;
            case KeyFrameType.PlayAudio:
                PlayTutorialAudio(td);
                break;
            case KeyFrameType.WaitUntilAudioFinish:
                WatchThePoseThatStop(td);
                break;
            case KeyFrameType.Waiting:
                //Do nothing
                break;
            case KeyFrameType.StopForDuration:
                DurationStop(td);
                break;
            case KeyFrameType.PlayIndicator:
                PlayIndicator(td);
                break;
            case KeyFrameType.FadeToBlack:
                TutorialFadein();
                break;
            case KeyFrameType.FadeFromBlack:
                TutorialFadeOut();
                break;
            case KeyFrameType.JumpToFrame:
                TutorialJumpBackFrame();
                break;
            case KeyFrameType.ContinuePlay:
                ContinuePlaying();
                break;
            case KeyFrameType.ChangeAnimationFPS:
                ChangeAnimationFPS(td);
                break;
            case KeyFrameType.ShowMirrorHint:
                ShowMirrorHint();
                break;
        }
    }

    #region Webcam Controls
    private void HideWebCamera()
    {
        InLevelUI.instance.HideWebCam();
        OutputDataReader.instance.DisableVisibility();
        GameManager.instance.DisableDetection();
        modelController.TutorialToNext();
    }

    private void ShowWebCamera()
    {
        InLevelUI.instance.ShowWebCam();
        OutputDataReader.instance.EnableVisibility();
        GameManager.instance.EnableDetection();
        modelController.TutorialToNext();
    }
    #endregion

    #region Hint Text Controls
    private void ShowHintText(TutorialData td)
    {
        InLevelUI.instance.SetGuideTextUI(td.HintText);
        InLevelUI.instance.ShowGuideTextUI();
        modelController.TutorialToNext();
    }

    private void HideHintText()
    {
        InLevelUI.instance.HideGuideTextUI();
        modelController.TutorialToNext();
    }
    #endregion
    
    #region Screen Fade Controls
    private void TutorialFadein()
    {
        InLevelUI.instance.FadeInCurtain();
        modelController.TutorialToNext();
    }

    private void TutorialFadeOut()
    {
        InLevelUI.instance.FadeOutCurtain();
        modelController.TutorialToNext();
    }
    #endregion

    #region Audio Controls
    private void PlayTutorialAudio(TutorialData td)
    {
        if (td.tutorialSound == null)
        {
            // No sound to play; return
            return;
        }
        
        SFXManager.instance.PlaySFX(td.tutorialSound);
        modelController.TutorialToNext();
    }

    private void WatchThePoseThatStop(TutorialData td)
    {
        modelController.StopPlaying();  // Stop the model animation

        // Wait for the current tutorial SFX to finish playing
        if (!SFXManager.instance.SFXisPlaying(td.tutorialSound))
        {
            modelController.TutorialToNext();
        }
    }
    #endregion;

    #region Animation Playback Controls
    private void ContinuePlaying()
    {
        modelController.ContinuePlaying();
        modelController.TutorialToNext();
    }

    private void DurationStop(TutorialData td)
    {
        modelController.StopPlaying();
        if (td.stopDuration > 0)
        {
            Invoke(nameof(ToNextTutorial), td.stopDuration);
        }
        modelController.TutorialToNext();
    }

    private void ToNextTutorial()
    {
        modelController.TutorialToNext();
    }

    /// <summary>
    /// Changes the animation fps based on the value in the HintText field
    /// </summary>
    public void ChangeAnimationFPS(TutorialData td)
    {
        int number = 0;
        try
        {
            number = int.Parse(td.HintText);
        }
        catch (Exception) { }

        if (number > 0)
        {
            modelController.SetPlayFPS(number);
        }

        modelController.TutorialToNext();
    }

    private void TutorialJumpBackFrame()
    {
        if (modelController.GetCurrentTutorial().jumpBackFrame > 0)
        {
            modelController.ContinuePlaying(modelController.GetCurrentTutorial().jumpBackFrame);
            modelController.TutorialToNext();
        }
        else
        {
            ContinuePlaying();
        }
    }
    #endregion

    #region Other Controls
    private void PlayIndicator(TutorialData td)
    {
        IndicatorManager.instance.PlayIndicator(td.indicatorLine, td.arrowType);
        modelController.TutorialToNext();
    }

    private void ShowMirrorHint()
    {
        InLevelUI.instance.ShowMirrorHint();
        modelController.TutorialToNext();
    }
    #endregion
}
