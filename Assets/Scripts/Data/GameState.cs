/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

/// <summary>
/// Game states that dictate the entire main game flow
/// </summary>
public enum GameState 
{
    GameStart,
    Loading,
    LevelPrepare,
    LevelStart,
    InLevel,
    LevelEnd
}

/// <summary>
/// Player input settings for the experience
/// </summary>
public enum PlayerInputDevice : int
{
    CameraWithWholeBody,
    CameraWithHalfBody,
    NoCamera,
}

/// <summary>
/// Enum indicating type of level
/// </summary>
public enum LevelType : int
{
    Intro = 0,
    Guided = 1,
    UnguidedSlow = 2,
    UnguidedFast = 3
}

/// <summary>
/// Enum for scoring feedback
/// </summary>
public enum ScoringFeedback : int
{
    Miss = 0,
    Close = 1,
    OK = 2,
    Good = 3,
    Great = 4,
    Excellent = 5
}
