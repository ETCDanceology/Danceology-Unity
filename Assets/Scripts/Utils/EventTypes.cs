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

/// <summary>
/// Enum containing all different event types used to trigger gameplay events
/// </summary>
public enum EventTypes
{
    FinishedDataLoading,
    FinishedLevelStart,
    ModelStartedPlaying,
    ModelStoppedPlaying,
    ReachedKeyFrame,
    ReachedTutorialKey, // only used for tutorial key frames
    AudioSourceFinished,
    ScoredFrame,
}
