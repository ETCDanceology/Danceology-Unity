/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using UnityEngine;

/// <summary>
/// Class for a given animated 3D model
/// </summary>
public class AnimModelMovement : MonoBehaviour
{
    // All joint positions for the given model
    private JointPoint[] jointPoints;
    public JointPoint[] JointPoints { get { return jointPoints; } }

    /// <summary>
    /// Animation-related fields
    /// </summary>
    private Animator anim;
    private string animationName = "Base Layer.Back Exercise";
    private float clipLength;
    private float clipFrameRate;
    private float numFrames;

    /// <summary>
    /// Level-related fields
    /// </summary>
    public TutorialKeyFrameData levelTutorialData;
    private LevelData levelData;
    private LevelType levelType;

    /// <summary>
    /// Miscellaneous fields used within the experience
    /// </summary>
    private bool loopPlayback;                  // Whether to loop the current animation when playback has finished
    private float playbackFPS;                  // FPS for the current playback
    private float timer;                        // Timer for current frame
    private bool isPlaying;                     // Flag on whether to play the animation
    private int tutorialInd = 0;                // Index for the current tutorial phase
    private int currentPoseIndex = 0;           // Frame index for the current pose

    /// <summary>
    /// Initialization of key fields
    /// </summary>
    void Awake()
    {
        anim = GetComponent<Animator>();
        AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
        clipLength = clips[0].clip.length;
        clipFrameRate = clips[0].clip.frameRate;
        numFrames = clipLength * clipFrameRate;

        levelTutorialData.StackKeyframeTutorialData();
        InitializeJointPoints();
    }

    /// <summary>
    /// Main update loop
    /// </summary>
    private void Update()
    {
        // Special case for guided tutorial level - handling current tutorial frames before updating
        if (levelType == LevelType.Guided)
        {
            if (isTutorialKeyPoseFrame(currentPoseIndex))
            {
                HandleTutorialKeyPose();
            }
        }

        if (!isPlaying) return;

        // Update logic on current frame
        timer += Time.deltaTime;
        if (timer >= (1 / playbackFPS))
        {
            // Reached time for frame update; increment frame index
            currentPoseIndex += 1;
            GameProgress.instance.UpdateProgress(currentPoseIndex);
            timer = 0;

            if (currentPoseIndex >= levelData.poseData.Length)
            {
                if (loopPlayback)
                {
                    currentPoseIndex = 0;
                }
                else
                {
                    FinishPlaying();
                    return;
                }
            }

            SetPose();
            MovementCompare.instance.CallNextFrameData();

            // Handling key pose frames
            if (levelData.isKeyPoseFrame(currentPoseIndex))
            {
                HandleKeyPose();
            }
        }
    }

    /// <summary>
    /// Initializes all the joint positions and references within the current models
    /// Requires a humanoid avatar bounded to the current Animator
    /// </summary>
    private void InitializeJointPoints()
    {
        jointPoints = new JointPoint[PositionIndex.Count.Int()];
        for (var i = 0; i < PositionIndex.Count.Int(); i++) jointPoints[i] = new JointPoint();

        if (anim.avatar == null) return;
        MovementUtil.InitializeJointPoints(jointPoints, anim);
        anim.avatar = null;
    }

    /// <summary>
    /// Starts playing the current level given initializing fields
    /// </summary>
    public void StartPlaying(LevelData levelData, LevelType levelType, float playbackFPS, bool loopPlayback = false)
    {
        this.levelData = levelData;
        GameProgress.instance.SetTotalAction(levelData.poseData.Length);

        this.levelType = levelType;
        this.playbackFPS = playbackFPS;
        this.loopPlayback = loopPlayback;

        currentPoseIndex = 0;
        timer = 0;
        SetPose();
        isPlaying = true;
        tutorialInd = 0;
        EventBus.Broadcast(EventTypes.ModelStartedPlaying, this);
    }

    /// <summary>
    /// Broadcasts event when a key pose is reached within the level
    /// </summary>
    private void HandleKeyPose()
    {
        EventBus.Broadcast(EventTypes.ReachedKeyFrame, currentPoseIndex);
    }

    /// <summary>
    /// Sets the current pose for the animation based on the currentPoseIndex
    /// </summary>
    private void SetPose()
    {
        anim.Play(animationName, 0, currentPoseIndex / numFrames);
    }

    /// <summary>
    /// Continues playing the current animation
    /// </summary>
    public void ContinuePlaying()
    {
        isPlaying = true;
    }

    /// <summary>
    /// Continues playing from a specified index
    /// </summary>
    public void ContinuePlaying(int resetIndex)
    {
        // If exists, set the previous pose first to ensure smoother transition
        if (resetIndex - 1 >= 0)
        {
            currentPoseIndex = resetIndex - 1;
            SetPose();
        }

        currentPoseIndex = resetIndex;
        SetPose();
        ContinuePlaying();
    }

    /// <summary>
    /// Stops playing the current animation
    /// </summary>
    public void StopPlaying()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Called when the current animation finishes playing
    /// </summary>
    public void FinishPlaying()
    {
        StopPlaying();
        EventBus.Broadcast(EventTypes.ModelStoppedPlaying, this);
    }

    #region Getters and Setters
    /// <summary>
    /// Gets the current level data
    /// </summary>
    public LevelData GetLevelData()
    {
        return levelData;
    }

    /// <summary>
    /// Gets the current frame index
    /// </summary>
    public int GetCurrentFrame()
    {
        return currentPoseIndex;
    }

    /// <summary>
    /// Gets the current playback FPS
    /// </summary>
    public float GetPlayFPS()
    {
        return playbackFPS;
    }

    /// <summary>
    /// Sets the current playback FPS
    /// </summary>
    public void SetPlayFPS(int fps)
    {
        playbackFPS = fps;
    }

    /// <summary>
    /// Gets the position for given joint if it exists; else, returns null
    /// </summary>
    public Vector3? GetJointPosition(PositionIndex positionIndex)
    {
        return jointPoints[positionIndex.Int()]?.Transform?.position;
    }

    /// <summary>
    /// Returns whether the current animation is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    #endregion

    #region Tutorial

    /// <summary>
    /// Check if the given keyframe index is a tutorial key frame
    /// </summary>
    private bool isTutorialKeyPoseFrame(int currentPose)
    {
        // No tutorial data
        if (levelTutorialData.keyframeTutorialData == null) return false;

        // Finished tutorial
        if (tutorialInd >= levelTutorialData.keyframeTutorialData.Count) return false;

        // Checks if we're in range as a tutorial key pose frame
        return currentPose >= levelTutorialData.keyframeTutorialData[tutorialInd].frameCount;
    }

    /// <summary>
    /// Broadcast a event to trigger a tutorial key
    /// </summary>
    private void HandleTutorialKeyPose()
    {
        EventBus.Broadcast(EventTypes.ReachedTutorialKey, levelTutorialData.keyframeTutorialData[tutorialInd]);
    }

    /// <summary>
    /// Increments tutorial index
    /// </summary>
    public void TutorialToNext()
    {
        tutorialInd++;
    }

    /// <summary>
    /// Grabs the current tutorial data
    /// </summary>
    public TutorialData GetCurrentTutorial()
    {
        return levelTutorialData.keyframeTutorialData[tutorialInd];
    }
    #endregion    
}
