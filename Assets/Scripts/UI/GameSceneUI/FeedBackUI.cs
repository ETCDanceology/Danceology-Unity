/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using TMPro;
using MoreMountains.Feedbacks;

public class FeedBackUI : CSingletonMono<FeedBackUI>
{
    public TextMeshProUGUI feedbacktext;
    public int ShowingBeats = 2;
    private bool isShowfeedback;
    public MMF_Player TextFeedBackShow;
    public MMF_Player TextFeedBackHide;
    public MMF_Player TextExcelentFBShow;

    /// <summary>
    /// Initialize this feedback UI, all children set to localscale(0,0,0), hide itself at first
    /// </summary>
    public void FeedBackInit()
    {
        isShowfeedback = false;
        UIUtils.instance.HideUI(gameObject);
    }

    /// <summary>
    /// Show feedback
    /// </summary>
    public void ShowFeedBack(FeedbackUIInfo feedback)
    {
        if (GameManager.instance.levelType == LevelType.Guided) return;
        
        if (isShowfeedback)
        {
            SetFeedBackParameters(feedback);
        }
        else
        {
            isShowfeedback = true;
            SetFeedBackParameters(feedback);
            UIUtils.instance.ShowUI(gameObject);
            TextFeedBackShow.PlayFeedbacks();
            Invoke(nameof(HideFeedBackAnim), GameManager.instance.beat_second * ShowingBeats);
        }
    }

    /// <summary>
    /// Set the feed back parameter by config data
    /// </summary>
    private void SetFeedBackParameters(FeedbackUIInfo feedback)
    {
        feedbacktext.font = feedback.fontMaterial;      
        feedbacktext.text = feedback.word;
        SFXManager.instance.PlaySFX(feedback.SFX);
    }

    /// <summary>
    /// When the feedback UI should no longer be shown, do this to hide it
    /// </summary>
    private void HideFeedBackAnim()
    {
        isShowfeedback = false;
        TextFeedBackHide.PlayFeedbacks();
    }
}
