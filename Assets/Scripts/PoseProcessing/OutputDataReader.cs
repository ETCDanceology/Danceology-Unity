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
/// Class used to manage the output (visualization) of joint positions processed from the model
/// </summary>
public class OutputDataReader : CSingletonMono<OutputDataReader>
{
    private bool isDisabled = false;
    public MovementCompare movementCompare;
    public GameObject jointImg;
    public Transform imgFolder;
    private float[,,] data;
    private float[,,] prevData;
    private float[,,] prevCapturedData;
    private int mainFocus = 0;
    private int prevMainFocus;
    private int prevCapturedMainFocus;

    [Header("MainScene")]
    public float xAdjust;
    public float yAdjust;
    public float scale;

    [Header("Calibration")]
    public float calibXAdjust;
    public float calibYAdjust;
    public float calibScale;

    // Used for displaying joint connections
    private static int[] nodeConnection = new int[18] { 0, 0, 1, 2, 3, 1, 5, 6, 1, 8, 9, 1, 11, 12, 0, 0, 14, 15 };

    private void Start()
    {
        float screenScale = Camera.main.pixelWidth / 1920f;
        xAdjust *= screenScale;
        yAdjust *= screenScale;
        scale *= screenScale;
    }

    /// <summary>
    /// Given the processed output from the model for a given frame in the webcam, 
    /// initializes the data and determines the main person to focus on
    /// </summary>
    public bool ProcessInputImage(List<Candidate> candidates, List<PeopleBody> bodySubset)
    {
        prevCapturedData = data;
        prevCapturedMainFocus = mainFocus;

        data = new float[bodySubset.Count, 18, 4];
        mainFocus = 0;

        float minDiff = Mathf.Infinity;
        for (int i = 0; i < bodySubset.Count; i++)
        {
            float bodyDiff = 0;
            for (int j = 0; j < bodySubset[i].body_part.Length; j++)
            {
                int pos = bodySubset[i].body_part[j];
                if (pos == -1)
                {
                    data[i, j, 0] = -1;
                    data[i, j, 1] = -1;
                    data[i, j, 2] = -1;
                    data[i, j, 3] = -1;

                    if (prevCapturedData != null && prevCapturedData.GetLength(0) > 0)
                    {
                        bodyDiff += Mathf.Sqrt((float)Mathf.Pow(prevCapturedData[prevCapturedMainFocus, j, 0], 2) +
                            (float)Mathf.Pow(prevCapturedData[prevCapturedMainFocus, j, 1], 2));
                    }
                } 
                else
                {
                    data[i, j, 0] = candidates[pos].x;
                    data[i, j, 1] = candidates[pos].y;
                    data[i, j, 2] = candidates[pos].score;
                    data[i, j, 3] = candidates[pos].ind;

                    if (prevCapturedData != null && prevCapturedData.GetLength(0) > 0)
                    {
                        bodyDiff += Mathf.Sqrt((float)Mathf.Pow(prevCapturedData[prevCapturedMainFocus, j, 0] - data[i, j, 0], 2) +
                            (float)Mathf.Pow(prevCapturedData[prevCapturedMainFocus, j, 1] - data[i, j, 1], 2));
                    }
                }
            }

            if (bodyDiff < minDiff)
            {
                minDiff = bodyDiff;
                mainFocus = i;
            }
        }
        if (GameManager.instance.state == GameState.InLevel) movementCompare.UpdatePlayerPose(data, mainFocus);
        return candidates.Count != 0;
    }

    /// <summary>
    /// Disables the joint marking UI for the player webcam
    /// </summary>
    public void DisableVisibility()
    {
        isDisabled = true;
        ObjectPool.instance.DisableAll();
    }

    /// <summary>
    /// Enables the joint marking UI for the player webcam
    /// </summary>
    public void EnableVisibility()
    {
        isDisabled = false;
    }

    /// <summary>
    /// General update loop
    /// </summary>
    private void FixedUpdate()
    {
        if (ObjectPool.instance) 
        {
            ObjectPool.instance.DisableAll();
        }

        if (data == null || data.GetLength(0) == 0 || isDisabled)
        {
            // Nothing to do
            return;
        }

        int j = mainFocus; // Only grab for single person
        
        // Check for hand flipping
        if (prevData != null && prevData.GetLength(0) > 0 && prevData[prevMainFocus, 4, 0] != -1 && 
            prevData[prevMainFocus, 7, 0] != -1 && data[j, 4, 0] != -1 && data[j, 7, 0] != -1)
        {
            Vector2 currRHandPos = new Vector2(data[j, 4, 0], data[j, 4, 1]);
            Vector2 currLHandPos = new Vector2(data[j, 7, 0], data[j, 7, 1]);
            Vector2 prevRHandPos = new Vector2(prevData[prevMainFocus, 4, 0], prevData[prevMainFocus, 4, 1]);
            Vector2 prevLHandPos = new Vector2(prevData[prevMainFocus, 7, 0], prevData[prevMainFocus, 7, 1]);

            // Compute distance from current pos to previous
            float RHandMatching = Vector2.Distance(currRHandPos, prevRHandPos);
            float LHandMatching = Vector2.Distance(currLHandPos, prevLHandPos);
            float RHandSwitched = Vector2.Distance(currRHandPos, prevLHandPos);
            float LHandSwitched = Vector2.Distance(currLHandPos, prevRHandPos);

            if (LHandMatching > LHandSwitched && RHandMatching > RHandSwitched)
            {
                // Makes more sense to swap the two points
                SwapDataPoints(j, 4, 7);
                SwapDataPoints(j, 3, 6);
            }
        }

        float widthScale = Screen.width / 1920f;
        float heightScale = Screen.height / 1080f;

        // 1-13 are nodes for body, 0 & 14-17 are face
        for (int i = 1; i < 14; i++)
        {
            // Current Data Node is missing, go on to next
            if (data[j, i, 0] == -1)
            {
                continue;
            }

            Vector3 curPos;
            if (GameManager.instance.state == GameState.InLevel)
            {
                curPos = new Vector3(xAdjust - data[j, i, 0] * scale, yAdjust - data[j, i, 1] * scale, 0);
            }
            else
            { 
                curPos = new Vector3((calibXAdjust - data[j, i, 0] * calibScale) * widthScale, (calibYAdjust - data[j, i, 1] * calibScale) * heightScale, 0);
            }

            if (i == 4 || i == 7 || i == 10 || i == 13) // Right hand | Left Hand | Right Ankle | Left Ankle
            {
                Vector2 endPos = new Vector2(data[j, i, 0], data[j, i, 1]);
                Vector2 startPos = endPos;

                if (data[j, i - 1, 0] != -1)
                {
                    startPos = new Vector2(data[j, i - 1, 0], data[j, i - 1, 1]);
                }

                ObjectPool.instance.SetBandPos((WristBand)i, curPos, Quaternion.FromToRotation(Vector2.up, endPos - startPos));
            }
            else
            {
                ObjectPool.instance.SetSpecifiedObj(j * 18 + i, curPos);
            }
        }

        prevData = data;
        prevMainFocus = mainFocus;
    }

    /// <summary>
    /// Helper function to swap the data values for two given joints
    /// </summary>
    private void SwapDataPoints(int j, int jointIndexA, int jointIndexB)
    {
        float xA = data[j, jointIndexA, 0];
        float yA = data[j, jointIndexA, 1];

        data[j, jointIndexA, 0] = data[j, jointIndexB, 0];
        data[j, jointIndexA, 1] = data[j, jointIndexB, 1];

        data[j, jointIndexB, 0] = xA;
        data[j, jointIndexB, 1] = yA;
    }
}
