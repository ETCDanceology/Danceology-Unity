/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System;
using UnityEngine;

/// <summary>
/// Wrapper class for level data information
/// </summary>
[Serializable]
public class LD
{
    public LevelData levelData;
}

/// <summary>
/// Class describing all information stored in a level data JSON file that contains pose information
/// for a given level
/// </summary>
[Serializable]
public class LevelData
{
    public string name;
    public PoseData[] poseData;
    public KeyPoseData[] keyPoseData;
    public bool isGuided;

    /// <summary>
    /// Given a frame number, returns whether that frame is a key pose within the level
    /// </summary>
    public bool isKeyPoseFrame(int frame)
    {
        return Array.Find(keyPoseData, keyPose => keyPose.frameCount == frame) != null;
    }
}

#region Pose Data
/// <summary>
/// Class containing structure of all pose data within the level
/// </summary>
[Serializable]
public class PoseData
{
    public float score;
    public Keypoint[] keypoints;
    public Keypoint3D[] keypoints3D;
}

/// <summary>
/// Class containing structure of all 2D keypoint data
/// z-coordinate measurements within this struct are inaccurate
/// </summary>
[Serializable]
public class Keypoint
{
    public float x;
    public float y;
    public float z;
    public float score; // Confidence scoring done by ML model on accuracy of this point
    public string name; // Human-facing label for this given joint point
}

/// <summary>
/// Class containing structure of all 3D keypoint data
/// </summary>
[Serializable]
public class Keypoint3D
{
    public float x;
    public float y;
    public float z;
    public float score; // Confidence scoring done by ML model on accuracy of this point
    public string name; // Human-facing label for this given joint point
}
#endregion

/// <summary>
/// Class containing structure of all key pose data within the level
/// </summary>
[Serializable]
public class KeyPoseData
{
    /// <summary>
    /// Name of the pose sprite
    /// </summary>
    public string poseSpritePath;

    /// <summary>
    /// Boolean on whether the pose sprite should be reversed
    /// </summary>
    public bool isReversed;

    /// <summary>
    /// Frame count of when this key pose appears within the level
    /// </summary>
    public int frameCount;

    /// <summary>
    /// Path to the poses within the Resources folder that will be used when loading Next Pose UI figures
    /// </summary>
    private const string poseFolderPath = "poses/";

    /// <summary>
    /// Sprite of the key pose, to be used for Next Pose UI
    /// </summary>
    private Sprite _poseSprite;
    public Sprite poseSprite
    {
        get
        {
            if (_poseSprite == null) _poseSprite = Resources.Load<Sprite>(poseFolderPath + poseSpritePath);
            return _poseSprite;
        }
    }
}

/// <summary>
/// General data loading class
/// </summary>
public class DataLoad
{
    private static LevelData levelData = null;

    /// <summary>
    /// Loads the level data given a path to the level data
    /// </summary>
    private static void LoadLevelData(string levelDataPath = "exercise_slow")
    {
        string jsonStr = Resources.Load<TextAsset>(levelDataPath).text;
        levelData = JsonUtility.FromJson<LD>(jsonStr).levelData;
    }

    /// <summary>
    /// Retrieves the level data, loading from the Resources folder if necessary
    /// </summary>
    public static LevelData GetLevelData()
    {
        if (levelData == null) LoadLevelData();
        return levelData;
    }
}
