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
using DG.Tweening;
using MoreMountains.Feedbacks;

/// <summary>
/// Manages rhythm-based UI animations
/// </summary>
public class UIRhythmManager : MonoBehaviour
{
    public List<Transform> UIsNeedToFollowRhythm = new List<Transform>();
    public MMF_Player rhythmManager;

    void Start()
    {
        rhythmManager.DurationMultiplier = GameManager.instance.beat_second / 2;
    }

    private void OnEnable()
    {
        EventBus.AddListener<AnimModelMovement>(EventTypes.ModelStartedPlaying, StartUIRhythm);
    }

    private void OnDisable()
    {
        EventBus.RemoveListener<AnimModelMovement>(EventTypes.ModelStartedPlaying, StartUIRhythm);
    }

    /// <summary>
    /// Start all the rhythm-based animation for UI
    /// </summary>
    public void StartUIRhythm(AnimModelMovement model)
    {
        StartCoroutine(TargetLocationRhythm());
    }

    /// <summary>
    /// Coroutine loop that will trigger rhythm-based events while the level progresses
    /// </summary>
    IEnumerator TargetLocationRhythm()
    {
        while (GameManager.instance.state != GameState.LevelEnd)
        {
            rhythmManager.PlayFeedbacks();
            yield return new WaitForSeconds(GameManager.instance.beat_second);
        }
    }
}
