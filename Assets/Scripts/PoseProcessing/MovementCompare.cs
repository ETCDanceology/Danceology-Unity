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

public class MovementCompare : CSingletonMono<MovementCompare>
{
    public FeedBackUIData feedback_config;           // Data for feedback UI

    public bool webCamMirrored = true;               // If mirrored, then data fetched will be both multiplied by -1

    [Header("Scoring Adjustments")]
    public float tolerance;                          // Allowed angle tolerance, larger value allows larger mistakes
    public float maxAngleDifference = 120;           // Maximum angle difference allowed (i.e. larger angle diffs will be clamped by this value)
    public float outOfFrameScoringPercentage = 0.5f; // If a joint is out of frame, this is the default padding score they get for that joint [0, 1]

    [Tooltip("# frames before and after the key frame will also be calculated")]
    public int compareMovementThreshold;

    [Header("Other Values")]
    public int fps;
    private float secondPerFrame;
    private float nextFrameTime;
    private LevelData levelData;
    private int frame;
    private List<Vector3[]> curFrameSample; // Record vector3 of all joints cur frame on the sample video
    private int listFrameCount; // Depend on threshold
    private int frameOnDetection = -1;

    private bool nextComparisonAvailable = false;
    private bool allFrameReady; // Frames + threshold all ready for comparison

    // Score is given based on the difference between the movement of the dancer and the user. Out of 100 maximum
    private float totalScore; // Record the total score on the top right
    private float lastScore; // Latest comparison score
    private int compare_counts; // Count how many times

    // Points corresponding for two models, -1 represent not available
    private static int[] camModelToRecordingModel = new int[] { 0, -1, 12, 14, 16, 11, 13, 15, 24, 26, 28, 23, 25, 27, 5, 2, 8, 7 };

    // Points to examine: Head: 0-16, 0-17, Arm: 2-3, 3-4, 5-6, 6-7, Leg: 8-9, 9-10, 11-12, 12-13
    private static Vector2Int[] jointConnections = new Vector2Int[] { 
        new Vector2Int(2, 3), new Vector2Int(3, 4), new Vector2Int(5, 6), new Vector2Int(6, 7),         // Legs
        new Vector2Int(8, 9), new Vector2Int(9, 10), new Vector2Int(11, 12), new Vector2Int(12, 13),    // Arms
        new Vector2Int(2, 5), new Vector2Int(8, 11),       // Shoulders and Hips
    };
    private void Start()
    {
        listFrameCount = 1 + compareMovementThreshold * 2;
        curFrameSample = new List<Vector3[]>();
        secondPerFrame = 1f / fps;
        nextFrameTime = float.MaxValue;
        levelData = DataLoad.GetLevelData();
        compare_counts = 0;
        totalScore = 0;
    }

    /// <summary>
    /// Called once at the beginning of the level to initialize fps and related fields
    /// </summary>
    public void SetGameStart(float fps)
    {
        frame = 0;
        secondPerFrame = 1f / fps;
        nextFrameTime = Time.time;
        curFrameSample.Clear();
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void FixedUpdate()
    {
        if (Time.time > nextFrameTime && frame < levelData.poseData.Length)
        {
            nextFrameTime += secondPerFrame;
        }
    }

    /// <summary>
    /// Advances pose data frame count for comparison purposes
    /// </summary>
    public void CallNextFrameData()
    {
        if (frame < levelData.poseData.Length)
        {
            Vector3[] tmp = new Vector3[levelData.poseData[frame].keypoints.Length];
            for (int i = 0; i < levelData.poseData[frame].keypoints.Length; i++)
            {
                tmp[i] = new Vector3(-levelData.poseData[frame].keypoints[i].x, -levelData.poseData[frame].keypoints[i].y, 0);
            }
            curFrameSample.Add(tmp);

            if (curFrameSample.Count > listFrameCount)
            {
                curFrameSample.RemoveAt(0);
            }
            if (frameOnDetection != -1 && frame >= frameOnDetection + compareMovementThreshold)
            {
                allFrameReady = true;
            }
            frame++;
        }
    }

    /// <summary>
    /// Enable next frame for comparison
    /// </summary>
    public void EnableCompare()
    {
        nextComparisonAvailable = true;
    }

    /// <summary>
    /// If the next player pose data is available from the webcam,
    /// updates it and compares to reference pose data
    /// </summary>
    public void UpdatePlayerPose(float[,,] data, int mainFocus)
    {
        if (!nextComparisonAvailable) return;

        frameOnDetection = frame;
        StartCoroutine(PlayerPoseCompare(data, mainFocus));
    }

    /// <summary>
    /// Compares current player pose data with the current reference pose data
    /// </summary>
    private IEnumerator PlayerPoseCompare(float[,,] data, int mainFocus)
    {
        // Only enable comparison for the frame right after the pose is enabled
        if (!allFrameReady)
        {
            yield return new WaitForFixedUpdate();
        }

        nextComparisonAvailable = false;
        allFrameReady = false;

        float curBestScore = 0;
        float paddedScoreValue = outOfFrameScoringPercentage * maxAngleDifference;
        for (int a = 0; a < listFrameCount; a++)
        {
            float maxTotal = maxAngleDifference * jointConnections.Length;

            // Compare current video frame with given data, if it matches, add score
            float totalDifference = 0;
            for (int i = 0; i < jointConnections.Length; i++)
            {
                int xJoint = jointConnections[i].x;
                int yJoint = jointConnections[i].y;

                // Chances that difference calculation is NaN: discard
                if (data.GetLength(0) == 0 || data[mainFocus, xJoint, 0] == -1 || data[mainFocus, yJoint, 0] == -1)
                {
                    // Joint not captured
                    totalDifference += paddedScoreValue;
                    continue;
                }

                int recordingXIndex = camModelToRecordingModel[xJoint];
                int recordingYIndex = camModelToRecordingModel[yJoint];

                if (recordingXIndex < 0 || recordingXIndex >= curFrameSample[a].Length || recordingYIndex < 0 || recordingYIndex >= curFrameSample[a].Length)
                {
                    // Joint index not in recording
                    maxTotal -= maxAngleDifference;
                    continue;
                }

                float calc = LimbAngleDifference(curFrameSample[a][recordingYIndex] - curFrameSample[a][recordingXIndex],
                                                 (webCamMirrored ? -1 : 1) * new Vector2(data[mainFocus, yJoint, 0] - data[mainFocus, xJoint, 0], data[mainFocus, yJoint, 1] - data[mainFocus, xJoint, 1]));
                if (calc == float.MinValue)
                {
                    // Limb Comparison Calculation error!
                    totalDifference += paddedScoreValue;
                    continue;
                }

                float curDifference = Mathf.Clamp(calc - tolerance, 0, maxAngleDifference);
                totalDifference += curDifference;
            }

            if (maxTotal == 0) lastScore = 0;
            else lastScore = (maxTotal - totalDifference) / maxTotal * 100;

            curBestScore = Mathf.Max(lastScore, curBestScore);
        }

        frameOnDetection = -1;
        compare_counts++;

        // Update scoring accordingly
        if (curBestScore != float.NaN)
        {
            EventBus.Broadcast(EventTypes.ScoredFrame, curBestScore);
            totalScore += curBestScore;

            FeedbackUIInfo output;
            if (curBestScore > 90)
            {
                output = feedback_config.feedbackUIs[(int)ScoringFeedback.Excellent];
                ResultUI.SharedInstance.AddToScoreDistribution(0);
            }
            else if (curBestScore > 80)
            {
                output = feedback_config.feedbackUIs[(int)ScoringFeedback.Great];
                ResultUI.SharedInstance.AddToScoreDistribution(1);
            }
            else if (curBestScore > 70)
            {
                output = feedback_config.feedbackUIs[(int)ScoringFeedback.Good];
                ResultUI.SharedInstance.AddToScoreDistribution(2);
            }
            else if (curBestScore > 60)
            {
                output = feedback_config.feedbackUIs[(int)ScoringFeedback.OK];
                ResultUI.SharedInstance.AddToScoreDistribution(3);
            }
            else if (curBestScore > 50)
            {
                output = feedback_config.feedbackUIs[(int)ScoringFeedback.Close];
                ResultUI.SharedInstance.AddToScoreDistribution(4);
            }
            else
            {
                output = feedback_config.feedbackUIs[(int)ScoringFeedback.Miss];
                ResultUI.SharedInstance.AddToScoreDistribution(5);
            }

            FeedBackUI.instance.ShowFeedBack(output);
            if (FindObjectOfType<AnimModelMovement>().IsPlaying())
            {
                GameManager.instance.AddNewScore(curBestScore);
            }
        }
    }

    /// <summary>
    /// Given two vectors: each from root to tip representing a limb, get their angle in degrees
    /// </summary>
    public float LimbAngleDifference(Vector2 reference, Vector2 player)
    {
        if (reference.magnitude * player.magnitude == 0) 
        {
            // If any of the two vector2 has 0 as magnitude, can't use as dot calculation!
            return float.MinValue;
        }

        float dot = Vector2.Dot(reference, player) / (reference.magnitude * player.magnitude);
        dot = Mathf.Clamp(dot, -1f, 1f);
        
        return Mathf.Acos(dot) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Get the total score average by the number of detections
    /// </summary>
    public int GetTotalScore()
    {
        return Mathf.RoundToInt(totalScore / compare_counts);
    }
}
