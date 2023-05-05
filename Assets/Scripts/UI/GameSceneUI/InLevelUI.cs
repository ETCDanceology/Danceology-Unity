/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class InLevelUI : CSingletonMono<InLevelUI>
{
    [Header("Video Player")]
    public RawImage videoPlayUI;            // Player webcam input UI
    public Image videoPlayUIOutframe;
    public GameObject videoPlayerMask;

    [Header("Loading")]
    public Image loadingUI;
 
    [Header("Calibration")]
    public GameObject calibUI;
    public TMP_Text calibText;
    public RectTransform calibrationVideoPanel;

    [Header("Guided Tutorial UI")]
    public GameObject guideUIParent;        // Parent all guide UI
    public TMP_Text guidedText;
    public GameObject guidedVideoUI;        // UI for playing guided video
    public Image curtain;
    public GameObject mirrorHint;           // Hint image for mirroring movements

    [Header("Feedback")]
    public GameObject feedbackUI;

    [Header("Additional Fields")]
    public GameObject pauseButton;

    /// <summary>
    /// Setting up In-Game UI for the "Prepare for Level" phase
    /// </summary>
    public void PrepareForLevelInit()
    {
        SetVideoToCalibrationMode();
        HideAllGuidedUI();

        // Hide Unnecessary UI
        UIUtils.instance.HideUI(feedbackUI);
        UIUtils.instance.HideUI(pauseButton);
        UIUtils.instance.HideUI(mirrorHint);
    }

    /// <summary>
    /// Allow the player to proceed without a camera
    /// </summary>
    public void ProceedWithNoCamera()
    {
        GameManager.instance.DisableCamera();
        HideWebCam();
        GameManager.instance.SetState(GameState.LevelStart);
    }

    /// <summary>
    /// Make the player display UI shrink to corner
    /// </summary>
    /// <param name="playWebcamAnimation">Whether to play the webcam shrinking animation</param>
    public void TransitionToInGameUI(bool playWebcamAnimation)
    {
        // Hide calibration UI
        calibUI.SetActive(false);
        UIUtils.instance.HideUI(loadingUI);

        // Show in-game UI
        UIUtils.instance.ShowUI(pauseButton);
        UIUtils.instance.ShowUI(feedbackUI);
        FeedBackUI.instance.FeedBackInit();

        // Transition webcam UI
        if (GameManager.instance.playerInputDevice != PlayerInputDevice.NoCamera)
        {
            SetVideoToGameMode(playWebcamAnimation);
        }
        else
        {
            EventBus.Broadcast(EventTypes.FinishedLevelStart);
        }
    }

    #region Position Calibration UI
    /// <summary>
    /// Sets the calibration UI based on the number of joints detected
    /// </summary>
    public void SetCalibUI(int numJointsDetected, float calibTimeElapsed, float maxCalibTime)
    {
        loadingUI.fillAmount = Mathf.Clamp(calibTimeElapsed / maxCalibTime, 0.0f, 1.0f);

        if (PoseDetector.IsEntireBodyOnScreen(numJointsDetected))
        {
           
            calibText.text = "Hold that pose!";
        }
        else
        {
            if (numJointsDetected == 0)
            {
                calibText.text = "Make sure your entire body is in the frame";
            }
            else
            { 
                calibText.text = "A little too close! Try backing further away or tilting the camera";
            }
        }
    }
    #endregion

    #region Webcam Video UI Controls
    /// <summary>
    /// Set up the webcam video player UI to calibration size and location
    /// </summary>
    private void SetVideoToCalibrationMode()
    {
        videoPlayUI.rectTransform.position = calibrationVideoPanel.position;
        videoPlayUI.rectTransform.sizeDelta = calibrationVideoPanel.sizeDelta;
        UIUtils.instance.HideUI(videoPlayUIOutframe);
        videoPlayerMask.GetComponent<Mask>().enabled = false;
        videoPlayerMask.GetComponent<Image>().enabled = false;
    }

    /// <summary>
    /// Set up the webcam video player UI to in-game size and location
    /// </summary>
    /// <param name="playWebcamAnimation">Whether to play the webcam animation</param>
    private void SetVideoToGameMode(bool playWebcamAnimation)
    {
        if (playWebcamAnimation)
        {
            StartCoroutine(ShrinkVideo());
        }
        else
        {
            videoPlayUI.rectTransform.position = videoPlayerMask.GetComponent<RectTransform>().position;
            videoPlayUI.rectTransform.sizeDelta = videoPlayerMask.GetComponent<RectTransform>().sizeDelta;
            UIUtils.instance.ShowUI(videoPlayUIOutframe);
            videoPlayerMask.GetComponent<Image>().enabled = true;
            videoPlayerMask.GetComponent<Mask>().enabled = true;

            EventBus.Broadcast(EventTypes.FinishedLevelStart);
        }
    }

    /// <summary>
    /// Plays animation to shrink video to the in-game position
    /// </summary>
    IEnumerator ShrinkVideo()
    {
        UIUtils.instance.ShowUI(videoPlayUIOutframe);

        Vector2 videoPlayerSize = videoPlayerMask.GetComponent<RectTransform>().sizeDelta;
        Tween t = videoPlayUI.rectTransform.DOSizeDelta(videoPlayerSize, 1.0f);
        videoPlayUI.rectTransform.DOLocalMove(Vector3.zero, 1.0f);
        yield return t.WaitForCompletion();

        videoPlayerMask.GetComponent<Image>().enabled = true;
        videoPlayerMask.GetComponent<Mask>().enabled = true;
        yield return new WaitForSeconds(0.5f);

        EventBus.Broadcast(EventTypes.FinishedLevelStart);
    }

    /// <summary>
    /// Hide web camera output
    /// </summary>
    public void HideWebCam()
    {
        UIUtils.instance.HideUI(videoPlayUI.gameObject);
        UIUtils.instance.HideUI(videoPlayUIOutframe.gameObject);
        videoPlayerMask.GetComponent<Image>().enabled = false;
        videoPlayerMask.GetComponent<Mask>().enabled = false;
    }

    /// <summary>
    /// Shows web camera output
    /// </summary>
    public void ShowWebCam()
    {
        if (GameManager.instance.playerInputDevice == PlayerInputDevice.NoCamera) return;

        UIUtils.instance.ShowUI(videoPlayUI.gameObject);
        UIUtils.instance.ShowUI(videoPlayUIOutframe.gameObject);
        videoPlayerMask.GetComponent<Image>().enabled = true;
        videoPlayerMask.GetComponent<Mask>().enabled = true;
    }

    #endregion

    #region Guide UI Controls
    /// <summary>
    /// Hides all UI used for tutorial guidance
    /// </summary>
    public void HideAllGuidedUI()
    {
        HideGuideVideo(); 
        HideGuideTextUI();
        FadeOutCurtain();
    }

    /// <summary>
    /// Shows the mirror hint UI and automatically hides it in 2 seconds
    /// </summary>
    public void ShowMirrorHint()
    {
        UIUtils.instance.ShowUI(mirrorHint);
        Invoke(nameof(HideMirrorHint), 2f);
    }

    /// <summary>
    /// Hides the mirror hint UI
    /// </summary>
    public void HideMirrorHint()
    {
        UIUtils.instance.HideUI(mirrorHint);
    }

    #region Guidance Video
    /// <summary>
    /// Hides guidance video
    /// </summary>
    public void HideGuideVideo()
    {
        UIUtils.instance.HideUI(guidedVideoUI);
    }

    /// <summary>
    /// Sets guidance video
    /// </summary>
    public void SetGuideVideo(VideoClip vc)
    {
        VideoPlayer vp = guidedVideoUI.GetComponent<VideoPlayer>();
        vp.clip = vc;
        guidedVideoUI.GetComponent<RawImage>().texture = vp.texture;
        vp.Play();
    }

    /// <summary>
    /// Shows guidance video
    /// </summary>
    public void ShowGuideVideo()
    {
        UIUtils.instance.ShowUI(guidedVideoUI);
    }
    #endregion

    #region Guidance Text UI
    /// <summary>
    /// Show the guidance text hint
    /// </summary>
    public void ShowGuideTextUI()
    {
        UIUtils.instance.ShowUI(guidedText.gameObject);
    }

    /// <summary>
    /// Set the guidance hint text
    /// </summary>
    public void SetGuideTextUI(string hint)
    {
        guidedText.text = hint;
    }

    /// <summary>
    /// Hide the guidance text hint
    /// </summary>
    public void HideGuideTextUI()
    {
        UIUtils.instance.HideUI(guidedText.gameObject);
    }
    #endregion

    #region Black Curtain UI
    /// <summary>
    /// Fades in the guided UI curtain
    /// </summary>
    /// <param name="fadeDuration">Fade duration time</param>
    public Tweener FadeInCurtain(float fadeDuration = 0.3f)
    {
        return curtain.DOFade(1, fadeDuration);
    }

    /// <summary>
    /// Fades out the guided UI curtain
    /// </summary>
    /// <param name="fadeDuration">Fade duration time</param>
    public Tweener FadeOutCurtain(float fadeDuration = 0.3f)
    {
        return curtain.DOFade(0, fadeDuration);
    }
    #endregion
    #endregion
}
