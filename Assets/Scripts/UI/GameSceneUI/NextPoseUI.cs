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
/// Data class for all next pose data when displaying UI
/// </summary>
public class NextPoseData
{
    public int poseId;
    public bool isReversed;
    public int frameCount;
}

public class NextPoseUI : MonoBehaviour
{
    public Sprite[] poses;

    public GameObject poseContentParent;
    public GameObject poseImagePrefab;
    private List<GameObject> poseImages;

    public GameObject targetPoseLocation;
    public float scrollSpeedFor30fps;

    public float shrinkCircleMaxSize;
    public float shrinkCircleMinSize;
    public GameObject shrinkCircle;
    private AnimModelMovement model;
    private Vector3 contentStartPos;
    private float fps;

    private List<int> shrinkFrame = new List<int>();
    private int shrinkTimeInd = 0;

    private void Start()
    {
        contentStartPos = poseContentParent.transform.position;
    }

    public void OnEnable()
    {
        EventBus.AddListener<AnimModelMovement>(EventTypes.ModelStartedPlaying, InitializeNextPoses);
    }
    
    public void OnDisable()
    {
        EventBus.RemoveListener<AnimModelMovement>(EventTypes.ModelStartedPlaying, InitializeNextPoses);
    }

    /// <summary>
    /// Initializes all next pose UI based on the given model
    /// </summary>
    void InitializeNextPoses(AnimModelMovement model)
    {
        this.model = model;
        fps = model.GetPlayFPS();
        LevelData levelData = model.GetLevelData();

        Vector3 targetPos = targetPoseLocation.transform.position;

        poseImages = new List<GameObject>();

        // Reset the shrink circle's list
        shrinkTimeInd = 0;
        shrinkFrame.Clear();

        foreach (KeyPoseData keyPose in levelData.keyPoseData)
        {
            GameObject poseImage = Instantiate(poseImagePrefab, poseContentParent.transform);
            poseImage.GetComponent<PoseImageUI>().Initialize(keyPose, targetPoseLocation);
            poseImages.Add(poseImage);

            // Determine how far to place from target
            float deltaX = scrollSpeedFor30fps * (fps / 30) * (keyPose.frameCount / fps);
            poseImage.transform.position = targetPos + (Vector3.right * deltaX);
            shrinkFrame.Add(keyPose.frameCount);
        }
    }

    /// <summary>
    /// General update loop
    /// </summary>
    void Update()
    {
        if (model == null) return;

        int currentFrame = model.GetCurrentFrame();
        float deltaX = scrollSpeedFor30fps * (fps / 30) * (currentFrame / fps);
        poseContentParent.transform.position = contentStartPos + Vector3.left * deltaX;
        int previousFrame = shrinkTimeInd >= 1 ? shrinkFrame[shrinkTimeInd - 1] : 0;
            
        float shrinkRatio = shrinkCircleMaxSize - (shrinkCircleMaxSize - shrinkCircleMinSize)*(currentFrame - previousFrame) /(shrinkFrame[shrinkTimeInd] - previousFrame);
        shrinkCircle.transform.localScale = Vector3.one * shrinkRatio;
    }

    /// <summary>
    /// Starts the shrinking animation for the circle UI
    /// </summary>
    public void StartShrink()
    {
        // Set to original max size
        if (shrinkTimeInd < shrinkFrame.Count - 1)
        {
            shrinkCircle.transform.localScale = Vector3.one * shrinkCircleMaxSize;
            shrinkTimeInd += 1;
        }
        else
        {
            shrinkCircle.transform.localScale = Vector3.zero;
        }
    }
}
